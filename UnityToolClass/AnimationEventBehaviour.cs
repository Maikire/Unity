using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 动画事件行为类
    /// </summary>
    public class AnimationEventBehaviour : MonoBehaviour
    {
        //在特定时机取消动画或触发事件
        //实际的使用过程中，需要什么事件就在这个脚本里添加

        private Animator Anim;
        public event Action AttackHandler;

        private void Start()
        {
            Anim = GetComponent<Animator>();
        }

        //需要手动添加动画事件
        private void OnCancelAnim(string animParam)
        {
            Anim.SetBool(animParam, false);
        }

        //需要手动添加动画事件
        private void OnAttack()
        {
            AttackHandler?.Invoke();
        }

    }
}
