using AI.FSM;
using Common;
using UnityEngine;

namespace ARPGDemo.Skill
{
    [RequireComponent(typeof(FSMBase))]
    /// <summary>
    /// 为NPC提供的技能系统
    /// </summary>
    public class CharacterSkillSystemNPC : CharacterSkillSystem
    {
        private FSMBase Fsm;

        protected override void Awake()
        {
            base.Awake();
            Fsm = this.GetComponent<FSMBase>();
        }

        /// <summary>
        /// 使用随机技能
        /// 为NPC提供
        /// </summary>
        public void UseRandomSkill()
        {
            //先选出可以生成的技能，然后在生成随机数
            SkillData[] skillData = SkillManger.Skills.FindAll(s => SkillManger.PrepareSkill(s.SkillID) != null);
            if (skillData != null && skillData.Length > 0)
            {
                int index = Random.Range(0, skillData.Length);
                UseSkill(skillData[index].SkillID);
            }
        }


    }
}
