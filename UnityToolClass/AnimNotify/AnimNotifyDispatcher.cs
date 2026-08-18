using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 对 Animator State 的时间轴进行求值并分发通知
    /// 播放游标、活动 NotifyState 和队列都由 Animator 实例独占
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class AnimNotifyDispatcher : MonoBehaviour
    {
        /// <summary>
        /// 用于动画时间比较的浮点容差
        /// </summary>
        private const float TimeTolerance = 0.00001f;

        /// <summary>
        /// 单次求值允许处理的最大动画循环数量
        /// </summary>
        private const int MaxLoopsPerEvaluation = 64;

        /// <summary>
        /// 保存各动画状态时间轴的运行时播放数据
        /// </summary>
        private readonly Dictionary<PlaybackKey, PlaybackState> playbackStates = new();

        /// <summary>
        /// 保存等待在 LateUpdate 阶段发送的非关键通知
        /// </summary>
        private readonly List<AnimNotifyContext> queuedNotifications = new();

        /// <summary>
        /// 保存当前求值区间内跨过的通知事件
        /// </summary>
        private readonly List<ScheduledEvent> scheduledEvents = new();

        /// <summary>
        /// 保存当前组件所属的 Animator
        /// </summary>
        private Animator animator;

        /// <summary>
        /// 当瞬时通知或 NotifyState 生命周期阶段被触发时调用
        /// </summary>
        public event Action<AnimNotifyContext> Notification;

        /// <summary>
        /// 缓存当前组件所属的 Animator
        /// </summary>
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 在本帧动画状态完成求值后发送排队通知
        /// </summary>
        private void LateUpdate()
        {
            FlushQueuedNotifications();
        }

        /// <summary>
        /// 组件禁用时发送剩余通知并清理全部运行时状态
        /// </summary>
        private void OnDisable()
        {
            FlushQueuedNotifications();
            EndAllActiveStatesImmediately();
            playbackStates.Clear();
            queuedNotifications.Clear();
            scheduledEvents.Clear();
        }

        /// <summary>
        /// 获取 Animator 上的分发器，不存在时自动添加
        /// </summary>
        public static AnimNotifyDispatcher GetOrAdd(Animator targetAnimator)
        {
            if (targetAnimator == null)
            {
                throw new ArgumentNullException(nameof(targetAnimator));
            }

            if (!targetAnimator.TryGetComponent(out AnimNotifyDispatcher dispatcher))
            {
                dispatcher = targetAnimator.gameObject.AddComponent<AnimNotifyDispatcher>();
            }

            return dispatcher;
        }

        /// <summary>
        /// 立即发送当前排队的非关键通知
        /// </summary>
        public void FlushQueuedNotifications()
        {
            if (queuedNotifications.Count == 0)
            {
                return;
            }

            AnimNotifyContext[] snapshot = queuedNotifications.ToArray();
            queuedNotifications.Clear();
            foreach (AnimNotifyContext context in snapshot)
            {
                Notification?.Invoke(context);
            }
        }

        /// <summary>
        /// 初始化指定 Animator State 的通知播放状态
        /// </summary>
        internal void EnterState(AnimNotifyStateMachineBehaviour source, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (source == null)
            {
                return;
            }

            PlaybackKey key = new(source.GetInstanceID(), layerIndex);
            if (playbackStates.TryGetValue(key, out PlaybackState previous))
            {
                previous.IsActive = false;
                EndActiveStates(previous, stateInfo, layerIndex, true);
            }

            float currentTime = GetEvaluationTime(stateInfo);
            PlaybackState playback = new(source, stateInfo, currentTime);
            playbackStates[key] = playback;
            InitializeAtCurrentTime(playback, stateInfo, layerIndex, currentTime);
        }

        /// <summary>
        /// 根据当前播放时间求值指定 Animator State 的通知时间轴
        /// </summary>
        internal void EvaluateState(AnimNotifyStateMachineBehaviour source, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (source == null)
            {
                return;
            }

            PlaybackKey key = new(source.GetInstanceID(), layerIndex);
            if (!playbackStates.TryGetValue(key, out PlaybackState playback))
            {
                EnterState(source, stateInfo, layerIndex);
                return;
            }

            // 如果当前播放时间小于上一次求值时间，说明动画回退或跳转，直接结束所有活动持续通知并重新初始化
            float currentTime = GetEvaluationTime(stateInfo);
            if (currentTime + TimeTolerance < playback.PreviousNormalizedTime)
            {
                EndActiveStates(playback, stateInfo, layerIndex, true);
                if (!playback.IsActive)
                {
                    return;
                }

                playback.ActiveStates.Clear();
                playback.PreviousNormalizedTime = currentTime;
                playback.StateInfo = stateInfo;
                InitializeAtCurrentTime(playback, stateInfo, layerIndex, currentTime);
                return;
            }

            ScheduleCrossedEvents(playback, playback.PreviousNormalizedTime, currentTime, stateInfo.loop);
            DispatchScheduledEvents(playback, stateInfo, layerIndex, currentTime);
            if (!playback.IsActive)
            {
                return;
            }

            TickActiveStates(playback, stateInfo, layerIndex, currentTime);
            if (!playback.IsActive)
            {
                return;
            }

            playback.PreviousNormalizedTime = currentTime;
            playback.StateInfo = stateInfo;
        }

        /// <summary>
        /// 结束指定 Animator State 的通知播放状态
        /// </summary>
        internal void ExitState(AnimNotifyStateMachineBehaviour source, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (source == null)
            {
                return;
            }

            PlaybackKey key = new(source.GetInstanceID(), layerIndex);
            if (!playbackStates.TryGetValue(key, out PlaybackState playback))
            {
                return;
            }

            playback.IsActive = false;
            EndActiveStates(playback, stateInfo, layerIndex, false);
            playbackStates.Remove(key);
        }

        /// <summary>
        /// 获取用于通知求值的安全归一化播放时间
        /// </summary>
        private static float GetEvaluationTime(AnimatorStateInfo stateInfo)
        {
            float time = stateInfo.normalizedTime;
            if (float.IsNaN(time) || float.IsInfinity(time))
            {
                return 0f;
            }

            return stateInfo.loop ? Mathf.Max(0f, time) : Mathf.Clamp01(time);
        }

        /// <summary>
        /// 根据状态进入时的播放位置初始化瞬时通知和活动持续通知
        /// </summary>
        private void InitializeAtCurrentTime(PlaybackState playback, AnimatorStateInfo stateInfo, int layerIndex, float currentTime)
        {
            int loopIndex = Mathf.FloorToInt(currentTime);
            float localTime = currentTime - loopIndex;
            if (!stateInfo.loop && Mathf.Approximately(currentTime, 1f))
            {
                loopIndex = 0;
                localTime = 1f;
            }

            // 初始化瞬时通知
            IReadOnlyList<AnimNotify> notifies = playback.Source.Notifies;
            for (int index = 0; index < notifies.Count; index++)
            {
                AnimNotify notify = notifies[index];
                if (notify != null && Mathf.Abs(notify.NormalizedTime - localTime) <= TimeTolerance)
                {
                    Emit(new AnimNotifyContext(notify, AnimNotifyPhase.Notify, animator, stateInfo, layerIndex, currentTime));

                    if (!playback.IsActive)
                    {
                        return;
                    }
                }
            }

            // 初始化活动持续通知
            IReadOnlyList<AnimNotifyState> notifyStates = playback.Source.NotifyStates;
            for (int index = 0; index < notifyStates.Count; index++)
            {
                AnimNotifyState notifyState = notifyStates[index];
                if (notifyState == null || notifyState.Duration <= TimeTolerance)
                {
                    continue;
                }

                if (localTime + TimeTolerance < notifyState.NormalizedTime
                    || localTime >= notifyState.EndNormalizedTime - TimeTolerance)
                {
                    continue;
                }

                StateOccurrence occurrence = new(index, loopIndex);
                playback.ActiveStates.Add(occurrence, notifyState);

                Emit(new AnimNotifyContext(notifyState, AnimNotifyPhase.StateBegin, animator, stateInfo, layerIndex, currentTime));

                if (!playback.IsActive)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 收集指定播放区间内跨过的全部通知事件
        /// </summary>
        private void ScheduleCrossedEvents(PlaybackState playback, float previousTime, float currentTime, bool isLooping)
        {
            scheduledEvents.Clear();
            if (currentTime <= previousTime + TimeTolerance)
            {
                return;
            }

            int firstLoop = Mathf.Max(0, Mathf.FloorToInt(previousTime));
            int lastLoop = Mathf.Max(firstLoop, Mathf.FloorToInt(currentTime));
            if (!isLooping)
            {
                firstLoop = 0;
                lastLoop = 0;
            }
            if (lastLoop - firstLoop + 1 > MaxLoopsPerEvaluation)
            {
                firstLoop = lastLoop - MaxLoopsPerEvaluation + 1;
            }

            // 收集跨过的瞬时通知和持续通知的开始/结束事件
            for (int loopIndex = firstLoop; loopIndex <= lastLoop; loopIndex++)
            {
                IReadOnlyList<AnimNotify> notifies = playback.Source.Notifies;
                for (int index = 0; index < notifies.Count; index++)
                {
                    AnimNotify notify = notifies[index];
                    if (notify == null)
                    {
                        continue;
                    }

                    float eventTime = loopIndex + notify.NormalizedTime;
                    if (WasCrossed(eventTime, previousTime, currentTime))
                    {
                        scheduledEvents.Add(
                            new ScheduledEvent(eventTime, AnimNotifyPhase.Notify, notify, index, loopIndex));
                    }
                }

                // 收集跨过的持续通知的开始/结束事件
                IReadOnlyList<AnimNotifyState> notifyStates = playback.Source.NotifyStates;
                for (int index = 0; index < notifyStates.Count; index++)
                {
                    AnimNotifyState notifyState = notifyStates[index];
                    if (notifyState == null || notifyState.Duration <= TimeTolerance)
                    {
                        continue;
                    }

                    float beginTime = loopIndex + notifyState.NormalizedTime;
                    if (WasCrossed(beginTime, previousTime, currentTime))
                    {
                        scheduledEvents.Add(
                            new ScheduledEvent(beginTime, AnimNotifyPhase.StateBegin, notifyState, index, loopIndex));
                    }

                    float endTime = loopIndex + notifyState.EndNormalizedTime;
                    if (WasCrossed(endTime, previousTime, currentTime))
                    {
                        scheduledEvents.Add(
                            new ScheduledEvent(endTime, AnimNotifyPhase.StateEnd, notifyState, index, loopIndex));
                    }
                }
            }

            scheduledEvents.Sort(ScheduledEvent.Compare);
        }

        /// <summary>
        /// 判断指定通知时间是否位于本次播放求值区间内
        /// </summary>
        private static bool WasCrossed(float eventTime, float previousTime, float currentTime)
        {
            return eventTime > previousTime + TimeTolerance
                && eventTime <= currentTime + TimeTolerance;
        }

        /// <summary>
        /// 按时间顺序发送当前收集的通知事件
        /// </summary>
        private void DispatchScheduledEvents(PlaybackState playback, AnimatorStateInfo stateInfo, int layerIndex, float currentTime)
        {
            ScheduledEvent[] snapshot = scheduledEvents.ToArray();
            foreach (ScheduledEvent scheduledEvent in snapshot)
            {
                if (scheduledEvent.Phase == AnimNotifyPhase.StateBegin)
                {
                    StateOccurrence occurrence = new(scheduledEvent.NotifyIndex, scheduledEvent.LoopIndex);
                    if (playback.ActiveStates.ContainsKey(occurrence))
                    {
                        continue;
                    }

                    AnimNotifyState notifyState = (AnimNotifyState)scheduledEvent.Notify;
                    playback.ActiveStates.Add(occurrence, notifyState);
                }
                else if (scheduledEvent.Phase == AnimNotifyPhase.StateEnd)
                {
                    StateOccurrence occurrence = new(scheduledEvent.NotifyIndex, scheduledEvent.LoopIndex);
                    if (!playback.ActiveStates.Remove(occurrence))
                    {
                        continue;
                    }
                }

                Emit(new AnimNotifyContext(scheduledEvent.Notify, scheduledEvent.Phase, animator, stateInfo, layerIndex, currentTime));

                if (!playback.IsActive)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 更新当前仍处于活动窗口内的持续通知
        /// </summary>
        private void TickActiveStates(PlaybackState playback, AnimatorStateInfo stateInfo, int layerIndex, float currentTime)
        {
            if (playback.ActiveStates.Count == 0)
            {
                return;
            }

            AnimNotifyState[] snapshot = new AnimNotifyState[playback.ActiveStates.Count];
            playback.ActiveStates.Values.CopyTo(snapshot, 0);
            foreach (AnimNotifyState notifyState in snapshot)
            {
                Emit(new AnimNotifyContext(notifyState, AnimNotifyPhase.StateTick, animator, stateInfo, layerIndex, currentTime));

                if (!playback.IsActive)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 结束指定播放状态中的全部活动持续通知
        /// </summary>
        private void EndActiveStates(PlaybackState playback, AnimatorStateInfo stateInfo, int layerIndex, bool forceImmediate)
        {
            if (playback.ActiveStates.Count == 0)
            {
                return;
            }

            AnimNotifyState[] snapshot = new AnimNotifyState[playback.ActiveStates.Count];
            playback.ActiveStates.Values.CopyTo(snapshot, 0);
            playback.ActiveStates.Clear();
            float currentTime = GetEvaluationTime(stateInfo);
            foreach (AnimNotifyState notifyState in snapshot)
            {
                AnimNotifyContext context =
                    new(notifyState, AnimNotifyPhase.StateEnd, animator, stateInfo, layerIndex, currentTime);

                if (forceImmediate)
                {
                    DispatchImmediately(context);
                }
                else
                {
                    Emit(context);
                }
            }
        }

        /// <summary>
        /// 立即结束分发器中的全部活动持续通知
        /// </summary>
        private void EndAllActiveStatesImmediately()
        {
            List<KeyValuePair<PlaybackKey, PlaybackState>> snapshot = new(playbackStates);
            foreach (KeyValuePair<PlaybackKey, PlaybackState> pair in snapshot)
            {
                PlaybackState playback = pair.Value;
                playback.IsActive = false;
                EndActiveStates(playback, playback.StateInfo, pair.Key.LayerIndex, true);
            }
        }

        /// <summary>
        /// 根据通知执行方式立即发送或加入待发送队列
        /// </summary>
        private void Emit(AnimNotifyContext context)
        {
            if (context.Notify == null || string.IsNullOrWhiteSpace(context.EventName))
            {
                return;
            }

            if (context.IsBranchingPoint)
            {
                DispatchImmediately(context);
                return;
            }

            queuedNotifications.Add(context);
        }

        /// <summary>
        /// 立即发送有效的通知上下文
        /// </summary>
        private void DispatchImmediately(AnimNotifyContext context)
        {
            if (context.Notify != null && !string.IsNullOrWhiteSpace(context.EventName))
            {
                Notification?.Invoke(context);
            }
        }

    }
}
