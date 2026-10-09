using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// 并行节点（所有子节点都会依次执行一次）
    /// </summary>
    public class ParallelNode : ControlNodes
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private List<BTNode> children;
        /// <summary>
        /// 当前子节点索引
        /// </summary>
        private int currentIndex = 0;

        public ParallelNode()
        {
            children = new List<BTNode>();
        }

        public override void AddChild(BTNode child)
        {
            base.AddChild(child);
            children.Add(child);
        }

        public override void EnterNode(BehaviorTree bt)
        {
            base.EnterNode(bt);
            currentIndex = 0;
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            BTNode child;

            while (currentIndex < children.Count)
            {
                child = children[currentIndex];

                if (State != BTState.Running)
                {
                    child.EnterNode(bt);
                }

                State = child.TickNode(bt);

                switch (State)
                {
                    case BTState.Success:
                        child.ExitNode(bt);
                        currentIndex++;
                        break;

                    case BTState.Failure:
                        child.ExitNode(bt);
                        currentIndex++;
                        break;

                    case BTState.Running:
                        return BTState.Running;

                    case BTState.Abort:
                        child.Abort(bt);
                        currentIndex = 0;
                        return BTState.Abort;
                }
            }

            currentIndex = 0;
            return State = BTState.Success;
        }

        public override void ExitNode(BehaviorTree bt)
        {
            base.ExitNode(bt);
            currentIndex = 0;
        }

    }
}
