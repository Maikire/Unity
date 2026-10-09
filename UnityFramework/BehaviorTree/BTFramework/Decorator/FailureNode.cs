namespace BehaviorTree
{
    /// <summary>
    /// 失败节点（即使子节点返回成功，该节点也会返回失败）
    /// </summary>
    public class FailureNode : ControlNodes
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private BTNode child;

        public FailureNode()
        {
            State = BTState.Failure;
        }

        public override void AddChild(BTNode child)
        {
            base.AddChild(child);
            this.child = child;
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            if (State != BTState.Running)
            {
                child.EnterNode(bt);
            }

            State = child.TickNode(bt);

            switch (State)
            {
                case BTState.Success:
                    child.ExitNode(bt);
                    return State = BTState.Failure;

                case BTState.Failure:
                    child.ExitNode(bt);
                    return BTState.Failure;

                case BTState.Running:
                    return BTState.Running;

                case BTState.Abort:
                    child.Abort(bt);
                    return BTState.Abort;

                default:
                    child.ExitNode(bt);
                    return State = BTState.Failure;
            }
        }

    }
}
