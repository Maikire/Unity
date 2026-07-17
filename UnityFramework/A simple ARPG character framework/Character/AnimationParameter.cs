using System;
using UnityEngine;

namespace CharacterSystem
{
    //可序列化，将当前对象“嵌入”到脚本后，可以在编辑器中显示属性
    //对应的，这将生成一个对象，不需要手动 new AnimationParameter()
    
    /// <summary>
    /// AnimationParameter
    /// </summary>
    [Serializable]
    public class AnimationParameter
    {
        [Tooltip("走路")]
        public string Walk = "Walk";

        [Tooltip("跑步")]
        public string Run = "Run";

        [Tooltip("跳跃")]
        public string Jump = "Jump";

        [Tooltip("二次跳跃")]
        public string DoubleJump = "DoubleJump";

        [Tooltip("滑行")]
        public string Slide = "Slide";

        [Tooltip("贴墙跳跃")]
        public string WallJump = "SlideJump";

        [Tooltip("坠落")]
        public string Fall = "Fall";

        [Tooltip("冲刺")]
        public string Dash = "Dash";

        [Tooltip("闲置")]
        public string Idle = "Idle";

        [Tooltip("受伤")]
        public string Injure = "Injure";

        [Tooltip("死亡")]
        public string Die = "Die";

        [Tooltip("复活")]
        public string Revive = "Revive";

    }
}
