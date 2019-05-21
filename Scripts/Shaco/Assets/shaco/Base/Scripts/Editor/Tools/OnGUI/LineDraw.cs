using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace shacoEditor
{
    public partial class GUIHelper
    {
        public class LineDraw
        {
            static public void DrawLineWithArrow(Vector3 start, Vector3 end, float lineWidth = 1, float lineHeight = 10, float distance = -1, float arrowAngle = 150)
            {
                DrawLine(start, end, lineWidth);
                DrawArrow(start, end, lineWidth, lineHeight, distance, arrowAngle);
            }

            public class CrossRectRet
            {
                public Vector3 CrossPointStart = Vector3.zero;
                public Vector3 CrossPointEnd = Vector3.zero;
            }
            static public CrossRectRet DrawLineWithArrowAndCrossRect(Vector3 start, Vector3 end, Rect rectStart, Rect rectEnd, float lineWidth = 1, float lineHeight = 10, float arrowAngle = 150)
            {
                CrossRectRet ret = new CrossRectRet();
                ret.CrossPointStart = GetCrossPointByRect(end, start, rectStart);
                ret.CrossPointEnd = GetCrossPointByRect(start, end, rectEnd);

                DrawLineWithArrow(ret.CrossPointStart, ret.CrossPointEnd, lineWidth, lineHeight, arrowAngle);
                return ret;
            }

            static public Vector3 GetCrossPointByRect(Vector3 start, Vector3 end, Rect rectCross)
            {
                Vector3 leftTop = rectCross.position;
                Vector3 leftDown = new Vector3(leftTop.x, leftTop.y + rectCross.height);
                Vector3 rightTop = new Vector3(leftTop.x + rectCross.width, leftTop.y);
                Vector3 rightDown = new Vector3(leftTop.x + rectCross.width, leftTop.y + rectCross.height);

                Vector3[] crossLines = new Vector3[4];
                bool[] isIntersectLine = new bool[4];
                isIntersectLine[0] = shaco.MathS.GetCrossPoint(start, end, leftTop, leftDown, out crossLines[0]);
                isIntersectLine[1] = shaco.MathS.GetCrossPoint(start, end, leftTop, rightTop, out crossLines[1]);
                isIntersectLine[2] = shaco.MathS.GetCrossPoint(start, end, rightTop, rightDown, out crossLines[2]);
                isIntersectLine[3] = shaco.MathS.GetCrossPoint(start, end, leftDown, rightDown, out crossLines[3]);

                Vector3 crossPoint = crossLines[0];
                for (int i = 1; i < crossLines.Length; ++i)
                {
                    if (isIntersectLine[i])
                    {
                        crossPoint = crossLines[i];
                        break;
                    }
                }
                return crossPoint;
            }

            static public void DrawLine(Vector3 start, Vector3 end, float lineWidth = 1)
            {
#if UNITY_EDITOR

                Vector3 dir = end - start;
                float angle = AngleFixed(start, end);
                float height = dir.magnitude;

                //draw line
                Matrix4x4 matrixOldTmp = GUI.matrix;
                var pivotPoint = new Vector2(start.x + lineWidth / 2, start.y);
                GUIUtility.RotateAroundPivot(angle, pivotPoint);
                GUI.DrawTexture(new Rect(start.x, start.y, lineWidth, height), Texture2D.whiteTexture);
                GUI.matrix = matrixOldTmp;

#endif
            }

            static public void DrawArrow(Vector3 start, Vector3 end, float lineWidth, float lineHeight, float distancePercent = -1, float arrowAngle = 150)
            {
                var startToEndVector = end - start;
                var realDistancePercent = (distancePercent < 0 || distancePercent > 1 ? 1.0f : distancePercent);
                var startPoint = startToEndVector.normalized * (realDistancePercent * startToEndVector.magnitude) + start;

                Vector3 dir = startPoint + (startPoint - start).normalized * lineHeight;
                Vector3 leftPos = shaco.MathS.RotateByPoint(dir, startPoint, -arrowAngle);
                Vector3 rightPos = shaco.MathS.RotateByPoint(dir, startPoint, arrowAngle);

                DrawLine(startPoint, leftPos, lineWidth);
                DrawLine(startPoint, rightPos, lineWidth);
            }

            static public float AngleFixed(Vector3 src, Vector3 des)
            {
                Vector3 dir = des - src;
                float angle = Vector3.Angle(dir, Vector3.up);
                if (src.x < des.x)
                    angle = -angle;

                return angle;
            }
        }
    }

}