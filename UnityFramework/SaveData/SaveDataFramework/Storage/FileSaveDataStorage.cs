using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 本地后端先提交旧档备份，再编码临时新档并替换正式档，要求目录内单写入者
    /// </summary>
    public sealed class FileSaveDataStorage : ISynchronousSaveDataStorage, ISaveDataStoragePersistence
    {
        /// <summary>
        /// 正式档的统一扩展名，与 .backup 备份路径分离
        /// </summary>
        public const string PrimaryFileExtension = ".dat";

        /// <summary>
        /// 绝对存档目录
        /// </summary>
        private readonly string directory;

        /// <summary>
        /// 单档字节上限
        /// </summary>
        private readonly long maxBytes;

        /// <summary>
        /// 文件后端支持写入
        /// </summary>
        public bool CanWrite => true;

        /// <summary>
        /// 文件后端支持备份
        /// </summary>
        public bool SupportsBackup => true;

        /// <summary>
        /// Web 原始文件仅确认当前会话，其他平台确认本地文件系统提交
        /// </summary>
        public SaveDataPersistenceScope PersistenceScope
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return SaveDataPersistenceScope.CurrentSession;
#else
                return SaveDataPersistenceScope.PersistentStorage;
#endif
            }
        }

        /// <summary>
        /// 配置绝对目录，只使用 .dat 正式档和 .backup 备份
        /// </summary>
        public FileSaveDataStorage(string directory, string extension = PrimaryFileExtension, long maxBytes = 8 * 1024 * 1024)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Path.IsPathRooted(directory))
            {
                throw new ArgumentException("存档目录必须为绝对路径", nameof(directory));
            }

            if (maxBytes < 1 || maxBytes > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBytes));
            }

            this.directory = Path.GetFullPath(directory);
            if (!string.Equals(extension, PrimaryFileExtension, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("正式档扩展名必须为 .dat", nameof(extension));

            this.maxBytes = maxBytes;
        }

        /// <summary>
        /// 使用 FormC 和小写统一跨平台名称，拒绝路径穿越和系统保留名
        /// </summary>
        public string NormalizeName(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId))
            {
                throw new ArgumentException("栏位 ID 不能为空", nameof(slotId));
            }

            string name;
            if (slotId.EndsWith(PrimaryFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                name = slotId[..^PrimaryFileExtension.Length];
            }
            else
            {
                name = slotId;
            }

            if (name.Length == 0 || name.Length > 128 || name != name.Trim() || name.Contains(".")
                || name.Any(character => char.IsControl(character) || "/\\:*?\"<>|".IndexOf(character) >= 0))
            {
                throw new ArgumentException("栏位 ID 包含非法文件名字符", nameof(slotId));
            }

            name = name.Normalize(NormalizationForm.FormC).ToLowerInvariant();

            string[] reservedName = new[]
            {
                //CON：控制台设备、PRN：打印机设备、AUX：辅助设备、NUL：空设备
                "con", "prn", "aux", "nul"
            };

            if (reservedName.Contains(name)
                || name.Length == 4
                && (name.StartsWith("com", StringComparison.Ordinal) || name.StartsWith("lpt", StringComparison.Ordinal))
                && name[3] >= '1'
                && name[3] <= '9')
            {
                throw new ArgumentException("栏位 ID 是系统保留名", nameof(slotId));
            }

            return name;
        }

        /// <summary>
        /// 获取固定的 .dat 正式档路径
        /// </summary>
        public string GetPath(string slotId) => Path.Combine(directory, NormalizeName(slotId) + PrimaryFileExtension);

        /// <summary>
        /// 检查正式档或备份
        /// </summary>
        public bool Exists(string slotId, bool backup = false) => File.Exists(GetReadPath(slotId, backup));

        /// <summary>
        /// 在分配内存前检查文件大小并读取完整字节
        /// </summary>
        public byte[] Read(string slotId, bool backup = false)
        {
            using var stream = new FileStream(GetReadPath(slotId, backup), FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length > maxBytes)
            {
                throw new InvalidDataException("存档超过字节上限");
            }

            var bytes = new byte[(int)stream.Length];
            int offset = 0;

            while (offset < bytes.Length)
            {
                int count = stream.Read(bytes, offset, bytes.Length - offset);
                if (count == 0)
                {
                    throw new EndOfStreamException("存档读取不完整");
                }

                offset += count;
            }

            return bytes;
        }

        /// <summary>
        /// 仅列出 .dat 正式档的规范栏位名，拒绝大小写或 Unicode 名称冲突
        /// </summary>
        public string[] GetSaveNames()
        {
            if (!Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (string path in Directory.GetFiles(directory))
            {
                string suffix = Path.GetExtension(path).ToLowerInvariant();
                if (suffix != PrimaryFileExtension)
                {
                    continue;
                }

                string name = Path.GetFileNameWithoutExtension(path);
                if (Path.GetFileName(path) != NormalizeName(name) + PrimaryFileExtension)
                {
                    throw new InvalidDataException("存在非规范栏位名，请先重命名为 NFC 小写名称");
                }

                names.Add(name);
            }

            return names.OrderBy(name => name, StringComparer.Ordinal).ToArray();
        }

        /// <summary>
        /// 先备份旧正式档，再调用编码回调并原子提交新内容
        /// </summary>
        public void Write(string slotId, Func<byte[]> createData) => WriteCore(slotId, createData, CancellationToken.None);

        /// <summary>
        /// 在最终提交前支持取消，提交完成后返回成功
        /// </summary>
        private void WriteCore(string slotId, Func<byte[]> createData, CancellationToken cancellationToken)
        {
            if (createData == null)
            {
                throw new ArgumentNullException(nameof(createData));
            }

            string path = GetPath(slotId);
            Directory.CreateDirectory(directory);
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(path))
            {
                byte[] previous = Read(slotId);
                WriteFile(GetBackupPath(slotId), previous, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            WriteFile(path, createData(), cancellationToken);
        }

        /// <summary>
        /// 备份始终仅使用 .backup
        /// </summary>
        private string GetBackupPath(string slotId) => Path.Combine(directory, NormalizeName(slotId) + ".backup");

        /// <summary>
        /// 读取路径固定为 .dat 正式档或 .backup 备份
        /// </summary>
        private string GetReadPath(string slotId, bool backup)
        {
            if (backup)
            {
                return GetBackupPath(slotId);
            }
            else
            {
                return GetPath(slotId);
            }
        }

        /// <summary>
        /// 通过同目录唯一临时文件刷新并提交
        /// </summary>
        private void WriteFile(string destination, byte[] bytes, CancellationToken cancellationToken)
        {
            if (bytes == null || bytes.Length == 0 || bytes.LongLength > maxBytes)
            {
                throw new InvalidDataException("新存档字节大小无效");
            }

            string temporary = destination + ".sd-" + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (File.Exists(destination))
                {
                    File.Replace(temporary, destination, null);
                }
                else
                {
                    File.Move(temporary, destination);
                }
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
        }

        /// <summary>
        /// 将已经验证的备份恢复到原栏位，保留原始备份和坏档诊断副本
        /// </summary>
        public void Repair(string slotId, byte[] validatedBackup) => RepairCore(slotId, validatedBackup, CancellationToken.None);

        /// <summary>
        /// 修复正式档前保留原文件
        /// </summary>
        private void RepairCore(string slotId, byte[] validatedBackup, CancellationToken cancellationToken)
        {
            string path = GetPath(slotId);
            Directory.CreateDirectory(directory);
            cancellationToken.ThrowIfCancellationRequested();

            if (File.Exists(path))
            {
                File.Copy(path, path + ".corrupt-" + Guid.NewGuid().ToString("N"), false);
            }

            WriteFile(path, validatedBackup, cancellationToken);
        }

        /// <summary>
        /// 删除指定正式档和可选备份
        /// </summary>
        public void Delete(string slotId, bool includeBackup)
        {
            File.Delete(GetPath(slotId));
            if (includeBackup)
            {
                File.Delete(GetBackupPath(slotId));
            }
        }

        /// <summary>
        /// 显式清理超过指定年龄的本框架临时文件，调用方应确保没有其他进程写入
        /// </summary>
        public int CleanupTemporaryFiles(TimeSpan minimumAge)
        {
            if (minimumAge < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumAge));
            }

            if (!Directory.Exists(directory))
            {
                return 0;
            }

            int removed = 0;
            foreach (string path in Directory.GetFiles(directory, "*.tmp"))
            {
                string file = Path.GetFileName(path);
                int marker = file.LastIndexOf(".sd-", StringComparison.Ordinal);
                if (marker < 0 || !IsTemporarySaveFile(file.Substring(0, marker))
                    || file.Length - marker - 8 != 32 || !Guid.TryParseExact(file.Substring(marker + 4, file.Length - marker - 8), "N", out _))
                {
                    continue;
                }

                if (DateTime.UtcNow - File.GetLastWriteTimeUtc(path) < minimumAge)
                {
                    continue;
                }

                File.Delete(path);
                removed++;
            }
            return removed;
        }

        /// <summary>
        /// 仅识别规范 .dat 正式档或 .backup 备份的临时文件
        /// </summary>
        private bool IsTemporarySaveFile(string file)
        {
            try
            {
                string suffix = Path.GetExtension(file);
                if (suffix != PrimaryFileExtension && suffix != ".backup")
                    return false;

                return file == NormalizeName(Path.GetFileNameWithoutExtension(file)) + suffix;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// 异步契约中的存在检查，Manager 可在后台调度文件 IO
        /// </summary>
        public Task<bool> ExistsAsync(string slotId, bool backup, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Exists(slotId, backup));
        }

        /// <summary>
        /// 异步契约中的读取
        /// </summary>
        public Task<byte[]> ReadAsync(string slotId, bool backup, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Read(slotId, backup));
        }

        /// <summary>
        /// 异步契约中的列举
        /// </summary>
        public Task<string[]> GetSaveNamesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetSaveNames());
        }

        /// <summary>
        /// 异步契约中的备份和提交
        /// </summary>
        public Task WriteAsync(string slotId, Func<byte[]> createData, CancellationToken cancellationToken)
        {
            WriteCore(slotId, createData, cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 异步契约中的原栏位修复
        /// </summary>
        public Task RepairAsync(string slotId, byte[] validatedBackup, CancellationToken cancellationToken)
        {
            RepairCore(slotId, validatedBackup, cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 异步契约中的删除，文件操作过程中不能取消
        /// </summary>
        public Task DeleteAsync(string slotId, bool includeBackup, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Delete(slotId, includeBackup);
            return Task.CompletedTask;
        }

    }
}
