using UnityEngine;

namespace shaco
{
    public enum Direction
    {
		None = -2,
        Automatic = -1,
        Right,
        Left,
        Down,
        Up,
        Horizontol,
		Vertical
    }

    public class Pivot
    {
        static public Vector3 LeftTop = new Vector3(0, 1);
        static public Vector3 MiddleTop = new Vector3(0.5f, 1);
        static public Vector3 RightTop = new Vector3(1, 1);
        static public Vector3 LeftMiddle = new Vector3(0, 0.5f);
        static public Vector3 Middle = new Vector3(0.5f, 0.5f);
        static public Vector3 RightMiddle = new Vector3(1, 0.5f);
        static public Vector3 LeftBottom = new Vector3(0, 0);
        static public Vector3 MiddleBottom = new Vector3(0.5f, 0);
        static public Vector3 RightBottom = new Vector3(1, 0);
    }
}