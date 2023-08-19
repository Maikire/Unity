using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Default
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

        public PointerEventHandler PointerPress;
        public PointerEventHandler PointerClick;
        public PointerEventHandler PointerDown;
        public PointerEventHandler PointerUp;
        public PointerEventHandler PointerEnter;
        public PointerEventHandler PointerExit;
        public PointerEventHandler InitializePotentialDrag;
        public PointerEventHandler BeginDrag;
        public PointerEventHandler Drag;
        public PointerEventHandler EndDrag;
        public PointerEventHandler Drop;
        public PointerEventHandler Scroll;
        public BaseEventHandler UpdateSelected;
        public BaseEventHandler Select;
        public BaseEventHandler Deselect;
        public BaseEventHandler Submit;
        public BaseEventHandler Cancel;
        public AxisEventHandler Move;

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
            PointerPress?.Invoke(eventData);
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

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            InitializePotentialDrag?.Invoke(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDrag?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Drag?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDrag?.Invoke(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Drop?.Invoke(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            Scroll?.Invoke(eventData);
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            UpdateSelected?.Invoke(eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            Select?.Invoke(eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Deselect?.Invoke(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Submit?.Invoke(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            Cancel?.Invoke(eventData);
        }

        public void OnMove(AxisEventData eventData)
        {
            Move?.Invoke(eventData);
        }


    }
}
