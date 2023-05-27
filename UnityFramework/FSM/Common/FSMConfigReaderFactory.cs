using System.Collections.Generic;

namespace AI.FSM
{
    /// <summary>
    /// FSM配置文件读取工厂
    /// </summary>
    public class FSMConfigReaderFactory
    {
        /// <summary>
        /// 储存已生成的对象，循环利用
        /// </summary>
        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> Cache = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        /// <summary>
        /// 获取配置表
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> GetConfig(string path)
        {
            if (Cache.ContainsKey(path))
            {
                return Cache[path];
            }
            else
            {
                FSMConfigReader configReader = new FSMConfigReader(path);
                Cache.Add(path, configReader.Map);
                return configReader.Map;
            }
        }


    }
}

