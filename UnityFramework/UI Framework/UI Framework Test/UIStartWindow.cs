using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UIFramework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Default
{
    /// <summary>
    /// 游戏开始界面
    /// </summary>
    public class UIStartWindow : UIWindow
    {
        //游戏开始时，注册UI交互事件
        private void Start()
        {
            GetUIEventListener("Start").PointerClick += OnStartButtonClick;
            GetUIEventListener("Setting").PointerClick += OnSettingButtonClick;
            
            //使用Unity事件
            //GetUIEventListener("Start").PointerClick.AddListener(OnBaseButtonClick);
        }

        /// <summary>
        /// 提供当前面板负责的交互行为
        /// </summary>
        private void OnStartButtonClick(PointerEventData eventData)
        {
            UIController.Instance.GameStart();

            //双击
            //if (eventData.clickCount == 2)
            //{
            //    UIController.Instance.GameStart();
            //}

            //拿到按钮的引用
            //print(eventData.pointerPress);
        }

        /// <summary>
        /// 进入设置面板
        /// </summary>
        public void OnSettingButtonClick(PointerEventData eventData)
        {
            UIManager.Instance.GetWindow<UISettingWindow>().SetVisible(true);
        }


    }
}
