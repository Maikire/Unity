using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// CreateAssetBundles
    /// </summary>
    public class CreateAssetBundles
    {
        /// <summary>
        /// BuildAllAssetBundles
        /// </summary>
        [MenuItem("Build/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            // 自定义路径
            string assetBundleDirectory = EditorUtility.OpenFolderPanel("选择导出路径", Application.dataPath, "");
            if (string.IsNullOrEmpty(assetBundleDirectory))
            {
                return;
            }

            //固定路径
            //string assetBundleDirectory = Application.streamingAssetsPath + "/AssetBundles";

            BuildAssetBundleOptions assetBundleOptions;
            BuildTarget targetPlatform;

#if UNITY_STANDALONE_WIN //Windows 独立平台应用程序
            assetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            targetPlatform = BuildTarget.StandaloneWindows64;
#elif UNITY_WSA //UWP
            assetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            targetPlatform = BuildTarget.WSAPlayer;
#elif UNITY_STANDALONE_OSX //Mac OS X（包括 Universal、PPC 和 Intel 架构）
            assetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            targetPlatform = BuildTarget.StandaloneOSX;
#elif UNITY_STANDALONE_LINUX //Linux
            assetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            targetPlatform = BuildTarget.StandaloneLinux;
#elif UNITY_ANDROID
            assetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            targetPlatform = BuildTarget.Android;
#elif UNITY_IOS
            assetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            targetPlatform = BuildTarget.iOS;
#elif UNITY_WEBGL
            assetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            targetPlatform = BuildTarget.WebGL;
#endif

            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }
            else
            {
                Directory.Delete(assetBundleDirectory, true);
                Directory.CreateDirectory(assetBundleDirectory);
            }

            string[] allTxtPaths = CopyLuaToTxt();

            GetFilesInfo();

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, assetBundleOptions, targetPlatform);

            DeleteTxtFiles(allTxtPaths);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 获取文件信息，用于调试
        /// </summary>
        private static void GetFilesInfo()
        {
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            List<string> allAssetPaths = new List<string>();

            foreach (string bundleName in bundleNames)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                allAssetPaths.AddRange(assetPaths);
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("All AB paths:");

            foreach (string path in allAssetPaths)
            {
                stringBuilder.AppendLine(path);
            }

            Debug.Log(stringBuilder.ToString());
        }

        /// <summary>
        /// 将 .lua 文件转换为 .txt 文件
        /// </summary>
        /// <returns>.txt 文件路径 & lt;/returns>
        private static string[] CopyLuaToTxt()
        {
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            List<string> allAssetPaths = new List<string>();
            List<string> allTxtPaths = new List<string>();

            foreach (string bundleName in bundleNames)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                allAssetPaths.AddRange(assetPaths);
            }

            foreach (string path in allAssetPaths)
            {
                if (path.EndsWith(".lua"))
                {
                    // 读取.lua 文件内容
                    var utf8 = new System.Text.UTF8Encoding(false);
                    string content = File.ReadAllText(path, utf8);

                    // 构造对应的.txt 文件路径
                    string txtPath = Path.ChangeExtension(path, "txt");
                    File.WriteAllText(txtPath, content, utf8);

                    allTxtPaths.Add(txtPath);
                }
            }

            AssetDatabase.Refresh();

            return allTxtPaths.ToArray();
        }

        /// <summary>
        /// 删除 .txt 文件
        /// </summary>
        /// <param name="dir"></param>
        private static void DeleteTxtFiles(string[] allTxtPaths)
        {
            foreach (string path in allTxtPaths)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

    }
}
