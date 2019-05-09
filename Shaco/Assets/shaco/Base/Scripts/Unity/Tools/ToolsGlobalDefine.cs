namespace shacoEditor
{
    public class ToolsGlobalDefine
    {
        public class MenuPriority
        {
            public enum Viewer
            {
                UI_MANAGER,
                EVENT_MANAGER,
                Observer,
                UI_STATE_CHANGED_INSPECTOR,
                BEHAVIOUR_TREE,
                BEHAVIOUR_RUNTIME_TREE,
            }

            public enum Tools
            {
                FILE_TOOLS,
                RUN_GAME,
                BUILD_INSPECTOR,
                EXPORT_PACKAGE_1,
                EXPORT_PACKAGE_2,
                MUILTY_PREFABS_APPLY,
                LOG_LOCATION,
                EXCEL_HELPER_INSPECTOR,
                CHANGE_COMPONENT_DATA,
                LOCALIZATION_REPLACE,
                UPDATE,
            }
            
            public class Default
            {
                public const int HOT_UPDATE_EXPORT = 0;
            }
        }

        public class ProjectMenuPriority
        {
            public const int FIND_REFERENCE = 20;
        }

        public class HierachyMenuPriority
        {
            public const int CREATE_UI = 80;
            public const int REFERENCE_BIND = 21;
            public const int RENAME_BY_SEQUEUE = 22;
        }
    }
}
