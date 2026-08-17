using System;
using System.Collections.Generic;

namespace Common
{
    /// <summary>
    /// 线程交叉访问助手
    /// </summary>
    public class ThreadCrossHelper : MonoSingleton<ThreadCrossHelper>
    {
        /// <summary>
        /// 行为信息类
        /// </summary>
        private class ActionDelayItem
        {
            /// <summary>
            /// 行为
            /// </summary>
            public Action Act;
            /// <summary>
            /// 延迟时间
            /// </summary>
            public DateTime DelayTime;
            /// <summary>
            ///true：可用
            /// </summary>
            public bool Enabled;

            /// <summary>
            /// 创建行为信息
            /// </summary>
            /// <param name="action"> 行为 & lt;/param>
            /// <param name="time"> 延迟时间 & lt;/param>
            public ActionDelayItem(Action action, DateTime time)
            {
                Act = action;
                DelayTime = time;
                Enabled = true;
            }

            /// <summary>
            /// 重置
            /// </summary>
            /// <param name="action"> 行为 & lt;/param>
            /// <param name="time"> 延迟时间 & lt;/param>
            public void Reset(Action action, DateTime time)
            {
                Act = action;
                DelayTime = time;
                Enabled = true;
            }
        }

        /// <summary>
        /// 行为列表
        /// </summary>
        private List<ActionDelayItem> ActionList;

        private void Start()
        {
            ActionList = new List<ActionDelayItem>();
        }

        private void Update()
        {
            for (int i = ActionList.Count - 1; i >= 0; i--)
            {
                if (ActionList[i].Enabled && DateTime.Now >= ActionList[i].DelayTime)
                {
                    ActionList[i].Act?.Invoke();
                    ActionList[i].Enabled = false;
                }
            }
        }

        /// <summary>
        /// 在主线程中调用指定的方法
        /// </summary>
        /// <param name="act"> 委托 & lt;/param>
        /// <param name="delay"> 延迟时间 & lt;/param>
        public void ExecuteOnMainThread(Action act, float delay = 0)
        {
            lock (ActionList)
            {
                ActionDelayItem temp = ActionList.Find((iteam) => { return !iteam.Enabled; });
                if (temp != null)
                {
                    temp.Reset(act, DateTime.Now.AddSeconds(delay));
                }
                else
                {
                    ActionList.Add(new ActionDelayItem(act, DateTime.Now.AddSeconds(delay)));
                }
            }
        }

    }
}
