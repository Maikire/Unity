using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace SaveData
{
    /// <summary>
    /// Newtonsoft Json UTF8 编解码
    /// </summary>
    public sealed class JsonSaveDataSerializer : ISaveDataSerializer
    {
        /// <summary>
        /// 严格 UTF8 编码，拒绝无效字节
        /// </summary>
        private static readonly UTF8Encoding utf8 = new UTF8Encoding(false, true);

        /// <summary>
        /// 此实例使用的 JSON 缩进规则
        /// </summary>
        private readonly Formatting formatting;

        /// <summary>
        /// 创建 JSON 编码器，调试时默认保留缩进
        /// </summary>
        public JsonSaveDataSerializer(bool prettyPrint = true)
        {
            if (prettyPrint)
            {
                formatting = Formatting.Indented;
            }
            else
            {
                formatting = Formatting.None;
            }
        }

        /// <summary>
        /// JSON 正式档也统一使用 .dat 扩展名
        /// </summary>
        public string FileExtension => FileSaveDataStorage.PrimaryFileExtension;

        /// <summary>
        /// 编码逻辑文档
        /// </summary>
        public byte[] Serialize(SaveDataDocument document) => SerializeObject(document);

        /// <summary>
        /// 编码项目 DTO
        /// </summary>
        public byte[] SerializeObject(object value)
        {
            JToken token = SaveDataJson.ToToken(value);
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            using var json = new JsonTextWriter(writer)
            {
                Formatting = formatting
            };
            token.WriteTo(json);
            return utf8.GetBytes(writer.ToString());
        }

        /// <summary>
        /// 读取单个对象，拒绝重复键和尾随内容，允许 UTF8 BOM
        /// </summary>
        public JObject Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new InvalidDataException("存档内容为空");
            }

            try
            {
                using var text = new StringReader(utf8.GetString(bytes).TrimStart('\uFEFF'));
                using var reader = new JsonTextReader(text)
                {
                    DateParseHandling = DateParseHandling.None,
                    MaxDepth = SaveDataJson.MaximumDepth
                };
                var root = JObject.Load(reader, new JsonLoadSettings
                {
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
                });
                if (reader.Read())
                {
                    throw new InvalidDataException("存档包含尾随内容");
                }

                return root;
            }
            catch (Exception exception) when (exception is JsonException || exception is DecoderFallbackException)
            {
                throw new InvalidDataException("存档 JSON 或 UTF8 无效", exception);
            }
        }

    }
}
