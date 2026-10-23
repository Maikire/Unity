using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 将当前会话文件提交到声明的持久存储，成功完成任务才表示后端已确认提交
    /// </summary>
    public interface ISaveDataFileSynchronizer
    {
        /// <summary>
        /// 等待持久化确认，文件修改后不可用调用方取消来伪装提交已经中止
        /// </summary>
        Task SynchronizeAsync();

    }
}
