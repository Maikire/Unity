namespace AI.FSM
{
    /// <summary>
    /// FSM条件类
    /// </summary>
    public abstract class FSMTrigger
    {
        /// <summary>
        /// TriggerID
        /// </summary>
        public FSMTriggerID TriggerID;

        public FSMTrigger()
        {
            Init();
        }

        /// <summary>
        /// 子类必须初始化（给TriggerID赋值）
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 是否满足条件
        /// </summary>
        /// <returns></returns>
        public abstract bool HandleTrigger(FSMBase fsm);


    }
}
