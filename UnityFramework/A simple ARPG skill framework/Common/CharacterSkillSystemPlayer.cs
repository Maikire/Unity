using ARPGDemo.Character;
using UnityEngine;

namespace ARPGDemo.Skill
{
    [RequireComponent(typeof(CharacterInputController), typeof(CharacterSelected))]
    /// <summary>
    /// 为玩家提供的技能系统
    /// </summary>
    public class CharacterSkillSystemPlayer : CharacterSkillSystem
    {
        private CharacterSelected SelectCharacter;
        private CharacterInputController InputController;

        protected override void Awake()
        {
            base.Awake();
            SelectCharacter = this.GetComponent<CharacterSelected>();
            InputController = GetComponent<CharacterInputController>();
        }

        protected override void Start()
        {
            base.Start();
            EventBehaviour.CancelAnim.AddListener(CanMove);
        }

        public override void UseSkill(int skillID, bool isBatter = false)
        {
            base.UseSkill(skillID, isBatter);

            if (SuccessfullyUsed)
            {
                //禁止移动（攻击的优先级高于移动）
                InputController.IsMove = false;

                //选中目标
                SelectCharacter.SelectTargets(Skill.AttackTargets);
            }
        }

        /// <summary>
        /// 使角色可以移动
        /// </summary>
        private void CanMove()
        {
            InputController.IsMove = true;
        }


    }
}
