using UnityEngine;

namespace shaco
{

    [System.Serializable]
    public class Bezier : System.Object
    {
        /// <summary>  
        /// 线性贝赛尔曲线  
        /// </summary>  
        /// <param name="P0"></param>  
        /// <param name="P1"></param>  
        /// <param name="t"> 0.0 >= t <= 1.0 </param>  
        /// <returns></returns>  
        public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, float t)
        {
            Vector3 B = Vector3.zero;
            float t1 = (1 - t);
            B = t1 * P0 + P1 * t;
            //B.y = t1*P0.y + P1.y*t;  
            //B.z = t1*P0.z + P1.z*t;  
            return B;
        }

        /// <summary>  
        /// 二次贝塞尔曲线
        /// </summary>  
        /// <param name="P0"></param>  
        /// <param name="P1"></param>  
        /// <param name="P2"></param>  
        /// <param name="t">0.0 >= t <= 1.0 </param>  
        /// <returns></returns>  
        public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, float t)
        {
            Vector3 B = Vector3.zero;
            float t1 = (1 - t) * (1 - t);
            float t2 = t * (1 - t);
            float t3 = t * t;
            B = P0 * t1 + 2 * t2 * P1 + t3 * P2;
            //B.y = P0.y*t1 + 2*t2*P1.y + t3*P2.y;  
            //B.z = P0.z*t1 + 2*t2*P1.z + t3*P2.z;  
            return B;
        }

        /// <summary>  
        /// 三次贝塞尔曲线
        /// </summary>  
        /// <param name="P0"></param>  
        /// <param name="P1"></param>  
        /// <param name="P2"></param>  
        /// <param name="P3"></param>  
        /// <param name="t">0.0 >= t <= 1.0 </param>  
        /// <returns></returns>  
        public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
        {
            Vector3 B = Vector3.zero;
            float t1 = (1 - t) * (1 - t) * (1 - t);
            float t2 = (1 - t) * (1 - t) * t;
            float t3 = t * t * (1 - t);
            float t4 = t * t * t;
            B = P0 * t1 + 3 * t2 * P1 + 3 * t3 * P2 + P3 * t4;
            //B.y = P0.y*t1 + 3*t2*P1.y + 3*t3*P2.y + P3.y*t4;  
            //B.z = P0.z*t1 + 3*t2*P1.z + 3*t3*P2.z + P3.z*t4;  
            return B;
        }

        static public System.Collections.Generic.List<Vector3> GetPath(Vector3 beginPoint, Vector3 endPoint, int smooth, params Vector3[] controlPoints)
        {
            var retValue = new System.Collections.Generic.List<Vector3>();
            if (0 == smooth)
                return retValue;
            float t = 1 / (float)smooth;

            if (controlPoints.Length == 0)
            {
                for (int i = 0; i < smooth; ++i)
                {
                    var pathPoint = Bezier.BezierCurve(beginPoint, endPoint, (float)(i * t));
                    retValue.Add(pathPoint);
                }
            }
            else if (controlPoints.Length == 1)
            {
                for (int i = 0; i < smooth; ++i)
                {
                    var pathPoint = Bezier.BezierCurve(beginPoint, controlPoints[0], endPoint, (float)(i * t));
                    retValue.Add(pathPoint);
                }
            }
            else
            {
                for (int i = 0; i < smooth; ++i)
                {
                    var pathPoint = Bezier.BezierCurve(beginPoint, controlPoints[0], controlPoints[1], endPoint, (float)(i * t));
                    retValue.Add(pathPoint);
                }
            }
            return retValue;
        }

        // static public Bezier[] GetBeziers(Vector3 beginPoint, Vector3 endPoint, params Vector3[] controlPoints)
        // {
        //     Bezier[] retValue = null;

        //     if (controlPoints.Length > 2)
        //     {
        //         var beginPointTmp = beginPoint;
        //         var endPointTmp = controlPoints[1];

        //         retValue = new Bezier[controlPoints.Length];
        //         int loopCount = controlPoints.Length;
        //         for (int i = 0; i < loopCount; ++i)
        //         {
        //             retValue[i] = new shaco.Bezier(beginPointTmp, controlPoints[i], endPointTmp);
        //             beginPointTmp = controlPoints[i];

        //             if (i + 2 >= loopCount)
        //                 endPointTmp = endPoint;
        //             else
        //                 endPointTmp = controlPoints[i + 2];
        //         }
        //     }
        //     else if (controlPoints.Length == 2)
        //         retValue = new Bezier[] { new shaco.Bezier(beginPoint, controlPoints[0], controlPoints[1], endPoint) };
        //     else
        //         retValue = new Bezier[] { new shaco.Bezier(beginPoint, controlPoints[0], endPoint) };

        //     return retValue;
        // }
    }
}