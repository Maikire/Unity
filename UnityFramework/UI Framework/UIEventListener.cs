using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIFramework
{
    public delegate void PointerEventHandler(PointerEventData eventData);
    public delegate void BaseEventHandler(BaseEventData eventData);
    public delegate void AxisEventHandler(AxisEventData eventData);

    /// <summary>
    /// UI事件监听器，提供所有能用到的UGUI事件（带事件参数类）。
    /// 脚本添加到需要交互的UI
    /// </summary>
    public class UIEventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler, IEventSystemHandler
    {
        public event PointerEventHandler PointerClick;
        public event PointerEventHandler PointerDown;
        public event PointerEventHandler PointerUp;
        public event PointerEventHandler PointerEnter;
        public event PointerEventHandler PointerExit;
        public event PointerEventHandler InitializePotentialDrag;
        public event PointerEventHandler BeginDrag;
        public event PointerEventHandler Drag;
        public event PointerEventHandler EndDrag;
        public event PointerEventHandler Drop;
        public event PointerEventHandler Scroll;
        public event BaseEventHandler UpdateSelected;
        public event BaseEventHandler Select;
        public event BaseEventHandler Deselect;
        public event BaseEventHandler Submit;
        public event BaseEventHandler Cancel;
        public event AxisEventHandler Move;

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
