using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 配置在 Animator State 上的通知时间轴
    /// 只保存通知数据，全部运行时播放状态都位于 AnimNotifyDispatcher
    /// </summary>
    public sealed class AnimNotifyStateMachineBehaviour : StateMachineBehaviour
    {
        [Tooltip("瞬时动画通知")]
        [SerializeField]
        private List<AnimNotify> notifies = new();

        [Tooltip("具有 Begin、Tick、End 生命周期的持续动画通知")]
        [SerializeField]
        private List<AnimNotifyState> notifyStates = new();

        /// <summary>
        /// 获取瞬时通知的只读列表
        /// </summary>
        public IReadOnlyList<AnimNotify> Notifies => notifies;

        /// <summary>
        /// 获取持续通知的只读列表
        /// </summary>
        public IReadOnlyList<AnimNotifyState> NotifyStates => notifyStates;

        /// <summary>
        /// 在进入状态时调用
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimNotifyDispatcher.GetOrAdd(animator).EnterState(this, stateInfo, layerIndex);
        }

        /// <summary>
        /// 在状态更新时调用
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimNotifyDispatcher.GetOrAdd(animator).EvaluateState(this, stateInfo, layerIndex);
        }

        /// <summary>
        /// 在退出状态时调用
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator != null && animator.TryGetComponent(out AnimNotifyDispatcher dispatcher))
            {
                dispatcher.ExitState(this, stateInfo, layerIndex);
            }
        }

    }
}
