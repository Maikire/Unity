namespace AI.FSM
{
    /// <summary>
    /// 复活
    /// </summary>
    public class ReviveTrigger : FSMTrigger
    {
        public override void Init()
        {
            TriggerID = FSMTriggerID.Revive;
        }

        public override bool HandleTrigger(FSMBase fsm)
        {
            return fsm.Character.HP > 0;
        }


    }
}
