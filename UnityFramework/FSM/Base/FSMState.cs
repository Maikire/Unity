using System.Collections.Generic;

namespace AI.FSM
{
    /// <summary>
    /// FSM状态类
    /// </summary>
    public abstract class FSMState
    {
        /// <summary>
        /// StateID
        /// 子类必须给StateID赋值
        /// </summary>
        public abstract FSMStateID StateID { get; }
        /// <summary>
        /// true: 搜索目标的时候忽略墙体
        /// </summary>
        public virtual bool IgnoreWalls { get => false; }
        
        /// <summary>
        /// 条件列表
        /// </summary>
        private List<FSMTrigger> Triggers;
        /// <summary>
        /// 映射表：条件 —— 目标状态
        /// </summary>
        private Dictionary<FSMTriggerID, FSMStateID> Map;

        public FSMState()
        {
            Map = new Dictionary<FSMTriggerID, FSMStateID>();
            Triggers = new List<FSMTrigger>();
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// 添加映射、添加FSMTrigger对象
        /// 由状态机调用
        /// </summary>
        /// <param name="triggerID"></param>
        /// <param name="stateID"></param>
        public void Add(FSMTriggerID triggerID, FSMStateID stateID)
        {
            Map.Add(triggerID, stateID);
            Triggers.Add(FSMCreateFactory.CreateTrigger(triggerID));
        }

        /// <summary>
        /// 判断当前状态的条件是否满足
        /// 如果条件满足，则切换状态
        /// </summary>
        public void JudgeState(FSMBase fsm)
        {
            foreach (FSMTrigger trigger in Triggers)
            {
                if (trigger.HandleTrigger(fsm))
                {
                    fsm.ChangeActiveState(Map[trigger.TriggerID]);
                    return;
                }
            }
        }

        /// <summary>
        /// 进入状态
        /// </summary>
        /// <param name="fsm">状态机</param>
        public virtual void EnterState(FSMBase fsm) { }

        /// <summary>
        /// 在状态中
        /// </summary>
        /// <param name="fsm">状态机</param>
        public virtual void ActionState(FSMBase fsm) { }

        /// <summary>
        /// 退出状态
        /// </summary>
        /// <param name="fsm">状态机</param>
        public virtual void ExitState(FSMBase fsm) { }


    }
}
