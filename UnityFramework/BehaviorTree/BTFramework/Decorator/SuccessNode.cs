namespace BehaviorTree
{
    /// <summary>
    /// 成功节点（即使子节点返回失败，该节点也会返回成功）
    /// </summary>
    public class SuccessNode : ControlNodes
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private BTNode child;

        public SuccessNode()
        {
            State = BTState.Success;
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
                    return BTState.Success;

                case BTState.Failure:
                    child.ExitNode(bt);
                    return State = BTState.Success;

                case BTState.Running:
                    return BTState.Running;

                case BTState.Abort:
                    child.Abort(bt);
                    return BTState.Abort;

                default:
                    child.ExitNode(bt);
                    return State = BTState.Success;
            }
        }

    }
}
