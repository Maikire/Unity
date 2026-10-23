using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveData.Example
{
    /// <summary>
    /// SaveData 场景入口，自动调用示例并提供手动操作面板
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SaveDataExampleRunner : MonoBehaviour
    {
        [Tooltip("启动时依次演示默认值、读写、预览、栏位列表、备份、错误处理、删除")]
        [SerializeField]
        private bool runOnStart = true;

        [Tooltip("显示手动操作面板，运行结果同时输出到 Console")]
        [SerializeField]
        private bool showGUI = true;

        [Tooltip("手动操作的栏位 ID，示例存档保存在 SaveDataExample 独立目录")]
        [SerializeField]
        private string slotId = "example-player";

        private readonly SaveDataExamples examples = new SaveDataExamples();
        private readonly List<string> messages = new List<string>();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private Task activeOperation;
        private SaveSlotInfo[] slots = Array.Empty<SaveSlotInfo>();
        private bool initialized;
        private bool busy;
        private bool destroyed;
        private Vector2 scrollPosition;

        /// <summary>
        /// 初始化后订阅操作结果，再按顺序调用示例
        /// </summary>
        private void Start()
        {
            try
            {
                examples.Initialize();
                initialized = true;
                SaveDataManager.Completed += OnCompleted;
                Report("示例存档目录：" + SaveDataPathHelper.GetPersistentDirectoryPath(SaveDataExamples.DirectoryName));
                Run(async token =>
                {
                    bool loaded = await SaveDataManager.LoadOrDefaultAsync(slotId, token);
                    if (loaded)
                    {
                        Report("已恢复手动栏位的进度");
                    }
                    else
                    {
                        Report("手动栏位使用新游戏默认值，保存后才会创建文件");
                    }
                    if (runOnStart)
                    {
                        await examples.RunAsync(Report, token);
                    }
                    await RefreshSlotsAsync(token);
                });
            }
            catch (Exception exception)
            {
                Report("初始化失败：" + exception.Message);
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        /// 在主线程发起操作，执行期间禁用修改进度和重复点击
        /// </summary>
        private void Run(Func<CancellationToken, Task> operation)
        {
            if (!initialized || busy || destroyed)
                return;

            activeOperation = ExecuteAsync(operation);
        }

        /// <summary>
        /// 统一观察异步异常，退出场景时响应取消
        /// </summary>
        private async Task ExecuteAsync(Func<CancellationToken, Task> operation)
        {
            busy = true;
            try
            {
                await operation(lifetime.Token);
            }
            catch (OperationCanceledException)
            {
                Report("示例操作已取消");
            }
            catch (Exception exception)
            {
                Report("操作失败：" + exception.Message);
                Debug.LogException(exception, this);
            }
            finally
            {
                busy = false;
            }
        }

        /// <summary>
        /// 只读扫描所有栏位，坏档可以通过 IsValid 和 Error 单独展示
        /// </summary>
        private async Task RefreshSlotsAsync(CancellationToken token)
        {
            slots = await SaveDataManager.GetSaveSlotsAsync(token);
        }

        /// <summary>
        /// 记录通知结果
        /// </summary>
        private void OnCompleted(SaveDataResult result)
        {
            if (result.Success)
            {
                Report($"{result.Operation} [{result.SlotId}]：成功");
            }
            else
            {
                Report($"{result.Operation} [{result.SlotId}]：{result.ErrorCode}");
            }
        }

        /// <summary>
        /// 保留最近的消息，并同步输出到 Console
        /// </summary>
        private void Report(string message)
        {
            if (destroyed)
                return;

            messages.Add(message);
            if (messages.Count > 40)
            {
                messages.RemoveAt(0);
            }
            Debug.Log("[SaveData Example] " + message, this);
        }

        /// <summary>
        /// 直接调用框架接口，方便对照每个按钮的使用方式
        /// </summary>
        private void OnGUI()
        {
            if (!showGUI)
                return;

            GUILayout.BeginArea(new Rect(20, 20, Mathf.Min(700, Screen.width - 40), Screen.height - 40), GUI.skin.box);
            GUILayout.Label("SaveData 使用示例");
            GUILayout.Label($"金币：{examples.Progress.Coins}    检查点：{examples.Progress.CheckpointId}");
            if (busy)
            {
                GUILayout.Label("正在执行存档操作…");
            }
            else
            {
                GUILayout.Label("先修改进度，再保存或读档观察变化");
            }
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && initialized && !busy;
            slotId = GUILayout.TextField(slotId);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("金币 +10"))
            {
                examples.Progress.Coins = examples.Progress.Coins + 10;
            }
            if (GUILayout.Button("切换检查点"))
            {
                if (examples.Progress.CheckpointId == "village")
                {
                    examples.Progress.CheckpointId = "castle";
                }
                else
                {
                    examples.Progress.CheckpointId = "village";
                }
            }
            if (GUILayout.Button("运行完整演示"))
            {
                Run(async token =>
                {
                    await examples.RunAsync(Report, token);
                    await RefreshSlotsAsync(token);
                });
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存"))
            {
                Run(async token =>
                {
                    await SaveDataManager.SaveAsync(slotId, token);
                    await RefreshSlotsAsync(token);
                });
            }
            if (GUILayout.Button("读档或默认值"))
            {
                Run(async token =>
                {
                    bool loaded = await SaveDataManager.LoadOrDefaultAsync(slotId, token);
                    if (loaded)
                    {
                        Report("已恢复存档");
                    }
                    else
                    {
                        Report("已恢复完整默认值");
                    }
                });
            }
            if (GUILayout.Button("只读预览"))
            {
                Run(async token =>
                {
                    SaveDataDocument document = await SaveDataManager.ReadAsync(slotId, cancellationToken: token);
                    Report("预览快照：" + document.Modules[SaveDataExampleParticipant.Id]);
                });
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载备份"))
            {
                Run(token => SaveDataManager.LoadAsync(slotId, backup: true, cancellationToken: token));
            }
            if (GUILayout.Button("修复备份并读档"))
            {
                Run(async token =>
                {
                    await SaveDataManager.RestoreBackupAsync(slotId, token);
                    await SaveDataManager.LoadAsync(slotId, cancellationToken: token);
                    await RefreshSlotsAsync(token);
                });
            }
            if (GUILayout.Button("删除栏位及备份"))
            {
                Run(async token =>
                {
                    await SaveDataManager.DeleteAsync(slotId, includeBackup: true, cancellationToken: token);
                    await RefreshSlotsAsync(token);
                    Report("文件已删除，运行时进度保留；点击读档或默认值可开始新游戏");
                });
            }
            if (GUILayout.Button("刷新栏位"))
            {
                Run(RefreshSlotsAsync);
            }
            GUILayout.EndHorizontal();
            GUI.enabled = previousEnabled;

            foreach (SaveSlotInfo slot in slots)
            {
                GUILayout.Label($"{slot.SlotId} | 有效：{slot.IsValid} | 备份：{slot.HasBackup}");
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label(string.Join("\n", messages));
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 先取消并等待当前请求结束，再结束自己拥有的会话
        /// </summary>
        private async void OnDestroy()
        {
            destroyed = true;
            lifetime.Cancel();
            try
            {
                if (activeOperation != null)
                {
                    await activeOperation;
                }
                if (initialized)
                {
                    SaveDataManager.Completed -= OnCompleted;
                    examples.Close();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                lifetime.Dispose();
            }
        }

    }
}
