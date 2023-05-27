namespace AI.FSM
{
    /// <summary>
    /// 闲置状态
    /// </summary>
    public class IdleState : FSMState
    {
        public override void Init()
        {
            StateID = FSMStateID.Idle;
            IgnoreWalls = false;
        }

        public override void EnterState(FSMBase fsm)
        {
            base.EnterState(fsm);
            fsm.Anim.SetBool(fsm.Character.PlayerAnimationParameter.Revive, true);
            fsm.Anim.SetBool(fsm.Character.PlayerAnimationParameter.Idle, true);
        }


    }
}
