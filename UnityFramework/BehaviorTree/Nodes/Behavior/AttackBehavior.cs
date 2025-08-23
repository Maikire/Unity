using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 攻击行为
    /// </summary>
    public class AttackBehavior : BehaviorNode
    {
        public override bool IgnoreWalls => true;

        /// <summary>
        /// 计时器
        /// </summary>
        private float timer = 0;
        /// <summary>
        /// 用于判断攻击距离
        /// </summary>
        private float judgeAttackDistance = 0;
        /// <summary>
        /// 已经初始化
        /// </summary>
        private bool isInit = false;

        public override void EnterNode(BehaviorTree bt)
        {
            base.EnterNode(bt);

            if (isInit == false)
            {
                Init(bt);
            }

            if (bt.blackboard.currentSkill == null)
            {
                GetSkill(bt);
            }
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            return Attack(bt);
        }

        public override void ExitNode(BehaviorTree bt)
        {
            base.ExitNode(bt);
            bt.MoveToTarget(bt.transform.position, 1, bt.blackboard.moveSpeed);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="bt"></param>
        private void Init(BehaviorTree bt)
        {
            timer = bt.blackboard.attackTimeInterval;
            isInit = true;
        }

        /// <summary>
        /// 获取技能
        /// </summary>
        /// <param name="bt"></param>
        private void GetSkill(BehaviorTree bt)
        {
            bt.blackboard.currentSkill = bt.blackboard.npcSkill.GetRandomSkillData();
            if (bt.blackboard.currentSkill == null)
            {
                return;
            }

            if (bt.blackboard.currentSkill.MoveDistance != 0)
            {
                judgeAttackDistance = bt.blackboard.currentSkill.MoveDistance;
            }
            else
            {
                judgeAttackDistance = bt.blackboard.currentSkill.AttackDistance;
            }

            bt.blackboard.attackNodeDistance = judgeAttackDistance;
        }

        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="bt"></param>
        private BTState Attack(BehaviorTree bt)
        {
            // 看向目标
            bt.transform.LookAt(bt.blackboard.foundTargets[0]);

            timer += Time.deltaTime;
            if (timer >= bt.blackboard.attackTimeInterval)
            {
                if (bt.blackboard.currentSkill == null)
                {
                    return BTState.Failure;
                }

                if (Vector3.Distance(bt.transform.position, bt.blackboard.foundTargets[0].position) <= judgeAttackDistance)
                {
                    // 放技能
                    bt.blackboard.npcSkill.UseSkill(bt.blackboard.currentSkill.SkillID);
                    SetPosition(bt);

                    GetSkill(bt);
                    timer = 0;

                    return BTState.Success;
                }
                else
                {
                    return BTState.Failure;
                }
            }

            bt.blackboard.anim.SetBool(bt.blackboard.character.PlayerAnimationParameter.Idle, true);
            return BTState.Running;
        }

        /// <summary>
        /// 设置释放位置
        /// </summary>
        /// <param name="bt"></param>
        private void SetPosition(BehaviorTree bt)
        {
            bt.blackboard.npcSkill.SetPosition(bt.transform.position, bt.transform.rotation);
        }

    }
}
