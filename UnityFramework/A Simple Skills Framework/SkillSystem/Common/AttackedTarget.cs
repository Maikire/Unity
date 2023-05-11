using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Skill
{
    public class AttackedTarget
    {
        [Tooltip("被攻击过的目标")]
        public Transform Target;
        [Tooltip("被攻击的次数")]
        public int Times;

        public AttackedTarget(Transform Target, int Number)
        {
            this.Target = Target;
            this.Times = Number;
        }
    }
}
