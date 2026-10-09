namespace BehaviorTree
{
    /// <summary>
    /// 死亡行为
    /// </summary>
    public class DeadBehavior : NPCBehaviorNode
    {
        public override bool IgnoreWalls => true;

        public override void EnterNode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            base.EnterNode(bt);
            data.anim.SetBool(data.character.PlayerAnimationParameter.die, true);
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            return BTState.Running;
        }

    }
}
