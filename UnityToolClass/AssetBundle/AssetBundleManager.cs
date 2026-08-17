using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Common
{
    /// <summary>
    /// AB 包管理器
    /// </summary>
    public static class AssetBundleManager
    {
        /// <summary>
        /// manifest 字典，key 为 AB 包的根目录名称，value 为对应的 manifest 文件
        /// </summary>
        private static Dictionary<string, AssetBundleManifest> manifestDic = new();

        /// <summary>
        /// 正在加载 manifest 的字典，key 为 AB 包的根目录名称，value 为对应的 LoadingManifest 对象
        /// </summary>
        private static Dictionary<string, LoadingManifest> loadingManifest = new();

        /// <summary>
        /// 缓存的 bundle 字典，key 为 AB 包在 StreamingAssets 内的相对路径，value 为对应的 BundleRecord 对象
        /// </summary>
        private static Dictionary<string, BundleRecord> bundles = new();

        /// <summary>
        /// 正在加载的 bundle 字典，key 为 AB 包在 StreamingAssets 内的相对路径，value 为对应的 LoadingRecord 对象
        /// </summary>
        private static Dictionary<string, LoadingRecord> loadingBundles = new();

        /// <summary>
        /// AssetBundleLoader 实例
        /// </summary>
        private static AssetBundleLoader loader = AssetBundleLoader.Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            foreach (BundleRecord record in bundles.Values)
            {
                if (record.bundle != null)
                {
                    record.bundle.Unload(false);
                }
            }

            manifestDic.Clear();
            loadingManifest.Clear();
            bundles.Clear();
            loadingBundles.Clear();
        }

        /// <summary>
        /// 加载资源包
        /// </summary>
        /// <param name="path">AB 包在 StreamingAssets 内的相对路径</param>
        /// <param name="callback">委托</param>
        /// <returns></returns>
        public static AssetBundleHandle LoadAssetBundle(string path, Action<AssetBundleHandle> callback = null)
        {
            AssetBundleHandle handle = new(path);

            if (callback != null)
            {
                handle.Completed += callback;
            }

            loader.StartCoroutine(ToLoadAssetBundle(handle));

            return handle;
        }

        /// <summary>
        /// 释放资源包
        /// </summary>
        public static void Release(AssetBundleHandle handle)
        {
            if (handle == null)
            {
                return;
            }

            // 已经释放过
            if (handle.IsReleased)
            {
                return;
            }

            // Bundle 还在加载
            if (!handle.IsDone)
            {
                handle.releaseRequested = true;
                return;
            }

            // 加载失败，没有资源需要释放
            if (handle.acquiredBundles == null)
            {
                handle.IsReleased = true;
                handle.Bundle = null;
                return;
            }

            // 释放这张 Handle 持有的全部引用
            for (int i = handle.acquiredBundles.Count - 1; i >= 0; --i)
            {
                ReleaseBundle(handle.acquiredBundles[i]);
            }

            handle.acquiredBundles = null;
            handle.Bundle = null;
            handle.IsReleased = true;
        }

        /// <summary>
        /// 加载资源包
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private static IEnumerator ToLoadAssetBundle(AssetBundleHandle handle)
        {
            string path = handle.Path;

            if (!TryResolveBundlePath(path, out string startName, out string bundleName, out string startPath))
            {
                Debug.LogError("Invalid path: " + path);
                LoadFailed(handle);
                yield break;
            }

            // 加载 manifest 文件
            AssetBundleManifest targetManifest = null;
            yield return AcquireManifest(startPath, startName,
                manifest =>
                {
                    targetManifest = manifest;
                });

            if (targetManifest == null)
            {
                LoadFailed(handle);
                yield break;
            }

            // 获取依赖文件
            string[] dependencies = targetManifest.GetAllDependencies(bundleName);
            List<string> acquired = new();

            foreach (string dependency in dependencies)
            {
                AssetBundle dependencyBundle = null;

                yield return AcquireBundle(startName, dependency, startPath,
                    bundle =>
                    {
                        dependencyBundle = bundle;
                    });

                if (dependencyBundle == null)
                {
                    for (int i = acquired.Count - 1; i >= 0; --i)
                    {
                        ReleaseBundle(acquired[i]);
                    }

                    LoadFailed(handle);
                    yield break;
                }

                acquired.Add(CombinePath(startName, dependency));
            }

            // 加载目标 Bundle
            AssetBundle targetBundle = null;

            yield return AcquireBundle(startName, bundleName, startPath,
                bundle =>
                {
                    targetBundle = bundle;
                });

            if (targetBundle == null)
            {
                for (int i = acquired.Count - 1; i >= 0; --i)
                {
                    ReleaseBundle(acquired[i]);
                }

                LoadFailed(handle);
                yield break;
            }

            acquired.Add(CombinePath(startName, bundleName));

            handle.Bundle = targetBundle;
            handle.acquiredBundles = acquired;
            handle.IsDone = true;

            // 有可能用户在加载过程中已经 Release()
            if (handle.releaseRequested)
            {
                Release(handle);
                yield break;
            }

            handle.InvokeCompleted();
        }

        /// <summary>
        /// 加载 manifest
        /// </summary>
        /// <param name="startPath">AB 包根目录路径</param>
        /// <param name="startName">AB 包的根目录名称</param>
        /// <param name="callback">委托</param>
        /// <returns></returns>
        private static IEnumerator AcquireManifest(string startPath, string startName, Action<AssetBundleManifest> callback)
        {
            // 已经加载完成
            if (manifestDic.TryGetValue(startName, out AssetBundleManifest manifest))
            {
                callback?.Invoke(manifest);
                yield break;
            }

            // 已经有人正在加载
            if (loadingManifest.TryGetValue(startName, out LoadingManifest loading))
            {
                while (!loading.isDone)
                {
                    yield return null;
                }

                if (loading.manifest == null)
                {
                    callback?.Invoke(null);
                    yield break;
                }

                // 加载成功后直接返回结果
                if (manifestDic.TryGetValue(startName, out manifest))
                {
                    callback?.Invoke(manifest);
                }
                else
                {
                    Debug.LogError($"Bundle load state error: {startName}");
                    callback?.Invoke(null);
                }

                yield break;
            }

            // 没加载，也没人正在加载
            loading = new LoadingManifest();

            loadingManifest.Add(startName, loading);

            using UnityWebRequest request_ab = UnityWebRequestAssetBundle.GetAssetBundle(CombinePath(startPath, startName), 0);
            yield return request_ab.SendWebRequest();

            if (request_ab.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load AssetBundle: " + request_ab.error);

                // 通知所有等待者
                CompleteLoading(startName, loading, null);

                callback?.Invoke(null);
                yield break;
            }

            AssetBundle bundle_ab = DownloadHandlerAssetBundle.GetContent(request_ab);
            if (bundle_ab == null)
            {
                Debug.LogError($"Failed to get manifest bundle content: {startName}");
                CompleteLoading(startName, loading, null);
                callback?.Invoke(null);
                yield break;
            }

            manifest = bundle_ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            bundle_ab.Unload(false);

            if (manifest == null)
            {
                Debug.LogError($"AssetBundleManifest was not found in manifest bundle: {startName}");
                CompleteLoading(startName, loading, null);
                callback?.Invoke(null);
                yield break;
            }

            manifestDic.Add(startName, manifest);

            // 通知所有等待者
            CompleteLoading(startName, loading, manifest);

            callback?.Invoke(manifest);
        }

        /// <summary>
        /// 加载 bundle
        /// </summary>
        /// <param name="startName">AB 包的根目录名称</param>
        /// <param name="bundleName">AB 包名称</param>
        /// <param name="startPath">AB 包根目录路径</param>
        /// <param name="callback">委托</param>
        /// <returns></returns>
        private static IEnumerator AcquireBundle(string startName, string bundleName, string startPath, Action<AssetBundle> callback)
        {
            string key = CombinePath(startName, bundleName);

            // 已经加载完成
            if (bundles.TryGetValue(key, out BundleRecord record))
            {
                record.refCount++;
                callback?.Invoke(record.bundle);
                yield break;
            }

            // 已经有人正在加载
            if (loadingBundles.TryGetValue(key, out LoadingRecord loadingRecord))
            {
                loadingRecord.acquireCount++;

                // 等待那个加载任务完成
                while (!loadingRecord.isDone)
                {
                    yield return null;
                }

                // 加载失败
                if (loadingRecord.bundle == null)
                {
                    callback?.Invoke(null);
                    yield break;
                }

                // 加载成功
                if (bundles.TryGetValue(key, out record))
                {
                    callback?.Invoke(record.bundle);
                }
                else
                {
                    Debug.LogError($"Bundle load state error: {key}");
                    callback?.Invoke(null);
                }

                yield break;
            }

            // 没加载，也没人正在加载。第一个请求拥有第一个引用
            loadingRecord = new LoadingRecord
            {
                acquireCount = 1
            };

            loadingBundles.Add(key, loadingRecord);

            using UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(CombinePath(startPath, bundleName), 0);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load AssetBundle: {bundleName}\n{request.error}");

                // 通知所有等待者
                CompleteLoading(key, loadingRecord, null);

                callback?.Invoke(null);
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle == null)
            {
                Debug.LogError($"Failed to get AssetBundle content: {bundleName}");
                CompleteLoading(key, loadingRecord, null);
                callback?.Invoke(null);
                yield break;
            }

            // 第一个请求完成记录
            record = new BundleRecord(bundle, loadingRecord.acquireCount);
            bundles.Add(key, record);

            // 通知所有等待者
            CompleteLoading(key, loadingRecord, bundle);

            callback?.Invoke(bundle);
        }

        /// <summary>
        /// 加载完成，通知所有等待者
        /// </summary>
        private static void CompleteLoading(string key, LoadingRecord loading, AssetBundle bundle)
        {
            loading.bundle = bundle;
            loading.isDone = true;
            loadingBundles.Remove(key);
        }

        /// <summary>
        /// 加载完成，通知所有等待者
        /// </summary>
        private static void CompleteLoading(string key, LoadingManifest loading, AssetBundleManifest manifest)
        {
            loading.manifest = manifest;
            loading.isDone = true;
            loadingManifest.Remove(key);
        }

        /// <summary>
        /// 加载失败，结束 Handle
        /// </summary>
        private static void LoadFailed(AssetBundleHandle handle)
        {
            handle.Bundle = null;
            handle.acquiredBundles = null;
            handle.IsDone = true;

            if (handle.releaseRequested)
            {
                handle.IsReleased = true;
                return;
            }

            handle.InvokeCompleted();
        }

        /// <summary>
        /// 释放 bundle 的引用计数，如果引用计数小于等于 0，则卸载 bundle
        /// </summary>
        private static bool ReleaseBundle(string key)
        {
            if (!bundles.TryGetValue(key, out BundleRecord record))
            {
                return false;
            }

            record.refCount--;

            if (record.refCount <= 0)
            {
                record.bundle.Unload(false);
                bundles.Remove(key);
            }

            return true;
        }

        /// <summary>
        /// 处理路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startName">AB 包的根目录名称</param>
        /// <param name="bundleName">AB 包名称</param>
        /// <param name="startPath">AB 包根目录路径</param>
        /// <returns></returns>
        private static bool TryResolveBundlePath(string path, out string startName, out string bundleName, out string startPath)
        {
            startName = null;
            bundleName = null;
            startPath = null;

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string normalizedPath = path.Replace('\\', '/').Trim().TrimStart('/');
            string[] segments = normalizedPath.Split('/');
            if (segments.Length < 2)
            {
                return false;
            }

            for (int i = 0; i < segments.Length; ++i)
            {
                segments[i] = segments[i].Trim();
                if (string.IsNullOrEmpty(segments[i])
                    || segments[i] == "."
                    || segments[i] == ".."
                    || segments[i].Contains(':'))
                {
                    return false;
                }
            }

            startName = segments[0];
            bundleName = string.Join("/", segments, 1, segments.Length - 1);

            if (string.IsNullOrEmpty(startName) || string.IsNullOrEmpty(bundleName))
            {
                return false;
            }

            startPath = PathHelper.GetPath(CombinePath(null, startName));

            return true;
        }

        /// <summary>
        /// 组合路径
        /// </summary>
        private static string CombinePath(string left, string right)
        {
            if (string.IsNullOrEmpty(right))
            {
                return null;
            }

            if (left == null)
            {
                return $"/{right}";
            }

            return $"{left}/{right}";
        }

    }
}
