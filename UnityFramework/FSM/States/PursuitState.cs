namespace AI.FSM
{
    /// <summary>
    /// 追逐状态
    /// </summary>
    public class PursuitState : FSMState
    {
        public override FSMStateID StateID => FSMStateID.Pursuit;
        public override bool IgnoreWalls => true;

        public override void EnterState(FSMBase fsm)
        {
            base.EnterState(fsm);
            fsm.Anim.SetBool(fsm.Character.PlayerAnimationParameter.Run, true);
        }

        public override void ActionState(FSMBase fsm)
        {
            base.ActionState(fsm);
            fsm.MoveToTarget(fsm.FoundTarget.position, fsm.Data.AttackStateDistance, fsm.Data.MoveSpeed);
        }

        public override void ExitState(FSMBase fsm)
        {
            base.ExitState(fsm);
            fsm.MoveToTarget(fsm.transform.position, 1, fsm.Data.MoveSpeed);
        }

    }
}
