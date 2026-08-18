using System;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 动画状态时间轴上的瞬时通知
    /// 只保存编辑期数据，播放游标等运行时状态由 AnimNotifyDispatcher 保存
    /// </summary>
    [Serializable]
    public class AnimNotify
    {
        [Tooltip("通知名称，接入 GAS 时应填写已注册的 GameplayTag 完整名称")]
        [SerializeField]
        private string eventName;

        [Tooltip("通知在动画状态内的归一化时间，0 表示开始，1 表示结束")]
        [Range(0f, 1f)]
        [SerializeField]
        private float normalizedTime;

        [Tooltip("随通知传递的数值强度")]
        [SerializeField]
        private float eventMagnitude = 1f;

        [Tooltip("关键玩法通知应使用 Branching Point，以便跨过触发点时立即执行")]
        [SerializeField]
        private AnimNotifyTickType tickType;

        /// <summary>
        /// 获取通知名称
        /// </summary>
        public string EventName => eventName ?? string.Empty;

        /// <summary>
        /// 获取动画状态内的归一化触发时间
        /// </summary>
        public float NormalizedTime => Mathf.Clamp01(normalizedTime);

        /// <summary>
        /// 获取通知携带的数值强度
        /// </summary>
        public float EventMagnitude => eventMagnitude;

        /// <summary>
        /// 获取通知的执行方式
        /// </summary>
        public AnimNotifyTickType TickType => tickType;

    }
}
