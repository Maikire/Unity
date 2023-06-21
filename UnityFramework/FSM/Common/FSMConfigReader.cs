using Common;
using System.Collections.Generic;

namespace AI.FSM
{
    /// <summary>
    /// FSM配置文件读取器
    /// </summary>
    public class FSMConfigReader
    {
        /// <summary>
        /// 大字典：状态--映射
        /// 小字典：条件编号--状态编号
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Map
        {
            get;
            private set;
        }

        /// <summary>
        /// 大字典的键值
        /// </summary>
        private string MainKey;

        public FSMConfigReader(string path)
        {
            Map = new Dictionary<string, Dictionary<string, string>>();

            string configfile = ConfigReader.GetConfigFile(path);
            ConfigReader.ReadConfigFile(configfile, BulidLine);
        }

        /// <summary>
        /// 读取一行
        /// </summary>
        /// <param name="line"></param>
        private void BulidLine(string line)
        {
            //去除空白
            line = line.Trim();

            //判断字符串是否为空，有两种方法
            //if (line == "" || line == null) return;
            if (string.IsNullOrEmpty(line)) return;

            if (line.StartsWith("["))
            {
                MainKey = line.Substring(1, line.Length - 2);
                Map.Add(MainKey, new Dictionary<string, string>());
            }
            else
            {
                string[] values = line.Split("->");
                Map[MainKey].Add(values[0], values[1]);
            }
        }


    }
}

