using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]

    /// <summary>
    /// 所有窗口的父类
    /// </summary>
    public abstract class UIWindow : MonoBehaviour
    {
        /// <summary>
        /// 画布组
        /// </summary>
        protected CanvasGroup UICanvasGroup;
        /// <summary>
        /// 存储UI事件监听器
        /// </summary>
        private Dictionary<string, UIEventListener> UIEventDic;
        /// <summary>
        /// true: 是基础的UI画布（打开场景时基础UI画布不会被隐藏，其他UI画布会被隐藏）
        /// </summary>
        public bool IsBasicUICanvas { get; protected set; }

        public virtual void Awake()
        {
            UICanvasGroup = this.GetComponent<CanvasGroup>();
            UIEventDic = new Dictionary<string, UIEventListener>();
        }

        /// <summary>
        /// 设置UI的可见性
        /// </summary>
        /// <param name="state">true：设置为可见</param>
        public void SetVisible(bool state)
        {
            this.gameObject.SetActive(state);
            if (state)
            {
                UIController.Instance.UICanvasStack.Push(this);
            }
            else
            {
                UIController.Instance.UICanvasStack.Pop();
            }
        }

        /// <summary>
        /// 在一定时间内（秒），将UI的透明度从指定的值平滑过度到另一个指定的值
        /// </summary>
        /// <param name="sourceAlpha">起始alpha</param>
        /// <param name="targetAlpha">目标alpha</param>
        /// <param name="time">时间范围（秒）</param>
        /// <returns></returns>
        public void SetVisibleAlpha(float sourceAlpha, float targetAlpha, float time)
        {
            StartCoroutine(SetVisibleAlphaDelay(sourceAlpha, targetAlpha, time));
        }

        /// <summary>
        /// 在一定时间内（秒），将UI的透明度从指定的值平滑过度到另一个指定的值
        /// </summary>
        /// <param name="sourceAlpha">起始alpha</param>
        /// <param name="targetAlpha">目标alpha</param>
        /// <param name="time">时间范围（秒）</param>
        /// <returns></returns>
        private IEnumerator SetVisibleAlphaDelay(float sourceAlpha, float targetAlpha, float time)
        {
            UICanvasGroup.alpha = sourceAlpha;
            float difference = targetAlpha - UICanvasGroup.alpha;

            if (!this.gameObject.activeSelf && targetAlpha > 0)
            {
                SetVisible(true);
            }

            while (true)
            {
                yield return new WaitForSeconds(0.02f);

                UICanvasGroup.alpha += difference / (time * 50);

                if (UICanvasGroup.alpha <= 0)
                {
                    SetVisible(false);
                    yield break;
                }
                else if (UICanvasGroup.alpha == targetAlpha)
                {
                    yield break;
                }
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
                Transform item = this.transform.FindChildByName(UIName);
                if (item == null)
                {
                    throw new Exception($"UI Name \"{UIName}\" not found");
                }

                UIEventListener uiEventListener = UIEventListener.GetListener(item);
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

