using UnityEngine;

namespace ARPGDemo.Character
{
    //可序列化，将当前对象“嵌入”到脚本后，可以在编辑器中显示属性
    //对应的，这将生成一个对象，不需要手动 new AnimationParameter()
    [System.Serializable]
    /// <summary>
    /// AnimationParameter
    /// </summary>
    public class AnimationParameter
    {
        [Tooltip("跑步")]
        public string Run = "run";

        [Tooltip("死亡")]
        public string Death = "death";

        [Tooltip("闲置")]
        public string Idle = "idle";

        [Tooltip("复活")]
        public string Revive = "revive";

    }
}
