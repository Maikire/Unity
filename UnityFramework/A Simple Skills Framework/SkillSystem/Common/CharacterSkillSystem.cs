using ARPGDemo.Character;
using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ARPGDemo.Skill
{
    [RequireComponent(typeof(CharacterSkillManger))]
    /// <summary>
    /// 封装技能系统，对外提供简单的技能释放功能
    /// </summary>
    public class CharacterSkillSystem : MonoBehaviour
    {
        public SkillData Skill;
        private CharacterSkillManger SkillManger;
        private Animator Anim;
        private AnimationEventBehaviour EventBehaviour;
        private CharacterSelected SelectCharacter;
        private Queue<int> SkillID; //在使用技能时入队，技能释放完成（调用动画事件）后出队

        private void Awake()
        {
            SkillManger = this.GetComponent<CharacterSkillManger>();
            Anim = this.GetComponentInChildren<Animator>();
            EventBehaviour = this.GetComponentInChildren<AnimationEventBehaviour>();
            SelectCharacter = this.GetComponent<CharacterSelected>();
            SkillID = new Queue<int>();
        }

        private void Start()
        {
            EventBehaviour.TargetAttack.AddListener(DeploySkill);
            EventBehaviour.DirectionAttack.AddListener(DeploySkill);
        }

        /// <summary>
        /// 使用技能
        /// 为玩家提供
        /// </summary>
        /// <param name="skillID">技能ID</param>
        public void UseSkill(int skillID, bool isBatter = false)
        {
            //是否连击
            if (isBatter && Skill != null)
            {
                skillID = Skill.NextBatterID;
            }

            //准备技能
            Skill = SkillManger.PrepareSkill(skillID);
            if (Skill == null) return;

            //入队
            SkillID.Enqueue(skillID);

            //播放动画
            Anim.SetBool(Skill.AnimationName, true);

            //查找目标
            Skill.AttackTargets = FindTargets();

            //单攻/群攻
            if (Skill.AttackType == SkillAttackType.Single)
            {
                //朝向目标
                LookAtTarget(Skill.AttackTargets?[0]);

                //选中目标
                SelectCharacter.SelectTargets(Skill.AttackTargets);
            }
            else
            {
                SelectCharacter.SelectTargets(Skill.AttackTargets);
            }
        }

        /// <summary>
        /// 使用随机技能
        /// 为NPC提供
        /// 目前先这样写，后期用AI
        /// </summary>
        public void UseRandomSkill()
        {
            //先选出可以生成的技能，然后在生成随机数
            SkillData[] skillData = SkillManger.Skills.FindAll(s => SkillManger.PrepareSkill(s.SkillID) != null);
            if (skillData != null && skillData.Length > 0)
            {
                int index = Random.Range(0, skillData.Length);
                UseSkill(skillData[index].SkillID);
            }
        }

        /// <summary>
        /// 生成技能
        /// </summary>
        private void DeploySkill()
        {
            SkillData skillData = SkillManger.PrepareSkill(SkillID.Dequeue());
            SkillManger.GenerateSkill(skillData, skillData.StartPosition, skillData.StartRotation);
        }

        /// <summary>
        /// 设置释放位置
        /// </summary>
        /// <param name="skillData">技能数据</param>
        /// <param name="startPosition">起始位置</param>
        /// <param name="startRotation">起始旋转</param>
        public void SetPosition(Vector3 startPosition, Quaternion startRotation)
        {
            Skill.StartPosition = startPosition;
            Skill.StartRotation = startRotation;
        }

        /// <summary>
        /// 寻找目标
        /// </summary>
        /// <returns></returns>
        private Transform[] FindTargets()
        {
            //这部分的代码与技能生成器中的代码一样，而且这里的代码一定优先执行，但是这两部分的代码一个都不能删
            //虽然代码有重复，但是可以应对某些特殊情况：
            //按下技能键时攻击范围内没有目标，但是动画播放到某一帧并释放技能的时候，
            //攻击范围内有目标了，所以技能生成器中的代码是有必要的
            IAttackSelector selector = DeployerFactory.CreateAttackSelector(Skill); //攻击选区
            Transform[] targets = selector.SelectTarget(Skill, this.transform);
            return targets == null || targets.Length == 0 ? null : targets;
        }

        /// <summary>
        /// 朝向目标，仅沿Y轴旋转
        /// </summary>
        /// <param name="target">目标</param>
        private void LookAtTarget(Transform target)
        {
            if (target != null)
            {
                Vector3 temp = new Vector3(target.position.x, this.transform.position.y, target.position.z);
                this.transform.LookAt(temp);
            }
        }


    }
}
