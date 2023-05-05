using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Skill
{
    /// <summary>
    /// 指定目标技能释放器
    /// </summary>
    public class TargetAttackSkillDeployer : SkillDeployer
    {
        public override void DeploySkill()
        {
            base.DeploySkill();

            //指定时间后回收目标
            RecoverGameObject();

            //执行选区（寻找目标）算法
            CalculateTargets();

            //执行影响算法
            ImpactTargets();
        }

        public override void RecoverGameObject()
        {
            ObjectPool.Instance.RecoverGameObject(this.gameObject, SkillData.DurationTime);
        }


    }
}
