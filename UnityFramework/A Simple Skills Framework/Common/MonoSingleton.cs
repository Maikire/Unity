using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 单例模式
    /// 脚本单例类
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        //T 表示子类类型

        /* 适用性：场景中存在唯一的对象
         * 使用方法：
         *         1.继承时必须传递子类类型。
         *         2.在任意脚本生命周期中，通过子类类型访问属性 Instance
         *         3.子类中，Init() 代替 Awake()
         */

        //按需加载
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        //创建物体会立即执行 Awake()，处于静态区的 instance 会被初始化
                        new GameObject("Singleton of " + typeof(T)).AddComponent<T>();
                    }
                    else
                    {
                        instance.Init();
                    }
                }

                return instance;
            }
        }

        private void Awake()
        {
            //如果脚本挂在了物体上，就直接赋值，这样就不用执行 FindObjectOfType<T>() 了
            if (instance == null)
            {
                instance = this as T;
                Init();
            }
        }

        /// <summary>
        /// 以此代替子类中的 Awake()
        /// </summary>
        public virtual void Init() { }


    }
}
