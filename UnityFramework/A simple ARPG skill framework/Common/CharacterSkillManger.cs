using ARPGDemo.Character;
using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Skill
{
    [RequireComponent(typeof(CharacterStatus))]
    /// <summary>
    /// 技能管理器
    /// </summary>
    public class CharacterSkillManger : MonoBehaviour
    {
        [Tooltip("技能列表")]
        public SkillData[] Skills;
        /// <summary>
        /// 角色状态
        /// </summary>
        private CharacterStatus Character;

        private void Awake()
        {
            Character = this.GetComponent<CharacterStatus>();
        }

        private void Start()
        {
            InitSkill();
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        private void InitSkill()
        {
            foreach (var skill in Skills)
            {
                skill.Prefab = ResourceManger.Load<GameObject>(skill.PrefabName);
                skill.Owner = this.gameObject;
                skill.AttackedTargets = new Dictionary<string, AttackedTarget>();
            }
        }

        /// <summary>
        /// 准备技能
        /// </summary>
        /// <param name="skillID">技能ID</param>
        /// <returns></returns>
        public SkillData PrepareSkill(int skillID)
        {
            SkillData skillData = Skills.Find(s => s.SkillID == skillID);
            if (skillData != null && skillData.CoolRemain <= 0 && Character.MP >= skillData.CostMP)
            {
                return skillData;
            }
            return null;
        }

        /// <summary>
        /// 生成技能
        /// </summary>
        public void GenerateSkill(SkillData skillData, Vector3 position, Quaternion rotation)
        {
            //生成技能
            GameObject skill = ObjectPool.Instance.GetGameObject(skillData.Name, skillData.Prefab, position, rotation);
            SkillDeployer deployer = skill.GetComponent<SkillDeployer>();

            //传递技能数据，创建算法对象
            deployer.SkillData = skillData;

            //执行技能算法
            deployer.DeploySkill();
        }

        /// <summary>
        /// 技能冷却计时器
        /// </summary>
        public IEnumerator CoolTimer(SkillData skillData)
        {
            skillData.CoolRemain = skillData.CoolTime;

            while (skillData.CoolRemain > 0)
            {
                yield return new WaitForSeconds(0.1f);
                skillData.CoolRemain -= 0.1f;
            }
        }


    }
}
