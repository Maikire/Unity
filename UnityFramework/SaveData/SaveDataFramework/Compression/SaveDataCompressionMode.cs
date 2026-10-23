namespace SaveData
{
    /// <summary>
    /// 内置压缩选项，自定义实现通过会话选项接入
    /// </summary>
    public enum SaveDataCompressionMode
    {
        /// <summary>
        /// 默认保留原始编码字节
        /// </summary>
        None,
        /// <summary>
        /// 使用 MessagePack 插件编码 LZ4 压缩块
        /// </summary>
        Lz4,
        /// <summary>
        /// 使用项目提供的压缩实现
        /// </summary>
        Custom
    }
}
