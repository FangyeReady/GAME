using UnityEngine;
using System.Collections;

namespace shaco
{
    public class MathS : shaco.Base.MathS
    {
        static public Vector2 RotateByPoint(Vector2 pointSrc, Vector2 pointDes, float angle)
        {
            //negative
            angle = -angle;

            //angle to raidan
            float radian = angle * 3.14159265358979323846264338327950288f / 180;

            float x_new = (pointSrc.x - pointDes.x) * Mathf.Cos(radian) - (pointSrc.y - pointDes.y) * Mathf.Sin(radian) + pointDes.x;
            float y_new = (pointSrc.x - pointDes.x) * Mathf.Sin(radian) + (pointSrc.y - pointDes.y) * Mathf.Cos(radian) + pointDes.y;

            return new Vector2(x_new, y_new);
        }

        static public Vector3 RotateByPoint(Vector3 pointSrc, Vector3 pointDes, Vector3 axis, float angle)
        {
            Vector3 ret = pointSrc;
            Vector2 retTmp = Vector2.zero;
            int directionNum = 0;
            directionNum += axis.x != 0 ? 1 : 0;
            directionNum += axis.y != 0 ? 1 : 0;
            directionNum += axis.z != 0 ? 1 : 0;
            if (directionNum != 1)
            {
                Log.Error("axis number must be 1");
                return ret;
            }

            if (axis.x != 0)
            {
                retTmp = RotateByPoint(new Vector2(pointSrc.y, pointSrc.z), new Vector2(pointDes.y, pointDes.z), angle);
                ret.y = retTmp.x;
                ret.z = retTmp.y;
            }
            if (axis.y != 0)
            {
                retTmp = RotateByPoint(new Vector2(pointSrc.x, pointSrc.z), new Vector2(pointDes.x, pointDes.z), angle);
                ret.x = retTmp.x;
                ret.z = retTmp.y;
            }
            if (axis.z != 0)
            {
                retTmp = RotateByPoint(new Vector2(pointSrc.x, pointSrc.y), new Vector2(pointDes.x, pointDes.y), angle);
                ret.x = retTmp.x;
                ret.y = retTmp.y;
            }
            return ret;
        }

		/// <summary>
		/// get cross point of pointSrc1->pointSrc2 and pointDes1->pointDes2
		/// </summary>
		/// <param name="pointSrc1"></param>
		/// <param name="pointSrc2"></param>
		/// <param name="pointDes1"></param>
		/// <param name="pointDes2"></param>
		/// <param name="crossPoint"></param> 
		/// <returns></returns> if has cross point return true, otherwise return false
		public static bool GetCrossPoint(Vector3 pointSrc1, Vector3 pointSrc2, Vector3 pointDes1, Vector3 pointDes2, out Vector3 crossPoint)
		{
			bool ret = true;
			float s1 = fArea(pointSrc1, pointSrc2, pointDes1), s2 = fArea(pointSrc1, pointSrc2, pointDes2);
			crossPoint = new Vector3((pointDes2.x * s1 + pointDes1.x * s2) / (s1 + s2), (pointDes2.y * s1 + pointDes1.y * s2) / (s1 + s2));

			ret = IntersectLine(pointSrc1, pointSrc2, pointDes1, pointDes2);
			return ret;
		}
		public static float Cross(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
		{
			return (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
		}
		public static float Area(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			return Cross(p1, p2, p1, p3);
		}
		public static float fArea(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			return Mathf.Abs(Area(p1, p2, p3));
		}
	
		static public double Mult(Vector3 a, Vector3 b, Vector3 c)
		{
			return (a.x - c.x) * (b.y - c.y) - (b.x - c.x) * (a.y - c.y);
		}

		/// <summary>
		/// Intersects the line of pointSrc1->pointSrc2 and pointDes1->pointDes2
		/// </summary>
		/// <returns><c>true</c>, if line was intersected, <c>false</c> otherwise.</returns>
		/// <param name="pointSrc1">Point src1.</param>
		/// <param name="pointDes1">Point des1.</param>
		/// <param name="pointSrc2">Point src2.</param>
		/// <param name="pointDes2">Point des2.</param>
		static public bool IntersectLine(Vector3 pointSrc1, Vector3 pointDes1, Vector3 pointSrc2, Vector3 pointDes2)
		{
			if (Mathf.Max(pointSrc1.x, pointDes1.x) < Mathf.Min(pointSrc2.x, pointDes2.x))
			{
				return false;
			}
			if (Mathf.Max(pointSrc1.y, pointDes1.y) < Mathf.Min(pointSrc2.y, pointDes2.y))
			{
				return false;
			}
			if (Mathf.Max(pointSrc2.x, pointDes2.x) < Mathf.Min(pointSrc1.x, pointDes1.x))
			{
				return false;
			}
			if (Mathf.Max(pointSrc2.y, pointDes2.y) < Mathf.Min(pointSrc1.y, pointDes1.y))
			{
				return false;
			}
			if (Mult(pointSrc2, pointDes1, pointSrc1) * Mult(pointDes1, pointDes2, pointSrc1) < 0)
			{
				return false;
			}
			if (Mult(pointSrc1, pointDes2, pointSrc2) * Mult(pointDes2, pointDes1, pointSrc2) < 0)
			{
				return false;
			}
			return true;
		} 
    }
}
