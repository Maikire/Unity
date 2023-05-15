using ARPGDemo.Character;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Common
{
    /// <summary>
    /// 动画事件行为类
    /// </summary>
    public class AnimationEventBehaviour : MonoBehaviour
    {
        //在特定时机取消动画或触发事件
        //实际的使用过程中，需要什么事件就在这个脚本里添加

        [Serializable]
        public class AttackEventHandler : UnityEvent { }

        private Animator Anim;
        public AttackEventHandler TargetAttack;
        public AttackEventHandler DirectionAttack;

        private void Awake()
        {
            Anim = this.GetComponent<Animator>();
        }

        //需要手动添加动画事件

        /// <summary>
        /// 取消动画时触发
        /// </summary>
        /// <param name="animParam">AnimatorParameter</param>
        private void OnCancelAnim(string animParam)
        {
            Anim.SetBool(animParam, false);
        }

        /// <summary>
        /// 近战攻击时触发
        /// </summary>
        private void OnMeleeAttack()
        {
            TargetAttack?.Invoke();
        }

        /// <summary>
        /// 远程攻击时触发
        /// </summary>
        private void OnRemoteAttack()
        {
            DirectionAttack?.Invoke();
        }

    }
}
