using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using shaco.Base;

namespace shaco
{
    public partial class UnityHelper
    {
        public delegate void CALL_FUNC_ANIMATION();
        public delegate bool FOREACH_CHILD_CALLFUNC(int index, GameObject child);

        static public void ChangeParent(GameObject target, GameObject parent)
        {
            var oldPos = target.transform.position;
            //var oldScale = target.transform.localScale;
            var oldAngle = target.transform.eulerAngles;

            target.transform.SetParent(parent == null ? null : parent.transform);

            target.transform.position = oldPos;
            //target.transform.localScale = oldScale;
            target.transform.eulerAngles = oldAngle;
        }

        static public void ChangeParentLocalPosition(GameObject target, GameObject parent)
        {
            var oldPos = target.transform.localPosition;
            var oldScale = target.transform.localScale;
            var oldAngle = target.transform.localEulerAngles;

            target.transform.SetParent(parent == null ? null : parent.transform);

            target.transform.localPosition = oldPos;
            target.transform.localScale = oldScale;
            target.transform.localEulerAngles = oldAngle;
        }

        /// <summary>
        /// get the top of the root node, and this node not have parent
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        static public GameObject GetRoot(GameObject child)
        {
            return GetRoot(child, null);
        }

        /// <summary>
        /// get the top of the root node by parentName
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        static public GameObject GetRoot(GameObject child, string parentName)
        {
            if (child == null)
                return null;

            Transform prevParent = child.transform;
            var parent = prevParent.parent;

            while (parent != null)
            {
                prevParent = parent;
                if (!string.IsNullOrEmpty(parentName) && parent.name == parentName)
                    break;

                parent = parent.transform.parent;
            }

            return null != prevParent ? prevParent.gameObject : null;
        }

        static public T GetRootWithComponent<T>(GameObject child) where T : Component
        {
            if (child == null)
                return null;

            Transform prevParent = child.transform;
            var parent = prevParent.parent;

            while (parent != null)
            {
                prevParent = parent;
                if (parent.GetComponent<T>() != null)
                    break;

                parent = parent.transform.parent;
            }

            return null != prevParent ? prevParent.gameObject.GetComponent<T>() : null;
        }

        /// <summary>
        /// remove target's all children
        /// </summary>
        /// <param name="target"></param>
        static public void RemoveChildren(GameObject target, params GameObject[] ignoreChildren)
        {
            bool hasIgnoreChild = !ignoreChildren.IsNullOrEmpty();
            for (int i = target.transform.childCount - 1; i >= 0; --i)
            {
                var childTmp = target.transform.GetChild(i);
                bool shouldRemoveChild = true;
                if (hasIgnoreChild)
                {
                    for (int j = ignoreChildren.Length - 1; j >= 0; --j)
                    {
                        if (ignoreChildren[j] == childTmp.gameObject)
                        {
                            shouldRemoveChild = false;
                            break;
                        }
                    }
                }

                if (shouldRemoveChild)
                {
                    if (!Application.isPlaying)
                        MonoBehaviour.DestroyImmediate(childTmp.gameObject);
                    else
                        MonoBehaviour.Destroy(childTmp.gameObject);
                }
            }
        }

        /// <summary>
        /// call 'callfunc' when animation play end
        /// </summary>
        /// <param name="ani"></param> animation target
        /// <param name="animationName"></param> animation name
        /// <param name="callfunc"></param> event call function
        static public void CallOnAnimationEnd(Animation ani, CALL_FUNC_ANIMATION callfunc)
        {
            var timeUpdate = shaco.Repeat.CreateRepeatForver(shaco.DelayTime.Create(60.0f));

            timeUpdate.RunAction(ani.gameObject);
            timeUpdate.onFrameFunc = (float percent) =>
            {
                if (!ani.isPlaying)
                {
                    timeUpdate.StopMe();
                    callfunc();
                }
            };
        }

        static public void CallOnAnimationEnd(Animator ani, CALL_FUNC_ANIMATION callfunc)
        {
            int animationindex = 0;
            var timeUpdate = shaco.Repeat.CreateRepeatForver(shaco.DelayTime.Create(60.0f));

            timeUpdate.RunAction(ani.gameObject);
            timeUpdate.onFrameFunc = (float percent) =>
            {
                bool isStop = false;
                var animationTmp = ani.GetCurrentAnimatorStateInfo(animationindex);
                if (animationTmp.normalizedTime >= 1.0f)
                {
                    isStop = true;
                }

                if (isStop)
                {
                    timeUpdate.StopMe();
                    callfunc();
                }
            };
        }

        static public void SetLocalPositionByPivot(GameObject target, Vector3 newPosition, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("setLocalPositionByArchor error: target dose not contain RectTransform !");
                return;
            }
            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);

            target.transform.localPosition = new Vector3(
                newPosition.x - sizeTmp.x * newPivot.x,
                newPosition.y - sizeTmp.y * newPivot.y,
                newPosition.z);
        }

        static public Vector3 GetLocalPositionByPivot(GameObject target, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("getLocalPositioByArchor error: dose not contain RectTransform !");
                return Vector3.zero;
            }
            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);

            return new Vector3(
                rectTrans.localPosition.x + sizeTmp.x * newPivot.x,
                rectTrans.localPosition.y + sizeTmp.y * newPivot.y,
                rectTrans.localPosition.z);
        }

        static public void SetPivotByLocalPosition(GameObject target, Vector2 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("SetPivotByLocalPosition error: dose not contain RectTransform !");
                return;
            }

            var pivotOffset = pivot - rectTrans.pivot;
            var sizeTmp = GetRealSize(rectTrans);
            rectTrans.pivot = pivot;
            target.transform.localPosition = new Vector3(
                target.transform.localPosition.x + sizeTmp.x * pivotOffset.x,
                target.transform.localPosition.y + sizeTmp.y * pivotOffset.y,
                target.transform.localPosition.z);
        }

        static public Vector3 GetWorldPositionByPivot(GameObject target, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("GetWorldPositionByPivot error: dose not contain RectTransform !");
                return Vector3.zero;
            }

            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);
            var t1 = target.transform.TransformPoint(Vector3.zero);
            var t2 = target.transform.TransformPoint(new Vector3(sizeTmp.x, sizeTmp.y));
            var t3 = t2 - t1;
            var t4 = new Vector3(t3.x * newPivot.x, t3.y * newPivot.y, 0);

            var ret = new Vector3(
                rectTrans.position.x + t4.x,
                rectTrans.position.y + t4.y,
                rectTrans.position.z);

            return ret;
        }

        static public void SetWorldPositionByPivot(GameObject target, Vector3 newPosition, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("setLocalPositionByArchor error: target dose not contain RectTransform !");
                return;
            }
            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);
            var newOffset = new Vector3(sizeTmp.x * newPivot.x, sizeTmp.y * newPivot.y, 0);
            newOffset = target.transform.TransformPoint(newOffset);

            target.transform.position += new Vector3(
                newPosition.x - newOffset.x,
                newPosition.y - newOffset.y,
                0);
        }

        static public Vector2 GetRealPivot(RectTransform transform)
        {
            var retValue = Vector2.zero;
            var textTmp = transform.GetComponent<UnityEngine.UI.Text>();
            if (null == textTmp)
            {
                retValue = transform.pivot;
            }
            else
            {
                retValue = textTmp.alignment.ToPivot();
            }
            return retValue;
        }

        static public Vector2 GetRealSize(RectTransform transform)
        {
            var retValue = Vector2.zero;
            var textTmp = transform.GetComponent<UnityEngine.UI.Text>();
            if (null == textTmp)
            {
                retValue = transform.sizeDelta;
            }
            else
            {
                retValue = GetTextRealSize(textTmp);
            }
            return retValue;
        }

        static public Vector2 GetTextRealSize(UnityEngine.UI.Text textTarget)
        {
            var retValue = Vector2.zero;
            // if (textTarget.resizeTextForBestFit && textTarget.horizontalOverflow != HorizontalWrapMode.Overflow)
            // {
            //     TextGenerationSettings generationSettings = textTarget.GetGenerationSettings(Vector2.zero);
            //     generationSettings.fontSize = textTarget.cachedTextGeneratorForLayout.fontSizeUsedForBestFit;

            //     retValue.x = textTarget.cachedTextGeneratorForLayout.GetPreferredWidth(textTarget.text, generationSettings) / textTarget.pixelsPerUnit;
            //     retValue.y = textTarget.preferredHeight;
            // }
            // else
            // {
            retValue = new Vector2(textTarget.preferredWidth, textTarget.preferredHeight);
            // }

            if (textTarget.horizontalOverflow == HorizontalWrapMode.Wrap)
            {
                retValue.x = Mathf.Min(retValue.x, textTarget.rectTransform.sizeDelta.x);
            }
            if (textTarget.verticalOverflow == VerticalWrapMode.Truncate)
            {
                retValue.y = Mathf.Min(retValue.y, textTarget.rectTransform.sizeDelta.y);
            }

            return retValue;
        }

        static public Rect GetLocalRect(RectTransform target)
        {
            var pos = GetLocalPositionByPivot(target.gameObject, shaco.Pivot.LeftBottom);
            var size = GetRealSize(target);
            return new Rect(pos.x, pos.y, size.x, size.y);
        }

        static public Rect GetWorldRect(RectTransform target)
        {
            var pos1 = GetWorldPositionByPivot(target.gameObject, shaco.Pivot.LeftBottom);
            var pos2 = GetWorldPositionByPivot(target.gameObject, shaco.Pivot.RightTop);
            return new Rect(pos1.x, pos1.y, pos2.x - pos1.x, pos2.y - pos1.y);
        }

        /// <summary>
        /// Iterate through all the child
        /// </summary>
        /// <param name="index"></param> current child index
        /// <param name="child"></param> child target
        static public void ForeachChildren(GameObject target, FOREACH_CHILD_CALLFUNC callfunc)
        {
            int index = 0;
            ForeachChildren(target, callfunc, ref index);

        }
        static private void ForeachChildren(GameObject target, FOREACH_CHILD_CALLFUNC callfunc, ref int index)
        {
            for (int i = 0; i < target.transform.childCount; ++i)
            {
                GameObject child = target.transform.GetChild(i).gameObject;

                if (!callfunc(index++, child))
                {
                    break;
                }
                ForeachChildren(child, callfunc, ref index);
            }
        }

        /// <summary>
        /// find value in list
        /// </summary>
        /// <returns></returns> index of list
        static public int FindListValue<T>(List<T> list, T findValue)
        {
            int ret = -1;
            for (int i = 0; i < list.Count; ++i)
            {
                if (findValue.Equals(list[i]))
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// remove values by removeValue
        /// </summary>
        /// <returns></returns> remove count
        static public int RemoveListValue<T>(List<T> list, T removeValue, bool removeAll = true)
        {
            int ret = 0;
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (removeValue.Equals(list[i]))
                {
                    ++ret;
                    list.RemoveAt(i);

                    if (!removeAll)
                        break;
                }
            }
            return ret;
        }

        static public Vector3 GetColliderSize(GameObject target)
        {
            Vector3 ret = Vector3.zero;
            var collider = target.GetComponent<Collider>();

            if (collider == null)
            {
                Log.Error("not find 'Collider' Component in target=" + target);
                return ret;
            }

            var oldRotation = target.transform.rotation;
            target.transform.eulerAngles = Vector3.zero;
            ret = collider.bounds.size;
            target.transform.rotation = oldRotation;

            return ret;
        }

        /// <summary>
        /// check values all valid in list
        /// </summary>
        /// <typeparam name="T"></typeparam> value type
        /// <param name="listTarget"></param> 
        /// <returns></returns>
        static public bool IsValidListValues<T>(List<T> listTarget)
        {
            bool ret = true;
            for (int i = listTarget.Count - 1; i >= 0; --i)
            {
                if (listTarget[i] == null)
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }
        static public bool IsValidListValues<T>(T[] listTarget)
        {
            bool ret = true;
            for (int i = listTarget.Length - 1; i >= 0; --i)
            {
                if (listTarget[i] == null)
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        //fixed missing shader when import asset bundle
        public static void ResetShader(UnityEngine.Object obj)
        {
            List<Material> listMat = new List<Material>();
            listMat.Clear();

            if (obj is Material)
            {
                Material m = obj as Material;
                listMat.Add(m);
            }
            else if (obj is GameObject)
            {
                GameObject go = obj as GameObject;
                var renders = go.GetComponentsInChildren<Renderer>();

                if (null != renders)
                {
                    foreach (Renderer item in renders)
                    {
                        Material[] materialsArr = item.sharedMaterials;
                        foreach (Material m in materialsArr)
                            listMat.Add(m);
                    }
                }

                var particleRenders = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
                if (null != particleRenders)
                {
                    foreach (ParticleSystemRenderer item in particleRenders)
                    {
                        Material[] materialsArr = item.sharedMaterials;
                        foreach (Material m in materialsArr)
                            listMat.Add(m);
                    }
                }

                var graphics = go.GetComponentsInChildren<UnityEngine.UI.Graphic>();
                if (null != graphics)
                {
                    foreach (var item in graphics)
                        listMat.Add(item.material);
                }
            }

            for (int i = 0; i < listMat.Count; i++)
            {
                Material m = listMat[i];

                if (null == m)
                    continue;

                var shaderName = m.shader.name;
                var newShader = Shader.Find(shaderName);
                if (newShader != null)
                    m.shader = newShader;
                else
                {
                    Log.Error("missing shader name=" + shaderName);
                }
            }
        }

        static public string GetEnviromentCommandValue(string key)
        {
            string retValue = string.Empty;
            foreach (var value1 in System.Environment.GetCommandLineArgs())
            {
                if (value1.Contains(key))
                {
                    int indexFindValue = value1.IndexOf("=");
                    if (indexFindValue < 0)
                    {
                        Log.Error("UnityHelper.GetEnviromentCommandValue error: not find '=' in command=" + value1);
                    }
                    else
                    {
                        //skip '=' flag
                        ++indexFindValue;
                        for (int i = indexFindValue; i < value1.Length; ++i)
                        {
                            char cValueTmp = value1[i];
                            if (cValueTmp != ' ')
                            {
                                indexFindValue = i;
                                break;
                            }
                        }
                        if (indexFindValue >= 0 && indexFindValue < value1.Length)
                        {
                            retValue = value1.Substring(indexFindValue, value1.Length - indexFindValue);
                        }
                    }
                    break;
                }
            }

            if (string.IsNullOrEmpty(retValue))
            {
                Log.Warning("UnityHelper.GetEnviromentCommandValue warning: not find command value by key=" + key);
            }

            return retValue;
        }

        static public string GetUnityProjectPath()
        {
            return System.IO.Directory.GetParent(Application.dataPath).ToString();
        }

        static public void SafeDontDestroyOnLoad(Object target)
        {
            if (Application.isPlaying)
                MonoBehaviour.DontDestroyOnLoad(target);
        }

        static public void SafeDestroy(Object target, float delayTime = 0)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                MonoBehaviour.Destroy(target, delayTime);
            }
            else
            {
                MonoBehaviour.DestroyImmediate(target);
            }
#else
            MonoBehaviour.Destroy(target, delayTime);
#endif
        }

        static public void SetLayerRecursively(GameObject target, int layerIndex)
        {
            target.layer = layerIndex;
            ForeachChildren(target, (int index, GameObject child) =>
            {
                child.layer = layerIndex;
                return true;
            });
        }
    }
}