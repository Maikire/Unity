using UnityEngine.EventSystems;

namespace UI
{
    public interface IPointerPressHandler : IEventSystemHandler
    {
        /// <summary>
        /// 长按按钮
        /// </summary>
        /// <param name="eventData">PointerEventData</param>
        void OnPointerPress(PointerEventData eventData);
    }
}
