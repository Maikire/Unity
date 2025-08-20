using Character;

namespace SkillSystem
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
