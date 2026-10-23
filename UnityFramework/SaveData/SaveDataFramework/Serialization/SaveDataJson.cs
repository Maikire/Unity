using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SaveData
{
    /// <summary>
    /// 统一 Newtonsoft DTO 策略
    /// </summary>
    public static class SaveDataJson
    {
        /// <summary>
        /// 文档与 DTO 共同使用的最大容器嵌套深度，包含根对象
        /// </summary>
        public const int MaximumDepth = 64;

        /// <summary>
        /// 创建编码器
        /// </summary>
        public static JsonSerializer CreateSerializer() => JsonSerializer.Create(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DateParseHandling = DateParseHandling.None,
            Culture = CultureInfo.InvariantCulture,
            MaxDepth = MaximumDepth
        });

        /// <summary>
        /// 从 DTO 生成独立字段树
        /// </summary>
        public static JToken ToToken(object value)
        {
            if (value == null)
            {
                throw new InvalidDataException("模块快照不能为空");
            }

            if (value is SaveDataDocument document)
            {
                document.ValidateRootExtensions();
            }

            JToken token = JToken.FromObject(value, CreateSerializer());
            ValidateDepth(token);
            return token;
        }

        /// <summary>
        /// 从字段树生成全新的 DTO
        /// </summary>
        public static T FromToken<T>(JToken value) where T : class
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                throw new InvalidDataException("模块快照不能为空");
            }

            ValidateDepth(value);

            try
            {
                return value.ToObject<T>(CreateSerializer()) ?? throw new InvalidDataException("DTO 解码结果为空");
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("存档 DTO 结构无效", exception);
            }
        }

        /// <summary>
        /// 使用迭代遍历限制对象和数组的嵌套层数，与 JsonTextReader 的深度语义一致
        /// </summary>
        public static void ValidateDepth(JToken value)
        {
            if (value == null)
            {
                throw new InvalidDataException("存档字段树不能为空");
            }

            var pending = new Stack<(JToken Token, int Depth)>();
            int rootDepth;
            if (value is JContainer)
            {
                rootDepth = 1;
            }
            else
            {
                rootDepth = 0;
            }
            pending.Push((value, rootDepth));
            while (pending.Count != 0)
            {
                (JToken token, int depth) = pending.Pop();
                if (depth > MaximumDepth)
                {
                    throw new InvalidDataException("存档嵌套深度超过上限");
                }

                if (token is not JContainer container)
                {
                    continue;
                }

                foreach (JToken child in container.Children())
                {
                    JToken nested;
                    if (child is JProperty property)
                    {
                        nested = property.Value;
                    }
                    else
                    {
                        nested = child;
                    }

                    int nestedDepth;
                    if (nested is JContainer)
                    {
                        nestedDepth = depth + 1;
                    }
                    else
                    {
                        nestedDepth = depth;
                    }
                    pending.Push((nested, nestedDepth));
                }
            }
        }

        /// <summary>
        /// 无类型快照只保留未知的顶层字段，当前提供的字段整体替换
        /// </summary>
        public static JToken MergeSnapshot(JToken previous, JToken current)
        {
            ValidateDepth(current);
            if (previous is JObject oldObject && current is JObject newObject)
            {
                var copy = (JObject)newObject.DeepClone();
                foreach (JProperty property in oldObject.Properties())
                {
                    if (copy.Property(property.Name, StringComparison.Ordinal) == null)
                    {
                        copy.Add(property.Name, property.Value.DeepClone());
                    }
                }
                return copy;
            }
            return current.DeepClone();
        }

        /// <summary>
        /// 根据 DTO 契约保留未知对象字段，字典、数组、动态字段、自定义转换结果整体替换
        /// </summary>
        public static JToken MergeSnapshot(JToken previous, JToken current, Type snapshotType)
        {
            if (snapshotType == null)
            {
                throw new ArgumentNullException(nameof(snapshotType));
            }

            ValidateDepth(current);
            return MergeTypedSnapshot(previous, current, snapshotType, CreateSerializer());
        }

        /// <summary>
        /// 按对象成员契约递归合并
        /// </summary>
        private static JToken MergeTypedSnapshot(JToken previous, JToken current, Type snapshotType, JsonSerializer serializer)
        {
            JsonContract contract = serializer.ContractResolver.ResolveContract(snapshotType);
            if (snapshotType == typeof(object) || typeof(JToken).IsAssignableFrom(snapshotType)
                || contract.Converter != null || contract is not JsonObjectContract objectContract
                || previous is not JObject oldObject || current is not JObject newObject)
            {
                return current.DeepClone();
            }

            var copy = (JObject)newObject.DeepClone();
            foreach (JProperty property in oldObject.Properties())
            {
                JsonProperty member = objectContract.Properties.GetClosestMatchProperty(property.Name);
                if (member == null)
                {
                    if (copy.Property(property.Name, StringComparison.Ordinal) == null)
                    {
                        copy.Add(property.Name, property.Value.DeepClone());
                    }
                    continue;
                }

                JProperty replacement = copy.Property(member.PropertyName, StringComparison.Ordinal);
                if (!member.Ignored && member.Converter == null && member.PropertyType != null && replacement != null)
                {
                    replacement.Value = MergeTypedSnapshot(property.Value, replacement.Value, member.PropertyType, serializer);
                }
            }
            return copy;
        }

    }
}
