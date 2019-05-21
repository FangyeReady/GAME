using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    [RequireComponent(typeof(LineRenderer))]
    public class CurveMoveComponent : MonoBehaviour
    {
        public enum ControlPointMode
        {
            NoControlPoint,
            OneControlPoint,
            TwoControlPoint
        }

        public enum MovePositionMode
        {
            Absolute,
            Relative
        }

        [System.Serializable]
        public class MoveInfo
        {
            public Transform beginPoint;
            public Transform endPoint;
            public ControlPointMode controlPointMode = ControlPointMode.OneControlPoint;
            public List<Transform> controlPoints = new List<Transform>();

            public MoveInfo()
            {
                controlPoints.AddRange(new Transform[(int)ControlPointMode.TwoControlPoint]);
            }
        }

        public MovePositionMode movePositionMode = MovePositionMode.Absolute;

        [HideInInspector]
        public bool editorMode = true;

        [HideInInspector]
        public Transform moveTarget;

        [HideInInspector]
        public float moveDuration = 1.0f;

        [HideInInspector]
        public List<MoveInfo> movePaths = new List<MoveInfo>();

        private LineRenderer _lineRenderer;
        private ActionS _editorUpdateAction = null;
        private ActionS _currentMoveAction = null;
        //数字越大编辑器模式下绘制的线条越顺滑
        private int smooth = 100;
        private int _currentSmooth = 0;

        void OnEnable()
        {
#if !UNITY_EDITOR
            editorMode = false;
#else
            if (Application.isPlaying)
                editorMode = false;
#endif
        }

        void Start()
        {

            CheckComponents();
        }

        void Update()
        {
            UpdateLineRenderPositions();
        }

        public void CheckComponents()
        {
            CheckLineRenderComponent();

            if (movePaths.Count > 0)
            {
                moveTarget = CheckPrimitivePoint("move", moveTarget, PrimitiveType.Cube, transform.position.x, transform.position.y);
            }
            else
            {
                // SafeDestroyIfParent(this, moveTarget);
                // moveTarget = null;
            }

            UnityHelper.ForeachChildren(this.gameObject, (int index, GameObject child) =>
            {
                child.SetActive(editorMode);
                return true;
            });

            if (!editorMode)
            {
                SafeStopEditorUpdate();
            }
            else
            {
                if (!Application.isPlaying && null == _editorUpdateAction)
                {
                    _editorUpdateAction = Repeat.CreateRepeatForver(DelayTime.Create(999999));
                    _editorUpdateAction.onFrameFunc = (float percent) =>
                    {
                        this.UpdateLineRenderPositions();
                    };
                    _editorUpdateAction.RunAction(this.gameObject);
                }
            }

            if (movePaths.Count > 0)
            {
                CheckMovePathComponent(movePaths[0], null);
                for (int i = 1; i < movePaths.Count; ++i)
                {
                    CheckMovePathComponent(movePaths[i], movePaths[i - 1].endPoint);
                }
            }
            else
            {
                UnityHelper.RemoveChildren(this.gameObject);
                SafeStopEditorUpdate();
            }
        }

        public void PlayMoveAction()
        {
            if (movePaths.Count == 0)
                return;
            PlayMoveAction(this.moveTarget, movePaths[movePaths.Count - 1].endPoint.transform.position, this.moveDuration);
        }

        public void PlayMoveAction(Transform moveTarget, Vector3 moveEndWorldPosition, float duration)
        {
            if (movePaths.Count == 0)
            {
                Log.Warning("CurveMoveCompnent PlayMoveAction warning: no move path");
                return;
            }

            SafeStopMoveAction();

            //set action values
            if (null != moveTarget)
            {
                this.moveTarget = moveTarget;
            }
            this.SetMoveEndWorldPosition(moveEndWorldPosition);
            this.moveDuration = duration;

            if (null == this.moveTarget)
            {
                Log.Warning("CurveMoveCoponent PlayMoveAction warning: move target is missing");
                return;
            }

            //create action
            if (duration > 0)
            {
                // _tempMoveTarget.transform.position = movePaths[0].beginPoint.position;

                var actions = new ActionS[movePaths.Count];
                for (int i = 0; i < movePaths.Count; ++i)
                {
                    actions[i] = CreatesMoveAction(movePaths[i], duration / movePaths.Count);
                }

                if (actions.Length > 0)
                {
                    var sequeue = shaco.Sequeue.Create(actions);
                    sequeue.RunAction(this.moveTarget.gameObject);
                    _currentMoveAction = sequeue;
                }
            }
            //create negative action
            else
            {
                duration = -duration;

                // this._tempMoveTarget.transform.position = movePaths[movePaths.Count - 1].endPoint.position;
                var actions = new ActionS[movePaths.Count];
                for (int i = 0; i < movePaths.Count; ++i)
                {
                    actions[i] = CreatesMoveAction(movePaths[movePaths.Count - i - 1], duration / movePaths.Count).Reverse();
                }

                if (actions.Length > 0)
                {
                    var sequeue = shaco.Sequeue.Create(actions);
                    sequeue.RunAction(this.moveTarget.gameObject);
                    _currentMoveAction = sequeue;
                }
            }

            //run action
            if (null != _currentMoveAction)
            {
                _currentMoveAction.onCompleteFunc += (shaco.ActionS ac) =>
                {
                    _currentMoveAction = null;
                };
            }
        }

        public bool IsPlayingMoveAction()
        {
            return _currentMoveAction == null ? false : _currentMoveAction.isPlaying;
        }

        public bool IsPausedMoveAction()
        {
            return _currentMoveAction == null ? false : _currentMoveAction.isPaused;
        }

        public void SafeStopMoveAction()
        {
            if (null != _currentMoveAction)
            {
                _currentMoveAction.StopMe(true);
                _currentMoveAction = null;
            }
        }

        public void SafePauseMoveAction()
        {
            if (null != _currentMoveAction)
            {
                _currentMoveAction.Pause();
            }
        }

        public void SafeResumeMoveAction()
        {
            if (null != _currentMoveAction)
            {
                _currentMoveAction.Resume();
            }
        }

        public void RemoveComponentWithMoveInfo(MoveInfo moveInfo)
        {
            try
            {
                for (int i = 0; i < moveInfo.controlPoints.Count; ++i)
                {
                    SafeDestroyIfParent(this, moveInfo.controlPoints[i]);
                    moveInfo.controlPoints[i] = null;
                }
                SafeDestroyIfParent(this, moveInfo.beginPoint);
                moveInfo.beginPoint = null;
                SafeDestroyIfParent(this, moveInfo.endPoint);
                moveInfo.endPoint = null;
            }
            catch (System.Exception e)
            {
                shaco.Log.Error("RemoveComponentWithMoveInfo error: e=" + e);
            }
        }

        private void UpdateLineRenderPositions()
        {
            if (!editorMode || movePaths.Count == 0 || _lineRenderer == null || !_lineRenderer.enabled)
                return; 

            for (int i = 0; i < movePaths.Count; ++i)
            {
                UpdateLineRenderPosition(movePaths[i], i);
            }
        }

        private void SetMoveEndWorldPosition(Vector3 worldPosition)
        {
            if (movePaths.Count == 0)
                return;
            movePaths[movePaths.Count - 1].endPoint.position = worldPosition;
        }

        private void UpdateLineRenderPosition(MoveInfo moveInfo, int index)
        {
            int controlPointCount = GetControlPointCountWithControlPointMode(moveInfo);
            var points = new Vector3[controlPointCount];
            for (int i = 0; i < controlPointCount; ++i)
            {
                points[i] = moveInfo.controlPoints[i].position;
            }

            int drawLineCount = _currentSmooth / movePaths.Count;

            var path = Bezier.GetPath(moveInfo.beginPoint.position, moveInfo.endPoint.position, drawLineCount, points);
            for (int j = 0; j < path.Count; ++j)
            {
                //把每条线段绘制出来 完成贝塞尔曲线的绘制
                int setIndex = j + index * drawLineCount;
                _lineRenderer.SetPosition(setIndex, path[j]);
            }
        }

        private ActionS CreatesMoveAction(MoveInfo moveInfo, float duration)
        {
            ActionS retValue = null;
            int controlPointCount = GetControlPointCountWithControlPointMode(moveInfo);
            var points = new Vector3[controlPointCount];
            for (int i = 0; i < controlPointCount; ++i)
            {
                points[i] = moveInfo.controlPoints[i].transform.position;
            }

            switch (this.movePositionMode)
            {
                case MovePositionMode.Absolute:
                    {
                        retValue = CurveMoveTo.Create(moveInfo.beginPoint.position, moveInfo.endPoint.position, duration, points);
                        break;
                    }
                case MovePositionMode.Relative:
                    {
                        retValue = CurveMoveBy.Create(moveInfo.beginPoint.position, moveInfo.endPoint.position, duration, points);
                        break;
                    }
                default: Log.Error("CurveMoveComponent CreatesMoveAction error: unsupport move mode=" + movePositionMode); break;
            }

            return retValue;
        }

        private void CheckLineRenderComponent()
        {
            _lineRenderer = this.GetComponent<LineRenderer>();

            if (!editorMode)
            {
                if (null != _lineRenderer)
                {
                    _currentSmooth = 0;

#if UNITY_5_6_OR_NEWER
                    _lineRenderer.positionCount = 0;
#else
                    _lineRenderer.SetVertexCount(0);
#endif
                    _lineRenderer.enabled = false;
                }
            }
            else
            {
                if (null == _lineRenderer)
                {
#if UNITY_5_6_OR_NEWER
                    _lineRenderer.startWidth = 0.1f;
                    _lineRenderer.endWidth = 0.1f;
#else
                    _lineRenderer.SetWidth(0.1f, 0.1f);
#endif
                }
                if (null != _lineRenderer) _lineRenderer.enabled = true;

                if (movePaths.Count == 0)
                    _currentSmooth = 0;
                else
                {
                    int unuseLineCount = smooth % movePaths.Count;
                    _currentSmooth = unuseLineCount > 0 ? smooth - unuseLineCount : smooth;
                }
#if UNITY_5_6_OR_NEWER
                _lineRenderer.positionCount = _currentSmooth;
#else
                _lineRenderer.SetVertexCount(0);
#endif
            }
        }

        private void SafeDestroyIfParent(object parent, object target)
        {
            if (null != target && null != parent)
            {
                if (((Component)target).transform.parent == ((Component)parent).transform)
                    DestroyImmediate(((Component)target).gameObject);
            }
        }

        private void CheckMovePathComponent(MoveInfo moveInfo, Transform prevEndPoint)
        {
            if (editorMode)
            {
                int controlPointCount = GetControlPointCountWithControlPointMode(moveInfo);
                Vector3 offsetPoint = transform.position;

                if (prevEndPoint != null)
                {
                    moveInfo.beginPoint = prevEndPoint;
                    offsetPoint = prevEndPoint.position;
                }

                moveInfo.beginPoint = CheckPrimitivePoint("begin", moveInfo.beginPoint, PrimitiveType.Sphere, offsetPoint.x, offsetPoint.y);
                moveInfo.endPoint = CheckPrimitivePoint("end", moveInfo.endPoint, PrimitiveType.Sphere, controlPointCount + 1 + offsetPoint.x, offsetPoint.y);

                for (int i = 0; i < controlPointCount; ++i)
                {
                    moveInfo.controlPoints[i] = CheckPrimitivePoint("control_" + i, moveInfo.controlPoints[i], PrimitiveType.Sphere, i + 1 + offsetPoint.x, 2 + offsetPoint.y);
                    SafeSetActive(moveInfo.controlPoints[i], true);
                }
                for (int i = controlPointCount; i < moveInfo.controlPoints.Count; ++i)
                {
                    SafeSetActive(moveInfo.controlPoints[i], false);
                }
            }
        }

        private Transform CheckPrimitivePoint(string name, Transform point, PrimitiveType type, float xOffset, float yOffset)
        {
            if (null == point)
            {
                point = GameObject.CreatePrimitive(type).transform;
                point.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                point.name = name;
                UnityHelper.ChangeParent(point.gameObject, this.gameObject);
                point.position = new Vector3(xOffset, yOffset);
            }
            SafeSetActive(point, true);
            return point;
        }

        private int GetControlPointCountWithControlPointMode(MoveInfo moveInfo)
        {
            int retValue = 0;
            switch (moveInfo.controlPointMode)
            {
                case shaco.CurveMoveComponent.ControlPointMode.NoControlPoint: retValue = 0; break;
                case shaco.CurveMoveComponent.ControlPointMode.OneControlPoint: retValue = 1; break;
                case shaco.CurveMoveComponent.ControlPointMode.TwoControlPoint: retValue = 2; break;
                default: shaco.Log.Error("GetControlPointCountWithControlPointMode erorr: unsupport type=" + moveInfo.controlPointMode); break;
            }
            return retValue;
        }

        private void SafeSetActive(Object target, bool isActive)
        {
            if (null != target) ((Component)target).gameObject.SetActive(isActive);
        }

        private void SafeStopEditorUpdate()
        {
            if (null != _editorUpdateAction)
            {
                _editorUpdateAction.StopMe();
                _editorUpdateAction = null;
            }
        }
    }
}