namespace BehaviorTree
{
    /// <summary>
    /// 重复节点（重复执行子节点）
    /// </summary>
    public class RepeaterNode : ControlNodes
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private BTNode child;
        /// <summary>
        /// 子节点状态
        /// </summary>
        private BTState childState;

        public RepeaterNode()
        {
            childState = BTState.Success;
        }

        public override void AddChild(BTNode child)
        {
            base.AddChild(child);
            this.child = child;
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            if (childState != BTState.Running)
            {
                child.EnterNode(bt);
            }

            childState = child.TickNode(bt);

            switch (childState)
            {
                case BTState.Failure:
                    child.ExitNode(bt);
                    return State = BTState.Failure;

                case BTState.Abort:
                    child.Abort(bt);
                    return State = BTState.Abort;

                default:
                    break;
            }

            return State = BTState.Running;
        }

    }
}
