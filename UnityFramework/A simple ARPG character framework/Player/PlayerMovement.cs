using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// 移动
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Tooltip("移动速度")]
        public float moveSpeed = 4f;
        [Tooltip("跳跃高度")]
        public float jumpHeight = 1f;
        [Tooltip("旋转速度")]
        public float rotationSpeed = 15;
        [Tooltip("重力")]
        public float gravity = -30f;
        [Tooltip("地面检测半径")]
        public float groundCheckRadius = 0.3f;
        [Tooltip("地面检测图层")]
        public LayerMask groundLayer = ~0;

        [Tooltip("允许的最大跳跃次数")]
        public int maxJumpCount = 2;
        /// <summary>
        /// 当前跳跃次数
        /// </summary>
        private int jumpCount = 0;

        /// <summary>
        /// 是否在地面上
        /// </summary>
        public bool IsGrounded { get { return isGrounded; } }
        /// <summary>
        /// 是否在地面上
        /// </summary>
        private bool isGrounded;

        /// <summary>
        /// 待处理的移动输入
        /// </summary>
        private Vector3 pendingMoveInput;
        /// <summary>
        /// 速度
        /// </summary>
        private Vector3 velocity;

        private CharacterController controller;
        private Transform cameraTransform;

        private void Start()
        {
            controller = this.GetComponent<CharacterController>();
            cameraTransform = this.GetComponentInChildren<Camera>().transform;

            rotationSpeed *= 10;
            velocity.y = -10f;
        }

        private void Update()
        {
            CheckGrounded();
            ApplyGravity();
            ApplyMovement();

            pendingMoveInput = Vector3.zero;
        }

        /// <summary>
        /// 移动角色
        /// </summary>
        /// <param name="direction"></param>
        public void Move(Vector3 direction)
        {
            pendingMoveInput = Vector3.ClampMagnitude(direction, 1f);
        }

        /// <summary>
        /// 旋转角色和摄像机
        /// </summary>
        /// <param name="direction"></param>
        public void Look(Vector3 direction)
        {
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            Vector3 targetRotY = this.transform.eulerAngles + Vector3.up * direction.y;
            this.transform.rotation = Quaternion.Slerp(
                this.transform.rotation,
                Quaternion.Euler(targetRotY),
                rotationSpeed * Time.deltaTime
            );

            float eulerX = cameraTransform.eulerAngles.x;
            if (eulerX > 180f)
            {
                eulerX -= 360f;
            }

            Vector3 targetRotX = Vector3.right * (eulerX + direction.x) + Vector3.up * cameraTransform.eulerAngles.y;
            targetRotX.x = Mathf.Clamp(targetRotX.x, -90f, 90f);

            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                Quaternion.Euler(targetRotX),
                rotationSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// 旋转视角
        /// </summary>
        /// <param name="direction"></param>
        public void Look(Vector3 direction, int a = 1)
        {
            // Y轴
            Vector3 targetRotY = this.transform.eulerAngles + Vector3.up * direction.y;
            this.transform.rotation = Quaternion.Slerp(
                this.transform.rotation,
                Quaternion.Euler(targetRotY),
                rotationSpeed * Time.deltaTime
            );

            // X轴
            float ex = cameraTransform.eulerAngles.x;
            if (ex > 180)
            {
                ex -= 360;
            }

            Vector3 targetRotX = Vector3.right * (ex + direction.x) + Vector3.up * cameraTransform.eulerAngles.y;
            if (targetRotX.x <= -90)
            {
                targetRotX.x = -90;
            }
            else if (targetRotX.x >= 90)
            {
                targetRotX.x = 90;
            }

            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                Quaternion.Euler(targetRotX),
                rotationSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// 跳跃
        /// </summary>
        public void Jump()
        {
            if (isGrounded)
            {
                jumpCount = 0;
            }
            else if (jumpCount == 0)
            {
                // 如果角色未跳跃且不在地面上（例如走下边缘掉落），消耗掉第一次跳跃
                jumpCount = 1;
            }

            if (jumpCount < maxJumpCount)
            {
                velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
                jumpCount++;
                isGrounded = false;
            }
        }

        /// <summary>
        /// 检查角色是否在地面上
        /// </summary>
        private void CheckGrounded()
        {
            isGrounded =
                controller.isGrounded ||
                Physics.CheckSphere(this.transform.position, groundCheckRadius, groundLayer);

            if (isGrounded && velocity.y < 0f)
            {
                velocity.y = -10f;
                jumpCount = 0;
            }
        }

        /// <summary>
        /// 应用重力
        /// </summary>
        private void ApplyGravity()
        {
            if (!isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// 应用移动输入
        /// </summary>
        private void ApplyMovement()
        {
            Vector3 move = pendingMoveInput;
            if (move.sqrMagnitude > 0f)
            {
                move = Quaternion.Euler(0f, this.transform.eulerAngles.y, 0f) * move;
            }

            velocity.x = move.x * moveSpeed;
            velocity.z = move.z * moveSpeed;

            Vector3 displacement = velocity * Time.deltaTime;

            controller.Move(displacement);
        }


    }
}
