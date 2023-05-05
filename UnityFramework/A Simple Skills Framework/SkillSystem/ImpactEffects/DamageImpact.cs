using ARPGDemo.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Skill
{
    /// <summary>
    /// 伤害效果
    /// </summary>
    public class DamageImpact : IImpactEffect
    {
        private CharacterStatus AttackerStatus;

        public void Execute(SkillDeployer skillDeployer)
        {
            SkillData skillData = skillDeployer.SkillData;

            if (skillData.AttackTargets == null) return;

            AttackerStatus = skillData.Owner.GetComponent<CharacterStatus>();

            if (skillData.DurationTime == 0)
            {
                OnceDamage(skillData);
            }
            else
            {
                skillDeployer.StartCoroutine(RepeatDamage(skillDeployer));
            }
        }

        /// <summary>
        /// 单次攻击
        /// 伤害 = 技能固定伤害 + 攻击力 * 技能倍率
        /// </summary>
        /// <param name="skillData"></param>
        private void OnceDamage(SkillData skillData)
        {
            float damage = skillData.AttackDamage + AttackerStatus.AttackPower * skillData.AttackRatio;
            foreach (var item in skillData.AttackTargets)
            {
                item.GetComponent<CharacterStatus>().Damage(damage);
            }

            RecordAttack(skillData);
        }

        /// <summary>
        /// 持续性伤害效果
        /// </summary>
        /// <param name="skillData"></param>
        /// <returns></returns>
        private IEnumerator RepeatDamage(SkillDeployer skillDeployer)
        {
            SkillData skillData = skillDeployer.SkillData;
            float durationTime = 0;
            do
            {
                OnceDamage(skillData);
                yield return new WaitForSeconds(skillData.AttackInterval);
                durationTime += skillData.AttackInterval;
                skillDeployer.CalculateTargets(); //重新计算目标
            }
            while (durationTime < skillData.DurationTime);
        }

        /// <summary>
        /// 记录攻击单位和受击次数
        /// </summary>
        private void RecordAttack(SkillData skillData)
        {
            foreach (var item in skillData.AttackTargets)
            {
                if (!skillData.AttackedTargets.ContainsKey(item.name))
                {
                    skillData.AttackedTargets.Add(item.name, new AttackedTarget(item, 1));
                }
                else
                {
                    skillData.AttackedTargets[item.name].Number++;
                }
            }
        }

        //创建攻击特效，没有特效，所以没写

    }
}
