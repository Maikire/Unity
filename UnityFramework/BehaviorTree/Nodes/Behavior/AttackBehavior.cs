using Common;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 攻击行为
    /// </summary>
    public class AttackBehavior : NPCBehaviorNode
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
            var data = GetBlackboard(bt);
            base.EnterNode(bt);

            if (isInit == false)
            {
                Init(bt);
            }

            if (data.currentSkill == null)
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
            var data = GetBlackboard(bt);
            base.ExitNode(bt);
            data.driver.MoveToTarget(data.owner.position, 1, data.moveSpeed);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="bt"></param>
        private void Init(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            timer = data.attackTimeInterval;
            GetSkill(bt);

            isInit = true;
        }

        /// <summary>
        /// 获取技能
        /// </summary>
        /// <param name="bt"></param>
        private void GetSkill(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            data.currentSkill = data.npcSkill.GetRandomSkillData(data.foundTargets);
            if (data.currentSkill == null)
            {
                return;
            }

            if (data.currentSkill.MoveDistance != 0)
            {
                judgeAttackDistance = data.currentSkill.MoveDistance;
            }
            else
            {
                judgeAttackDistance = data.currentSkill.AttackDistance;
            }

            data.attackNodeDistance = judgeAttackDistance;
        }

        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="bt"></param>
        private BTState Attack(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            // 看向目标
            data.owner.LookAtTarget(data.foundTargets[0]);

            timer += Time.deltaTime;
            if (timer >= data.attackTimeInterval)
            {
                if (data.currentSkill == null)
                {
                    return BTState.Failure;
                }

                if (Vector3.Distance(data.owner.position, data.foundTargets[0].position) <= judgeAttackDistance)
                {
                    // 放技能
                    data.currentSkill.FoundTargets = data.foundTargets;
                    data.npcSkill.UseSkill(data.currentSkill.SkillID);
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

            data.anim.SetBool(data.character.PlayerAnimationParameter.idle, true);
            return BTState.Running;
        }

        /// <summary>
        /// 设置释放位置
        /// </summary>
        /// <param name="bt"></param>
        private void SetPosition(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            data.npcSkill.SetPosition(data.owner.position, data.owner.rotation);
        }

    }
}
