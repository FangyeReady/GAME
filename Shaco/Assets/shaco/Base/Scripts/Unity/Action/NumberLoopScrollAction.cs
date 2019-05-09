using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace shaco
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class NumberLoopScrollAction : MonoBehaviour
    {
        //滚动方向
        public shaco.Direction scrollDirection = shaco.Direction.Up;

        //滚动速度
        public float perNumberScrollTime = 0.1f;

        //暂时不使用滚动动画，表现则和UnityEngine.UI.Text一样了
        public bool unuseScrollAction = false;

        //当前数字
        [HideInInspector]
        public string text
        {
            get { return _text; }
            set
            {
                _prevText = _text;
                _text = value;

                if (_prevText != _text)
                {
                    if (!unuseScrollAction && Application.isPlaying)
                        RunTextScrollAction();
                    else
                        UpdateTextLayout();
                }
            }
        }
        private string _text;

        //滚动数字模板
        [SerializeField]
        private Text model;

        //每个数组模板滚动流畅度
        [SerializeField]
        [Range(1, 10)]
        private int smooth = 5;

        //当前显示的数字所在滚动下标
        [SerializeField]
        private int displayIndex = 0;

        //所有的滚动动画组建
        private List<LoopScrollAction> _scrollActions = new List<LoopScrollAction>();

        //当前停止滚动下标
        private int _currentPauseIndex = -1;

        //需要变化的数字位，从当前位开始往高位的数字都不用做滚动
        private int _currentChangedIndex = 0;

        private string _prevText = string.Empty;

        void Start()
        {
            model.gameObject.SetActive(false);
        }

        public void UpdateTextLayout()
        {
            UpdateTextLayout(text);
        }

        /// <summary>
        /// 播放文字滚动动画
        /// </summary>
        private void RunTextScrollAction()
        {
            var numberString = _text.ToString();
            if (!UpdateTextLayout(numberString))
            {
                return;
            }

            _currentPauseIndex = _scrollActions.Count - 1;
            bool hasAction = false;

            //开始所有滚动动画，从地位到高位越来越慢
            var timeTmp = this.perNumberScrollTime;
            for (int i = _scrollActions.Count - 1; i >= _currentChangedIndex; --i)
            {
                var actionTmp = _scrollActions[i];

                //只有纯数字才有动画
                if (char.IsDigit(numberString[i]))
                {
                    actionTmp.perNumberScrollTime = timeTmp;
                    timeTmp *= 1.1f;

                    hasAction |= actionTmp.RunScrollAction(0, false);
                }

                if (!hasAction)
                {
                    --_currentPauseIndex;
                }
            }

            //推动数字持续做滚动动画，直到需要停止为止
            for (int i = _currentChangedIndex; i < _scrollActions.Count; ++i)
            {
                StartCoroutine(DelayAddScrolCallBack(_scrollActions[i], i, numberString));
            }
        }

        private IEnumerator DelayAddScrolCallBack(LoopScrollAction scrollAction, int index, string numberString)
        {
            yield return new WaitForSeconds(perNumberScrollTime * (_scrollActions.Count - index - 1));

            scrollAction.onScrollCallBack = (int loop) =>
            {
                if (GetNumberStringInScrollAction(scrollAction) == numberString[index])
                {
                    //跳过符号位
                    do
                    {
                        --_currentPauseIndex;
                        if (_currentPauseIndex >= 0 && char.IsDigit(numberString[_currentPauseIndex]))
                            break;

                    } while (_currentPauseIndex >= 0);

                    //最后一次滚动
                    if (_currentPauseIndex < _currentChangedIndex)
                    {
                        this.GetComponent<GridLayoutGroup>().enabled = true;
                    }
                    return false;
                }
                else
                    return true;
            };
        }

        private bool UpdateTextLayout(string NumberLoopScrollAction)
        {
            if (null == model)
            {
                shaco.Log.Error("NumberLoopScrollAction UpdateTextLayout error: no text model");
                return false;
            }

            if (_currentPauseIndex >= _currentChangedIndex)
            {
                shaco.Log.Warning("NumberLoopScrollAction UpdateTextLayout warning: please wait scrolling");
                return false;
            }

#if UNITY_EDITOR
            if (FindParent(this.gameObject, model.gameObject) != null)
            {
                shaco.Log.Error("NumberLoopScrollAction UpdateTextLayout error: text model can't be a parent of me");
                return false;
            }
#endif
            //重新计算需要更新的数字位
            UpdateChangedIndex();

            //重置数字对象
            ResetNumberTextScrollActions();

            //检查并重新计算布局大小
            CheckLayoutGroup(text);

            //添加数字对象
            for (int i = _currentChangedIndex; i < text.Length; ++i)
            {
                AddScrollTextModel(text[i]);
            }
            return true;
        }

        private void UpdateChangedIndex()
        {
            _currentChangedIndex = 0;
            if (!string.IsNullOrEmpty(_prevText))
            {
                if (_text.Length <= _prevText.Length)
                {
                    int indexOffsetTmp = _prevText.Length - _text.Length;
                    for (int i = 0; i < _text.Length; ++i)
                    {
                        if (_text[i] == _prevText[indexOffsetTmp + i])
                        {
                            ++_currentChangedIndex;
                        }
                        else
                            break;
                    }
                }
            }
        }

        private void CheckLayoutGroup(string numberString)
        {
            var layoutTmp = this.GetComponent<GridLayoutGroup>();
            StartCoroutine(DelayShowGridLayout());

            //设置显示大小
            var newSizeTmp = Vector2.zero;
            switch (scrollDirection)
            {
                case shaco.Direction.Up:
                case shaco.Direction.Down: newSizeTmp.x = numberString.Length * (layoutTmp.cellSize.x + layoutTmp.spacing.x); newSizeTmp.y = layoutTmp.cellSize.y; break;
                case shaco.Direction.Left:
                case shaco.Direction.Right: newSizeTmp.x = layoutTmp.cellSize.x; newSizeTmp.y = numberString.Length * (layoutTmp.cellSize.y + layoutTmp.spacing.y); break;
                default: shaco.Log.Error("NumberLoopScrollAction CheckLayoutGroup error: unsupport type=" + scrollDirection); break;
            }
            this.GetComponent<RectTransform>().sizeDelta = newSizeTmp;
        }

        private IEnumerator DelayShowGridLayout()
        {
            yield return new WaitForEndOfFrame();
            this.GetComponent<GridLayoutGroup>().enabled = true;
            StartCoroutine(DelayHideGridLayout());
        }


        private IEnumerator DelayHideGridLayout()
        {
            yield return new WaitForEndOfFrame();

            if (Application.isPlaying)
            {
                this.GetComponent<GridLayoutGroup>().enabled = false;
            }
        }

        private char GetNumberStringInScrollAction(LoopScrollAction action)
        {
            return action.GetScrollTarget(displayIndex).GetComponent<Text>().text.ToChar();
        }

        //给每个单位数字添加滚动模板
        private void AddScrollTextModel(char txt)
        {
            //创建单位滚动动画对象
            var newActionTarget = new GameObject();
            newActionTarget.name = "txt_" + txt;
            UnityHelper.ChangeParentLocalPosition(newActionTarget, this.gameObject);
            var newScrollAction = newActionTarget.AddComponent<LoopScrollAction>();
            _scrollActions.Add(newScrollAction);

            //设置参数
            newScrollAction.scrollDirection = this.scrollDirection;

            //设置容器大小
            newActionTarget.GetComponent<RectTransform>().sizeDelta = this.GetComponent<GridLayoutGroup>().cellSize;

            //设置数字
            if (char.IsDigit(txt))
            {
                //将目标数字放在最后一位
                var numberTmp = txt.ToString().ToInt();
                for (int i = smooth - 1; i >= 1; --i)
                {
                    var numTmp = numberTmp - i;

                    //回滚到数字9
                    if (numTmp < 0)
                        numTmp += 10;

                    AddTextByModel(newScrollAction, numTmp.ToString());
                }

                AddTextByModel(newScrollAction, numberTmp.ToString());
            }
            //设置字符
            else
            {
                AddTextByModel(newScrollAction, txt.ToString());
            }

            //如果是编辑模式和不用动画模式下，默认将目标位数字调整为目标数字，为了方便查看显示数字和布局
            if ((!Application.isPlaying || unuseScrollAction) && newScrollAction.transform.childCount > 0)
            {
                SetDisplayText(newScrollAction, txt.ToString());
            }

            //刷新布局
            newScrollAction.UpdateTextLayout();
        }

        private void SetDisplayText(LoopScrollAction scrollAction, string txt)
        {
            if (scrollAction.transform.childCount == 0)
                return;

            int safeDisplayIndex = displayIndex;
            if (safeDisplayIndex < 0 || safeDisplayIndex > scrollAction.transform.childCount)
            {
                shaco.Log.Error("NumberLoopScrollAction AddScrollTextModel error: display index out of range, index=" + safeDisplayIndex + " count=" + scrollAction.transform.childCount);
                safeDisplayIndex = 0;
            }

            var childTmp = scrollAction.transform.GetChild(safeDisplayIndex);
            if (null != childTmp)
            {
                var textTmp = childTmp.GetComponent<Text>();
                if (null != textTmp)
                {
                    textTmp.text = txt;
                }
            }
        }

        private void ResetNumberTextScrollActions()
        {
            for (int i = _scrollActions.Count - 1; i >= _currentChangedIndex; --i)
            {
                try
                {
                    UnityHelper.SafeDestroy(_scrollActions[i].gameObject);
                }
                catch (System.Exception e)
                {
                    shaco.Log.Warning("ResetNumberTextScrollActions warning: e=" + e);
                }
                _scrollActions.RemoveAt(i);
            }

            for (int i = this.transform.childCount - 1; i >= _currentChangedIndex; --i)
            {
                UnityHelper.SafeDestroy(this.transform.GetChild(i).gameObject);
            }
        }

        private Text AddTextByModel(LoopScrollAction scrollAction, string txt)
        {
            var newText = ((GameObject)(MonoBehaviour.Instantiate(model.gameObject))).GetComponent<Text>();
            newText.gameObject.SetActive(true);
            newText.text = txt;
            UnityHelper.ChangeParentLocalPosition(newText.gameObject, scrollAction.gameObject);

            return newText;
        }

        static private GameObject FindParent(GameObject child, GameObject parentFind)
        {
            GameObject retValue = null;
            if (null == parentFind)
            {
                shaco.Log.Error("UnityHelper GetRoot error: parentFind is valid");
                return retValue;
            }

            if (child == null)
                return retValue;

            Transform prevParent = child.transform;
            var parent = prevParent.parent;

            while (parent != null)
            {
                prevParent = parent;
                if (parentFind.gameObject == parent.gameObject)
                    break;

                parent = parent.transform.parent;
            }

            retValue = null != parent ? parent.gameObject : null;
            return retValue;
        }
    }
}