using Common;
using UnityEngine;

namespace Default.UI
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
