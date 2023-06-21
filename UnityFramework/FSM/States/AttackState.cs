using Common;
using UnityEngine;

namespace AI.FSM
{
    /// <summary>
    /// 攻击状态
    /// </summary>
    public class AttackState : FSMState
    {
        public override FSMStateID StateID => FSMStateID.Attack;
        public override bool IgnoreWalls => true;

        /// <summary>
        /// 计时器
        /// </summary>
        private float Timer;
        /// <summary>
        /// 用于判断攻击距离
        /// </summary>
        private float JudgeAttackDistance;

        public override void EnterState(FSMBase fsm)
        {
            base.EnterState(fsm);
            Timer = 0;
            JudgeAttackDistance = 0;
            GetSkill(fsm);
        }

        public override void ActionState(FSMBase fsm)
        {
            base.ActionState(fsm);
            Attack(fsm);
        }

        public override void ExitState(FSMBase fsm)
        {
            base.ExitState(fsm);
            fsm.MoveToTarget(fsm.transform.position, 1, fsm.Data.MoveSpeed);
        }

        /// <summary>
        /// 获取技能
        /// </summary>
        /// <param name="fsm"></param>
        private void GetSkill(FSMBase fsm)
        {
            fsm.CurrentSkill = fsm.NPCSkillSystem.GetRandomSkillData();

            if (fsm.CurrentSkill.MoveDistance != 0)
            {
                JudgeAttackDistance = fsm.CurrentSkill.MoveDistance;
            }
            else
            {
                JudgeAttackDistance = fsm.CurrentSkill.AttackDistance;
            }
        }

        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="fsm"></param>
        private void Attack(FSMBase fsm)
        {
            //看向目标
            fsm.transform.LookAtTarget(fsm.FoundTarget);

            Timer += Time.deltaTime;
            if (Timer > fsm.Data.AttackTimeInterval)
            {
                if (fsm.CurrentSkill != null && Vector3.Distance(fsm.transform.position, fsm.FoundTarget.position) <= JudgeAttackDistance)
                {
                    //放技能
                    fsm.NPCSkillSystem.UseSkill(fsm.CurrentSkill.SkillID);
                    SetPosition(fsm);
                    GetSkill(fsm);
                    Timer = 0;
                }
                else
                {
                    //靠近目标，达到能释放技能的位置
                    fsm.MoveToTarget(fsm.FoundTarget.position, JudgeAttackDistance * fsm.Data.SkillDistanceEffect, fsm.Data.MoveSpeed);
                }
            }
        }

        /// <summary>
        /// 设置释放位置
        /// </summary>
        /// <param name="fsm"></param>
        private void SetPosition(FSMBase fsm)
        {
            if (fsm.NPCSkillSystem.Skill != null)
            {
                Transform owner = fsm.NPCSkillSystem.Skill.Owner.transform;
                fsm.NPCSkillSystem.SetPosition(owner.position, owner.rotation);
            }
        }


    }
}

