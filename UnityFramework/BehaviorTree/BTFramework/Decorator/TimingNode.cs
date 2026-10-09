using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 计时节点（用于控制子节点处于 Running 状态的时间）
    /// </summary>
    public class TimingNode : ControlNodes
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private BTNode child;
        /// <summary>
        /// 最大时间
        /// </summary>
        private float maxTime;
        /// <summary>
        /// 当前时间
        /// </summary>
        private float currentTime = 0;

        public TimingNode(float time)
        {
            this.maxTime = time;
        }

        public override void AddChild(BTNode child)
        {
            base.AddChild(child);
            this.child = child;
        }

        public override void EnterNode(BehaviorTree bt)
        {
            base.EnterNode(bt);
            currentTime = 0;
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            currentTime += Time.deltaTime;
            if (currentTime < maxTime)
            {
                if (State != BTState.Running)
                {
                    child.EnterNode(bt);
                }

                State = child.TickNode(bt);

                switch (State)
                {
                    case BTState.Success:
                        child.ExitNode(bt);
                        currentTime = 0;
                        return BTState.Success;

                    case BTState.Failure:
                        child.ExitNode(bt);
                        currentTime = 0;
                        return BTState.Failure;

                    case BTState.Running:
                        return BTState.Running;

                    case BTState.Abort:
                        child.Abort(bt);
                        currentTime = 0;
                        return BTState.Abort;

                    default:
                        child.ExitNode(bt);
                        currentTime = 0;
                        return State = BTState.Success;
                }
            }
            else
            {
                child.ExitNode(bt);
                currentTime = 0;
                return State = BTState.Success;
            }
        }

        public override void ExitNode(BehaviorTree bt)
        {
            base.ExitNode(bt);
            currentTime = 0;
        }

    }
}
