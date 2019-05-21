using UnityEngine;
using System.Collections;

namespace shaco
{
    public class CurveMoveTo : ActionS
    {
        public bool useWorldPosition = true;

        protected Vector3 _beginPoint;
        protected Vector3[] _controlPoints;
        protected Vector3 _endPoint;

        protected bool _isRelativeMove = false;

        static public CurveMoveTo Create(Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
        {
            if (controlPoints.Length > 2)
            {
                shaco.Log.Error("CurveMoveTo Create warning: max support 2 control point !");
            }

            CurveMoveTo retValue = new CurveMoveTo();
            retValue._beginPoint = beginPoint;
            retValue._controlPoints = controlPoints;
            retValue._endPoint = endPoint;
            retValue.Duration = duration;

            return retValue;
        }

        public override void RunAction(GameObject target)
        {
            base.RunAction(target);

            if (!_isRelativeMove)
            {
                onCompleteFunc += (ActionS action) =>
                {
                    if (useWorldPosition)
                    {
                        if (_endPoint != Target.transform.position)
                            Target.transform.position = _endPoint;
                    }
                    else
                    {
                        if (_endPoint != Target.transform.localPosition)
                            Target.transform.localPosition = _endPoint;
                    }
                };
            }
        }

        public override float UpdateAction(float prePercent, float delayTime)
        {
            var eplasedPercent = this.GetElapsedPercent();
            Vector3 newPos = Vector3.zero;

            if (_controlPoints.Length == 0)
                newPos = Bezier.BezierCurve(_beginPoint, _endPoint, eplasedPercent);
            else if (_controlPoints.Length == 1)
                newPos = Bezier.BezierCurve(_beginPoint, _controlPoints[0], _endPoint, eplasedPercent);
            else
                newPos = Bezier.BezierCurve(_beginPoint, _controlPoints[0], _controlPoints[1], _endPoint, eplasedPercent);

            if (useWorldPosition)
                Target.transform.position = newPos;
            else
                Target.transform.localPosition = newPos;

            return base.UpdateAction(prePercent, delayTime);
        }

        public override ActionS Clone()
        {
            return CurveMoveTo.Create(_beginPoint, _endPoint, Duration, _controlPoints);
        }

        public override ActionS Reverse()
        {
            var pointReverse = new Vector3[_controlPoints.Length];
            for (int i = 0; i < _controlPoints.Length; ++i)
                pointReverse[_controlPoints.Length - i - 1] = _controlPoints[i];
            return CurveMoveTo.Create(_endPoint, _beginPoint, Duration, pointReverse);
        }
    }
}