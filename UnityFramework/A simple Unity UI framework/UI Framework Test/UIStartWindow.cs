using UnityEngine.EventSystems;

namespace Default
{
    /// <summary>
    /// 游戏开始界面
    /// </summary>
    public class UIStartWindow : UIWindow
    {
        //注册UI交互事件
        private void Start()
        {
            GetUIEventListener("Start").PointerClick.AddListener(OnStartButtonClick);
            GetUIEventListener("Setting").PointerClick.AddListener(OnSettingButtonClick);
        }

        /// <summary>
        /// 按下 Start 按钮
        /// </summary>
        private void OnStartButtonClick(PointerEventData eventData)
        {
            SetVisibleAlpha(1, 0, 0.2f);
        }

        /// <summary>
        /// 进入设置面板
        /// </summary>
        private void OnSettingButtonClick(PointerEventData eventData)
        {
            UIManager.Instance.GetWindow<UISettingWindow>().SetVisible(true);
        }


    }
}
