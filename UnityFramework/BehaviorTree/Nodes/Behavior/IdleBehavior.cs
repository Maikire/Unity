namespace BehaviorTree
{
    /// <summary>
    /// 闲置行为
    /// </summary>
    public class IdleBehavior : NPCBehaviorNode
    {
        public override bool IgnoreWalls => false;

        public override BTState TickNode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            data.anim.SetBool(data.character.PlayerAnimationParameter.idle, true);
            return BTState.Success;
        }

    }
}
