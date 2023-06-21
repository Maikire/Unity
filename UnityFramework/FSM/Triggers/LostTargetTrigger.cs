namespace AI.FSM
{
    /// <summary>
    /// 丢失目标
    /// </summary>
    public class LostTargetTrigger : FSMTrigger
    {
        public override FSMTriggerID TriggerID => FSMTriggerID.LostTarget;

        public override bool HandleTrigger(FSMBase fsm)
        {
            return fsm.FoundTarget == null;
        }


    }
}
