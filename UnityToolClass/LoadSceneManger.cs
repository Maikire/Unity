using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
    /// <summary>
    /// 加载场景管理器
    /// </summary>
    public class LoadSceneManger : MonoSingleton<LoadSceneManger>
    {
        /// <summary>
        /// 异步加载
        /// </summary>
        private List<AsyncOperation> operations;
        /// <summary>
        /// 加载进度
        /// </summary>
        public float LoadProgress
        {
            get
            {
                if (operations.Count != 0)
                {
                    return operations[0].progress / 0.9f;
                }

                return 0;
            }
        }

        protected override void Init()
        {
            base.Init();

            DontDestroyOnLoad(this);
            operations = new List<AsyncOperation>();
        }

        /// <summary>
        /// 加载场景
        /// 请确保sceneName[0]是主场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void LoadScene(params string[] sceneName)
        {
            if (sceneName == null || sceneName.Length == 0)
            {
                return;
            }

            operations.Clear();

            StartCoroutine(ToLoadScene(sceneName));
        }

        /// <summary>
        /// 加载过度场景
        /// </summary>
        private IEnumerator LoadTransitionScene()
        {
            yield return SceneManager.LoadSceneAsync("TransitionScene", LoadSceneMode.Single);
        }

        /// <summary>
        /// ToLoadScene
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator ToLoadScene(string[] sceneName)
        {
            yield return LoadTransitionScene();

            operations.Add(SceneManager.LoadSceneAsync(sceneName[0], LoadSceneMode.Single));

            AsyncOperation async;
            for (int i = 1; i < sceneName.Length; i++)
            {
                async = SceneManager.LoadSceneAsync(sceneName[i], LoadSceneMode.Additive);
                async.allowSceneActivation = false;
                operations.Add(async);
            }

            StartCoroutine(WaitForSingleScene());
        }

        /// <summary>
        /// WaitForSingleScene
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForSingleScene()
        {
            while (!operations[0].isDone)
            {
                yield return null;
            }

            for (int i = 1; i < operations.Count; i++)
            {
                operations[i].allowSceneActivation = true;
            }
        }


    }
}