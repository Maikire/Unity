using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 行为树配置及执行状态，业务数据由派生黑板提供
    /// </summary>
    [Serializable]
    public class BTBlackboard
    {
        [Tooltip("行为树数据")]
        public BTAsset nodeAsset;

        /// <summary>
        /// 外部中断
        /// </summary>
        [NonSerialized]
        public List<Func<bool>> interruptions = new();

        /// <summary>
        /// 节点栈
        /// </summary>
        [NonSerialized]
        public Stack<BTNode> nodeStack = new();

        /// <summary>
        /// 优先级节点
        /// </summary>
        [NonSerialized]
        public BTNode priorityNode;

        /// <summary>
        /// 根节点
        /// </summary>
        [NonSerialized]
        public BTNode root;

        /// <summary>
        /// 当前执行的节点
        /// </summary>
        public BTNode Current => nodeStack.Count == 0 ? null : nodeStack.Peek();


    }
}
