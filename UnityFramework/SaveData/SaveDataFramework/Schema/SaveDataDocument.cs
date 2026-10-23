using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace SaveData
{
    /// <summary>
    /// 文档标记
    /// </summary>
    [Serializable]
    public sealed class SaveDataDocument
    {
        /// <summary>
        /// JSON 文件包装使用的根级保留标记
        /// </summary>
        internal const string EnvelopeMarker = "$saveDataEnvelope";

        /// <summary>
        /// 逻辑成员与协议标记统一按不区分大小写的规则保留
        /// </summary>
        private static readonly HashSet<string> reservedRootFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "schemaId", "version", "savedAtUtc", "modules", "metadata", EnvelopeMarker
        };

        /// <summary>
        /// 项目数据契约的稳定字符串标识
        /// </summary>
        [JsonProperty("schemaId", Required = Required.Always)]
        public string SchemaId;

        /// <summary>
        /// 项目结构版本，与后端修订号及配置版本独立
        /// </summary>
        [JsonProperty("version", Required = Required.Always)]
        public int Version;

        /// <summary>
        /// 最近保存的 UTC 时间
        /// </summary>
        [JsonProperty("savedAtUtc")]
        public DateTime SavedAtUtc;

        /// <summary>
        /// 字符串模块 ID 到独立快照的映射，保留未注册模块
        /// </summary>
        [JsonProperty("modules", Required = Required.Always)]
        public Dictionary<string, JToken> Modules = new Dictionary<string, JToken>(StringComparer.Ordinal);

        /// <summary>
        /// 模块和项目的元数据
        /// </summary>
        [JsonProperty("metadata")]
        public JObject Metadata = new JObject();

        /// <summary>
        /// 当前程序不认识的根字段，重新保存时继续保留，禁止使用保留字段及其大小写变体
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Extensions = new Dictionary<string, JToken>(StringComparer.Ordinal);

        /// <summary>
        /// 对于当前程序不认识的根字段，在编码和存储提交前拒绝覆盖逻辑成员或文件协议的根级扩展字段
        /// </summary>
        internal void ValidateRootExtensions()
        {
            if (Extensions == null)
                throw new InvalidDataException("根级扩展字段不能为空");

            foreach (string key in Extensions.Keys)
            {
                if (reservedRootFields.Contains(key))
                    throw new InvalidDataException("扩展字段不能覆盖文档或文件协议的根级保留字段");
            }
        }

    }
}
