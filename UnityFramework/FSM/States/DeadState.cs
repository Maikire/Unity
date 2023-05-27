namespace AI.FSM
{
    /// <summary>
    /// 死亡状态
    /// </summary>
    public class DeadState : FSMState
    {
        public override void Init()
        {
            StateID = FSMStateID.Dead;
            IgnoreWalls = false;
        }


    }
}
