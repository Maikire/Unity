using ARPGDemo.Character;
using Common;
using System;
using System.Collections;
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
            List<Transform> targets = new List<Transform>();

            //获取范围内有特定标签的目标
            RaycastHit[] raycastHits = Physics.SphereCastAll(skillTransform.position, skillData.AttackDistance, skillTransform.forward, skillData.AttackDistance);
            foreach (string item in skillData.AttackTargetTags)
            {
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    if (raycastHits[i].transform.tag == item)
                    {
                        targets.Add(raycastHits[i].transform);
                    }
                }
            }

            //扇形选取
            //targets = targets.FindAll(t => Vector3.Angle(skillTransform.forward, t.position - skillTransform.position) <= (skillData.AttackAngle / 2));

            //选取活动的目标
            //targets = targets.FindAll(t => t.GetComponent<CharacterStatus>().HP > 0);

            //扇形选取活动的目标
            targets = targets.FindAll(t => Vector3.Angle(skillTransform.forward, t.position - skillTransform.position) <= (skillData.AttackAngle / 2)
                                        && t.GetComponent<CharacterStatus>().HP > 0);

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
