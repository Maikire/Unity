using Common;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// UI管理器：管理（记录/隐藏）所有窗口，提供查找窗口的方法
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        /// <summary>
        /// 窗口对象字典
        /// key:窗口对象名称
        /// value:窗口对象引用
        /// </summary>
        private Dictionary<string, UIWindow> UIWindowDIC;

        private void Start()
        {
            InitDic();
        }

        /// <summary>
        /// 初始化字典
        /// </summary>
        private void InitDic()
        {
            UIWindowDIC = new Dictionary<string, UIWindow>();
            foreach (var item in this.GetComponentsInChildren<UIWindow>())
            {
                UIWindowDIC.Add(item.GetType().Name, item);
                if (!item.IsBasicUICanvas)
                {
                    item.gameObject.SetActive(false);
                }
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

    }
}
