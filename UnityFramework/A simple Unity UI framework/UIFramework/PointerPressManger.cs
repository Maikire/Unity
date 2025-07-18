using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// 按下按钮时一直执行
    /// </summary>
    public class PointerPressManger : MonoBehaviour
    {
        private UIEventListener EventListener;
        private PointerEventData EventData;

        private void Awake()
        {
            EventListener = this.GetComponent<UIEventListener>();
        }

        private void Start()
        {
            EventListener.onPointerDown.AddListener(OnTurnOn);
            EventListener.onPointerUp.AddListener(OnTurnOff);
            this.enabled = false;
        }

        private void Update()
        {
            EventListener.OnPointerPress(EventData);
        }

        /// <summary>
        /// 开启循环调用事件
        /// </summary>
        /// <param name="eventData"></param>
        private void OnTurnOn(PointerEventData eventData)
        {
            EventData = eventData;
            this.enabled = true;
        }

        /// <summary>
        /// 关闭循环调用事件
        /// </summary>
        /// <param name="eventData"></param>
        private void OnTurnOff(PointerEventData eventData)
        {
            EventData = eventData;
            this.enabled = false;
        }

    }
}
