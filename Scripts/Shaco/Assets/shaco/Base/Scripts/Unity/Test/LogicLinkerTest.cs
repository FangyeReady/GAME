using UnityEngine;
using System.Collections;
using shaco;

namespace shaco.Test
{
    public class LogicLinkerTest : MonoBehaviour
    {
        public LogicLinker ScriptLogicLinker;

        void Start()
        {
            if (ScriptLogicLinker == null)
                ScriptLogicLinker = this.GetComponent<LogicLinker>();
        }

        void Update()
        {
            //        if (Input.GetKeyUp(KeyCode.A))
            //            va = !va;
            //        if (Input.GetKeyUp(KeyCode.B))
            //            vb = !vb;
            //        if (Input.GetKeyUp(KeyCode.C))
            //            vc = !vc;
            if (Input.GetKeyUp(KeyCode.Space))
            {
                var item = ScriptLogicLinker.getLinkItem("A");
                ScriptLogicLinker.tryExcute(item, null, new object[] { true }, null);
            }
        }

        public bool a(bool va)
        {
            return va;
        }

        public void A()
        {
            Log.Info("A");
        }

        public bool b(double vb)
        {
            return vb == 1.0;
        }

        public void B()
        {
            Log.Info("B");
        }

        public bool c(string vc)
        {
            return vc == 1.0f.ToString();
        }

        public void C()
        {
            Log.Info("C");
        }

        public bool d(Vector3 v3)
        {
            return v3.x == 1 && v3.y == 2 && v3.z == 3;
        }

        public void D()
        {
            Log.Info("D");
        }
    }
}
