using Common;
using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;

namespace Default
{
    /// <summary>
    /// 处理游戏流程（与UI相关的）
    /// </summary>
    public class UIController : GameController
    {
        protected override void BeforeGameStart()
        {
            UIManager.Instance.GetWindow<UIStartWindow>().SetVisible(true);
        }

        public override void GameStart()
        {
            UIManager.Instance.GetWindow<UIStartWindow>().SetVisible(false);
        }



    }
}
