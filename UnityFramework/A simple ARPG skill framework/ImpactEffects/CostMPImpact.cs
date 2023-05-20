using ARPGDemo.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Skill
{
    /// <summary>
    /// 消耗法力值
    /// </summary>
    public class CostMPImpact : IImpactEffect
    {
        public void Execute(SkillDeployer skillDeployer)
        {
            CharacterStatus status = skillDeployer.SkillData.Owner.GetComponent<CharacterStatus>();
            status.MP -= skillDeployer.SkillData.CostMP;
        }

    }
}
