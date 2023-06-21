using ARPGDemo.Character;

namespace AI.FSM
{
    /// <summary>
    /// 击杀目标
    /// </summary>
    public class KilledTargetTrigger : FSMTrigger
    {
        public override FSMTriggerID TriggerID => FSMTriggerID.KilledTarget;

        public override bool HandleTrigger(FSMBase fsm)
        {
            return fsm.FoundTarget.GetComponent<CharacterStatus>().HP <= 0;
        }


    }
}
