using UnityEngine;
using System.Collections;

namespace shaco
{
    public class CurveMoveBy : CurveMoveTo
    {
        private GameObject _tempMoveTarget = null;

        static new public CurveMoveBy Create(Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
        {
            if (controlPoints.Length > 2)
            {
                shaco.Log.Error("CurveMoveBy Create warning: max support 2 control point !");
            }

            CurveMoveBy retValue = new CurveMoveBy();
            retValue._beginPoint = beginPoint;
            retValue._controlPoints = controlPoints;
            retValue._endPoint = endPoint;
            retValue.Duration = duration;

            return retValue;
        }

        ~CurveMoveBy()
        {
            if (_tempMoveTarget != null)
            {
                MonoBehaviour.DestroyImmediate(_tempMoveTarget);
                _tempMoveTarget = null;
            }
        }

        public override void RunAction(GameObject target)
        {
            if (null == _tempMoveTarget)
            {
                _tempMoveTarget = new GameObject("TempMoveTarget");
            }

            _isRelativeMove = true;
            base.RunAction(_tempMoveTarget);

            var realMoveTarget = target;
            var relativeEndPosition = (useWorldPosition ? realMoveTarget.transform.position : realMoveTarget.transform.localPosition) + (_endPoint - _beginPoint);
            var prevPosition = _beginPoint;

            this.onFrameFunc += (float percent) =>
            {
                if (useWorldPosition)
                {
                    var moveOffsetTmp = this._tempMoveTarget.transform.position - prevPosition;
                    realMoveTarget.transform.position += moveOffsetTmp;
                    prevPosition = this._tempMoveTarget.transform.position;
                }
                else
                {
                    var moveOffsetTmp = this._tempMoveTarget.transform.localPosition - prevPosition;
                    realMoveTarget.transform.localPosition += moveOffsetTmp;
                    prevPosition = this._tempMoveTarget.transform.localPosition;
                }
            };
            this.onCompleteFunc += (shaco.ActionS ac) =>
            {
                if (useWorldPosition)
                    realMoveTarget.transform.position = relativeEndPosition;
                else
                    realMoveTarget.transform.localPosition = relativeEndPosition;

                MonoBehaviour.DestroyImmediate(_tempMoveTarget);
                _tempMoveTarget = null;
            };
        }

        public override ActionS Clone()
        {
            return CurveMoveBy.Create(_beginPoint, _endPoint, Duration, _controlPoints);
        }

        public override ActionS Reverse()
        {
            var pointReverse = new Vector3[_controlPoints.Length];
            for (int i = 0; i < _controlPoints.Length; ++i)
                pointReverse[_controlPoints.Length - i - 1] = _controlPoints[i];
            return CurveMoveBy.Create(_endPoint, _beginPoint, Duration, pointReverse);
        }
    }
}