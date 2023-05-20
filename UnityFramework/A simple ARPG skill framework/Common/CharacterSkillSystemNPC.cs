using Common;
using UnityEngine;

namespace ARPGDemo.Skill
{
    /// <summary>
    /// 为NPC提供的技能系统
    /// </summary>
    public class CharacterSkillSystemNPC : CharacterSkillSystem
    {
        /// <summary>
        /// 获取随机技能
        /// </summary>
        /// <returns></returns>
        public SkillData GetRandomSkillData()
        {
            //先选出可以生成的技能，然后在生成随机数
            SkillData[] skillData = SkillManger.Skills.FindAll(s => SkillManger.PrepareSkill(s.SkillID) != null);
            if (skillData != null && skillData.Length > 0)
            {
                int index = Random.Range(0, skillData.Length);
                return skillData[index];
            }
            return null;
        }


    }
}
