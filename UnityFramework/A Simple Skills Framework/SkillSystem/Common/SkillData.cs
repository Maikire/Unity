using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ARPGDemo.Skill
{
    [Serializable]
    /// <summary>
    /// 技能数据类
    /// </summary>
    public class SkillData
    {
        [Tooltip("技能ID")]
        public int SkillID;

        [Tooltip("技能名称")]
        public string Name;

        [Tooltip("技能描述")]
        public string Description;

        [Tooltip("冷却时间（秒）")]
        public float CoolTime;

        [Tooltip("冷却剩余（秒）")]
        public float CoolRemain;

        [Tooltip("法力消耗")]
        public float CostMP;

        [Tooltip("攻击距离")]
        public float AttackDistance;

        [Tooltip("攻击角度")]
        public float AttackAngle;

        [Tooltip("移动距离")]
        public float MoveDistance;

        [Tooltip("移动速度")]
        public float MoveSpeed;

        [HideInInspector]
        [Tooltip("起始旋转")]
        public Quaternion StartRotation;

        [HideInInspector]
        [Tooltip("释放起点")]
        public Vector3 StartPosition;

        [HideInInspector]
        [Tooltip("移动到目标地点")]
        public Vector3 TargetPosition;

        [HideInInspector]
        [Tooltip("用于判断移动的点")]
        public Transform JudgeTransform;

        [Tooltip("攻击目标 的 tag")]
        public string[] AttackTargetTags = { "Enemy" };

        [HideInInspector]
        [Tooltip("攻击目标 的 数组")]
        public Transform[] AttackTargets;

        [HideInInspector]
        [Tooltip("被攻击过的目标")]
        public Dictionary<string, AttackedTarget> AttackedTargets;

        [Tooltip("技能影响类型")]
        public string[] ImpactType = { "CostSP", "Damage" };

        [Tooltip("连击的下一个技能ID")]
        public int NextBatterID;

        [Tooltip("可触发连击/蓄力的最短时间")]
        public float BatterTimeMin;

        [Tooltip("可触发连击/蓄力的最长时间")]
        public float BatterTimeMax;

        [Tooltip("固定伤害")]
        public float AttackDamage;

        [Tooltip("伤害倍率")]
        public float AttackRatio;

        [Tooltip("持续时间")]
        public float DurationTime;

        [Tooltip("伤害间隔")]
        public float AttackInterval;

        [HideInInspector]
        [Tooltip("技能所属")]
        public GameObject Owner;

        [Tooltip("技能预制件名称")]
        public string PrefabName;

        [HideInInspector]
        [Tooltip("技能预制件")]
        public GameObject Prefab;

        [Tooltip("动画名称")]
        public string AnimationName;

        [Tooltip("受击特效名称")]
        public string HitEffectName;

        [HideInInspector]
        [Tooltip("受击特效预制件")]
        public GameObject HitEffectPrefab;

        [Tooltip("技能等级")]
        public int Level;

        [Tooltip("释放类型（指定目标/指定方向/指定位置...）")]
        public SkillDeployType DeployType;

        [Tooltip("攻击类型（单攻/群攻...）")]
        public SkillAttackType AttackType;

        [Tooltip("选择类型（矩形/扇形...）")]
        public SkillSelectorType SelectorType;


    }
}
