using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.UI
{
    /// <summary>
    /// 处理游戏流程（与UI相关的）
    /// </summary>
    public class UIController : MonoSingleton<UIController>
    {
        public override void Init()
        {
            base.Init();
            BeforeGameStart();
        }

        private void BeforeGameStart()
        {
            UIManager.Instance.GetWindow<UIPlayCanvas>().SetVisible(true);
        }

        public void GameStart()
        {

        }


    }
}
