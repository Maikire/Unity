using UnityEngine;

namespace SkillSystem
{
    /// <summary>
    /// 攻击选区
    /// </summary>
    public interface IAttackSelector
    {
        /// <summary>
        /// 搜索目标
        /// </summary>
        /// <param name="skillData">技能数据</param>
        /// <param name="skillTransform">技能的位置</param>
        /// <returns></returns>
        Transform[] SelectTarget(SkillData skillData, Transform skillTransform);

    }
}
