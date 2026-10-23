namespace SaveData
{
    /// <summary>
    /// 可扩展压缩契约，实现须支持后台调用、返回独立数组
    /// </summary>
    public interface ISaveDataCompression
    {
        /// <summary>
        /// 压缩逻辑编码字节，分配和输出均应遵守字节上限
        /// </summary>
        byte[] Compress(byte[] input, int maxBytes);

        /// <summary>
        /// 在分配前校验声明长度和上限，仅接受精确匹配的完整解压输出
        /// </summary>
        byte[] Decompress(byte[] input, int uncompressedLength, int maxBytes);

    }
}
