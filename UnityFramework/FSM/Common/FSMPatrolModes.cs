using System;
using UnityEngine;

namespace AI.FSM
{
    [Serializable]
    /// <summary>
    /// 寻路模式
    /// </summary>
    public enum FSMPatrolModes
    {
        [Tooltip("无（在巡逻状态下，触发 完成巡逻）")]
        None,

        [Tooltip("单次")]
        Once,

        [Tooltip("循环")]
        Loop,

        [Tooltip("往返")]
        RoundTrip,


    }
}

