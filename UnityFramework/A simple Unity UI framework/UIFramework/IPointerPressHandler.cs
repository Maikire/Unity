using UnityEngine.EventSystems;

namespace Default
{
    public interface IPointerPressHandler : IEventSystemHandler
    {
        /// <summary>
        /// 长按按钮
        /// </summary>
        /// <param name="eventData">PointerEventData</param>
        /// <param name="pressTime">按下按钮的时间</param>
        void OnPointerPress(PointerEventData eventData);
    }
}
