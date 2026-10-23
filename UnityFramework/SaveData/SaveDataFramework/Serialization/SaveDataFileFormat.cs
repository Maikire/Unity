namespace SaveData
{
    /// <summary>
    /// 选择加密前的完整文件格式，启用加密后直接输出整文件密文
    /// </summary>
    public enum SaveDataFileFormat
    {
        /// <summary>
        /// 编辑器和开发构建使用 JSON，发布构建使用字节文件
        /// </summary>
        Automatic,
        /// <summary>
        /// 使用 JSON 文件内容
        /// </summary>
        Json,
        /// <summary>
        /// 使用带格式头的 bytes 文件
        /// </summary>
        Bytes
    }
}
