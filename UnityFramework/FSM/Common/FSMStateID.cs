using UnityEngine;

namespace AI.FSM
{
    /// <summary>
    /// StateID
    /// </summary>
    public enum FSMStateID
    {
        [Tooltip("默认")]
        Default,

        [Tooltip("死亡")]
        Dead,

        [Tooltip("闲置")]
        Idle,

        [Tooltip("追逐")]
        Pursuit,

        [Tooltip("攻击")]
        Attack,

        [Tooltip("巡逻")]
        Patrol,


    }
}