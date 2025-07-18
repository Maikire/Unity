using Common;
using System.Collections.Generic;

namespace UI
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
        }

        /// <summary>
        /// 返回上一层UI
        /// 如果没有上一层UI，则什么都不做
        /// </summary>
        public void Back()
        {
            if (UICanvasStack.Count != 0)
            {
                UICanvasStack.Peek().SetVisible(false);
            }
        }


    }
}
