using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace SaveData
{
    /// <summary>
    /// 通用模块文档 schema，项目可继承以识别旧结构并增加业务校验
    /// </summary>
    public class SaveDataSchema : ISaveDataSchema
    {
        /// <summary>
        /// 项目数据契约 ID
        /// </summary>
        public string SchemaId { get; }

        /// <summary>
        /// 当前项目版本
        /// </summary>
        public int CurrentVersion { get; }

        /// <summary>
        /// 必须注册的模块 ID
        /// </summary>
        public IReadOnlyCollection<string> RequiredParticipants { get; }

        /// <summary>
        /// 定义项目 schema、版本及必需模块
        /// </summary>
        public SaveDataSchema(string schemaId, int currentVersion = 1, params string[] requiredParticipants)
        {
            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentException("Schema ID 不能为空", nameof(schemaId));
            }

            if (currentVersion < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentVersion));
            }

            SchemaId = schemaId;
            CurrentVersion = currentVersion;
            RequiredParticipants = Array.AsReadOnly((string[])(requiredParticipants ?? Array.Empty<string>()).Clone());
            foreach (string id in RequiredParticipants)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("必需模块 ID 不能为空", nameof(requiredParticipants));
                }
            }
        }

        /// <summary>
        /// 创建当前项目的空模块文档
        /// </summary>
        public virtual SaveDataDocument CreateDefault() => new SaveDataDocument
        {
            SchemaId = SchemaId,
            Version = CurrentVersion
        };

        /// <summary>
        /// 解码前严格检查整数版本和字符串 schema ID，拒绝隐式类型转换
        /// </summary>
        public virtual SaveDataDocument Decode(JObject root)
        {
            if (root == null || root["version"]?.Type != JTokenType.Integer || root["schemaId"]?.Type != JTokenType.String)
            {
                throw new InvalidDataException("存档版本或 schema ID 类型无效");
            }

            return SaveDataJson.FromToken<SaveDataDocument>(root);
        }

        /// <summary>
        /// 校验 schema、当前版本、模块 ID、快照
        /// </summary>
        public virtual void Validate(SaveDataDocument document)
        {
            if (document == null || document.SchemaId != SchemaId)
            {
                throw new InvalidDataException("存档 schema 不匹配");
            }

            if (document.Version != CurrentVersion)
            {
                throw new InvalidDataException("存档版本未完成迁移");
            }

            if (document.Modules == null || document.Metadata == null || document.Extensions == null)
            {
                throw new InvalidDataException("存档模块或元信息为空");
            }

            foreach (KeyValuePair<string, JToken> module in document.Modules)
            {
                if (string.IsNullOrWhiteSpace(module.Key) || module.Value == null || module.Value.Type == JTokenType.Null)
                {
                    throw new InvalidDataException("模块 ID 或快照无效");
                }
            }
        }

    }
}
