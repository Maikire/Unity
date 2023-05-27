using System;
using UnityEngine;

namespace AI.FSM
{
    [Serializable]
    /// <summary>
    /// 有限状态机数据类
    /// </summary>
    public class FSMData
    {
        [Tooltip("状态配置表（在StreamingAssets中的路径）")]
        public string FSMConfigPath;

        [Tooltip("移动速度")]
        public float MoveSpeed;

        [Tooltip("视野距离")]
        public float ViewDistance;

        [Tooltip("视野角度")]
        public float ViewAngle;

        [Tooltip("可以被发现的目标的标签")]
        public string[] TargetTags = { "Player" };

        [Tooltip("进入攻击状态的距离")]
        public float AttackStateDistance;

        [Tooltip("攻击时间间隔")]
        public float AttackTimeInterval;

        [Tooltip("攻击状态下释放技能的距离的偏移量")]
        public float SkillDistanceEffect;

        [Tooltip("触发巡逻的概率，每秒判定一次，取值范围：0-1")]
        public float PatrolProbability;

        [Tooltip("判断是否到达路点的偏移量")]
        public float PatrolEffect;

        [Tooltip("寻路模式")]
        public FSMPatrolModes PatrolMode;

        [Tooltip("巡逻的路点")]
        public Transform[] WayPoints;

        [HideInInspector]
        [Tooltip("完成巡逻")]
        public bool CompletePatrol;


    }
}
