namespace BehaviorTree
{
    /// <summary>
    /// 行为树节点的基类
    /// </summary>
    public abstract class BTNode
    {
        /// <summary>
        /// 当前节点的执行状态
        /// </summary>
        public BTState State { get; protected set; } = BTState.Failure;

        /// <summary>
        /// 进入节点时执行一次
        /// </summary>
        public virtual void EnterNode(BehaviorTree bt)
        {
            bt.blackboard.nodeStack.Push(this);
        }

        /// <summary>
        /// 行为树节点的执行方法
        /// </summary>
        /// <returns></returns>
        public abstract BTState TickNode(BehaviorTree bt);

        /// <summary>
        /// 运行结束时执行一次
        /// </summary>
        public virtual void ExitNode(BehaviorTree bt)
        {
            bt.blackboard.nodeStack.Pop();
        }

        /// <summary>
        /// 中断节点的执行
        /// </summary>
        public void Abort(BehaviorTree bt)
        {
            ExitNode(bt);
            State = BTState.Abort;
        }

    }
}
