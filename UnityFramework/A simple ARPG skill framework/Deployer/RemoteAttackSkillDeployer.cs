using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem
{
    /// <summary>
    /// 远程技能释放器
    /// </summary>
    public class RemoteAttackSkillDeployer : SkillDeployer, IMovable
    {
        private List<Transform> AllImpactTargets;

        public override void DeploySkill()
        {
            base.DeploySkill();

            AllImpactTargets = new List<Transform>();

            SetSkillData();
            RecoverGameObject();
            MoveToTarget();
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <returns></returns>
        private void SetSkillData()
        {
            SkillData.TargetPosition = SkillData.Owner.transform.TransformPoint(0, 0, SkillData.MoveDistance);
            SkillData.JudgeTransform = this.transform;
            SkillData.AttackedTargets.Clear();
        }

        public override void RecoverGameObject()
        {
            this.StartCoroutine(ToRecover());
        }

        /// <summary>
        /// 回收物体
        /// </summary>
        /// <returns></returns>
        private IEnumerator ToRecover()
        {
            while (Vector3.Distance(SkillData.JudgeTransform.position, SkillData.TargetPosition) > 0.1f)
            {
                yield return null;
            }
            ObjectPool.Instance.RecoverGameObject(this.gameObject);
        }

        public void OnMove(Vector3 position)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, position, SkillData.MoveSpeed * Time.deltaTime);
            CalculateTargets();
            CalculateImpactTargets();
            ImpactTargets();
        }

        /// <summary>
        /// 计算受影响的目标
        /// 一个目标只会受到一次伤害
        /// </summary>
        private void CalculateImpactTargets()
        {
            if (SkillData.AttackTargets == null) return;

            AllImpactTargets.Clear();

            foreach (Transform item in SkillData.AttackTargets)
            {
                if (!SkillData.AttackedTargets.ContainsKey(item.name))
                {
                    AllImpactTargets.Add(item);
                }
            }

            SkillData.AttackTargets = AllImpactTargets.Count == 0 ? null : AllImpactTargets.ToArray();
        }


    }
}
