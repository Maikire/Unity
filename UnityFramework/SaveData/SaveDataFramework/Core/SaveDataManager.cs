using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 静态会话入口，保存时机由项目决定，异步请求串行执行
    /// </summary>
    public static class SaveDataManager
    {
        /// <summary>
        /// 在同一操作上下文中区分配置期限和调用方显式取消
        /// </summary>
        private sealed class ReadinessDeadline
        {
            /// <summary>
            /// 平台计时服务是否已完成本次实际时间期限
            /// </summary>
            internal bool Expired;
        }

        /// <summary>
        /// 当前会话已注册模块
        /// </summary>
        private static readonly List<ISaveDataParticipant> participants = new List<ISaveDataParticipant>();

        /// <summary>
        /// 串行操作门，包含读取、保存、恢复、删除
        /// </summary>
        private static readonly SaveDataOperationGate operationGate = new SaveDataOperationGate();

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器默认的私有 Newtonsoft JSON 编码器
        /// </summary>
        private static ISaveDataSerializer serializer = new JsonSaveDataSerializer();
#else
        /// <summary>
        /// Player 默认的私有文件编码器，开发构建使用 JSON，发布构建使用字节文件
        /// </summary>
        private static ISaveDataSerializer serializer = SaveDataSerializerFactory.Create(new SaveDataOptions());
#endif

        /// <summary>
        /// 当前会话的存储后端
        /// </summary>
        private static ISaveDataStorage storage;

        /// <summary>
        /// 当前项目迁移管线
        /// </summary>
        private static SaveDataMigrationPipeline migrations;

        /// <summary>
        /// 初始化时复制的会话配置
        /// </summary>
        private static SaveDataOptions options;

        /// <summary>
        /// 项目初始化线程，用于约束 Unity 回调线程
        /// </summary>
        private static int ownerThread;

        /// <summary>
        /// 等待和执行中的操作数量，非零时禁止重置或更改模块
        /// </summary>
        private static int pendingOperations;

        /// <summary>
        /// 覆盖同步与异步调用链的回调重入标记
        /// </summary>
        private static readonly AsyncLocal<bool> insideCallback = new AsyncLocal<bool>();

        /// <summary>
        /// 管理器生命周期状态
        /// </summary>
        public static SaveDataState State { get; private set; } = SaveDataState.Uninitialized;

        /// <summary>
        /// 最近成功保存、恢复、创建的栏位
        /// </summary>
        public static string CurrentSaveName { get; private set; }

        /// <summary>
        /// 最近完成的操作结果
        /// </summary>
        public static SaveDataResult LastResult { get; private set; }

        /// <summary>
        /// 当前后端声明的跨会话读取范围，未声明或未初始化时不推断持久性
        /// </summary>
        public static SaveDataPersistenceScope PersistenceScope
        {
            get
            {
                if (storage is ISaveDataStoragePersistence persistence)
                {
                    return persistence.PersistenceScope;
                }

                return SaveDataPersistenceScope.BackendDefined;
            }
        }

        /// <summary>
        /// 操作完成通知，订阅者异常不会改变已经完成的操作结果
        /// </summary>
        public static event Action<SaveDataResult> Completed;

        /// <summary>
        /// 通知回调异常的诊断接缝，诊断回调自身异常被隔离
        /// </summary>
        public static event Action<Exception> NotificationFailed;

        /// <summary>
        /// 初始化当前会话，重复初始化须要先 ResetSession()
        /// </summary>
        public static void Initialize(SaveDataOptions configuration = null)
        {
            if (State != SaveDataState.Uninitialized || pendingOperations != 0)
            {
                throw new InvalidOperationException("当前会话已初始化");
            }

            var source = configuration ?? SaveDataSettings.LoadOptions();
            if (source.Schema == null || source.MaxBytes < 1 || source.MaxNodes < 1 || source.MaxSlots < 1)
            {
                throw new ArgumentException("Schema 和容量配置无效", nameof(configuration));
            }

            var copy = new SaveDataOptions
            {
                Schema = source.Schema,
                Storage = source.Storage,
                Serializer = source.Serializer,
                FileFormat = source.FileFormat,
                PrettyPrintJson = source.PrettyPrintJson,
                EncryptionMode = source.EncryptionMode,
                Encryption = source.Encryption,
                EncryptionKeyProvider = source.EncryptionKeyProvider,
                EncryptionKeyId = source.EncryptionKeyId,
                CompressionMode = source.CompressionMode,
                Compression = source.Compression,
                DirectoryName = source.DirectoryName,
                MaxBytes = source.MaxBytes,
                MaxNodes = source.MaxNodes,
                MaxSlots = source.MaxSlots,
                ReadinessTimeout = source.ReadinessTimeout,
                TimeService = source.TimeService ?? SaveDataUnityBridge.CreateTimeService(),
                EnforceThreadAffinity = source.EnforceThreadAffinity,
                UseBackgroundIO = source.UseBackgroundIO && SaveDataUnityBridge.SupportsBackgroundIO,
                Migrations = (source.Migrations ?? Array.Empty<ISaveDataMigration>()).ToArray()
            };

            var nextSerializer = SaveDataSerializerFactory.Create(copy);
            var nextStorage = copy.Storage ?? SaveDataUnityBridge.CreateLocalStorage(copy, nextSerializer.FileExtension);
            var nextMigrations = new SaveDataMigrationPipeline(copy.Schema);

            foreach (ISaveDataMigration migration in copy.Migrations)
            {
                nextMigrations.Register(migration);
            }

            ownerThread = Thread.CurrentThread.ManagedThreadId;
            options = copy;
            serializer = nextSerializer;
            migrations = nextMigrations;
            storage = nextStorage;
            State = SaveDataState.Ready;
        }

        /// <summary>
        /// 清理会话模块、迁移、栏位、通知，调用前必须结束所有请求
        /// </summary>
        public static void ResetSession()
        {
            CheckThread();
            if (pendingOperations != 0 || insideCallback.Value)
            {
                throw new InvalidOperationException("有未完成操作，不能重置会话");
            }

            participants.Clear();
            storage = null;
            serializer = null;
            migrations = null;
            options = null;
            CurrentSaveName = null;
            LastResult = null;
            Completed = null;
            NotificationFailed = null;
            ownerThread = 0;
            State = SaveDataState.Uninitialized;
        }

        /// <summary>
        /// 注册模块，同字符串 ID 的不同实例被拒绝
        /// </summary>
        public static void Register(ISaveDataParticipant participant)
        {
            EnsureIdle();
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (string.IsNullOrWhiteSpace(participant.ParticipantId))
            {
                throw new ArgumentException("模块 ID 不能为空", nameof(participant));
            }

            if (participants.Contains(participant))
            {
                return;
            }

            if (participants.Any(value => value.ParticipantId == participant.ParticipantId))
            {
                throw new InvalidOperationException("模块 ID 已注册");
            }

            participants.Add(participant);
        }

        /// <summary>
        /// 注销指定实例，不存在时忽略，禁止在操作排队或执行时注销
        /// </summary>
        public static void Unregister(ISaveDataParticipant participant)
        {
            EnsureIdle();
            participants.Remove(participant);
        }

        /// <summary>
        /// 在首次读档前注册项目迁移，推荐通过初始化配置一次性交付
        /// </summary>
        public static void RegisterMigration(ISaveDataMigration migration)
        {
            EnsureIdle();
            migrations.Register(migration);
        }

        /// <summary>
        /// 仅文件后端可获取本地路径，远端后端拒绝
        /// </summary>
        public static string GetSavePath(string slotId)
        {
            EnsureConfigured();
            return storage switch
            {
                FileSaveDataStorage local => local.GetPath(slotId),
                SynchronizedFileSaveDataStorage synchronized => synchronized.GetPath(slotId),
                _ => throw new NotSupportedException("此后端没有本地路径")
            };
        }

        /// <summary>
        /// 同步检查正式档存在，只适用于同步后端
        /// </summary>
        public static bool Exists(string slotId) => RunSync("Exists", slotId, false, false, (id, sync, token) => ExistsCore(id, false, sync, token));

        /// <summary>
        /// 异步检查正式档存在
        /// </summary>
        public static Task<bool> ExistsAsync(string slotId, CancellationToken cancellationToken = default) =>
            ExecuteAsync("Exists", slotId, false, false, false, cancellationToken, (id, sync, token) => ExistsCore(id, false, sync, token));

        /// <summary>
        /// 同步列出正式栏位
        /// </summary>
        public static string[] GetSaveNames() => RunSync("List", null, false, false, (id, sync, token) => NamesCore(sync, token));

        /// <summary>
        /// 异步列出正式栏位
        /// </summary>
        public static Task<string[]> GetSaveNamesAsync(CancellationToken cancellationToken = default) =>
            ExecuteAsync("List", null, false, false, false, cancellationToken, (id, sync, token) => NamesCore(sync, token));

        /// <summary>
        /// 同步扫描栏位元信息，单个坏档不会阻止展示其他栏位
        /// </summary>
        public static SaveSlotInfo[] GetSaveSlots() => RunSync("Slots", null, false, false, (id, sync, token) => SlotsCore(sync, token));

        /// <summary>
        /// 异步扫描栏位信息
        /// </summary>
        public static Task<SaveSlotInfo[]> GetSaveSlotsAsync(CancellationToken cancellationToken = default) =>
            ExecuteAsync("Slots", null, false, false, false, cancellationToken, (id, sync, token) => SlotsCore(sync, token));

        /// <summary>
        /// 读取栏位
        /// </summary>
        private static async Task<SaveSlotInfo[]> SlotsCore(bool synchronous, CancellationToken token)
        {
            var slots = new List<SaveSlotInfo>();

            foreach (string id in await NamesCore(synchronous, token))
            {
                token.ThrowIfCancellationRequested();
                var info = new SaveSlotInfo
                {
                    SlotId = id
                };

                try
                {
                    SaveDataDocument document = await ReadCore(id, false, synchronous, token);
                    info.SchemaId = document.SchemaId;
                    info.Version = document.Version;
                    info.SavedAtUtc = document.SavedAtUtc;
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    info.Error = exception;
                }

                try
                {
                    info.HasBackup = storage.SupportsBackup && await ExistsCore(id, true, synchronous, token);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    info.Error = info.Error ?? exception;
                }

                slots.Add(info);
            }

            return slots.ToArray();
        }

        /// <summary>
        /// 保存当前选定栏位
        /// </summary>
        public static void Save() => Save(CurrentSaveName ?? throw new InvalidOperationException("尚未选定栏位"));

        /// <summary>
        /// 同步采集并保存，覆盖前备份，保留目标栏位未知模块
        /// </summary>
        public static void Save(string slotId) => RunSync("Save", slotId, true, false, SaveCore);

        /// <summary>
        /// 主线程采集，异步后端提交，后台只访问独立 DTO 和字节
        /// </summary>
        public static Task SaveAsync(string slotId, CancellationToken cancellationToken = default) =>
            ExecuteAsync("Save", slotId, false, true, false, cancellationToken, SaveCore);

        /// <summary>
        /// 同步读取并迁移独立快照
        /// </summary>
        public static SaveDataDocument Read(string slotId, bool backup = false) =>
            RunSync("Read", slotId, false, backup, (id, sync, token) => ReadCore(id, backup, sync, token));

        /// <summary>
        /// 异步读取并迁移独立快照
        /// </summary>
        public static Task<SaveDataDocument> ReadAsync(string slotId, bool backup = false, CancellationToken cancellationToken = default) =>
            ExecuteAsync("Read", slotId, false, false, backup, cancellationToken, (id, sync, token) => ReadCore(id, backup, sync, token));

        /// <summary>
        /// 同步读取并恢复，全部校验通过后提交，失败时反向回退
        /// </summary>
        public static void Load(string slotId, bool backup = false) =>
            RunSync("Load", slotId, true, backup, (id, sync, token) => LoadCore(id, backup, false, sync, token));

        /// <summary>
        /// 等待模块就绪后异步恢复
        /// </summary>
        public static Task LoadAsync(string slotId, bool backup = false, CancellationToken cancellationToken = default) =>
            ExecuteAsync("Load", slotId, false, true, backup, cancellationToken, (id, sync, token) => LoadCore(id, backup, false, sync, token));

        /// <summary>
        /// 存档不存在时恢复所有模块默认快照
        /// </summary>
        public static bool LoadOrDefault(string slotId) =>
            RunSync("LoadOrDefault", slotId, true, false, (id, sync, token) => LoadCore(id, false, true, sync, token));

        /// <summary>
        /// 等待所有模块后加载或完整恢复默认状态，返回是否读取到正式档
        /// </summary>
        public static Task<bool> LoadOrDefaultAsync(string slotId, CancellationToken cancellationToken = default) =>
            ExecuteAsync("LoadOrDefault", slotId, false, true, false, cancellationToken, (id, sync, token) => LoadCore(id, false, true, sync, token));

        /// <summary>
        /// 以结构化结果加载，保留诊断异常
        /// </summary>
        public static async Task<SaveDataResult> TryLoadAsync(string slotId, CancellationToken cancellationToken = default)
        {
            var result = new SaveDataResult();

            try
            {
                await ExecuteAsync("Load", slotId, false, true, false, cancellationToken,
                    (id, sync, token) => LoadCore(id, false, false, sync, token), result);
            }
            catch
            {
                // 异常由 ExecuteAsync 处理
            }

            return result;
        }

        /// <summary>
        /// 验证备份并修复原栏位，保留备份
        /// </summary>
        public static void RestoreBackup(string slotId) => RunSync("Repair", slotId, false, true, RepairCore);

        /// <summary>
        /// 异步验证备份并修复原栏位
        /// </summary>
        public static Task RestoreBackupAsync(string slotId, CancellationToken cancellationToken = default) =>
            ExecuteAsync("Repair", slotId, false, false, true, cancellationToken, RepairCore);

        /// <summary>
        /// 同步删除正式档及可选备份
        /// </summary>
        public static void Delete(string slotId, bool includeBackup = true) =>
            RunSync("Delete", slotId, false, false, (id, sync, token) => DeleteCore(id, includeBackup, sync, token));

        /// <summary>
        /// 异步删除正式档及可选备份
        /// </summary>
        public static Task DeleteAsync(string slotId, bool includeBackup = true, CancellationToken cancellationToken = default) =>
            ExecuteAsync("Delete", slotId, false, false, false, cancellationToken, (id, sync, token) => DeleteCore(id, includeBackup, sync, token));

        /// <summary>
        /// 统一操作调度，异步排队，同步入口拒绝等待，拒绝回调重入
        /// </summary>
        private static async Task<T> ExecuteAsync<T>(string operation, string slotId, bool synchronous, bool needsParticipants,
            bool backup, CancellationToken cancellationToken, Func<string, bool, CancellationToken, Task<T>> body, SaveDataResult result = null)
        {
            result ??= new SaveDataResult();
            result.Operation = operation;
            result.SlotId = slotId;
            result.UsedBackup = backup;
            bool entered = false;
            bool queued = false;
            try
            {
                EnsureConfigured();
                if (insideCallback.Value)
                {
                    throw new InvalidOperationException("模块和通知回调不能重入 Manager");
                }

                if (!synchronous && options.EnforceThreadAffinity && SynchronizationContext.Current == null)
                {
                    throw new InvalidOperationException("异步运行时回调需要初始化线程的同步上下文");
                }

                if (slotId == null && operation != "List" && operation != "Slots")
                {
                    throw new ArgumentNullException(nameof(slotId));
                }

                string id;
                if (slotId == null)
                {
                    id = null;
                }
                else
                {
                    id = storage.NormalizeName(slotId);
                }
                result.SlotId = id;

                if (State == SaveDataState.Faulted)
                {
                    throw new InvalidOperationException("运行时状态不一致，请重建后重置会话");
                }

                if (synchronous && (pendingOperations != 0 || storage is not ISynchronousSaveDataStorage))
                {
                    throw new InvalidOperationException("同步入口要求空闲的同步后端");
                }

                Interlocked.Increment(ref pendingOperations);
                queued = true;

                await operationGate.WaitAsync(cancellationToken);

                entered = true;
                CheckThread();

                if (State == SaveDataState.Faulted)
                {
                    throw new InvalidOperationException("上一个请求回退失败");
                }

                if (operation == "Save")
                {
                    State = SaveDataState.Saving;
                }
                else
                {
                    State = SaveDataState.Loading;
                }

                if (backup && !storage.SupportsBackup)
                {
                    throw new NotSupportedException("后端不支持备份");
                }

                if (needsParticipants)
                {
                    ISaveDataParticipant[] ordered = OrderParticipants();
                    if (synchronous)
                    {
                        foreach (ISaveDataParticipant participant in ordered)
                        {
                            Call(() =>
                            {
                                participant.EnsureReady();
                                return true;
                            });
                        }
                    }
                    else
                    {
                        await WaitForParticipantsAsync(ordered, cancellationToken);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                T value = await body(id, synchronous, cancellationToken);

                if (operation == "LoadOrDefault" && value is bool loaded)
                {
                    result.UsedDefaults = !loaded;
                }

                return value;
            }
            catch (Exception exception)
            {
                result.Error = exception;

                if (exception is SaveDataRestoreException restore)
                {
                    if (!restore.RuntimeStateConsistent)
                    {
                        State = SaveDataState.Faulted;
                    }
                }

                result.RuntimeStateConsistent = State != SaveDataState.Faulted
                    && (exception is not SaveDataRestoreException failure || failure.RuntimeStateConsistent);

                throw;
            }
            finally
            {
                if (entered)
                {
                    if (State != SaveDataState.Faulted)
                    {
                        State = SaveDataState.Ready;
                    }

                    LastResult = result;
                    Notify(Completed, result);
                    operationGate.Release();
                }
                if (queued)
                {
                    Interlocked.Decrement(ref pendingOperations);
                }
            }
        }

        /// <summary>
        /// 全部模块共用一个时间期限，结束后取消并等待计时任务清理
        /// </summary>
        private static async Task WaitForParticipantsAsync(ISaveDataParticipant[] ordered, CancellationToken cancellationToken)
        {
            using var readiness = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var stopTimer = new CancellationTokenSource();
            var deadline = new ReadinessDeadline();
            Task timer;

            if (options.ReadinessTimeout > TimeSpan.Zero)
            {
                timer = CancelAtDeadlineAsync(readiness, stopTimer.Token, deadline);
            }
            else
            {
                timer = Task.CompletedTask;
            }

            try
            {
                foreach (ISaveDataParticipant participant in ordered)
                {
                    readiness.Token.ThrowIfCancellationRequested();
                    await CallAsync(() => participant.WaitUntilReadyAsync(readiness.Token));
                    readiness.Token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested && deadline.Expired)
            {
                throw new TimeoutException("等待存档模块就绪超过配置期限，模块须响应取消", exception);
            }
            finally
            {
                stopTimer.Cancel();
                await timer;
            }
        }

        /// <summary>
        /// 期限到达时主动取消模块等待，计时失败也取消等待并传播错误
        /// </summary>
        private static async Task CancelAtDeadlineAsync(CancellationTokenSource readiness, CancellationToken stopTimer, ReadinessDeadline deadline)
        {
            try
            {
                await options.TimeService.DelayAsync(options.ReadinessTimeout, stopTimer);
                if (stopTimer.IsCancellationRequested)
                {
                    return;
                }
                deadline.Expired = true;
            }
            catch (OperationCanceledException) when (stopTimer.IsCancellationRequested)
            {
                return;
            }
            catch
            {
                readiness.Cancel();
                throw;
            }
            readiness.Cancel();
        }

        /// <summary>
        /// 同步后端执行期间不产生异步等待，禁止主线程阻塞
        /// </summary>
        private static T RunSync<T>(string operation, string slotId, bool needsParticipants, bool backup, Func<string, bool, CancellationToken, Task<T>> body)
        {
            Task<T> task = ExecuteAsync(operation, slotId, true, needsParticipants, backup, CancellationToken.None, body);
            if (!task.IsCompleted)
            {
                throw new InvalidOperationException("同步后端产生未完成异步任务");
            }

            return task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// 在目标栏位快照上采集模块，验证旧档和新档后执行备份提交
        /// </summary>
        private static async Task<bool> SaveCore(string id, bool synchronous, CancellationToken token)
        {
            if (!storage.CanWrite)
            {
                throw new NotSupportedException("后端只读");
            }

            bool exists = await ExistsCore(id, false, synchronous, token);
            if (!exists && (await NamesCore(synchronous, token)).Length >= options.MaxSlots)
            {
                throw new InvalidOperationException("栏位数量超过上限");
            }

            SaveDataDocument document;
            if (exists)
            {
                document = await ReadCore(id, false, synchronous, token);
            }
            else
            {
                document = options.Schema.CreateDefault();
            }

            foreach (ISaveDataParticipant participant in OrderParticipants())
            {
                JToken snapshot = Call(participant.Capture);
                document.Modules.TryGetValue(participant.ParticipantId, out JToken previous);
                SaveDataJson.ValidateDepth(snapshot);

                JToken mergedSnapshot;

                if (participant is ISaveDataSnapshotMergeParticipant merger)
                {
                    JToken previousClone = previous?.DeepClone();
                    JToken snapshotClone = snapshot.DeepClone();

                    mergedSnapshot = Call(() => merger.MergeSnapshot(previousClone, snapshotClone)).DeepClone();
                }
                else
                {
                    mergedSnapshot = SaveDataJson.MergeSnapshot(previous, snapshot);
                }

                document.Modules[participant.ParticipantId] = mergedSnapshot;

                if (participant is ISaveDataMetadataParticipant metadata)
                {
                    document.Metadata[participant.ParticipantId] = Call(metadata.CaptureMetadata).DeepClone();
                }
            }

            document.Version = options.Schema.CurrentVersion;
            document.SavedAtUtc = DateTime.UtcNow;
            ValidateDocument(document);

            await IO(async () =>
            {
                if (synchronous)
                {
                    ((ISynchronousSaveDataStorage)storage).Write(id, () => Encode(document));
                }
                else
                {
                    await storage.WriteAsync(id, () => Encode(document), token);
                }

                return true;
            }, synchronous, token);

            CurrentSaveName = id;

            return true;
        }

        /// <summary>
        /// 读取字节并执行项目解码和迁移，拒绝超限文档
        /// </summary>
        private static async Task<SaveDataDocument> ReadCore(string id, bool backup, bool synchronous, CancellationToken token)
        {
            byte[] bytes;
            if (synchronous)
            {
                bytes = ((ISynchronousSaveDataStorage)storage).Read(id, backup);
            }
            else
            {
                bytes = await IO(() => storage.ReadAsync(id, backup, token), false, token);
            }
            return await DecodeAsync(bytes, synchronous, token);
        }

        /// <summary>
        /// 异步入口在允许后台执行时调度解码，同步入口和 Web 平台直接完成
        /// </summary>
        private static Task<SaveDataDocument> DecodeAsync(byte[] bytes, bool synchronous, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!synchronous && options.UseBackgroundIO)
            {
                return Task.Run(() => Decode(bytes), token);
            }
            else
            {
                return Task.FromResult(Decode(bytes));
            }
        }

        /// <summary>
        /// 统一大小、逻辑结构、项目版本检查
        /// </summary>
        private static SaveDataDocument Decode(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0 || bytes.Length > options.MaxBytes)
            {
                throw new InvalidDataException("存档字节大小无效");
            }

            SaveDataDocument document = migrations.Upgrade(options.Schema.Decode(serializer.Deserialize(bytes)));
            ValidateDocument(document);

            return document;
        }

        /// <summary>
        /// 在备份完成后的编码回调检查字节上限
        /// </summary>
        private static byte[] Encode(SaveDataDocument document)
        {
            byte[] bytes = serializer.Serialize(document);
            if (bytes == null || bytes.Length == 0 || bytes.Length > options.MaxBytes)
            {
                throw new InvalidDataException("编码结果超过字节上限或为空");
            }

            return bytes;
        }

        /// <summary>
        /// 创建完整恢复计划并决定是否切换当前栏位
        /// </summary>
        private static async Task<bool> LoadCore(string id, bool backup, bool allowDefaults, bool synchronous, CancellationToken token)
        {
            bool loaded = !allowDefaults || await ExistsCore(id, false, synchronous, token);

            SaveDataDocument document;
            if (loaded)
            {
                document = await ReadCore(id, backup, synchronous, token);
            }
            else
            {
                document = options.Schema.CreateDefault();
            }

            ISaveDataParticipant[] ordered = OrderParticipants();
            JToken[] targets = new JToken[ordered.Length];

            for (int index = 0; index < ordered.Length; index++)
            {
                ISaveDataParticipant participant = ordered[index];

                if (document.Modules.TryGetValue(participant.ParticipantId, out JToken value))
                {
                    targets[index] = value.DeepClone();
                }
                else
                {
                    targets[index] = Call(participant.CreateDefault).DeepClone();
                }

                document.Modules[participant.ParticipantId] = targets[index].DeepClone();

                Call(() =>
                {
                    participant.ValidateRestore(targets[index].DeepClone());
                    return true;
                });
            }

            ValidateDocument(document);
            token.ThrowIfCancellationRequested();
            ApplyRestore(ordered, targets);
            CurrentSaveName = id;

            foreach (ISaveDataParticipant participant in ordered)
            {
                if (participant is ISaveDataMetadataParticipant metadata)
                {
                    NotifyRestored(metadata, !loaded);
                }
            }

            return loaded;
        }

        /// <summary>
        /// 采集所有回退快照后提交
        /// </summary>
        private static void ApplyRestore(ISaveDataParticipant[] ordered, JToken[] targets)
        {
            JToken[] previous = ordered.Select(participant => Call(participant.Capture).DeepClone()).ToArray();
            int committing = 0;

            try
            {
                for (; committing < ordered.Length; committing++)
                {
                    int index = committing;
                    Call(() =>
                    {
                        ordered[index].Restore(targets[index].DeepClone());
                        return true;
                    });
                }
            }
            catch (Exception exception)
            {
                var failures = new List<Exception>
                {
                    exception
                };

                for (int index = committing; index >= 0; index--)
                {
                    int rollback = index;
                    try
                    {
                        Call(() =>
                        {
                            ordered[rollback].Restore(previous[rollback].DeepClone());
                            return true;
                        });
                    }
                    catch (Exception failure)
                    {
                        failures.Add(failure);
                    }
                }

                throw new SaveDataRestoreException(failures.Count == 1, new AggregateException(failures));
            }
        }

        /// <summary>
        /// 验证备份后由后端修复栏位
        /// </summary>
        private static async Task<bool> RepairCore(string id, bool synchronous, CancellationToken token)
        {
            if (!storage.CanWrite)
            {
                throw new NotSupportedException("后端只读");
            }

            byte[] bytes;
            if (synchronous)
            {
                bytes = ((ISynchronousSaveDataStorage)storage).Read(id, true);
            }
            else
            {
                bytes = await IO(() => storage.ReadAsync(id, true, token), false, token);
            }

            await DecodeAsync(bytes, synchronous, token);

            await IO(async () =>
            {
                if (synchronous)
                {
                    ((ISynchronousSaveDataStorage)storage).Repair(id, bytes);
                }
                else
                {
                    await storage.RepairAsync(id, bytes, token);
                }
                return true;
            }, synchronous, token);

            return true;
        }

        /// <summary>
        /// 删除存档、清除当前栏位选择
        /// </summary>
        private static async Task<bool> DeleteCore(string id, bool includeBackup, bool synchronous, CancellationToken token)
        {
            if (!storage.CanWrite)
            {
                throw new NotSupportedException("后端只读");
            }

            await IO(async () =>
            {
                if (synchronous)
                {
                    ((ISynchronousSaveDataStorage)storage).Delete(id, includeBackup);
                }
                else
                {
                    await storage.DeleteAsync(id, includeBackup, token);
                }
                return true;
            }, synchronous, token);

            if (CurrentSaveName == id)
            {
                CurrentSaveName = null;
            }

            return true;
        }

        /// <summary>
        /// 后端存在检查
        /// </summary>
        private static Task<bool> ExistsCore(string id, bool backup, bool synchronous, CancellationToken token)
        {
            if (synchronous)
            {
                return Task.FromResult(((ISynchronousSaveDataStorage)storage).Exists(id, backup));
            }
            else
            {
                return IO(() => storage.ExistsAsync(id, backup, token), false, token);
            }
        }

        /// <summary>
        /// 后端栏位枚举
        /// </summary>
        private static Task<string[]> NamesCore(bool synchronous, CancellationToken token)
        {
            if (synchronous)
            {
                return Task.FromResult(((ISynchronousSaveDataStorage)storage).GetSaveNames());
            }
            else
            {
                return IO(() => storage.GetSaveNamesAsync(token), false, token);
            }
        }

        /// <summary>
        /// 只有同步文件后端在异步入口后台执行，网络后端保持调用上下文
        /// </summary>
        private static Task<T> IO<T>(Func<Task<T>> action, bool synchronous, CancellationToken token)
        {
            if (!synchronous && options.UseBackgroundIO && storage is ISynchronousSaveDataStorage)
            {
                return Task.Run(action, token);
            }
            else
            {
                return action();
            }
        }

        /// <summary>
        /// 校验项目数据、限制节点数量、限制非有限浮点数
        /// </summary>
        private static void ValidateDocument(SaveDataDocument document)
        {
            if (document == null || document.SchemaId != options.Schema.SchemaId || document.Version != options.Schema.CurrentVersion
                || document.Modules == null || document.Metadata == null || document.Extensions == null)
            {
                throw new InvalidDataException("通用存档文档结构无效");
            }

            foreach (KeyValuePair<string, JToken> module in document.Modules)
            {
                if (string.IsNullOrWhiteSpace(module.Key) || module.Value == null || module.Value.Type == JTokenType.Null)
                {
                    throw new InvalidDataException("模块 ID 或快照无效");
                }
            }

            document.ValidateRootExtensions();

            options.Schema.Validate(document);
            JToken tree = SaveDataJson.ToToken(document);
            int nodes = 0;

            foreach (JToken node in ((JContainer)tree).Descendants())
            {
                if (++nodes > options.MaxNodes)
                {
                    throw new InvalidDataException("存档节点超过上限");
                }

                if (node.Type == JTokenType.Float)
                {
                    double value = node.Value<double>();
                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        throw new InvalidDataException("存档包含非有限数值");
                    }
                }
            }
        }

        /// <summary>
        /// 依赖拓扑排序，拒绝缺少模块、依赖缺失、依赖环
        /// </summary>
        private static ISaveDataParticipant[] OrderParticipants()
        {
            if (participants.Count == 0)
            {
                throw new InvalidOperationException("没有注册存档模块");
            }

            var lookup = participants.ToDictionary(participant => participant.ParticipantId, StringComparer.Ordinal);

            foreach (string id in options.Schema.RequiredParticipants)
            {
                if (!lookup.ContainsKey(id))
                {
                    throw new InvalidOperationException($"必需模块 {id} 未注册");
                }
            }

            var visited = new HashSet<string>(StringComparer.Ordinal);
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var ordered = new List<ISaveDataParticipant>();

            foreach (ISaveDataParticipant participant in participants.OrderBy(value => value.RestoreOrder).ThenBy(value => value.ParticipantId, StringComparer.Ordinal))
            {
                Visit(participant, lookup, visiting, visited, ordered);
            }

            return ordered.ToArray();
        }

        /// <summary>
        /// 深度优先建立恢复顺序
        /// </summary>
        private static void Visit(ISaveDataParticipant participant, Dictionary<string, ISaveDataParticipant> lookup,
            HashSet<string> visiting, HashSet<string> visited, List<ISaveDataParticipant> ordered)
        {
            if (visited.Contains(participant.ParticipantId))
            {
                return;
            }

            if (!visiting.Add(participant.ParticipantId))
            {
                throw new InvalidOperationException("模块依赖存在环");
            }

            foreach (string id in participant.Dependencies ?? Array.Empty<string>())
            {
                if (id == null || !lookup.TryGetValue(id, out ISaveDataParticipant dependency))
                {
                    throw new InvalidOperationException("模块依赖尚未注册");
                }

                Visit(dependency, lookup, visiting, visited, ordered);
            }

            visiting.Remove(participant.ParticipantId);
            visited.Add(participant.ParticipantId);
            ordered.Add(participant);
        }

        /// <summary>
        /// 隔离模块回调并阻止重入
        /// </summary>
        private static T Call<T>(Func<T> callback)
        {
            CheckThread();
            bool previous = insideCallback.Value;
            insideCallback.Value = true;

            try
            {
                return callback();
            }
            finally
            {
                insideCallback.Value = previous;
            }
        }

        /// <summary>
        /// 在异步回调完成之前保持重入保护
        /// </summary>
        private static async Task CallAsync(Func<Task> callback)
        {
            CheckThread();
            bool previous = insideCallback.Value;
            insideCallback.Value = true;

            try
            {
                await callback();
            }
            finally
            {
                insideCallback.Value = previous;
            }
        }

        /// <summary>
        /// 成功恢复的通知
        /// </summary>
        private static void NotifyRestored(ISaveDataMetadataParticipant participant, bool defaults)
        {
            try
            {
                Call(() =>
                {
                    participant.OnRestored(defaults);
                    return true;
                });
            }
            catch (Exception exception)
            {
                Notify(NotificationFailed, exception, false);
            }
        }

        /// <summary>
        /// 逐一发送通知，隔离每个订阅者异常
        /// </summary>
        private static void Notify<T>(Action<T> handlers, T value, bool reportFailure = true)
        {
            if (handlers == null)
            {
                return;
            }

            foreach (Action<T> handler in handlers.GetInvocationList().Cast<Action<T>>())
            {
                try
                {
                    Call(() =>
                    {
                        handler(value);
                        return true;
                    });
                }
                catch (Exception exception)
                {
                    if (reportFailure)
                    {
                        Notify(NotificationFailed, exception, false);
                    }
                }
            }
        }

        /// <summary>
        /// 采用默认本地配置，检查当前调用线程
        /// </summary>
        private static void EnsureConfigured()
        {
            if (State == SaveDataState.Uninitialized)
            {
                Initialize();
            }

            CheckThread();
        }

        /// <summary>
        /// 注册和配置变更只允许发生在空闲会话
        /// </summary>
        private static void EnsureIdle()
        {
            EnsureConfigured();
            if (pendingOperations != 0 || insideCallback.Value || State != SaveDataState.Ready)
            {
                throw new InvalidOperationException("操作未完成，不能更改会话");
            }
        }

        /// <summary>
        /// 防止 Unity 运行时模块从后台线程访问
        /// </summary>
        private static void CheckThread()
        {
            if (options != null && options.EnforceThreadAffinity && Thread.CurrentThread.ManagedThreadId != ownerThread)
            {
                throw new InvalidOperationException("请从初始化线程和其同步上下文调用 Manager");
            }
        }

    }
}
