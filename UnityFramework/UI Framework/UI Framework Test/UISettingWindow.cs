using Common;
using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Default
{
    /// <summary>
    /// 游戏设置界面
    /// </summary>
    public class UISettingWindow : UIWindow
    {
        private void Start()
        {
            GetUIEventListener("Exit").PointerClick += OnExitButtonClick;
        }

        /// <summary>
        /// 退出画布
        /// </summary>
        public void OnExitButtonClick(PointerEventData eventData)
        {
            this.SetVisible(false);
        }


    }
}
