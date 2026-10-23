using Newtonsoft.Json.Linq;

namespace SaveData
{
    /// <summary>
    /// 将逻辑文档与字节编码分离，其他编码返回相同逻辑字段树
    /// </summary>
    public interface ISaveDataSerializer
    {
        /// <summary>
        /// 文件编码器返回统一的 .dat 扩展名
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// 编码已校验文档
        /// </summary>
        byte[] Serialize(SaveDataDocument document);

        /// <summary>
        /// 解码逻辑字段，结构识别和迁移交给项目 schema
        /// </summary>
        JObject Deserialize(byte[] bytes);

    }
}
