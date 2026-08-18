using System;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 动画状态时间轴上的持续通知，依次产生 Begin、Tick 和 End 阶段
    /// </summary>
    [Serializable]
    public sealed class AnimNotifyState : AnimNotify
    {
        [Tooltip("通知窗口的归一化持续时间，窗口不会超过当前动画循环")]
        [Range(0f, 1f)]
        [SerializeField]
        private float duration = 0.1f;

        /// <summary>
        /// 获取归一化持续时间
        /// </summary>
        public float Duration => Mathf.Clamp(duration, 0f, 1f - NormalizedTime);

        /// <summary>
        /// 获取窗口结束的归一化时间
        /// </summary>
        public float EndNormalizedTime => NormalizedTime + Duration;

    }
}
