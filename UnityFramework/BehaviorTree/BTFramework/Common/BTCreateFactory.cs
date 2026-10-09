using Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BehaviorTree
{
    /// <summary>
    /// 节点创建工厂
    /// </summary>
    public static class BTCreateFactory
    {
        /// <summary>
        /// 节点映射表，存储所有节点实例
        /// </summary>
        private static Dictionary<string, BTNode> nodeDic;
        /// <summary>
        /// Type 缓存
        /// </summary>
        private static Dictionary<string, Type> typeCache;
        /// <summary>
        /// 根节点的键值
        /// </summary>
        private static string rootID;
        /// <summary>
        /// 优先级节点的键值
        /// </summary>
        private static string priorityID;

        static BTCreateFactory()
        {
            nodeDic = new Dictionary<string, BTNode>();
            typeCache = new Dictionary<string, Type>();
            rootID = null;
            priorityID = null;
        }

        /// <summary>
        /// 构建行为树
        /// </summary>
        /// <param name="config">配置</param>
        /// <param name="root">根节点</param>
        /// <param name="priorityNode">优先级节点</param>
        public static void BuildBehaviorTree(string config, out BTNode root, out BTNode priorityNode)
        {
            nodeDic.Clear();
            typeCache.Clear();
            rootID = null;
            priorityID = null;

            ConfigReader.ReadConfig(config, ReadLine);
            root = nodeDic[rootID];

            if (priorityID == null)
            {
                priorityNode = new PriorityNode();
            }
            else
            {
                priorityNode = nodeDic[priorityID];
            }
        }

        /// <summary>
        /// 读取一行配置
        /// </summary>
        /// <param name="line"></param>
        private static void ReadLine(string line)
        {
            line = line.Trim();

            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            if (line.Contains("->"))
            {
                string[] parts = line.Split("->");

                if (parts[0].StartsWith("PriorityNode"))
                {
                    if (priorityID == null)
                    {
                        priorityID = parts[0];
                    }
                    else if (priorityID != parts[0])
                    {
                        throw new Exception($"多个优先级节点: {line}");
                    }
                }

                ControlNodes control = CreateNode(parts[0]) as ControlNodes;

                if (parts[1].StartsWith("PriorityNode"))
                {
                    throw new Exception($"优先级节点不能作为子节点: {line}");
                }
                else if (parts[1].StartsWith("{"))
                {
                    BTFilter filter = CreateFilter(parts[1]);
                    control.AddChild(filter);
                }
                else
                {
                    BTNode node = CreateNode(parts[1]);
                    control.AddChild(node);
                }
            }
            else
            {
                if (rootID != null)
                {
                    throw new Exception($"多个根节点: {line}");
                }

                rootID = line;
                CreateNode(line);
            }
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static Type GetType(string typeName)
        {
            if (typeCache.ContainsKey(typeName))
            {
                return typeCache[typeName];
            }

            Type type = Type.GetType(typeName);

            if (type == null)
            {
                throw new Exception($"找不到类型: {typeName}");
            }

            typeCache.Add(typeName, type);

            return type;
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="Exception"></exception>
        private static BTNode CreateNode(string token)
        {
            if (nodeDic.ContainsKey(token))
            {
                return nodeDic[token];
            }

            Match match;
            Type type;
            BTNode node = null;
            string nodeToken;

            if ((match = Regex.Match(token, "^([A-Za-z]+)\\[\\d+\\]$")).Success)
            {
                nodeToken = match.Groups[1].Value;

                type = GetType($"BehaviorTree.{nodeToken}");
                node = Activator.CreateInstance(type) as BTNode;

                nodeDic.Add(token, node);

                return node;
            }
            else if ((match = Regex.Match(token, "^([A-Za-z]+)(\\[\\d+\\])\\{([0-9]+(?:[.][0-9]+){0,1})\\}$")).Success)
            {
                nodeToken = match.Groups[1].Value;
                string tagToken = match.Groups[2].Value;
                string parameterToken = match.Groups[3].Value;

                type = GetType($"BehaviorTree.{nodeToken}");
                if (int.TryParse(parameterToken, out int i))
                {
                    node = Activator.CreateInstance(type, new object[] { i }) as BTNode;
                }
                else if (float.TryParse(parameterToken, out float f))
                {
                    node = Activator.CreateInstance(type, new object[] { f }) as BTNode;
                }

                nodeDic.Add(nodeToken + tagToken, node);

                return node;
            }

            throw new Exception($"无效的格式: {token}");
        }

        /// <summary>
        /// 创建过滤结构
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static BTFilter CreateFilter(string token)
        {
            Match match = Regex.Match(token, "^\\{([A-Za-z]+)\\[\\d+\\],([A-Za-z]+\\[\\d+\\](?:\\{(?:[0-9]+(?:[.][0-9]+){0,1})\\})?)\\}$");

            if (!match.Success)
            {
                throw new Exception($"无效的格式: {token}");
            }

            Type type = GetType($"BehaviorTree.{match.Groups[1].Value}");
            ConditionNode cond = Activator.CreateInstance(type) as ConditionNode;

            BTNode node = CreateNode(match.Groups[2].Value);

            return new BTFilter
            {
                condition = cond,
                child = node
            };
        }

    }
}
