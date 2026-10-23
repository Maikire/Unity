namespace SaveData
{
    /// <summary>
    /// 可选的后端持久性声明。后端是否能够长期保存数据，需要明确声明；如果没有声明，或者只是项目自己实现的后端，则不会保证跨会话持久化
    /// </summary>
    public interface ISaveDataStoragePersistence
    {
        /// <summary>
        /// 成功完成存储操作后，后端声明能够覆盖的读取范围
        /// </summary>
        SaveDataPersistenceScope PersistenceScope { get; }

    }
}
