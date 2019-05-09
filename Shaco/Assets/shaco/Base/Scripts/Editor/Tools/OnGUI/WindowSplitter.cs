using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUIHelper
    {
        /// <summary>
        /// 编辑器窗口分割线
        /// </summary>
        public class WindowSplitter
        {
            public bool isDragSplitter = false;

            private int _indexDragSplitWindow = -1;
            private List<Rect> _rectWindows = new List<Rect>();
            private shaco.Direction _direction = shaco.Direction.Horizontol;
            private float _splitterLineWidth = 1;
            private EditorWindow _windowTarget = null;
            private Vector2 _prevFullWindowSize = Vector2.zero;

            //最小分割窗口宽或者高
            private readonly float MIN_SPLIT_WINDOW_SIZE = 50;

            /// <summary>
            /// 构造函数
            /// </summary>
            public WindowSplitter(shaco.Direction direction = shaco.Direction.Horizontol, float splitterLineWidth = 1)
            {
                this._direction = direction;
                this._splitterLineWidth = splitterLineWidth;
            }

            /// <summary>
            /// 设置初始的分屏窗口大小
            /// <param name="target">窗口对象</param>
            /// <param name="defaultSplitPercents">默认分割比率，允许分割多个窗口</param>
            /// </summary>
            public void SetSplitWindow(EditorWindow target, params float[] defaultSplitPercents)
            {
                if (null == target || defaultSplitPercents.IsNullOrEmpty())
                {
                    Debug.LogError("WindowSplitter SetWindow error: invalid parameters");
                    return;
                }

                this._windowTarget = target;
                SetSplitWindow(target.position, defaultSplitPercents);
            }

            /// <summary>
            /// 设置初始的分屏窗口大小
            /// <param name="windowRect">窗口区域大小</param>
            /// <param name="defaultSplitPercents">默认分割比率，允许分割多个窗口</param>
            /// </summary>
            public void SetSplitWindow(Rect windowRect, params float[] defaultSplitPercents)
            {
                if (windowRect.width == 0 || windowRect.height == 0 || defaultSplitPercents.IsNullOrEmpty())
                {
                    Debug.LogError("WindowSplitter SetWindow error: invalid parameters");
                    return;
                }

                this._rectWindows.Clear();

                //获取总分割比率，计算每个窗口自身比率
                float allPercent = 0;
                var readlSplitPercents = new float[defaultSplitPercents.Length];
                for (int i = 0; i < defaultSplitPercents.Length; ++i)
                {
                    allPercent += defaultSplitPercents[i];
                }
                for (int i = 0; i < defaultSplitPercents.Length; ++i)
                {
                    readlSplitPercents[i] = defaultSplitPercents[i] / allPercent;
                }

                //刷新拆分区域大小
                UpdateWindowSize(windowRect.size, readlSplitPercents);
            }

            /// <summary>
            /// 绘制分屏窗口
            /// <return>如果有拖拽窗口返回true，反之false</return>
            /// </summary>
            public bool Draw()
            {
                bool retValue = false;

                if (null != _windowTarget && _prevFullWindowSize != _windowTarget.position.size)
                {
                    UpdateWindowSize(_windowTarget.position.size, GetSplitWindowsPercent());
                }

                retValue = HandleWindowResize();
                DrawSplitLine();
                return retValue;
            }

            /// <summary>
            /// 获取拆分窗口矩形大小
            /// <param name="index">窗口下标，从左往右，从上往下</param>
            /// <return>矩形大小</return>
            /// </summary>
            public Rect GetSplitWindowRect(int index)
            {
                if (index < 0 || index > _rectWindows.Count - 1)
                {
                    Debug.LogError("WindowSplitter GetSplitWindowRect error: out of range, index=" + index + " count=" + _rectWindows.Count);
                    return new Rect();
                }
                return _rectWindows[index];
            }

            /// <summary>
            /// 获取拆分窗口所占总窗口大小的百分比(0 ~ 1)
            /// <param name="index">窗口下标，从左往右，从上往下</param>
            /// <return>所占百分比</return>
            /// </summary>
            public float GetSplitWindowPercent(int index)
            {
                if (index < 0 || index > _rectWindows.Count - 1)
                {
                    Debug.LogError("WindowSplitter GetSplitWindowPercent error: out of range, index=" + index + " count=" + _rectWindows.Count);
                    return 0;
                }
                return GetSplitWindowsPercent()[index];
            }

            /// <summary>
            /// 强制刷新拆分窗口大小
            /// <param name="windowRect">窗口大小</param>
            /// </summary>
            public void ForceUpdateWindowSize(Rect windowRect)
            {
                UpdateWindowSize(windowRect.size, GetSplitWindowsPercent());
            }

            /// <summary>
            /// 绘制分割线
            /// </summary>
            private void DrawSplitLine()
            {
                if (null == Event.current || Event.current.type != EventType.Repaint)
                    return;

                //绘制带颜色的分割线
                Color colorTmp = new Color();
                if (EditorGUIUtility.isProSkin)
                {
                    colorTmp.r = 0.12f;
                    colorTmp.g = 0.12f;
                    colorTmp.b = 0.12f;
                    colorTmp.a = 1.0f;
                }
                else
                {
                    colorTmp.r = 0.6f;
                    colorTmp.g = 0.6f;
                    colorTmp.b = 0.6f;
                    colorTmp.a = 1.333f;
                }

                Color orgColor = GUI.color;
                GUI.color = GUI.color * colorTmp;

                for (int i = 0; i < _rectWindows.Count; ++i)
                {
                    var startPoint = Vector2.zero;
                    var endPoint = Vector2.zero;
                    var rectTmp = _rectWindows[i];

                    switch (_direction)
                    {
                        case shaco.Direction.Horizontol:
                            {
                                startPoint = new Vector2(rectTmp.xMax, 0);
                                endPoint = new Vector2(rectTmp.xMax, rectTmp.yMax);
                                break;
                            }
                        case shaco.Direction.Vertical:
                            {
                                startPoint = new Vector2(0, rectTmp.yMax);
                                endPoint = new Vector2(rectTmp.xMax, rectTmp.yMax);
                                break;
                            }
                        default:
                            {
                                shaco.Log.Error("unsupport direction type!");
                                break;
                            }
                    }

                    GUIHelper.LineDraw.DrawLine(startPoint, endPoint, _splitterLineWidth);
                }

                GUI.color = orgColor;
            }

            /// <summary>
            /// 刷新整个窗口大小，动态调整分屏窗口绘制
            /// <param name="fullWindowSize">2个分屏窗口中整个窗口的大小</param>
            /// <return></return>
            /// </summary>
            private void UpdateWindowSize(Vector2 fullWindowSize, float[] splitPercents)
            {
                if (_prevFullWindowSize != fullWindowSize)
                {
                    //设置拆分区域大小
                    _rectWindows.Clear();
                    float offsetSize = 0;

                    switch (_direction)
                    {
                        case shaco.Direction.Horizontol:
                            {
                                for (int i = 0; i < splitPercents.Length; ++i)
                                {
                                    float currentSplitPercent = splitPercents[i];
                                    Rect rectTmp = new Rect(offsetSize, 0, fullWindowSize.x * currentSplitPercent, fullWindowSize.y);
                                    _rectWindows.Add(rectTmp);
                                    offsetSize = rectTmp.xMax;
                                }
                                break;
                            }
                        case shaco.Direction.Vertical:
                            {
                                for (int i = 0; i < splitPercents.Length; ++i)
                                {
                                    float currentSplitPercent = splitPercents[i];
                                    Rect rectTmp = new Rect(0, offsetSize, fullWindowSize.x, fullWindowSize.y * currentSplitPercent);
                                    _rectWindows.Add(rectTmp);
                                    offsetSize = rectTmp.yMax;
                                }
                                break;
                            }
                        default:
                            {
                                shaco.Log.Error("unsupport direction type!");
                                break;
                            }
                    }
                    _prevFullWindowSize = fullWindowSize;
                }
            }

            /// <summary>
            /// 获取所有分割窗口所占百分比
            /// </summary>
            private float[] GetSplitWindowsPercent()
            {
                var retValue = new float[_rectWindows.Count];

                for (int i = 0; i < _rectWindows.Count; ++i)
                {
                    switch (_direction)
                    {
                        case shaco.Direction.Horizontol:
                            {
                                retValue[i] = _rectWindows[i].size.x / _prevFullWindowSize.x;
                                break;
                            }
                        case shaco.Direction.Vertical:
                            {
                                retValue[i] = _rectWindows[i].size.y / _prevFullWindowSize.y;
                                break;
                            }
                        default:
                            {
                                shaco.Log.Error("unsupport direction type!");
                                break;
                            }
                    }
                }

                return retValue;
            }

            /// <summary>
            /// 刷新分屏窗口大小
            /// <param name="index">下标</param>
            /// <param name="windowSize">新的窗口大小</param>
            /// </summary>
            private void UpdateSplitWindowsSize(int index, Vector2 windowSize)
            {
                if (index < 0 || index > _rectWindows.Count - 2)
                {
                    Debug.LogError("WindowSplitter UpdateSplitWindowSize error: out of range, index=" + index + " count=" + _rectWindows.Count);
                    return;
                }

                var sizeOffset = windowSize - _rectWindows[index].size;

                switch (_direction)
                {
                    case shaco.Direction.Horizontol:
                        {
                            var currentWindow = _rectWindows[index];
                            var otherWindow = _rectWindows[index + 1];
                            _rectWindows[index] = new Rect(currentWindow.x, currentWindow.y, currentWindow.size.x + sizeOffset.x, currentWindow.size.y);
                            _rectWindows[index + 1] = new Rect(otherWindow.x + sizeOffset.x, otherWindow.y, otherWindow.size.x - sizeOffset.x, otherWindow.size.y);
                            break;
                        }
                    case shaco.Direction.Vertical:
                        {
                            var currentWindow = _rectWindows[index];
                            var otherWindow = _rectWindows[index + 1];
                            _rectWindows[index] = new Rect(currentWindow.x, currentWindow.y, currentWindow.size.x, currentWindow.size.y + sizeOffset.y);
                            _rectWindows[index + 1] = new Rect(otherWindow.x, otherWindow.y + sizeOffset.y, otherWindow.size.x, otherWindow.size.y - sizeOffset.y);
                            break;
                        }
                    default:
                        {
                            shaco.Log.Error("unsupport direction type!");
                            break;
                        }
                }
            }

            /// <summary>
            /// 控制左右窗口大小
            /// <return>如果窗口有大小变化返回true，反之false </return>
            /// </summary>
            private bool HandleWindowResize()
            {
                bool retValue = false;


                if (_indexDragSplitWindow >= 0)
                {
                    if (_windowTarget != null)
                    {
                        _windowTarget.Repaint();
                    }
                }
                
                if (null == Event.current) 
                    return retValue;

                float expandSplitterLineWidthTmp = _splitterLineWidth * 6;

                //当鼠标放置在分割线上，绘制拖动图标
                //最后一个窗口右边的分割线忽略
                if (!isDragSplitter)
                {
                    _indexDragSplitWindow = -1;
                    for (int i = 0; i < _rectWindows.Count - 1; ++i)
                    {
                        var rectTmp = _rectWindows[i];
                        switch (_direction)
                        {
                            case shaco.Direction.Horizontol:
                                {
                                    rectTmp = new Rect(rectTmp.xMax - expandSplitterLineWidthTmp, rectTmp.y, expandSplitterLineWidthTmp * 2, rectTmp.height);
                                    break;
                                }
                            case shaco.Direction.Vertical:
                                {
                                    rectTmp = new Rect(rectTmp.x, rectTmp.yMax - expandSplitterLineWidthTmp, rectTmp.width, expandSplitterLineWidthTmp * 2);
                                    break;
                                }
                            default:
                                {
                                    shaco.Log.Error("unsupport direction type!");
                                    break;
                                }
                        }
                        if (rectTmp.Contains(Event.current.mousePosition))
                        {
                            _indexDragSplitWindow = i;
                            break;
                        }
                    }
                }

                //按下鼠标准备拖拽分割线
                if (_indexDragSplitWindow >= 0 && Event.current.type == EventType.MouseDown)
                {
                    isDragSplitter = true;
                }

                if (isDragSplitter)
                {
                    //拽动分割线，重新计算窗口大小
                    var currentRect = _rectWindows[_indexDragSplitWindow];
                    var nextRect = _rectWindows[_indexDragSplitWindow + 1];

                    Vector2 modifyWindowSize = Vector2.zero;
                    var clampMousePosition = Event.current.mousePosition;

                    switch (_direction)
                    {
                        case shaco.Direction.Horizontol:
                            {
                                //控制分割窗口最大大小
                                if (clampMousePosition.x > nextRect.xMax - MIN_SPLIT_WINDOW_SIZE)
                                    clampMousePosition.x = nextRect.xMax - MIN_SPLIT_WINDOW_SIZE;

                                modifyWindowSize = new Vector2(clampMousePosition.x - currentRect.position.x, currentRect.y);

                                //控制分割窗口最小大小
                                if (modifyWindowSize.x < MIN_SPLIT_WINDOW_SIZE)
                                    modifyWindowSize.x = MIN_SPLIT_WINDOW_SIZE;
                                break;
                            }
                        case shaco.Direction.Vertical:
                            {
                                //控制分割窗口最大大小
                                if (clampMousePosition.y > nextRect.yMax - MIN_SPLIT_WINDOW_SIZE)
                                    clampMousePosition.y = nextRect.yMax - MIN_SPLIT_WINDOW_SIZE;

                                modifyWindowSize = new Vector2(currentRect.y, clampMousePosition.y - currentRect.position.y);

                                //控制分割窗口最小大小
                                if (modifyWindowSize.y < MIN_SPLIT_WINDOW_SIZE)
                                    modifyWindowSize.y = MIN_SPLIT_WINDOW_SIZE;
                                break;
                            }
                        default:
                            {
                                shaco.Log.Error("unsupport direction type!");
                                break;
                            }
                    }

                    UpdateSplitWindowsSize(_indexDragSplitWindow, modifyWindowSize);
                    retValue = true;
                }

                //当鼠标在分割线上的时候，才绘制图标
                if (_indexDragSplitWindow >= 0)
                {
                    var rectTmp = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 60, 60);
                    rectTmp.x -= 30;
                    rectTmp.y -= 30;
                    EditorGUIUtility.AddCursorRect(rectTmp, _direction == shaco.Direction.Horizontol ? MouseCursor.SplitResizeLeftRight : MouseCursor.SplitResizeUpDown);
                }

                //停止拖拽分割线
                if (Event.current.type == EventType.MouseUp)
                {
                    isDragSplitter = false;
                    _indexDragSplitWindow = -1;
                }
                return retValue;
            }
        }
    }
}