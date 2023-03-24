using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    [RequireComponent(typeof(CanvasGroup))]

    /// <summary>
    /// 所有窗口的父类
    /// </summary>
    public class UIWindow : MonoBehaviour
    {
        private CanvasGroup UICanvasGroup; //画布组
        private Dictionary<string, UIEventListener> UIEventDic;

        private void Awake()
        {
            UICanvasGroup = this.GetComponent<CanvasGroup>();
            UIEventDic = new Dictionary<string, UIEventListener>();
        }

        /// <summary>
        /// 设置UI的可见性
        /// </summary>
        /// <param name="state">状态</param>
        public void SetVisible(bool state /*float delay = 0*/)
        {
            //可以设置 启用/禁用
            //可以设置 alpha （相对来讲，这样做性能更好）
            //UICanvasGroup.alpha = state ? 1 : 0;
            gameObject.SetActive(state);
        }

        /// <summary>
        /// 延迟设置UI的可见性
        /// </summary>
        /// <param name="alpha">alpha</param>
        /// <param name="delay">延迟时间</param>
        public void SetVisibleAlpha(float alpha, float delay)
        {
            StartCoroutine(SetVisibleAlphaDelay(alpha, delay));
        }

        /// <summary>
        /// 延迟设置UI的可见性
        /// </summary>
        /// <param name="alpha">alpha</param>
        /// <param name="delay">延迟时间</param>
        /// <returns></returns>
        private IEnumerator SetVisibleAlphaDelay(float alpha, float delay)
        {
            yield return new WaitForSeconds(delay); //至少延迟一帧
            UICanvasGroup.alpha = alpha;

            if (alpha <= 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 查找UI事件监听器
        /// </summary>
        /// <param name="UIName"></param>
        /// <returns></returns>
        public UIEventListener GetUIEventListener(string UIName)
        {
            if (!UIEventDic.ContainsKey(UIName))
            {
                UIEventListener uiEventListener = UIEventListener.GetListener(this.transform.FindChildByName(UIName));
                UIEventDic.Add(UIName, uiEventListener);
                return uiEventListener;
            }
            else
            {
                return UIEventDic[UIName];
            }
        }


    }
}
