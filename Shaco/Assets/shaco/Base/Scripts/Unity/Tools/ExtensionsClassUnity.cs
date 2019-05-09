using UnityEngine;
using System.Collections;

static public class shaco_ExtensionsClassUnity
{
    static public bool Contains(this Rect rect, Rect other)
    {
        return !(rect.xMax < other.xMin || rect.xMin > other.xMax
              || rect.yMax < other.yMin || rect.yMin > other.yMax);
    }

    static public Vector3 ToPivot(this TextAnchor anchor)
    {
        var retValue = shaco.Pivot.LeftTop;
        switch (anchor)
        {
            case TextAnchor.UpperLeft: retValue = shaco.Pivot.LeftTop; break;
            case TextAnchor.UpperCenter: retValue = shaco.Pivot.MiddleTop; break;
            case TextAnchor.UpperRight: retValue = shaco.Pivot.RightTop; break;
            case TextAnchor.MiddleLeft: retValue = shaco.Pivot.LeftMiddle; break;
            case TextAnchor.MiddleCenter: retValue = shaco.Pivot.Middle; break;
            case TextAnchor.MiddleRight: retValue = shaco.Pivot.RightMiddle; break;
            case TextAnchor.LowerLeft: retValue = shaco.Pivot.LeftBottom; break;
            case TextAnchor.LowerCenter: retValue = shaco.Pivot.MiddleBottom; break;
            case TextAnchor.LowerRight: retValue = shaco.Pivot.RightBottom; break;
            default: shaco.Log.Error("ToPivot error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public Vector3 ToNegativePivot(this TextAnchor anchor)
    {
        var retValue = shaco.Pivot.LeftTop;
        switch (anchor)
        {
            case TextAnchor.UpperLeft: retValue = shaco.Pivot.RightBottom; break;
            case TextAnchor.UpperCenter: retValue = shaco.Pivot.MiddleBottom; break;
            case TextAnchor.UpperRight: retValue = shaco.Pivot.LeftBottom; break;
            case TextAnchor.MiddleLeft: retValue = shaco.Pivot.RightMiddle; break;
            case TextAnchor.MiddleCenter: retValue = shaco.Pivot.Middle; break;
            case TextAnchor.MiddleRight: retValue = shaco.Pivot.LeftMiddle; break;
            case TextAnchor.LowerLeft: retValue = shaco.Pivot.RightTop; break;
            case TextAnchor.LowerCenter: retValue = shaco.Pivot.MiddleTop; break;
            case TextAnchor.LowerRight: retValue = shaco.Pivot.LeftTop; break;
            default: shaco.Log.Error("ToNegativePivot error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public TextAnchor ToNegativeAnchor(this TextAnchor anchor)
    {
        var retValue = TextAnchor.MiddleCenter;
        switch (anchor)
        {
            case TextAnchor.UpperLeft: retValue = TextAnchor.LowerRight; break;
            case TextAnchor.UpperCenter: retValue = TextAnchor.LowerCenter; break;
            case TextAnchor.UpperRight: retValue = TextAnchor.LowerLeft; break;
            case TextAnchor.MiddleLeft: retValue = TextAnchor.MiddleRight; break;
            case TextAnchor.MiddleCenter: retValue = TextAnchor.MiddleCenter; break;
            case TextAnchor.MiddleRight: retValue = TextAnchor.MiddleLeft; break;
            case TextAnchor.LowerLeft: retValue = TextAnchor.UpperRight; break;
            case TextAnchor.LowerCenter: retValue = TextAnchor.UpperCenter; break;
            case TextAnchor.LowerRight: retValue = TextAnchor.UpperLeft; break;
            default: shaco.Log.Error("ToNegativePivot error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public T FindComponent<T>(this GameObject target, bool ignoreActive = false) where T : UnityEngine.Component
    {
        T retValue = default(T);
        if (null == target)
            return retValue;

        bool shouldReturn = false;

        if (!target.activeSelf)
        {
            if (!ignoreActive)
            {
                shouldReturn = true;
            }
        }

        if (!shouldReturn)
        {
            retValue = target.GetComponent<T>();
            if (null != retValue)
            {
                shouldReturn = true;
            }
        }

        if (!shouldReturn)
        {
            for (int i = 0; i < target.transform.childCount; ++i)
            {
                var childTmp = target.transform.GetChild(i);
                retValue = FindComponent<T>(childTmp.gameObject, ignoreActive);

                if (null != retValue)
                {
                    break;
                }
            }
        }
        return retValue;
    }

    static public void UnloadAssetBundleLocal(this string path, bool unloadAllLoadedObjects)
    {
        shaco.ResourcesEx.UnloadAssetBundleLocal(path, unloadAllLoadedObjects);
    }

    static public bool LoadFromResourcesOrLocal(this shaco.Base.BehaviourRootTree tree, string path, string multiVersionControlRelativePath = "")
    {
        return tree.LoadFromJson(shaco.ResourcesEx.LoadResourcesOrLocal(path, multiVersionControlRelativePath).ToString());
    }

    static public byte[] ToBytes(this UnityEngine.Object value)
    {
        if (value is UnityEngine.TextAsset)
        {
            return ((UnityEngine.TextAsset)value).bytes;
        }
        else if (value is shaco.TextOrigin)
        {
            return ((shaco.TextOrigin)value).bytes;
        }
        else
        {
            shaco.Log.Error("ExtensionsClassUnity ToBytes error: target not have byte data, value=" + value);
            return null;
        }
    }
}