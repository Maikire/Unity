using Common;
using System.Collections.Generic;

namespace Default
{
    /// <summary>
    /// 处理UI流程
    /// </summary>
    public class UIController : MonoSingleton<UIController>
    {
        /// <summary>
        /// 将打开的UI入栈
        /// </summary>
        public Stack<UIWindow> UICanvasStack;

        private void Start()
        {
            UICanvasStack = new Stack<UIWindow>();
            BeforeGameStart();
        }

        private void BeforeGameStart()
        {
            UIManager.Instance.GetWindow<UIStartWindow>().SetVisible(true);
        }

        /// <summary>
        /// 返回上一层UI
        /// 如果没有上一层UI，就打开设置面板
        /// </summary>
        public void Back()
        {
            if (UICanvasStack.Count == 0 || UICanvasStack.Peek().GetType() == typeof(UIStartWindow))
            {
                UIManager.Instance.GetWindow<UISettingWindow>().SetVisible(true);
            }
            else
            {
                UICanvasStack.Peek().SetVisible(false);
            }
        }


    }
}
