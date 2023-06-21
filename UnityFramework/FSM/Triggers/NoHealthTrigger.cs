namespace AI.FSM
{
    /// <summary>
    /// 没有生命
    /// </summary>
    public class NoHealthTrigger : FSMTrigger
    {
        public override FSMTriggerID TriggerID => FSMTriggerID.NoHealth;

        public override bool HandleTrigger(FSMBase fsm)
        {
            return fsm.Character.HP <= 0;
        }


    }
}
