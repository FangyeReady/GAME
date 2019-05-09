using System.Collections;

namespace shaco.Base
{
    public partial class MathS
    {
		static private float GetParabolaEquation_A(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            float A = x1 * x1 - x2 * x2;
            float B = x1 - x2;
            float A1 = x2 * x2 - x3 * x3;
            float B1 = x2 - x3;

            float ret = ((B1 / B) * (y1 - y2) - y2 + y3)
                        / (A * (B1 / B) - A1);
            return ret;
        }

        static private float GetParabolaEquation_B(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            float A = x1 * x1 - x2 * x2;
            float B = x1 - x2;
            float A1 = x2 * x2 - x3 * x3;
            float B1 = x2 - x3;

            float ret = ((A1 / A) * (y1 - y2) - y2 + y3)
                        / (B * (A1 / A) - B1);
            return ret;
        }

        static private float GetParabolaEquation_C(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            float A = GetParabolaEquation_A(x1, y1, x2, y2, x3, y3);
            float B = GetParabolaEquation_B(x1, y1, x2, y2, x3, y3);
            float ret = y1 - A * x1 * x1 - B * x1;
            return ret;
        }
    }
}