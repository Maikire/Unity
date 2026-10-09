using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// 优先级节点（独立于主行为树，每轮按添加顺序检查条件，高优先级子节点可抢占正在运行的低优先级子节点）
    /// </summary>
    public class PriorityNode : ControlNodes
    {
        /// <summary>
        /// 过滤结构列表
        /// </summary>
        private List<BTFilter> filters;
        /// <summary>
        /// 当前正在执行的优先级子节点
        /// </summary>
        private BTNode runningChild = null;

        public PriorityNode()
        {
            filters = new List<BTFilter>();
        }

        public override void AddChild(BTFilter filter)
        {
            base.AddChild(filter);
            filters.Add(filter);
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            foreach (var item in filters)
            {
                if (item.condition.TickNode(bt) != BTState.Success)
                {
                    continue;
                }

                // 切换分支或被外部中断后，需要重新进入选中的子节点
                if (runningChild != item.child || !bt.blackboard.nodeStack.Contains(runningChild))
                {
                    // 按执行栈中断旧分支及其正在运行的后代，再进入新分支
                    bt.AbortAll();
                    item.child.EnterNode(bt);
                    runningChild = item.child;
                }

                State = item.child.TickNode(bt);

                if (State != BTState.Running)
                {
                    item.child.ExitNode(bt);
                    runningChild = null;
                }

                return State;
            }

            // 所有条件都不成立时，中断实际运行的优先级分支，让主行为树继续
            if (runningChild != null)
            {
                if (bt.blackboard.nodeStack.Contains(runningChild))
                {
                    bt.AbortAll();
                }

                runningChild = null;
            }

            return State = BTState.Failure;
        }

    }
}
