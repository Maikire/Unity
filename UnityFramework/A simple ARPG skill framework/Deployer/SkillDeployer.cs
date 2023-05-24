using System.Collections;
using UnityEngine;

namespace ARPGDemo.Skill
{
    /// <summary>
    /// 技能释放器
    /// </summary>
    public abstract class SkillDeployer : MonoBehaviour
    {
        /// <summary>
        /// 由技能管理器提供数据
        /// </summary>
        private SkillData skillData;
        public SkillData SkillData
        {
            get
            {
                return skillData;
            }
            set
            {
                skillData = value;
                InitDeployer();
            }
        }

        //算法对象
        private IAttackSelector Selector;
        private IImpactEffect[] Impacts;

        //移动
        private IMovable Move;

        /// <summary>
        /// 初始化释放器
        /// </summary>
        public void InitDeployer()
        {
            Selector = DeployerFactory.CreateAttackSelector(skillData); //攻击选区
            Impacts = DeployerFactory.CreateImpactEffects(skillData); //影响效果
            Move = this.GetComponent<IMovable>(); //移动
        }

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        public void MoveToTarget()
        {
            this.StartCoroutine(MoveToTargetCoroutine());
        }

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveToTargetCoroutine()
        {
            if (Move == null) yield break;
            while (this.enabled)
            {
                Move.OnMove(skillData.TargetPosition);
                yield return null;
            }
        }

        /// <summary>
        /// 选区（寻找目标）
        /// </summary>
        public void CalculateTargets()
        {
            skillData.AttackTargets = Selector.SelectTarget(skillData, this.transform);
        }

        /// <summary>
        /// 影响效果
        /// </summary>
        public void ImpactTargets()
        {
            foreach (var item in Impacts)
            {
                item.Execute(this);
            }
        }

        /// <summary>
        /// 释放技能
        /// </summary>
        public virtual void DeploySkill()
        {
            SkillData.AttackedTargets.Clear();
        }

        /// <summary>
        /// 回收技能
        /// </summary>
        public abstract void RecoverGameObject();


    }
}
