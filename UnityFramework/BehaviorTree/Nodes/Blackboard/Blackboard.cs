using CharacterSystem;
using SkillSystem;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTree
{
    /// <summary>
    /// NPC 业务黑板，保存感知、移动、动画及技能数据
    /// </summary>
    [Serializable]
    public class Blackboard : BTBlackboard
    {
        [Tooltip("移动速度")]
        public float moveSpeed = 3;

        [Tooltip("视野距离")]
        public float viewDistance = 10;

        [Tooltip("视野角度")]
        public float viewAngle = 90;

        [Tooltip("可以被发现的目标的标签")]
        public string[] targetTags = { "Player" };

        [Tooltip("进入攻击节点的距离")]
        public float attackNodeDistance = 5;

        [Tooltip("攻击时间间隔")]
        public float attackTimeInterval = 1;

        [Tooltip("判断是否到达路点的偏移量")]
        public float patrolEffect = 1;

        [Tooltip("寻路模式")]
        public BTPatrolModes patrolMode = BTPatrolModes.None;

        [Tooltip("巡逻的路点")]
        public Transform[] wayPoints;

        /// <summary>
        /// 拥有者
        /// </summary>
        [NonSerialized]
        public Transform owner;

        /// <summary>
        /// NPC 行为树驱动
        /// </summary>
        [NonSerialized]
        public NPCBehaviorTree driver;

        /// <summary>
        /// 发现的目标
        /// </summary>
        [NonSerialized]
        public Transform[] foundTargets;

        /// <summary>
        /// 寻路
        /// </summary>
        [NonSerialized]
        public NavMeshAgent navigation;

        /// <summary>
        /// 动画
        /// </summary>
        [NonSerialized]
        public Animator anim;

        /// <summary>
        /// 角色状态
        /// </summary>
        [NonSerialized]
        public CharacterStatus character;

        /// <summary>
        /// 技能
        /// </summary>
        [NonSerialized]
        public CharacterSkillSystemNPC npcSkill;

        /// <summary>
        /// 技能数据
        /// </summary>
        [NonSerialized]
        public SkillData currentSkill;


    }
}
