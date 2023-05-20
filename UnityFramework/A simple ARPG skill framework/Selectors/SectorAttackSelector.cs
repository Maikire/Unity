using ARPGDemo.Character;
using Common;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Skill
{
    /// <summary>
    /// 扇形/圆形选区
    /// </summary>
    public class SectorAttackSelector : IAttackSelector
    {
        /// <summary>
        /// 获取目标
        /// </summary>
        /// <param name="skillData">技能数据</param>
        /// <param name="skillTransform">技能位置</param>
        /// <returns></returns>
        public Transform[] SelectTarget(SkillData skillData, Transform skillTransform)
        {
            //指定范围搜索
            List<Transform> targets = skillTransform.SelectTargets(skillData.AttackDistance, skillData.AttackAngle, skillData.AttackTargetTags);

            //选取活动的目标
            targets = targets.FindAll(t => t.GetComponent<CharacterStatus>().HP > 0);

            //返回目标（群攻？单攻？）
            if (skillData.AttackType == SkillAttackType.Group)
            {
                return targets.ToArray();
            }
            else if (skillData.AttackType == SkillAttackType.Single)
            {
                Transform min = targets.ToArray().GetMin(t => Vector3.Distance(t.position, skillTransform.position));
                if (min == null)
                {
                    return null;
                }
                return new Transform[] { min };
            }

            return null;
        }


    }
}
