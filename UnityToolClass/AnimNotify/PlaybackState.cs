using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 保存单个通知时间轴的运行时播放状态
    /// </summary>
    internal sealed class PlaybackState
    {
        /// <summary>
        /// 获取提供通知配置的 Behaviour
        /// </summary>
        public AnimNotifyStateMachineBehaviour Source { get; }

        /// <summary>
        /// 获取当前活动的持续通知集合
        /// </summary>
        public Dictionary<StateOccurrence, AnimNotifyState> ActiveStates { get; } = new();

        /// <summary>
        /// 获取或设置最近一次求值时的 Animator 状态信息
        /// </summary>
        public AnimatorStateInfo StateInfo { get; set; }

        /// <summary>
        /// 获取或设置上一次求值时的归一化播放时间
        /// </summary>
        public float PreviousNormalizedTime { get; set; }

        /// <summary>
        /// 获取或设置当前播放状态是否仍然有效
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 初始化通知时间轴的运行时播放状态
        /// </summary>
        public PlaybackState(
            AnimNotifyStateMachineBehaviour source,
            AnimatorStateInfo stateInfo,
            float previousNormalizedTime)
        {
            Source = source;
            StateInfo = stateInfo;
            PreviousNormalizedTime = previousNormalizedTime;
        }

    }
}
