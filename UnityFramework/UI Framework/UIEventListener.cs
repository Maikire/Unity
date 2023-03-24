using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIFramework
{
    public delegate void PointerEventHandler(PointerEventData eventData);

    /// <summary>
    /// UI事件监听器，提供所有能用到的UGUI事件（带事件参数类）。
    /// 脚本添加到需要交互的UI
    /// </summary>
    public class UIEventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event PointerEventHandler PointerClick;
        public event PointerEventHandler PointerDown;
        public event PointerEventHandler PointerUp;
        public event PointerEventHandler PointerEnter;
        public event PointerEventHandler PointerExit;

        /// <summary>
        /// 获取UI事件监听器
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static UIEventListener GetListener(Transform transform)
        {
            UIEventListener uiEventListener = transform.GetComponent<UIEventListener>();
            if (uiEventListener == null)
            {
                uiEventListener = transform.AddComponent<UIEventListener>();
            }
            return uiEventListener;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClick?.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUp?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExit?.Invoke(eventData);
        }


    }
}
