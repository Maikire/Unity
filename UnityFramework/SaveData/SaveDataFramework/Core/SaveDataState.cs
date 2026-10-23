namespace SaveData
{
    /// <summary>
    /// 管理器当前生命周期状态
    /// </summary>
    public enum SaveDataState
    {
        /// <summary>
        /// 尚未初始化
        /// </summary>
        Uninitialized,
        /// <summary>
        /// 可以接受操作
        /// </summary>
        Ready,
        /// <summary>
        /// 正在保存
        /// </summary>
        Saving,
        /// <summary>
        /// 正在读取或恢复
        /// </summary>
        Loading,
        /// <summary>
        /// 回退失败，需要项目重建运行时后重置会话
        /// </summary>
        Faulted
    }
}
