using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace shaco
{
    public class MethodInfoEx
    {
        public object target = null;
        public MethodInfo method = null;
    }

    [System.Serializable]
    public class UIPrefab
    {
        public GameObject prefab = null;
        public Component[] componets = null;
        public List<MethodInfoEx> methodOnPreLoad = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnInit = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnOpen = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnHide = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnResume = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnClose = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnCustom = new List<MethodInfoEx>(0);

        public void ClearAllMethod()
        {
            methodOnPreLoad.Clear();
            methodOnInit.Clear();
            methodOnOpen.Clear();
            methodOnHide.Clear();
            methodOnResume.Clear();
            methodOnClose.Clear();
            methodOnCustom.Clear();
        }

        public UIPrefab(params Component[] componets)
        {
            if (null == componets)
                return;

            this.componets = componets;
            this.prefab = componets.Length > 0 ? componets[0].gameObject : null;
        }
    }
}

