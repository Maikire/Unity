namespace BehaviorTree
{
    /// <summary>
    /// 闲置行为
    /// </summary>
    public class IdleBehavior : BehaviorNode
    {
        public override bool IgnoreWalls => false;

        public override BTState TickNode(BehaviorTree bt)
        {
            bt.blackboard.anim.SetBool(bt.blackboard.character.PlayerAnimationParameter.Idle, true);
            return BTState.Success;
        }

    }
}
