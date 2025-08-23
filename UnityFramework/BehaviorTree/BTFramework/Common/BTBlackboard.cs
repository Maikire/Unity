using Character;
using SkillSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 行为树数据
    /// </summary>
    [Serializable]
    public class BTBlackboard
    {
        [Tooltip("行为树数据")]
        public BTAsset nodeAsset;

        [Tooltip("移动速度")]
        public float moveSpeed;
        [Tooltip("视野距离")]
        public float viewDistance;
        [Tooltip("视野角度")]
        public float viewAngle;
        [Tooltip("可以被发现的目标的标签")]
        public string[] targetTags = { "Player" };
        [Tooltip("进入攻击节点的距离")]
        public float attackNodeDistance;
        [Tooltip("攻击时间间隔")]
        public float attackTimeInterval;
        [Tooltip("判断是否到达路点的偏移量")]
        public float patrolEffect;
        [Tooltip("寻路模式")]
        public BTPatrolModes patrolMode = BTPatrolModes.None;
        [Tooltip("巡逻的路点")]
        public Transform[] wayPoints;

        [HideInInspector]
        [Tooltip("外部中断条件")]
        public List<Func<bool>> interruptions;
        [HideInInspector]
        [Tooltip("节点栈")]
        public Stack<BTNode> nodeStack;
        [HideInInspector]
        [Tooltip("优先级节点")]
        public BTNode priorityNode;
        [HideInInspector]
        [Tooltip("根节点")]
        public BTNode root;
        /// <summary>
        /// 当前节点
        /// </summary>
        public BTNode Current
        {
            get
            {
                if (nodeStack.Count == 0)
                {
                    return null;
                }

                return nodeStack.Peek();
            }
        }

        [HideInInspector]
        [Tooltip("角色的信息")]
        public CharacterStatus character;
        [HideInInspector]
        [Tooltip("角色的动画")]
        public Animator anim;
        [HideInInspector]
        [Tooltip("发现的目标")]
        public Transform[] foundTargets;
        [HideInInspector]
        [Tooltip("技能系统")]
        public CharacterSkillSystemNPC npcSkill;
        [HideInInspector]
        [Tooltip("当前的技能")]
        public SkillData currentSkill;

    }
}
