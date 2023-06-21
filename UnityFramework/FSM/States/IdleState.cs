namespace AI.FSM
{
    /// <summary>
    /// 闲置状态
    /// </summary>
    public class IdleState : FSMState
    {
        public override FSMStateID StateID => FSMStateID.Idle;
        public override bool IgnoreWalls => false;

        public override void EnterState(FSMBase fsm)
        {
            base.EnterState(fsm);
            fsm.Anim.SetBool(fsm.Character.PlayerAnimationParameter.Revive, true);
            fsm.Anim.SetBool(fsm.Character.PlayerAnimationParameter.Idle, true);
        }


    }
}
