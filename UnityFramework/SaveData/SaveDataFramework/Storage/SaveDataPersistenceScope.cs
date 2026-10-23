namespace SaveData
{
    /// <summary>
    /// 后端成功返回后的存储范围
    /// </summary>
    public enum SaveDataPersistenceScope
    {
        /// <summary>
        /// 由项目后端说明
        /// </summary>
        BackendDefined,
        /// <summary>
        /// 只确认当前会话可读取
        /// </summary>
        CurrentSession,
        /// <summary>
        /// 已提交到持久存储，可在相同存储身份下跨页面数据刷新
        /// </summary>
        PersistentStorage
    }
}
