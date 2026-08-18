using UnityEngine;

namespace Common
{
    /// <summary>
    /// 通知执行时临时创建的只读上下文
    /// </summary>
    public readonly struct AnimNotifyContext
    {
        /// <summary>
        /// 获取产生本次回调的无状态通知配置
        /// </summary>
        public AnimNotify Notify { get; }

        /// <summary>
        /// 获取本次回调的生命周期阶段
        /// </summary>
        public AnimNotifyPhase Phase { get; }

        /// <summary>
        /// 获取正在播放动画的 Animator
        /// </summary>
        public Animator Animator { get; }

        /// <summary>
        /// 获取回调时的动画状态信息
        /// </summary>
        public AnimatorStateInfo StateInfo { get; }

        /// <summary>
        /// 获取 Animator 层索引
        /// </summary>
        public int LayerIndex { get; }

        /// <summary>
        /// 获取回调发生时包含循环次数的归一化播放时间
        /// </summary>
        public float NormalizedTime { get; }

        /// <summary>
        /// 获取通知名称
        /// </summary>
        public string EventName => Notify?.EventName ?? string.Empty;

        /// <summary>
        /// 获取通知携带的数值强度
        /// </summary>
        public float EventMagnitude => Notify?.EventMagnitude ?? 0f;

        /// <summary>
        /// 获取本次通知是否为立即执行的 Branching Point
        /// </summary>
        public bool IsBranchingPoint => Notify?.TickType == AnimNotifyTickType.BranchingPoint;

        /// <summary>
        /// 初始化动画通知上下文
        /// </summary>
        internal AnimNotifyContext(
            AnimNotify notify,
            AnimNotifyPhase phase,
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex,
            float normalizedTime)
        {
            Notify = notify;
            Phase = phase;
            Animator = animator;
            StateInfo = stateInfo;
            LayerIndex = layerIndex;
            NormalizedTime = normalizedTime;
        }

    }
}
