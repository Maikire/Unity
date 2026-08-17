using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Common
{
    /// <summary>
    /// 路径助手（优先全部加载）
    /// </summary>
    public class PathHelperPriority : MonoSingleton<PathHelperPriority>
    {
        protected override void Init()
        {
            base.Init();

#if UNITY_EDITOR
            return;

#elif UNITY_ANDROID || UNITY_IOS
            StartCoroutine(GetFilePath());

#else
            return;
#endif
        }

        /// <summary>
        /// 分平台处理StreamingAssets路径
        /// </summary>
        /// <param name="path">StreamingAssets中的路径</param>
        /// <returns></returns>
        public string GetPath(string path)
        {
#if UNITY_EDITOR
            return Application.streamingAssetsPath + path;

#elif UNITY_ANDROID || UNITY_IOS
            return Application.persistentDataPath + path;
            
#else
            return Application.streamingAssetsPath + path;
#endif
        }

        /// <summary>
        /// 获取StreamingAssets中所有文件路径
        /// </summary>
        private IEnumerator GetFilePath()
        {
            string sourcePath = Application.streamingAssetsPath;
            string destinationPath = Application.persistentDataPath;

            //选取需要拷贝的文件
            string[] filePaths = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).
                Where(
                    file => file.EndsWith(".txt") ||
                    file.EndsWith(".xml") ||
                    file.EndsWith(".db")).
                ToArray();

            foreach (string filePath in filePaths)
            {
                string relativePath = filePath.Substring(sourcePath.Length).Replace("\\", "/");
                string destFilePath = destinationPath + relativePath;

                // 如果文件已存在，则跳过
                if (File.Exists(destFilePath)) continue;

                yield return CopyFile(relativePath);
            }
        }

        /// <summary>
        /// 将StreamingAssets中的文件拷贝到持久化路径中 path=StreamingAssets中的路径
        /// </summary>
        private IEnumerator CopyFile(string path)
        {
            //获取StreamingAssets路径
            //创建UnityWebRequest对象
            UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + path);

            //发送请求并等待返回
            yield return www.SendWebRequest();

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
