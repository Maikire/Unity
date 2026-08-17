using System;
using System.IO;

namespace Common
{
    /// <summary>
    /// 配置文件读取器
    /// </summary>
    public class ConfigReader
    {
        /// <summary>
        /// 读取 StreamingAssets 中的文本文件
        /// </summary>
        /// <param name="path">StreamingAssets 下的文件路径 & lt;/param>
        /// <returns></returns>
        public static string GetConfig(string path)
        {
            string localPath = PathHelper.GetPath(path);

#if UNITY_WEBGL
            // 创建 UnityWebRequest 对象
            UnityWebRequest www = UnityWebRequest.Get(localPath);

            // 发送请求并等待返回
            // 如果不是静态方法，可以使用协程
            //yield return www.SendWebRequest();
            www.SendWebRequest();
            while (!www.isDone) { }

            // 检查请求是否有错误
            if (www.result == UnityWebRequest.Result.Success)
            {
                // 读取文本内容
                return www.downloadHandler.text;
            }
            else
            {
                // 报错
                throw new Exception(www.error);
            }

#else
            string content;
            using (StreamReader sr = new StreamReader(localPath))
            {
                content = sr.ReadToEnd();
            }
            return content;
#endif
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="fileContent"> 文件内容 & lt;/param>
        /// <param name="handler"> 读取方法 & lt;/param>
        public static void ReadConfig(string fileContent, Action<string> handler)
        {
            // 通过字符串读取器实现
            // StringReader 提供了逐行读取的功能（ReadLine）
            // 程序在退出 using 代码框后，会自动释放资源（自动调用 Dispose）
            // 不论程序是正常退出还是异常退出，都会自动释放资源
            // 如果不在 using 代码框中，程序异常退出时不会执行 Dispose
            using (StringReader reader = new StringReader(fileContent))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line != string.Empty)
                    {
                        handler(line);
                    }
                }
            }
        }

    }
}