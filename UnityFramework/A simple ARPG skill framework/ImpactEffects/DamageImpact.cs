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
        public void Execute(SkillDeployer skillDeployer)
        {
            SkillData skillData = skillDeployer.SkillData;

            //如果写在 OnceDamage 中，每次调用方法都会执行一次 GetComponent
            //如果写在这里，只需要执行一次 GetComponent
            CharacterStatus attackerStatus = skillData.Owner.GetComponent<CharacterStatus>();

            if (skillData.AttackTargets == null) return;

            if (skillData.DurationTime == 0)
            {
                OnceDamage(skillData, attackerStatus);
            }
            else
            {
                skillDeployer.StartCoroutine(RepeatDamage(skillDeployer, attackerStatus));
            }
        }

        /// <summary>
        /// 单次攻击
        /// 伤害 = 技能固定伤害 + 攻击力 * 技能倍率
        /// </summary>
        /// <param name="skillData"></param>
        /// <param name="attackerStatus"></param>
        private void OnceDamage(SkillData skillData, CharacterStatus attackerStatus)
        {
            float damage = skillData.AttackDamage + attackerStatus.AttackPower * skillData.AttackRatio;

            foreach (var item in skillData.AttackTargets)
            {
                item.GetComponent<CharacterStatus>().Damage(damage);
            }

            RecordAttack(skillData);
        }

        /// <summary>
        /// 持续性伤害效果
        /// </summary>
        /// <param name="skillDeployer"></param>
        /// <param name="attackerStatus"></param>
        /// <returns></returns>
        private IEnumerator RepeatDamage(SkillDeployer skillDeployer, CharacterStatus attackerStatus)
        {
            SkillData skillData = skillDeployer.SkillData;

            float durationTime = 0;
            do
            {
                OnceDamage(skillData, attackerStatus);
                yield return new WaitForSeconds(skillData.AttackInterval);
                durationTime += skillData.AttackInterval;
                skillDeployer.CalculateTargets(); //重新计算目标
            }
            while (durationTime < skillData.DurationTime);
        }

        /// <summary>
        /// 记录被攻击单位和受击次数
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
                    skillData.AttackedTargets[item.name].Times++;
                }
            }
        }

        //创建攻击特效（因为没有做特效，所以没写这部分的代码）

    }
}
