using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// 设置面板中的一个选项的面板
    /// </summary>
    public class UISettingWindow_1 : UIWindow
    {
        private void Start()
        {
            GetUIEventListener("Exit").PointerClick.AddListener(OnExitButtonClick);
        }

        /// <summary>
        /// 退出画布
        /// </summary>
        private void OnExitButtonClick(PointerEventData eventData)
        {
            this.SetVisible(false);
        }


    }
}
