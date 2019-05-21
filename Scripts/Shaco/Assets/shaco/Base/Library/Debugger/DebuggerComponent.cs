//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:i@jiangyin.me
//------------------------------------------------------------

#if DEBUG_WINDOW
using GameFramework;
using GameFramework.Debugger;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 调试组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Debugger")]
    [RequireComponent(typeof(BaseComponent))]
    [RequireComponent(typeof(SettingComponent))]
    public partial class DebuggerComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 默认调试器漂浮框大小。
        /// </summary>
        internal static Rect DefaultIconRect = new Rect();

        /// <summary>
        /// 默认调试器窗口大小。
        /// </summary>
        internal static Rect DefaultWindowRect = new Rect();

        /// <summary>
        /// 默认调试器窗口缩放比例。
        /// </summary>
        internal static float DefaultWindowScale = 1f;

        internal static int DefaultFontSize = (int)((Screen.width + Screen.height) * 0.01f);

        private IDebuggerManager m_DebuggerManager = null;
        private Rect m_DragRect = new Rect(0f, 0f, float.MaxValue, (float)Screen.height / 35);
        private Rect m_IconRect = new Rect();
        private Rect m_WindowRect = new Rect();
        private float m_WindowScale = DefaultWindowScale;
        private Vector2 m_ScreenSize = new Vector2(Screen.width, Screen.height);

        [SerializeField]
        private GUISkin m_Skin = null;

        [SerializeField]
        private DebuggerActiveWindowType m_ActiveWindow = DebuggerActiveWindowType.Open;

        [SerializeField]
        private bool m_ShowFullWindow = false;

        [SerializeField]
        private ConsoleWindow m_ConsoleWindow = new ConsoleWindow();

        private SystemInformationWindow m_SystemInformationWindow = new SystemInformationWindow();
        private EnvironmentInformationWindow m_EnvironmentInformationWindow = new EnvironmentInformationWindow();
        private ScreenInformationWindow m_ScreenInformationWindow = new ScreenInformationWindow();
        private GraphicsInformationWindow m_GraphicsInformationWindow = new GraphicsInformationWindow();
        private InputSummaryInformationWindow m_InputSummaryInformationWindow = new InputSummaryInformationWindow();
        private InputTouchInformationWindow m_InputTouchInformationWindow = new InputTouchInformationWindow();
        private InputLocationInformationWindow m_InputLocationInformationWindow = new InputLocationInformationWindow();
        private InputAccelerationInformationWindow m_InputAccelerationInformationWindow = new InputAccelerationInformationWindow();
        private InputGyroscopeInformationWindow m_InputGyroscopeInformationWindow = new InputGyroscopeInformationWindow();
        private InputCompassInformationWindow m_InputCompassInformationWindow = new InputCompassInformationWindow();
        private PathInformationWindow m_PathInformationWindow = new PathInformationWindow();
        private SceneInformationWindow m_SceneInformationWindow = new SceneInformationWindow();
        private TimeInformationWindow m_TimeInformationWindow = new TimeInformationWindow();
        private QualityInformationWindow m_QualityInformationWindow = new QualityInformationWindow();
        private ProfilerInformationWindow m_ProfilerInformationWindow = new ProfilerInformationWindow();
        private WebPlayerInformationWindow m_WebPlayerInformationWindow = new WebPlayerInformationWindow();
        private RuntimeMemoryInformationWindow<Object> m_RuntimeMemoryAllInformationWindow = new RuntimeMemoryInformationWindow<Object>();
        private RuntimeMemoryInformationWindow<Texture> m_RuntimeMemoryTextureInformationWindow = new RuntimeMemoryInformationWindow<Texture>();
        private RuntimeMemoryInformationWindow<Mesh> m_RuntimeMemoryMeshInformationWindow = new RuntimeMemoryInformationWindow<Mesh>();
        private RuntimeMemoryInformationWindow<Material> m_RuntimeMemoryMaterialInformationWindow = new RuntimeMemoryInformationWindow<Material>();
        private RuntimeMemoryInformationWindow<AnimationClip> m_RuntimeMemoryAnimationClipInformationWindow = new RuntimeMemoryInformationWindow<AnimationClip>();
        private RuntimeMemoryInformationWindow<AudioClip> m_RuntimeMemoryAudioClipInformationWindow = new RuntimeMemoryInformationWindow<AudioClip>();
        private RuntimeMemoryInformationWindow<Font> m_RuntimeMemoryFontInformationWindow = new RuntimeMemoryInformationWindow<Font>();
        private RuntimeMemoryInformationWindow<GameObject> m_RuntimeMemoryGameObjectInformationWindow = new RuntimeMemoryInformationWindow<GameObject>();
        private RuntimeMemoryInformationWindow<UnityEngine.Component> m_RuntimeMemoryComponentInformationWindow = new RuntimeMemoryInformationWindow<UnityEngine.Component>();
        private ObjectPoolInformationWindow m_ObjectPoolInformationWindow = new ObjectPoolInformationWindow();

        private GeneralSettingsWindow m_GeneralSettingsWindow = new GeneralSettingsWindow();
        private QualitySettingsWindow m_QualitySettingsWindow = new QualitySettingsWindow();
        private OperationSettingsWindow m_OperationSettingsWindow = new OperationSettingsWindow();

        //add by shaco 2017/6/8 
        private int m_DebuggerWindowID = typeof(DebuggerComponent).GetHashCode();
        //add end

        private FpsCounter m_FpsCounter = null;

        /// <summary>
        /// 获取或设置调试窗口是否激活。
        /// </summary>
        public bool ActiveWindow
        {
            get
            {
                return m_DebuggerManager.ActiveWindow;
            }
            set
            {
                m_DebuggerManager.ActiveWindow = value;
                enabled = value;
            }
        }

        /// <summary>
        /// 获取或设置是否显示完整调试器界面。
        /// </summary>
        public bool ShowFullWindow
        {
            get
            {
                return m_ShowFullWindow;
            }
            set
            {
                SetShowFullWindow(value);
            }
        }

        /// <summary>
        /// 获取或设置调试器漂浮框大小。
        /// </summary>
        public Rect IconRect
        {
            get
            {
                return m_IconRect;
            }
            set
            {
                m_IconRect = value;
            }
        }

        /// <summary>
        /// 获取或设置调试器窗口大小。
        /// </summary>
        public Rect WindowRect
        {
            get
            {
                return m_WindowRect;
            }
            set
            {
                m_WindowRect = value;
            }
        }

        /// <summary>
        /// 获取或设置调试器窗口缩放比例。
        /// </summary>
        public float WindowScale
        {
            get
            {
                return m_WindowScale;
            }
            set
            {
                m_WindowScale = value;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            //add by shaco 2017/6/8 
            ResetDebuggerWindowRect();
            //add end

            m_DebuggerManager = GameFrameworkEntry.GetModule<IDebuggerManager>();
            if (m_DebuggerManager == null)
            {
                Log.Fatal("Debugger manager is invalid.");
                return;
            }

            if (m_ActiveWindow == DebuggerActiveWindowType.Auto)
            {
                ActiveWindow = Debug.isDebugBuild;
            }
            else
            {
                ActiveWindow = (m_ActiveWindow == DebuggerActiveWindowType.Open);
            }

            m_FpsCounter = new FpsCounter(0.5f);
        }

        private void Start()
        {
            RegisterDebuggerWindow("Console", m_ConsoleWindow);
            RegisterDebuggerWindow("Information/System", m_SystemInformationWindow);
            RegisterDebuggerWindow("Information/Environment", m_EnvironmentInformationWindow);
            RegisterDebuggerWindow("Information/Screen", m_ScreenInformationWindow);
            RegisterDebuggerWindow("Information/Graphics", m_GraphicsInformationWindow);
            RegisterDebuggerWindow("Information/Input/Summary", m_InputSummaryInformationWindow);
            RegisterDebuggerWindow("Information/Input/Touch", m_InputTouchInformationWindow);
            RegisterDebuggerWindow("Information/Input/Location", m_InputLocationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Acceleration", m_InputAccelerationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Gyroscope", m_InputGyroscopeInformationWindow);
            RegisterDebuggerWindow("Information/Input/Compass", m_InputCompassInformationWindow);
            RegisterDebuggerWindow("Information/Other/Scene", m_SceneInformationWindow);
            RegisterDebuggerWindow("Information/Other/Path", m_PathInformationWindow);
            RegisterDebuggerWindow("Information/Other/Time", m_TimeInformationWindow);
            RegisterDebuggerWindow("Information/Other/Quality", m_QualityInformationWindow);
            RegisterDebuggerWindow("Information/Other/Web Player", m_WebPlayerInformationWindow);
            RegisterDebuggerWindow("Profiler/Summary", m_ProfilerInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/All", m_RuntimeMemoryAllInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Texture", m_RuntimeMemoryTextureInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Mesh", m_RuntimeMemoryMeshInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Material", m_RuntimeMemoryMaterialInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/AnimationClip", m_RuntimeMemoryAnimationClipInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/AudioClip", m_RuntimeMemoryAudioClipInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Font", m_RuntimeMemoryFontInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/GameObject", m_RuntimeMemoryGameObjectInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Component", m_RuntimeMemoryComponentInformationWindow);

            if (GameEntry.GetComponent<ObjectPoolComponent>() != null)
            {
                RegisterDebuggerWindow("Profiler/Object Pool", m_ObjectPoolInformationWindow);
            }
            RegisterDebuggerWindow("Settings/General", m_GeneralSettingsWindow);
            RegisterDebuggerWindow("Settings/Quality", m_QualitySettingsWindow);
            RegisterDebuggerWindow("Settings/Operation", m_OperationSettingsWindow);
        }

        private void Update()
        {
            m_FpsCounter.Update(Time.deltaTime, Time.unscaledDeltaTime);

            //add by shaco 2017/6/8
            if (m_ScreenSize.x != Screen.width || m_ScreenSize.y != Screen.height)
            {
                m_ScreenSize = new Vector2(Screen.width, Screen.height);
                ResetDebuggerWindowRect();
            }
            //add end
        }

        private void OnGUI()
        {
            if (m_DebuggerManager == null || !m_DebuggerManager.ActiveWindow)
            {
                return;
            }

            GUISkin cachedGuiSkin = GUI.skin;
            Matrix4x4 cachedMatrix = GUI.matrix;

            GUI.skin = m_Skin;
            GUI.matrix = Matrix4x4.Scale(new Vector3(m_WindowScale, m_WindowScale, 1f));

            GUI.skin.button.fontSize = DefaultFontSize;
            GUI.skin.label.fontSize = DefaultFontSize;
            GUI.skin.toggle.fontSize = DefaultFontSize;

            if (m_ShowFullWindow)
            {
                m_WindowRect = GUILayout.Window(m_DebuggerWindowID, m_WindowRect, DrawWindow, "GAME FRAMEWORK DEBUGGER");
            }
            else
            {
                m_IconRect = GUILayout.Window(m_DebuggerWindowID, m_IconRect, DrawDebuggerWindowIcon, "DEBUG");
            }

            GUI.matrix = cachedMatrix;
            GUI.skin = cachedGuiSkin;
        }

        /// <summary>
        /// 注册调试窗口。
        /// </summary>
        /// <param name="path">调试窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试窗口。</param>
        /// <param name="args">初始化调试窗口参数。</param>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            m_DebuggerManager.RegisterDebuggerWindow(path, debuggerWindow, args);
        }

        /// <summary>
        /// 获取调试窗口。
        /// </summary>
        /// <param name="path">调试窗口路径。</param>
        /// <returns>要获取的调试窗口。</returns>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return m_DebuggerManager.GetDebuggerWindow(path);
        }

        private void DrawWindow(int windowId)
        {
            GUI.DragWindow(m_DragRect);
            GUILayout.Space(m_WindowRect.height * 0.015f);
            DrawDebuggerWindowGroup(m_DebuggerManager.DebuggerWindowRoot);
        }

        private void DrawDebuggerWindowGroup(IDebuggerWindowGroup debuggerWindowGroup)
        {
            if (debuggerWindowGroup == null)
            {
                return;
            }

            List<string> names = new List<string>();
            string[] debuggerWindowNames = debuggerWindowGroup.GetDebuggerWindowNames();
            for (int i = 0; i < debuggerWindowNames.Length; i++)
            {
                names.Add(string.Format("{0}", debuggerWindowNames[i]));
            }

            if (debuggerWindowGroup == m_DebuggerManager.DebuggerWindowRoot)
            {
                names.Add("Close");
            }

            int toolbarIndex = GUILayout.Toolbar(debuggerWindowGroup.SelectedIndex, names.ToArray(), GUILayout.Height(DefaultWindowRect.height / 12), GUILayout.Width(DefaultWindowRect.width * 0.98f));
            if (toolbarIndex >= debuggerWindowGroup.DebuggerWindowCount)
            {
                SetShowFullWindow(false);
                return;
            }

            if (debuggerWindowGroup.SelectedIndex != toolbarIndex)
            {
                debuggerWindowGroup.SelectedWindow.OnLeave();
                debuggerWindowGroup.SelectedIndex = toolbarIndex;
                debuggerWindowGroup.SelectedWindow.OnEnter();
            }

            IDebuggerWindowGroup subDebuggerWindowGroup = debuggerWindowGroup.SelectedWindow as IDebuggerWindowGroup;
            if (subDebuggerWindowGroup != null)
            {
                DrawDebuggerWindowGroup(subDebuggerWindowGroup);
            }

            if (debuggerWindowGroup.SelectedWindow != null)
            {
                debuggerWindowGroup.SelectedWindow.OnDraw();
            }
        }

        private void DrawDebuggerWindowIcon(int windowId)
        {
            GUI.DragWindow(m_DragRect);
            GUILayout.Space(m_WindowRect.height * 0.01f);
            Color32 color = Color.white;
            m_ConsoleWindow.RefreshCount();

            //modify by shaco 2017/6/8
            if (m_ConsoleWindow.FatalFilter && m_ConsoleWindow.FatalCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Exception);
            }
            else if (m_ConsoleWindow.ErrorFilter && m_ConsoleWindow.ErrorCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Error);
            }
            else if (m_ConsoleWindow.WarningFilter && m_ConsoleWindow.WarningCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Warning);
            }
            else
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Log);
            }
            //modify end

            string title = string.Format("<color=#{0}{1}{2}{3}>FPS: {4}</color>", color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"), color.a.ToString("x2"), m_FpsCounter.CurrentFps.ToString("F2"));
            if (GUILayout.Button(title, GUILayout.MaxWidth(IconRect.width), GUILayout.MaxHeight(IconRect.height)))
            {
                SetShowFullWindow(true);
            }
        }

        //add by shaco 2017/6/8
        private void SetShowFullWindow(bool isShow)
        {
            if (m_ShowFullWindow == isShow)
            {
                return;
            }

            m_ShowFullWindow = isShow;
            if (isShow)
            {
                ResetDebuggerWindowRect();
            }
        }

        private void ResetDebuggerWindowRect()
        {
            DefaultIconRect = new Rect(0, 0, (float)Screen.width / 8, (float)Screen.height / 8);
            DefaultIconRect.position = new Vector2(Screen.width - DefaultIconRect.width, Screen.height - DefaultIconRect.height);
            DefaultWindowRect = new Rect(0, 0, (float)Screen.width, (float)Screen.height);

            IconRect = DefaultIconRect;
            WindowRect = DefaultWindowRect;
            WindowScale = DefaultWindowScale;
        }
        //add end
    }
}
#endif