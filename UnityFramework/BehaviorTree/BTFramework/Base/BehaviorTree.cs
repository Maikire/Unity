using System;

namespace BehaviorTree
{
    /// <summary>
    /// 行为树执行器，由外部驱动初始化和更新
    /// </summary>
    public class BehaviorTree
    {
        /// <summary>
        /// 当前行为树使用的黑板
        /// </summary>
        public BTBlackboard blackboard { get; private set; }

        /// <summary>
        /// 通过配置资产或已构建的根节点初始化行为树
        /// </summary>
        public void Initialize(BTBlackboard data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            BTNode root = data.root;
            BTNode priorityNode = data.priorityNode;
            if (data.nodeAsset != null)
            {
                BTCreateFactory.BuildBehaviorTree(data.nodeAsset.config, out root, out priorityNode);
            }

            if (root == null)
            {
                throw new InvalidOperationException("行为树需要配置资产或根节点");
            }

            AbortAll();
            data.nodeStack.Clear();
            data.root = root;
            data.priorityNode = priorityNode ?? new PriorityNode();
            blackboard = data;
        }

        /// <summary>
        /// 获取业务层注入的具体黑板
        /// </summary>
        public T GetBlackboard<T>() where T : BTBlackboard
        {
            if (blackboard is T data)
            {
                return data;
            }

            throw new InvalidOperationException($"行为树未使用 {typeof(T).Name} 黑板");
        }

        /// <summary>
        /// 执行一轮节点更新
        /// </summary>
        public void Tick()
        {
            if (blackboard == null)
            {
                throw new InvalidOperationException("行为树尚未初始化");
            }

            foreach (var interruption in blackboard.interruptions)
            {
                if (interruption.Invoke())
                {
                    AbortAll();
                    return;
                }
            }

            BTState state = blackboard.priorityNode.TickNode(this);

            if (state == BTState.Success || state == BTState.Running)
            {
                return;
            }

            if (blackboard.root.State != BTState.Running)
            {
                blackboard.root.EnterNode(this);
            }

            state = blackboard.root.TickNode(this);

            if (state != BTState.Running)
            {
                blackboard.root.ExitNode(this);
            }
        }

        /// <summary>
        /// 中断所有正在执行的节点
        /// </summary>
        public void AbortAll()
        {
            BTNode node = blackboard?.Current;
            while (node != null)
            {
                node.Abort(this);
                node = blackboard.Current;
            }
        }


    }
}
