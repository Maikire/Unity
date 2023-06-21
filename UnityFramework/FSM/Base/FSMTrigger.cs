namespace AI.FSM
{
    /// <summary>
    /// FSM条件类
    /// </summary>
    public abstract class FSMTrigger
    {
        /// <summary>
        /// TriggerID
        /// 子类必须给TriggerID赋值
        /// </summary>
        public abstract FSMTriggerID TriggerID { get; }

        public FSMTrigger()
        {
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// 是否满足条件
        /// </summary>
        /// <returns></returns>
        public abstract bool HandleTrigger(FSMBase fsm);


    }
}
