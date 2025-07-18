using Common;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// UI输入控制器
    /// </summary>
    public class UIInputController : MonoSingleton<UIInputController>
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIController.Instance.Back();
            }
        }


    }
}
