using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SaveData
{
    /// <summary>
    /// 项目负责默认文档、结构识别、校验和版本演进
    /// </summary>
    public interface ISaveDataSchema
    {
        /// <summary>
        /// 项目 schema 的稳定字符串 ID
        /// </summary>
        string SchemaId { get; }

        /// <summary>
        /// 项目当前存档版本
        /// </summary>
        int CurrentVersion { get; }

        /// <summary>
        /// 必须注册的字符串模块 ID
        /// </summary>
        IReadOnlyCollection<string> RequiredParticipants { get; }

        /// <summary>
        /// 创建默认文档，模块默认值由参与模块补充
        /// </summary>
        SaveDataDocument CreateDefault();

        /// <summary>
        /// 识别逻辑字段并转换旧结构
        /// </summary>
        SaveDataDocument Decode(JObject root);

        /// <summary>
        /// 校验文档
        /// </summary>
        void Validate(SaveDataDocument document);

    }
}
