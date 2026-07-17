using Common;
using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// 鼠标指针控制
    /// </summary>
    public class CursorController : MonoSingleton<CursorController>
    {
        /// <summary>
        /// true: 打开UI
        /// </summary>
        private bool isUI = false;

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// ESC
        /// </summary>
        /// <param name="currentControlScheme"></param>
        public void Escape(string currentControlScheme)
        {
            if (currentControlScheme == "Keyboard&Mouse")
            {
                if (isUI)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }

            isUI = !isUI;
        }

    }
}
