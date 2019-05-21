using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace shaco
{
    public class DelayTime : ActionS
    {
        static public DelayTime Create(float duration)
        {
            DelayTime ret = new DelayTime();
            ret.Duration = duration;

            return ret;
        }

        public override ActionS Clone()
        {
            return DelayTime.Create(Duration);
        }
    }
}
