using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// 优先级节点（独立于主行为树，不在根节点及其子节点下，不可中断，每轮都会检测，成功时中断其他所有节点，执行优先级节点）
    /// </summary>
    public class PriorityNode : ControlNodes
    {
        /// <summary>
        /// 过滤结构列表
        /// </summary>
        private List<BTFilter> filters;
        /// <summary>
        /// 是否执行过子节点
        /// </summary>
        private bool isTickedChild = false;

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
                if (item.condition.TickNode(bt) == BTState.Success)
                {
                    if (State != BTState.Running)
                    {
                        bt.AbortAll();
                        item.child.EnterNode(bt);
                        isTickedChild = true;
                    }

                    State = item.child.TickNode(bt);

                    // 优先级节点不会被低级节点打断，所以这里不需要处理中断
                    if (State != BTState.Running)
                    {
                        item.child.ExitNode(bt);
                        isTickedChild = false;
                    }

                    return State;
                }
                else
                {
                    if (isTickedChild)
                    {
                        item.child.Abort(bt);
                        isTickedChild = false;
                    }
                }
            }

            isTickedChild = false;
            return State = BTState.Failure;
        }

    }
}
