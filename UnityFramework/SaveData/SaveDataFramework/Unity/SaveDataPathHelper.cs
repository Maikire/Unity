using System;
using System.IO;
using UnityEngine;

namespace SaveData
{
    /// <summary>
    /// 框架自有的持久化目录助手，不依赖项目业务工具
    /// </summary>
    public static class SaveDataPathHelper
    {
        /// <summary>
        /// 将相对目录组合到 Unity 持久化路径并创建目录
        /// </summary>
        /// <param name="path">
        /// 持久化根目录下的非空相对目录
        /// </param>
        /// <returns>
        /// 已创建的绝对持久化目录
        /// </returns>
        public static string GetPersistentDirectoryPath(string path) =>
            GetPersistentDirectoryPath(Application.persistentDataPath, path);

        /// <summary>
        /// 使用持久化根目录创建子目录，供平台桥接和隔离验证使用
        /// </summary>
        internal static string GetPersistentDirectoryPath(string persistentRoot, string path)
        {
            if (string.IsNullOrWhiteSpace(persistentRoot) || !Path.IsPathRooted(persistentRoot))
            {
                throw new NotSupportedException("平台没有本地持久化路径，请配置存储后端");
            }

            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
            {
                throw new ArgumentException("目录必须为非空相对路径", nameof(path));
            }

            string root = Path.GetFullPath(persistentRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string persistentPath = Path.GetFullPath(Path.Combine(root, path));
            if (!persistentPath.StartsWith(root, StringComparison.Ordinal))
            {
                throw new ArgumentException("存档目录不能穿越持久化目录", nameof(path));
            }

            Directory.CreateDirectory(persistentPath);
            return persistentPath;
        }

    }
}
