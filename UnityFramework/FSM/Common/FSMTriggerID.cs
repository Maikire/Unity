namespace AI.FSM
{
    /// <summary>
    /// TriggerID
    /// </summary>
    public enum FSMTriggerID
    {
        /// <summary>
        /// 没有生命
        /// </summary>
        NoHealth,

        /// <summary>
        /// 发现目标
        /// </summary>
        FoundTarget,

        /// <summary>
        /// 到达目标
        /// </summary>
        ReachTarget,

        /// <summary>
        /// 击杀目标
        /// </summary>
        KilledTarget,

        /// <summary>
        /// 超出攻击范围
        /// </summary>
        OutOfAttackRange,

        /// <summary>
        /// 丢失目标
        /// </summary>
        LostTarget,

        /// <summary>
        /// 触发巡逻状态
        /// </summary>
        Patrol,

        /// <summary>
        /// 完成巡逻
        /// </summary>
        CompletePatrol,

        /// <summary>
        /// 复活
        /// </summary>
        Revive,

    }
}
