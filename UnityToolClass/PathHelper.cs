using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Common
{
    /// <summary>
    /// 路径助手
    /// </summary>
    public class PathHelper
    {
        /// <summary>
        /// 手动分平台处理StreamingAssets路径
        /// </summary>
        /// <param name="path">StreamingAssets中的路径</param>
        /// <returns></returns>
        [Obsolete("Use GetPath instead", true)]
        public static string HandlePath(string path)
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
            //Application.streamingAssetsPath 会根据不同的平台返回对应的 StreamingAssets 目录，一般情况下使用这个即可
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

            return localPath;
        }

        /// <summary>
        /// 分平台处理StreamingAssets路径
        /// </summary>
        /// <param name="path">StreamingAssets中的路径</param>
        /// <returns></returns>
        public static string GetPath(string path)
        {
#if UNITY_EDITOR
            return Application.streamingAssetsPath + path;

#elif UNITY_ANDROID || UNITY_IPHONE
            string resPath = Application.persistentDataPath + path;
            if (!File.Exists(resPath))
            {
                CopyFile(path);
            }
            return resPath;
            
#else
            return Application.streamingAssetsPath + path;
#endif
        }

        /// <summary>
        /// 将StreamingAssets中的文件拷贝到持久化路径中 path=StreamingAssets中的路径
        /// </summary>
        private static void CopyFile(string path)
        {
            //获取StreamingAssets路径
            //创建UnityWebRequest对象
            UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + path);

            //发送请求并等待返回
            //如果不是静态方法，可以使用协程
            //yield return www.SendWebRequest();
            www.SendWebRequest();
            while (!www.isDone) { }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("拷贝文件出错：" + www.error);
            }
            else
            {
                //创建路径（如果不存在）
                Directory.CreateDirectory(Application.persistentDataPath + path.Substring(path.IndexOf('/'), path.LastIndexOf('/')));

                //写入文件
                File.WriteAllBytes(Application.persistentDataPath + path, www.downloadHandler.data);
            }
        }

    }
}

