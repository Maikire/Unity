using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterSystem
{
    /// <summary>
    /// 角色控制
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerMovement playerMovement;
        private PlayerInput playerInput;
        private InputActionAsset actionAsset;
        private InputActionMap actionMap_Player;
        private InputActionMap actionMap_UI;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction escapeAction;

        protected void Awake()
        {
            playerMovement = this.GetComponent<PlayerMovement>();
            playerInput = this.GetComponent<PlayerInput>();
        }

        protected void Start()
        {
            Config();
            AddEvent();
        }

        protected void Update()
        {
            if (actionMap_Player.enabled)
            {
                Move();
                Look();
            }
        }

        private void OnDestroy()
        {
            RemoveEvent();
        }

        /// <summary>
        /// 配置
        /// </summary>
        private void Config()
        {
            actionAsset = playerInput.actions;
            actionMap_Player = actionAsset.FindActionMap("Player", false);
            actionMap_UI = actionAsset.FindActionMap("UI", false);

            foreach (var item in actionAsset.actionMaps)
            {
                item.Disable();
            }

            actionMap_Player.Enable();
            //actionMap_UI.Enable();

            moveAction = actionMap_Player.FindAction("Move", false);
            lookAction = actionMap_Player.FindAction("Look", false);
            jumpAction = actionMap_Player.FindAction("Jump", false);
            escapeAction = actionMap_Player.FindAction("Escape", false);
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        private void AddEvent()
        {
            jumpAction.performed += OnJump;
            escapeAction.performed += OnEscape;
        }

        private void RemoveEvent()
        {
            jumpAction.performed -= OnJump;
            escapeAction.performed -= OnEscape;
        }

        /// <summary>
        /// 移动
        /// </summary>
        public void Move()
        {
            Vector2 moveVector = moveAction.ReadValue<Vector2>();

            // 如果只需要水平方向的移动，就只取x轴
            //Vector2 direction = Vector2.right * moveVector;
            Vector3 direction = new Vector3(moveVector.x, 0, moveVector.y);

            playerMovement.Move(direction);
        }

        /// <summary>
        /// 旋转视角
        /// </summary>
        /// <param name="context"></param>
        private void Look()
        {
            Vector2 lookVector = lookAction.ReadValue<Vector2>();
            Vector3 direction = new Vector3(-lookVector.y, lookVector.x, 0);
            direction.Normalize();

            playerMovement.Look(direction, 2);
        }

        /// <summary>
        /// 跳跃
        /// </summary>
        /// <param name="context"></param>
        private void OnJump(InputAction.CallbackContext context)
        {
            playerMovement.Jump();
        }

        /// <summary>
        /// ESC
        /// </summary>
        /// <param name="context"></param>
        private void OnEscape(InputAction.CallbackContext context)
        {
            CursorController.Instance.Escape(playerInput.currentControlScheme);
        }


    }
}
