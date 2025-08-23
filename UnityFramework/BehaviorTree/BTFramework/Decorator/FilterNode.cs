namespace BehaviorTree
{
    /// <summary>
    /// 过滤节点（每次执行都会先进行条件判断）
    /// </summary>
    public class FilterNode : ControlNodes
    {
        /// <summary>
        /// 过滤结构
        /// </summary>
        private BTFilter filter;
        /// <summary>
        /// 是否执行过子节点
        /// </summary>
        private bool isTickedChild = false;

        public override void AddChild(BTFilter filter)
        {
            base.AddChild(filter);
            this.filter = filter;
        }

        public override void EnterNode(BehaviorTree bt)
        {
            base.EnterNode(bt);
            isTickedChild = false;
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            if (filter.condition.TickNode(bt) == BTState.Success)
            {
                if (State != BTState.Running)
                {
                    filter.child.EnterNode(bt);
                    isTickedChild = true;
                }

                State = filter.child.TickNode(bt);

                switch (State)
                {
                    case BTState.Success:
                        filter.child.ExitNode(bt);
                        isTickedChild = false;
                        return BTState.Success;

                    case BTState.Failure:
                        filter.child.ExitNode(bt);
                        isTickedChild = false;
                        return BTState.Failure;

                    case BTState.Running:
                        return BTState.Running;

                    case BTState.Abort:
                        filter.child.Abort(bt);
                        isTickedChild = false;
                        return BTState.Abort;

                    default:
                        filter.child.ExitNode(bt);
                        isTickedChild = false;
                        return State = BTState.Failure;
                }
            }
            else
            {
                if (isTickedChild)
                {
                    filter.child.Abort(bt);
                    isTickedChild = false;
                    return State = BTState.Abort;
                }
                return State = BTState.Failure;
            }
        }

        public override void ExitNode(BehaviorTree bt)
        {
            base.ExitNode(bt);
            isTickedChild = false;
        }

    }
}
