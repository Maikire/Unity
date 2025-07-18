using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// 游戏设置界面
    /// </summary>
    public class UISettingWindow : UIWindow
    {
        private void Start()
        {
            GetUIEventListener("Exit").PointerClick.AddListener(OnExitButtonClick);
            GetUIEventListener("Setting_1").PointerClick.AddListener(OnSetting_1ButtonClick);
        }

        /// <summary>
        /// 退出画布
        /// </summary>
        private void OnExitButtonClick(PointerEventData eventData)
        {
            this.SetVisible(false);
        }

        /// <summary>
        /// 打开 Setting_1 
        /// </summary>
        /// <param name="eventData"></param>
        private void OnSetting_1ButtonClick(PointerEventData eventData)
        {
            UIManager.Instance.GetWindow<UISettingWindow_1>().SetVisible(true);
        }

    }
}
