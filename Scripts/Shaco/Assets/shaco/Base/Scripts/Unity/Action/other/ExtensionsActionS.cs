using UnityEngine;
using System.Collections;

static public class shaco_ExtensionsActionS
{
    public static shaco.MoveBy MoveBy(this GameObject target, Vector3 endPositon, float duration, bool isWorldPosition = true)
    {
        var ret = shaco.MoveBy.Create(endPositon, duration, isWorldPosition);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.MoveTo MoveTo(this GameObject target, Vector3 endPositon, float duration, bool isWorldPosition = true)
    {
        var ret = shaco.MoveTo.Create(endPositon, duration, isWorldPosition);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.CurveMoveBy CurveMoveBy(this GameObject target, Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
    {
        var ret = shaco.CurveMoveBy.Create(beginPoint, endPoint, duration, controlPoints);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.CurveMoveTo CurveMoveTo(this GameObject target, Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
    {
        var ret = shaco.CurveMoveTo.Create(beginPoint, endPoint, duration, controlPoints);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.RotateBy RotateBy(this GameObject target, Vector3 angle, float duration)
    {
        var ret = shaco.RotateBy.Create(angle, duration);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.RotateTo RotateTo(this GameObject target, Vector3 angle, float duration)
    {
        var ret = shaco.RotateTo.Create(angle, duration);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.ScaleBy ScaleBy(this GameObject target, Vector3 scale, float duration)
    {
        var ret = shaco.ScaleBy.Create(scale, duration);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.ScaleTo ScaleTo(this GameObject target, Vector3 scale, float duration)
    {
        var ret = shaco.ScaleTo.Create(scale, duration);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.ShakeRepeat ShakeRepeat(this GameObject target, Vector3 shakeDistance, int loop, float duration)
    {
        var ret = shaco.ShakeRepeat.Create(shakeDistance, loop, duration);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.TransparentBy TransparentBy(this GameObject target, float alpha, float duration)
    {
        var ret = shaco.TransparentBy.Create(alpha, duration);
        ret.RunAction(target);
        return ret;
    }

    public static shaco.TransparentTo TransparentTo(this GameObject target, float alpha, float duration)
    {
        var ret = shaco.TransparentTo.Create(alpha, duration);
        ret.RunAction(target);
        return ret;
    }

    public static void StopActions(this GameObject target, bool isPlayEndWithDirectly = false)
    {
        shaco.ActionS.StopActions(target, isPlayEndWithDirectly);
    }

    public static void StopActionByTag(this GameObject target, int tag, bool isPlayEndWithDirectly = false)
    {
        shaco.ActionS.StopActionByTag(target, tag, isPlayEndWithDirectly);
    }

    public static void StopAction<T>(this GameObject target, bool isPlayEndWithDirectly = false) where T : shaco.ActionS
    {
        shaco.ActionS.StopAction<T>(target, isPlayEndWithDirectly);
    }

    public static void StopAction(this GameObject target, shaco.ActionS action, bool isPlayEndWithDirectly = false)
    {
        shaco.ActionS.StopAction(target, action);
    }
}
