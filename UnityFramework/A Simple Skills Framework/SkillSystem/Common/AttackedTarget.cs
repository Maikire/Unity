using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.Skill
{
    public class AttackedTarget
    {
        public Transform Target;
        public int Number;

        public AttackedTarget(Transform Target, int Number)
        {
            this.Target = Target;
            this.Number = Number;
        }
    }
}
