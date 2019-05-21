using System;
using System.Collections;

namespace shaco.Base
{
    public partial class MathS
	{
        /// <summary>
        /// Is point on the left or right of the line
        /// </summary>
        /// <returns>return value < 0 is left, otherwise is right
        static public double PointToSideOfLine(double x1, double y1, double x2, double y2, double pointX, double pointY)
        {
            double ret = (x1 - pointX) * (y2 - pointY) - (y1 - pointY) * (x2 - pointX);
            return ret;
        }
		static public float PointToSideOfLine(float x1, float y1, float x2, float y2, float pointX, float pointY)
        {
            return PointToSideOfLine(x1, y1, x2, y2, pointX, pointY);
        }

        /// <summary>
        //Is the point in line
        /// </summary>
        //range: Allowable error value, if not need set it as 0
        public static bool IsPointInLine(double x1, double y1, double x2, double y2, double pointX, double pointY, double range = 1)
        {
            //if the point in line outside return false
            double cross = (x2 - x1) * (pointX - x1) + (y2 - y1) * (pointY - y1);
            if (cross <= 0) return false;
            double d2 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
            if (cross >= d2) return false;

            double r = cross / d2;
            double px = x1 + (x2 - x1) * r;
            double py = y1 + (y2 - y1) * r;

            return Math.Sqrt((pointX - px) * (pointX - px) + (py - pointY) * (py - pointY)) <= range;
        }

        /// <summary>
        //Get the point to line distance
        /// </summary>
		static public double GetDistancePointToLine(double lineX1, double lineY1, double lineX2, double lineY2, double pointX, double pointY)
        {
            double a = lineY2 - lineY1;
            double b = lineX1 - lineX2;
            double c = lineX2 * lineY1 - lineX1 * lineY2;
            return (Math.Abs(a * pointX + b * pointY + c)) / (Math.Sqrt(a * a + b * b));
        }

        /// <summary>
        //Get the value between min and max
        /// </summary>
        static public double GetValueByRange(double cur, double min, double max)
        {
            double ret = cur;
            if (ret < min)
                ret = min;
            if (ret > max)
                ret = max;
            return ret;
        }

        //Gets the current Y axis value in the line equation
        static public float GetYValueOfLineEquation(float x1, float y1, float x2, float y2, float xValue)
        {
            float y = (xValue - x1) / (x2 - x1) * (y2 - y1) + y1;
            return y;
        }

        //Gets the current Y axis value in the parabola equation
        static public float GetYValueOfParabolaEquation(float x1, float y1, float x2, float y2, float x3, float y3, float xValue)
        {
            //y = ax^2 + bx + c
            float A = GetParabolaEquation_A(x1, y1, x2, y2, x3, y3);
            float B = GetParabolaEquation_B(x1, y1, x2, y2, x3, y3);
            float C = GetParabolaEquation_C(x1, y1, x2, y2, x3, y3);
            float y = A * (xValue * xValue) + B * xValue + C;
            return y;
        }
	}
}