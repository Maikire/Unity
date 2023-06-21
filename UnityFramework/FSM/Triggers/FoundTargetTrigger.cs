namespace AI.FSM
{
    /// <summary>
    /// 发现目标
    /// </summary>
    public class FoundTargetTrigger : FSMTrigger
    {
        public override FSMTriggerID TriggerID => FSMTriggerID.FoundTarget;

        public override bool HandleTrigger(FSMBase fsm)
        {
            return fsm.FoundTarget != null;
        }


    }
}
