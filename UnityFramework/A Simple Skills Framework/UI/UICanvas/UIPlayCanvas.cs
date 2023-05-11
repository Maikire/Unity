using ARPGDemo.Skill;
using Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARPGDemo.UI
{
    /// <summary>
    /// UIPlayCanvas
    /// </summary>
    public class UIPlayCanvas : UIWindow
    {
        public CharacterSkillSystem SkillSystem;
        private Button[] SkillButtons;
        private float LastBatterClickTime; //上一次 按下 连击技能按钮 的 时间

        private void Start()
        {
            InitializeSkillButton();
            RegistrationEvent();
        }

        /// <summary>
        /// 初始化技能按键
        /// </summary>
        private void InitializeSkillButton()
        {
            //找到所有技能按键
            Transform skillButton = TransformHelper.FindChildByName(this.transform, "SkillButtons");
            SkillButtons = new Button[skillButton.childCount];
            for (int i = 0; i < skillButton.childCount; i++)
            {
                SkillButtons[i] = skillButton.GetChild(i).GetComponent<Button>();
            }
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegistrationEvent()
        {
            foreach (Button button in SkillButtons)
            {
                if (button.name == "1001")
                {
                    GetUIEventListener(button.name).PointerPress.AddListener(OnSkillButtonPress);
                }
                else
                {
                    GetUIEventListener(button.name).PointerClick.AddListener(OnSkillButtonClick);
                }
            }
        }

        /// <summary>
        /// OnSkillButtonPress
        /// </summary>
        /// <param name="eventData"></param>
        private void OnSkillButtonPress(PointerEventData eventData)
        {
            if (SkillSystem.Skill == null)
            {
                SkillSystem.UseSkill(int.Parse(eventData.pointerPress.name));
            }
            else
            {
                //SkillSystem.Skill 记录的是最后一次释放的技能的数据

                float interval = Time.time - LastBatterClickTime;

                if (interval < SkillSystem.Skill.BatterTimeMin) return;

                bool isBatter = interval <= SkillSystem.Skill.BatterTimeMax;
                SkillSystem.UseSkill(int.Parse(eventData.pointerPress.name), isBatter);
            }

            SetPosition();

            LastBatterClickTime = Time.time;
        }

        /// <summary>
        /// OnSkillButtonClick
        /// </summary>
        private void OnSkillButtonClick(PointerEventData eventData)
        {
            SkillSystem.UseSkill(int.Parse(eventData.pointerPress.name));
            SetPosition();
        }

        /// <summary>
        /// 设置释放位置
        /// </summary>
        private void SetPosition()
        {
            if (SkillSystem.Skill != null)
            {
                Transform owner = SkillSystem.Skill.Owner.transform;
                SkillSystem.SetPosition(owner.position, owner.rotation);
            }
        }


    }
}
