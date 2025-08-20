using UnityEngine;

namespace SkillSystem
{
    /// <summary>
    /// 被攻击过的目标
    /// </summary>
    public class SkillAttackedTarget
    {
        [Tooltip("被攻击过的目标")]
        public Transform Target;

        [Tooltip("被攻击的次数")]
        public int Times;

        public SkillAttackedTarget(Transform Target, int Number)
        {
            this.Target = Target;
            this.Times = Number;
        }
    }
}
