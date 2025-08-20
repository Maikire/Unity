namespace SkillSystem
{
    /// <summary>
    /// 技能影响效果
    /// </summary>
    public interface IImpactEffect
    {
        /// <summary>
        /// 执行影响效果
        /// 部分效果需要用到协程，未继承MonoBehaviour的类无法使用协程，
        /// 所以这里需要一个脚本对象，而技能释放器是最好的选择
        /// </summary>
        /// <param name="skillDeployer">技能释放器</param>
        void Execute(SkillDeployer skillDeployer);

    }
}
