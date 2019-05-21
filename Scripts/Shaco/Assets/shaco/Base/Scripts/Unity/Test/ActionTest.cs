using UnityEngine;
using System.Collections;

namespace shaco.Test
{
    public class ActionTest : MonoBehaviour
    {

        public shaco.SwapMoveAction actionSwapMove;
        public shaco.CurveMoveComponent actionCurveMove;

        public GameObject ActionTarget;
        public GameObject curveMoveEndTarget;

        void OnGUI()
        {
            if (GUILayout.Button("CurveMove"))
            {
                if (null != curveMoveEndTarget)
                    actionCurveMove.PlayMoveAction(ActionTarget.transform, curveMoveEndTarget.transform.position, 1.0f);
                else
                {
                    actionCurveMove.moveTarget = ActionTarget.transform;
                    actionCurveMove.moveDuration = -2;
                    actionCurveMove.PlayMoveAction();
                }
            }
            if (GUILayout.Button("LeftToRight"))
            {
                actionSwapMove.doAction(true);
            }
            if (GUILayout.Button("RightToLeft"))
            {
                actionSwapMove.doAction(false);
            }
            if (GUILayout.Button("ScaleTo_Big"))
            {
                ActionTarget.ScaleTo(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
            }
            if (GUILayout.Button("ScaleTo_Small"))
            {
                ActionTarget.ScaleTo(new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
            }
            if (GUILayout.Button("RepeatMove"))
            {
                ActionTarget.StopActions(true);
                var move1 = shaco.MoveBy.Create(new Vector3(1, 0, 0), 0.5f);
                var move2 = move1.Reverse();
                var seq = shaco.Sequeue.Create(move1, move2);
                seq.RunAction(ActionTarget);
            }
            if (GUILayout.Button("RepeatRotate"))
            {
                var rotate1 = shaco.RotateBy.Create(new Vector3(0, 0, 50), 0.5f);
                var rotate2 = rotate1.Reverse();
                var seq = shaco.Sequeue.Create(rotate1, rotate2);
                var rotateAction = shaco.Repeat.Create(seq, 2);
                rotateAction.RunAction(ActionTarget);
            }
            if (GUILayout.Button("RepeatScale"))
            {
                ActionTarget.StopActions(true);

                var scale1 = shaco.ScaleTo.Create(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
                var scale2 = shaco.ScaleTo.Create(new Vector3(1.0f, 1.0f, 1.0f), 0.5f);
                var seq = shaco.Sequeue.Create(scale1, scale2);
                var actionRepeat = shaco.Repeat.Create(seq, 4);
                actionRepeat.RunAction(ActionTarget);
            }
            if (GUILayout.Button("RepeatShake"))
            {
                ActionTarget.gameObject.StopAction<shaco.ShakeRepeat>(true);
                ActionTarget.ShakeRepeat(new Vector3(0.3f, 0, 0), 1, 0.5f).Tag = 100;
            }
            if (GUILayout.Button("TransparentBy"))
            {
                ActionTarget.gameObject.StopAction<shaco.TransparentBy>(true);
                ActionTarget.TransparentBy(-1, 1);
            }
            if (GUILayout.Button("TransparentTo"))
            {
                ActionTarget.gameObject.StopAction<shaco.TransparentBy>(true);
                ActionTarget.TransparentBy(1, 1);
            }
            if (GUILayout.Button("StopActions"))
            {
                ActionTarget.StopActions(true);
            }
        }
    }
}