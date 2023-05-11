using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManger
    {
        //名称, 路径
        private static Dictionary<string, string> ConfigFileDIC;

        //静态构造函数
        //初始化类的静态数据成员
        //在类被加载的时候执行一次
        static ResourceManger()
        {
            string fileContent = GetTextFromStreamingAssets("ConfigFile.txt");
            BulidText(fileContent);
        }

        /// <summary>
        ///读取StreamingAssets中的文件 
        /// </summary>
        /// <param name="path">StreamingAssets下的文件路径</param>
        /// <returns>读取到的字符串</returns>
        private static string GetTextFromStreamingAssets(string path)
        {
            string localPath;

            #region 分平台判断路径
            //这样写 性能不好
            //if (Application.platform == RuntimePlatform.Android)
            //{
            //    localPath = Application.streamingAssetsPath + "/" + path;
            //}
            //else
            //{
            //    localPath = "file://" + Application.streamingAssetsPath + "/" + path;
            //}

            //性能更高的写法，使用Unity宏标签
            //不同的平台会拥有不同的代码（如果发布到安卓平台，就只有第三段代码，其他的代码不会打包带走）
            //Application.dataPath 会定位到 Assets 目录
#if UNITY_EDITOR || UNITY_STANDALONE
            localPath = "file://" + Application.dataPath + "/StreamingAssets/" + path;
#elif UNITY_IPHONE
            localPath = "file://" + Application.dataPath + "/Raw/" + path;
#elif UNITY_ANDROID
            localPath = "jar:file://" + Application.dataPath + "!/assets/" + path;
#else
            localPath = "file://" + Application.streamingAssetsPath + "/" + path;
#endif
            #endregion

            WWW www = new WWW(localPath);

            //读取文件出错
            if (www.error != null)
            {
                Debug.LogError("error while reading files : " + localPath);
                return null;
            }

            while (true)
            {
                if (www.isDone)
                {
                    return www.text;
                }
            }
        }

        /// <summary>
        /// 加载文件到字典
        /// </summary>
        /// <param name="configFile"></param>
        private static void BulidText(string configFile)
        {
            ConfigFileDIC = new Dictionary<string, string>();

            //通过切割实现
            //string[] temp = configFile.Split("\r\n");
            //for (int i = 0; i < temp.Length - 1; i++)
            //{
            //    string[] temp_1 = temp[i].Split('=');
            //    ConfigFileDIC.Add(temp_1[0], temp_1[1]);
            //}

            //通过字符串读取器实现
            //StringReader 提供了逐行读取的功能（ReadLine()）
            //程序在退出 using 代码框后，会自动释放资源（自动调用 Dispose()）
            //不论程序是正常退出还是异常退出，都会自动释放资源
            //如果不在using代码框中，程序异常退出时不会执行 Dispose()
            using (StringReader reader = new StringReader(configFile))
            {
                //不够艺术
                //string line = reader.ReadLine();
                //while (line != null)
                //{
                //    string[] keyValue = line.Split('=');
                //    ConfigFileDIC.Add(keyValue[0], keyValue[1]);
                //    line = reader.ReadLine();
                //}

                //艺术
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] keyValue = line.Split('=');
                    ConfigFileDIC.Add(keyValue[0], keyValue[1]);
                }

                //在 using 代码框中，不需要写这两串代码
                //reader.Dispose();
                //reader.Close();
            }
        }

        /// <summary>
        /// 加载文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public static T Load<T>(string prefabName) where T : Object
        {
            return Resources.Load<T>(ConfigFileDIC[prefabName]);
        }


    }
}
