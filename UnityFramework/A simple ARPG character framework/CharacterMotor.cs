using UnityEngine;

namespace Character
{
    /// <summary>
    /// CharacterMotor
    /// </summary>
    public class CharacterMotor : MonoBehaviour
    {
        private CharacterController PlayerController;
        [Tooltip("移动速度")]
        public float MoveSpeed = 3;
        [Tooltip("转向速度")]
        public float RotationSpeed = 10;
        [Tooltip("下落速度")]
        public float DropSpeed = 1;

        private void Awake()
        {
            PlayerController = this.GetComponent<CharacterController>();
        }

        /// <summary>
        /// 注视目标方向旋转
        /// </summary>
        /// <param name="direction">方向</param>
        public void LookAtTarget(Vector3 direction)
        {
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * RotationSpeed);
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="direction">方向</param>
        public void Movement(Vector3 direction)
        {
            Vector3 fix = direction + Vector3.down * DropSpeed;
            PlayerController.Move(fix * Time.deltaTime * MoveSpeed);
        }


    }
}