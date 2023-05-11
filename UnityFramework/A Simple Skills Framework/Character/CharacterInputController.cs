using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Character
{
    [RequireComponent(typeof(CharacterMotor), typeof(PlayerStatus))]
    /// <summary>
    /// CharacterInputController
    /// </summary>
    public class CharacterInputController : MonoBehaviour
    {
        [Tooltip("Joystick")]
        public VariableJoystick MainJoystick;
        [HideInInspector]
        [Tooltip("true：角色可以移动")]
        public bool IsMove;
        private CharacterMotor Player;
        private PlayerStatus Status;
        private Animator PlayerAnimator;

        private void Awake()
        {
            Player = this.GetComponent<CharacterMotor>();
            Status = this.GetComponent<PlayerStatus>();
            PlayerAnimator = this.GetComponentInChildren<Animator>();
            IsMove = true;
        }

        private void Update()
        {
            JoystickMove();
        }

        /// <summary>
        /// 调用移动
        /// </summary>
        public void JoystickMove()
        {
            if (!IsMove) return;

            Vector3 direction = Vector3.forward * MainJoystick.Vertical + Vector3.right * MainJoystick.Horizontal;

            if (direction != Vector3.zero)
            {
                //旋转
                Player.LookAtTarget(direction);

                //移动
                Player.Movement(direction);

                //移动动画
                PlayerAnimator.SetBool(Status.PlayerAnimationParameter.Run, true);
                PlayerAnimator.SetBool(Status.PlayerAnimationParameter.Idle, false);
            }
            else
            {
                //闲置动画
                PlayerAnimator.SetBool(Status.PlayerAnimationParameter.Idle, true);
                PlayerAnimator.SetBool(Status.PlayerAnimationParameter.Run, false);
            }
        }


    }
}