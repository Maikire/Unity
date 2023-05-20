using ARPGDemo.Character;
using Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ARPGDemo.Skill
{
    [RequireComponent(typeof(CharacterSkillManger), typeof(CharacterSelected))]
    /// <summary>
    /// 封装技能系统，对外提供简单的技能释放功能
    /// </summary>
    public class CharacterSkillSystem : MonoBehaviour
    {
        [Tooltip("最后一次释放的技能的数据")]
        public SkillData Skill = null;
        private CharacterSkillManger SkillManger;
        private Animator Anim;
        private AnimationEventBehaviour EventBehaviour;
        private CharacterSelected SelectCharacter;
        private CharacterInputController InputController;

        //处理特殊情况：在一个技能释放的过程中释放其他技能
        //MOBA、拳皇等游戏会使用队列（或列表）
        //    在使用技能时入队，技能释放完成（调用动画事件）后出队
        //    如果需要记录技能的使用数据（例如：技能的释放顺序或技能的使用次数），可以使用列表
        //    private Queue<SkillData> SkillID;
        //ARPG游戏使用忽略操作（释放技能的过程中，无视其他指令）
        //    实现方法：判断动画的播放状态

        private void Awake()
        {
            SkillManger = this.GetComponent<CharacterSkillManger>();
            Anim = this.GetComponentInChildren<Animator>();
            EventBehaviour = this.GetComponentInChildren<AnimationEventBehaviour>();
            SelectCharacter = this.GetComponent<CharacterSelected>();
            InputController = this.GetComponent<CharacterInputController>();
        }

        private void Start()
        {
            Skill = null; //脚本挂在物体上就会生成一个 SkillData 对象，这个对象没有任何意义

            EventBehaviour.MeleeAttack.AddListener(DeploySkill);
            EventBehaviour.RemoteAttack.AddListener(DeploySkill);
            EventBehaviour.CancelAnim.AddListener(CanMove);
        }

        /// <summary>
        /// 使用技能
        /// 为玩家提供
        /// </summary>
        /// <param name="skillID">技能ID</param>
        /// <param name="isBatter">是否连击</param>
        public void UseSkill(int skillID, bool isBatter = false)
        {
            //是否连击
            if (isBatter && Skill != null)
            {
                skillID = Skill.NextBatterID;
            }

            //判断动画的播放状态
            if (Skill != null && Anim.GetBool(Skill.AnimationName)) return;

            //准备技能
            Skill = SkillManger.PrepareSkill(skillID);
            if (Skill == null) return;

            //技能冷却
            SkillManger.StartCoroutine(SkillManger.CoolTimer(Skill));

            //播放动画
            Anim.SetBool(Skill.AnimationName, true);

            //禁止移动（攻击的优先级高于移动）
            InputController.IsMove = false;

            //查找目标
            Skill.AttackTargets = FindTargets();

            //单攻/群攻
            if (Skill.AttackType == SkillAttackType.Single)
            {
                //朝向目标
                LookAtTarget(Skill.AttackTargets?[0]);

                //选中目标
                SelectCharacter.SelectTargets(Skill.AttackTargets);
            }
            else
            {
                //选中目标
                SelectCharacter.SelectTargets(Skill.AttackTargets);
            }
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

        /// <summary>
        /// 生成技能
        /// </summary>
        private void DeploySkill()
        {
            SkillManger.GenerateSkill(Skill, Skill.StartPosition, Skill.StartRotation);
        }

        /// <summary>
        /// 设置释放位置
        /// </summary>
        /// <param name="skillData">技能数据</param>
        /// <param name="startPosition">起始位置</param>
        /// <param name="startRotation">起始旋转</param>
        public void SetPosition(Vector3 startPosition, Quaternion startRotation)
        {
            Skill.StartPosition = startPosition;
            Skill.StartRotation = startRotation;
        }

        /// <summary>
        /// 寻找目标
        /// </summary>
        /// <returns></returns>
        private Transform[] FindTargets()
        {
            //这部分的代码与技能生成器中的代码一样，而且这里的代码一定优先执行，但是这两部分的代码一个都不能删
            //虽然代码有重复，但是可以应对某些特殊情况：
            //按下技能键时攻击范围内没有目标，但是动画播放到某一帧并释放技能的时候，
            //攻击范围内有目标了，所以技能生成器中的代码是有必要的
            IAttackSelector selector = DeployerFactory.CreateAttackSelector(Skill); //攻击选区
            Transform[] targets = selector.SelectTarget(Skill, this.transform);
            return targets == null || targets.Length == 0 ? null : targets;
        }

        /// <summary>
        /// 朝向目标，仅沿Y轴旋转
        /// </summary>
        /// <param name="target">目标</param>
        private void LookAtTarget(Transform target)
        {
            if (target != null)
            {
                Vector3 temp = new Vector3(target.position.x, this.transform.position.y, target.position.z);
                this.transform.LookAt(temp);
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
