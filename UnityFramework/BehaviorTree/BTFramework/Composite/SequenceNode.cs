using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// 顺序节点（全部成功才算成功）
    /// </summary>
    public class SequenceNode : ControlNodes
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private List<BTNode> children;
        /// <summary>
        /// 当前子节点索引
        /// </summary>
        private int currentIndex = 0;

        public SequenceNode()
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
                    case BTState.Failure:
                        child.ExitNode(bt);
                        currentIndex = 0;
                        return BTState.Failure;

                    case BTState.Running:
                        return BTState.Running;

                    case BTState.Abort:
                        child.Abort(bt);
                        currentIndex = 0;
                        return BTState.Abort;

                    case BTState.Success:
                        child.ExitNode(bt);
                        currentIndex++;
                        break;
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
