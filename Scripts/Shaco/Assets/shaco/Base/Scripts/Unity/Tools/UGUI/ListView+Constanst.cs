using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public partial class ListView
    {
        public enum CenterLayoutType
        {
            NoCenter,
            CenterHorizontalOnly,
            CenterVerticalOnly,
            Center
        }

        [System.Serializable]
        public class Item
        {
            public GameObject current;
            public Item prev;
            public Item next;
        }

        public delegate int SortCompareFunc(Item left, Item right);
        class _SortCompareFunc : IComparer<Item>
        {
            private SortCompareFunc compareFunc = null;
            public _SortCompareFunc(SortCompareFunc comp)
            {
                compareFunc = comp;
            }
            public int Compare(Item left, Item right)
            {
                return compareFunc == null ? 0 : compareFunc(left, right);
            }
        }

        private readonly float CHECK_OUT_OF_BOUNDS_FIXED_RATE = 0.5f;
    }
}
