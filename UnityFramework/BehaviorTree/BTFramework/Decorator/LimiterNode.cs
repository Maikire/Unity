namespace BehaviorTree
{
    /// <summary>
    /// 限制节点（限制子节点的总执行次数）
    /// </summary>
    public class LimiterNode : ControlNodes
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private BTNode child;
        /// <summary>
        /// 次数
        /// </summary>
        private int limitCount;
        /// <summary>
        /// 当前次数
        /// </summary>
        private int currentCount = 0;

        public LimiterNode(int limitCount)
        {
            this.limitCount = limitCount;
            currentCount = 0;
        }

        public override void AddChild(BTNode child)
        {
            base.AddChild(child);
            this.child = child;
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            while (currentCount < limitCount)
            {
                if (State != BTState.Running)
                {
                    child.EnterNode(bt);
                }

                State = child.TickNode(bt);

                switch (State)
                {
                    case BTState.Success:
                        currentCount++;
                        return BTState.Success;

                    case BTState.Failure:
                        currentCount++;
                        return BTState.Failure;

                    case BTState.Running:
                        return BTState.Running;

                    case BTState.Abort:
                        child.Abort(bt);
                        return BTState.Abort;
                }
            }

            return State = BTState.Success;
        }

    }
}
