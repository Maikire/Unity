namespace SaveData
{
    /// <summary>
    /// 设置资产使用的密钥来源
    /// </summary>
    public enum SaveDataKeyProviderMode
    {
        /// <summary>
        /// 使用实现 ISaveDataKeyProvider 的项目资产
        /// </summary>
        Custom,

        /// <summary>
        /// 使用设置资产中的本地密钥配置
        /// </summary>
        Local
    }
}
