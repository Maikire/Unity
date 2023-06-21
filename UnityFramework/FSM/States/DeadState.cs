namespace AI.FSM
{
    /// <summary>
    /// 死亡状态
    /// </summary>
    public class DeadState : FSMState
    {
        public override FSMStateID StateID => FSMStateID.Dead;
        public override bool IgnoreWalls => false;


    }
}
