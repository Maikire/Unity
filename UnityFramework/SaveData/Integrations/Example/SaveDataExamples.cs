using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData.Example
{
    /// <summary>
    /// 展示初始化、默认值、读写、预览、多栏位、备份、错误处理、会话结束
    /// </summary>
    public sealed class SaveDataExamples
    {
        /// <summary>
        /// 使用独立目录，便于检查和清理示例存档
        /// </summary>
        public const string DirectoryName = "SaveDataExample";

        private readonly SaveDataExampleParticipant participant;
        private bool ownsSession;

        /// <summary>
        /// 项目启动流程持有的游戏运行时
        /// </summary>
        public SaveDataExampleRuntime Progress { get; } = new SaveDataExampleRuntime();

        /// <summary>
        /// 创建运行时与适配器
        /// </summary>
        public SaveDataExamples()
        {
            participant = new SaveDataExampleParticipant(Progress);
        }

        /// <summary>
        /// 在 Unity 主线程读取项目设置，初始化示例的唯一会话
        /// </summary>
        public void Initialize(SaveDataOptions options = null)
        {
            options ??= SaveDataSettings.LoadOptions();
            // 保留项目的格式、压缩、加密和密钥配置，只指定示例目录与契约
            options.DirectoryName = DirectoryName;
            options.Schema = new SaveDataSchema("example.progress", 1, SaveDataExampleParticipant.Id);
            SaveDataManager.Initialize(options);
            ownsSession = true;
            try
            {
                SaveDataManager.Register(participant);
            }
            catch
            {
                Close();
                throw;
            }
        }

        /// <summary>
        /// 在临时栏位依次演示常用接口，结束后清理文件并恢复原来的游戏进度
        /// </summary>
        public async Task RunAsync(Action<string> report, CancellationToken cancellationToken = default)
        {
            if (!ownsSession)
                throw new InvalidOperationException("请先初始化示例会话");

            if (report == null)
                throw new ArgumentNullException(nameof(report));

            string slotId = "demo-" + Guid.NewGuid().ToString("N");
            string originalSlot = SaveDataManager.CurrentSaveName;
            SaveDataExampleProgress original = participant.CaptureData();
            try
            {
                // 不存在的栏位恢复完整默认值，此时还没有创建存档文件
                bool loaded = await SaveDataManager.LoadOrDefaultAsync(slotId, cancellationToken);
                report($"默认值：读取到文件={loaded}，金币={Progress.Coins}，检查点={Progress.CheckpointId}");

                Progress.Coins = 10;
                Progress.CheckpointId = "village";
                await SaveDataManager.SaveAsync(slotId, cancellationToken);

                // 第二次保存将前一次的 10 金币快照备份
                Progress.Coins = 30;
                Progress.CheckpointId = "castle";
                await SaveDataManager.SaveAsync(slotId, cancellationToken);

                Progress.Coins = 999;
                await SaveDataManager.LoadAsync(slotId, cancellationToken: cancellationToken);
                report($"保存与读档：未保存的 999 金币恢复为 {Progress.Coins}，检查点={Progress.CheckpointId}");

                // ReadAsync 只返回文档，预览数据不会恢复到运行时
                Progress.Coins = 888;
                SaveDataDocument document = await SaveDataManager.ReadAsync(slotId, cancellationToken: cancellationToken);
                SaveDataExampleProgress preview = SaveDataJson.FromToken<SaveDataExampleProgress>(document.Modules[SaveDataExampleParticipant.Id]);
                report($"只读预览：文件金币={preview.Coins}，运行时金币仍为 {Progress.Coins}");

                SaveSlotInfo[] slots = await SaveDataManager.GetSaveSlotsAsync(cancellationToken);
                foreach (SaveSlotInfo slot in slots)
                {
                    report($"栏位：{slot.SlotId}，有效={slot.IsValid}，存在备份={slot.HasBackup}");
                }

                // 加载备份只恢复运行时，正式文件仍保留第二次保存的数据
                await SaveDataManager.LoadAsync(slotId, backup: true, cancellationToken: cancellationToken);
                report($"加载备份：金币={Progress.Coins}，检查点={Progress.CheckpointId}");

                // 保留备份并删除正式档，模拟文件丢失，再从备份修复
                await SaveDataManager.DeleteAsync(slotId, includeBackup: false, cancellationToken: cancellationToken);
                // 修复备份只修改文件，需要再次 LoadAsync 才恢复游戏状态
                await SaveDataManager.RestoreBackupAsync(slotId, cancellationToken);
                await SaveDataManager.LoadAsync(slotId, cancellationToken: cancellationToken);
                report($"修复正式档后读档：金币={Progress.Coins}，检查点={Progress.CheckpointId}");

                // TryLoadAsync 用结构化结果表达不存在的栏位，调用方处理错误类别
                SaveDataResult missing = await SaveDataManager.TryLoadAsync(slotId + "-missing", cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                report($"错误处理：成功={missing.Success}，错误类别={missing.ErrorCode}");
            }
            finally
            {
                try
                {
                    // 请求结束后清理自己的临时栏位，包括备份
                    await SaveDataManager.DeleteAsync(slotId, includeBackup: true, cancellationToken: CancellationToken.None);
                    report("临时栏位及备份已删除");
                    if (originalSlot != null)
                    {
                        await SaveDataManager.LoadOrDefaultAsync(originalSlot, CancellationToken.None);
                    }
                }
                finally
                {
                    participant.RestoreData(original);
                }
            }
        }

        /// <summary>
        /// 所有异步请求结束后，由拥有会话的启动流程负责清理注册与通知
        /// </summary>
        public void Close()
        {
            if (!ownsSession)
                return;

            SaveDataManager.ResetSession();
            ownsSession = false;
        }

    }
}
