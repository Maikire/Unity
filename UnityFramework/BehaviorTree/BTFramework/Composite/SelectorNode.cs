using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// 选择节点（只要一个子节点成功就算成功）
    /// </summary>
    public class SelectorNode : ControlNodes
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private List<BTNode> children;
        /// <summary>
        /// 当前子节点索引
        /// </summary>
        private int currentIndex = 0;

        public SelectorNode()
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
                        currentIndex = 0;
                        return BTState.Success;

                    case BTState.Running:
                        return BTState.Running;

                    case BTState.Abort:
                        child.Abort(bt);
                        currentIndex = 0;
                        return BTState.Abort;

                    case BTState.Failure:
                        child.ExitNode(bt);
                        currentIndex++;
                        break;
                }
            }

            currentIndex = 0;
            return State = BTState.Failure;
        }

        public override void ExitNode(BehaviorTree bt)
        {
            base.ExitNode(bt);
            currentIndex = 0;
        }

    }
}
