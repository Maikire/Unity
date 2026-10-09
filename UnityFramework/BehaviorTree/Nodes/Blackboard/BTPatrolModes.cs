using System;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 寻路模式
    /// </summary>
    [Serializable]
    public enum BTPatrolModes
    {
        [Tooltip("无（在巡逻节点返回 失败）")]
        None,
        [Tooltip("单次")]
        Once,
        [Tooltip("循环")]
        Loop,
        [Tooltip("往返")]
        RoundTrip,

    }
}
