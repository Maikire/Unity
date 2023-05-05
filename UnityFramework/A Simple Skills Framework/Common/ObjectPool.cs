using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 对象池
    /// </summary>
    public class ObjectPool : MonoSingleton<ObjectPool>
    {
        private Dictionary<string, List<GameObject>> GameObjectDIC;

        public override void Init()
        {
            base.Init();
            GameObjectDIC = new Dictionary<string, List<GameObject>>();
        }

        /// <summary>
        /// 获取物体
        /// </summary>
        /// <param name="gameObjectKey">物体类型</param>
        /// <param name="prefab">预制件</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns></returns>
        public GameObject GetGameObject(string gameObjectKey, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject temp;

            if (!GameObjectDIC.ContainsKey(gameObjectKey))
            {
                GameObjectDIC.Add(gameObjectKey, new List<GameObject>());
            }

            //生成物体
            if ((temp = GameObjectDIC[gameObjectKey].Find(g => !g.activeInHierarchy)) == null)
            {
                temp = Instantiate(prefab);
                GameObjectDIC[gameObjectKey].Add(temp);
            }

            //设置参数
            SetParameters(temp, position, rotation);

            return temp;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        private void SetParameters(GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;

            //每个物体的重置的逻辑都不相同，所以需要使用接口
            //遍历物体中所有需要重置的逻辑
            foreach (var item in gameObject.GetComponents<IResettable>())
            {
                item.OnReset();
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 回收物体
        /// </summary>
        /// <param name="gameObjectKey">物体类型</param>
        /// <param name="gameObject">物体</param>
        public void RecoverGameObject(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 延迟回收物体
        /// </summary>
        /// <param name="gameObjectKey">物体类型</param>
        /// <param name="gameObject">物体</param>
        /// <param name="delay">延迟时间（秒）</param>
        public void RecoverGameObject(GameObject gameObject, float delay)
        {
            StartCoroutine(DelayRecover(gameObject, delay));
        }

        /// <summary>
        /// 延迟回收
        /// </summary>
        /// <param name="gameObjectKey"></param>
        /// <param name="gameObject"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator DelayRecover(GameObject gameObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 清空某一类别
        /// </summary>
        /// <param name="gameObjectKey"></param>
        public void Clear(string gameObjectKey)
        {
            //字典、队列等数据结构 存储的都是引用，这些都不占内存
            //真正占内存的是生成的物体，所以主要目的是清除物体（主要是模型、网格、材质（各种贴图）、组件...）
            //Destroy()清除的就是这些真正占内存的物体
            foreach (GameObject item in GameObjectDIC[gameObjectKey])
            {
                GameObject.Destroy(item);
            }
            GameObjectDIC.Remove(gameObjectKey);
        }

        /// <summary>
        /// 清空全部内容
        /// </summary>
        public void ClearAll()
        {
            //foreach是只读的，如果直接用 GameObjectDIC.Keys 会报错，
            //因为在执行 Clear(key) 时 GameObjectDIC.Keys 发生了改变，
            //MoveNext() 方法可能会出错
            foreach (string key in new List<string>(GameObjectDIC.Keys))
            {
                Clear(key);
            }
            GameObjectDIC.Clear();
        }


    }
}
