using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Default.UI
{
    [RequireComponent(typeof(PointerPressManger))]
    /// <summary>
    /// UI事件监听器，提供所有能用到的UGUI事件（带事件参数类）。
    /// 脚本添加到需要交互的UI
    /// </summary>
    public class UIEventListener : MonoBehaviour, IPointerPressHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler, IEventSystemHandler
    {
        [Serializable]
        public class PointerEventHandler : UnityEvent<PointerEventData> { }

        [Serializable]
        public class BaseEventHandler : UnityEvent<BaseEventData> { }

        [Serializable]
        public class AxisEventHandler : UnityEvent<AxisEventData> { }

        //使用了 C# 9 新引入的 Target-typed new 表达式语法。省略对构造函数的显式调用
        public PointerEventHandler onPointerPress = new();
        public PointerEventHandler onPointerClick = new();
        public PointerEventHandler onPointerDown = new();
        public PointerEventHandler onPointerUp = new();
        public PointerEventHandler onPointerEnter = new();
        public PointerEventHandler onPointerExit = new();
        public PointerEventHandler onInitializePotentialDrag = new();
        public PointerEventHandler onBeginDrag = new();
        public PointerEventHandler onDrag = new();
        public PointerEventHandler onEndDrag = new();
        public PointerEventHandler onDrop = new();
        public PointerEventHandler onScroll = new();
        public BaseEventHandler onUpdateSelected = new();
        public BaseEventHandler onSelect = new();
        public BaseEventHandler onDeselect = new();
        public BaseEventHandler onSubmit = new();
        public BaseEventHandler onCancel = new();
        public AxisEventHandler onMove = new();

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

        public void OnPointerPress(PointerEventData eventData)
        {
            onPointerPress?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onPointerClick?.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(eventData);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            onInitializePotentialDrag?.Invoke(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            onDrop?.Invoke(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            onScroll?.Invoke(eventData);
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            onUpdateSelected?.Invoke(eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            onSelect?.Invoke(eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            onDeselect?.Invoke(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            onSubmit?.Invoke(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            onCancel?.Invoke(eventData);
        }

        public void OnMove(AxisEventData eventData)
        {
            onMove?.Invoke(eventData);
        }

    }
}
