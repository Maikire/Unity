namespace BehaviorTree
{
    /// <summary>
    /// 死亡行为
    /// </summary>
    public class DeadBehavior : BehaviorNode
    {
        public override bool IgnoreWalls => true;

        public override void EnterNode(BehaviorTree bt)
        {
            base.EnterNode(bt);
            bt.blackboard.anim.SetBool(bt.blackboard.character.PlayerAnimationParameter.Die, true);
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            return BTState.Running;
        }

    }
}
