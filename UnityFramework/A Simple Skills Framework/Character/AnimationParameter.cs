using System.Collections;
using System.Collections.Generic;
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
        public string Run = "run";
        public string Death = "death";
        public string Idle = "idle";
        //public string Attack1 = "attack1";
        //public string Attack2 = "attack2";
        //public string Attack3 = "attack3";


    }
}
