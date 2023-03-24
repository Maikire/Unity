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
            //存在问题：Find方法在寻找子物体时会很麻烦，而且路径不能动态更改
            //解决：未知层级的情况下查找子物体的方法
            //this.transform.Find("Start").GetComponent<Button>().onClick.AddListener(OnStartButtonClick);

            //存在问题：
            //1.Button只有单击事件，而其他大多数事件（光标按下、光标抬起...）都不具备
            //2.Button只有单击事件，没有事件参数类（当多个按钮绑定同一个方法时，没有事件参数，就无法确定是哪个按钮被按下了）
            //解决：定义UI事件监听类，提供所有UGUI事件（带事件参数类）。脚本添加到需要交互的UI。
            //TransformHelper.FindChildByName(this.transform, "Start").GetComponent<Button>().onClick.AddListener(OnStartButtonClick);
            //this.transform.FindChildByName("Start").GetComponent<Button>().onClick.AddListener(OnStartButtonClick);

            //存在问题：
            //UI窗口查找UI事件监听器往往会有很多次
            //解决：获取UI事件监听器封装到父类（UIWindow）
            //this.transform.FindChildByName("Start").GetComponent<UIEventListener>().PointerClick += OnStartButtonClick;

            GetUIEventListener("Start").PointerClick += OnStartButtonClick;
            GetUIEventListener("Setting").PointerClick += OnSettingButtonClick;
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
