using Common;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ARPGDemo.UI
{
    /// <summary>
    /// UI管理器：管理（记录/隐藏）所有窗口，提供查找窗口的方法
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        //key:窗口对象名称  value:窗口对象引用
        private Dictionary<string, UIWindow> UIWindowDIC; //窗口对象字典

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init()
        {
            base.Init();
            InitDic();
        }

        /// <summary>
        /// 初始化字典
        /// </summary>
        private void InitDic()
        {
            UIWindowDIC = new Dictionary<string, UIWindow>();
            foreach (var item in FindObjectsOfType<UIWindow>())
            {
                AddWindow(item);
                item.SetVisible(false);
            }
        }

        /// <summary>
        /// 根据类型查找窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWindow<T>() where T : UIWindow
        {
            string T_Name = typeof(T).Name;
            if (!UIWindowDIC.ContainsKey(T_Name)) return null;
            return UIWindowDIC[T_Name] as T;
        }

        /// <summary>
        /// 添加窗口
        /// </summary>
        /// <param name="window">窗口对象</param>
        public void AddWindow(UIWindow window)
        {
            UIWindowDIC.Add(window.GetType().Name, window);
        }


    }
}
