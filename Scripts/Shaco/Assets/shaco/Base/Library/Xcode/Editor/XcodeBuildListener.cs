using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace shaco.iOS.Xcode
{
    public static class XcodeBuildListener
    {
        static private string DEVELOPMENT_TEAM = string.Empty;
        static private string PROVISIONING_PROFILE_SPECIFIER = string.Empty;
        static private shacoEditor.BuildInspector.IOSCodeIndentity IOS_CODE_INDENTITY;

        [UnityEditor.Callbacks.PostProcessBuild(1)]
        public static void OnPostProcessBuild(UnityEditor.BuildTarget target, string projectRootPath)
        {
#if UNITY_5_3_OR_NEWER
            if (UnityEditor.BuildTarget.iOS != target)
#else
            if (UnityEditor.BuildTarget.iPhone != target)
#endif  
            {
                return;
            }

            InitEnviromentCommandValues();

            if (string.IsNullOrEmpty(DEVELOPMENT_TEAM))
            {
                DEVELOPMENT_TEAM = shacoEditor.BuildInspector.GetIOSDevelepmentTeam();
                PROVISIONING_PROFILE_SPECIFIER = shacoEditor.BuildInspector.GetIOSProvisioningProfileSpecifier();
                IOS_CODE_INDENTITY = shacoEditor.BuildInspector.GetIOSCodeIndentity();
            }

            try
            {
                ModifyProject(projectRootPath);
                BuildMods(projectRootPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static bool InitEnviromentCommandValues()
        {
            DEVELOPMENT_TEAM = UnityHelper.GetEnviromentCommandValue("DEVELOPMENT_TEAM");
            PROVISIONING_PROFILE_SPECIFIER = UnityHelper.GetEnviromentCommandValue("PROVISIONING_PROFILE_SPECIFIER");

            return !string.IsNullOrEmpty(DEVELOPMENT_TEAM) && !string.IsNullOrEmpty(PROVISIONING_PROFILE_SPECIFIER);
        }

        private static void ModifyProject(string projectRootPath)
        {
            if (string.IsNullOrEmpty(DEVELOPMENT_TEAM) && string.IsNullOrEmpty(PROVISIONING_PROFILE_SPECIFIER))
            {
                //DEVELOPMENT_TEAM and PROVISIONING_PROFILE_SPECIFIER use on automatic build
                Debug.LogWarning("XcodeBuildListener ModifyProject erorr: ios enviroment is invalid, please call 'InitEnviromentCommandValues' at frist");
                return;
            }

            var project = new PBXProject();

            //ReadFromFile will throw an exception if project is invalid / not found
            project.ReadFromFile(PBXProject.GetPBXProjectPath(projectRootPath));

            var targetGUIID = project.TargetGuidByName(PBXProject.GetUnityTargetName());
            ModifyProject(project, projectRootPath, targetGUIID);
        }

        private static void ModifyProject(PBXProject project, string projectRootPath, string targetGUIID)
        {
            if (!string.IsNullOrEmpty(DEVELOPMENT_TEAM))
            {
                project.SetTargetAttributes("DevelopmentTeam", DEVELOPMENT_TEAM);
                project.SetBuildProperty(targetGUIID, "DEVELOPMENT_TEAM", DEVELOPMENT_TEAM);
            }
            if (!string.IsNullOrEmpty(PROVISIONING_PROFILE_SPECIFIER))
            {
                project.SetBuildProperty(targetGUIID, "PROVISIONING_PROFILE_SPECIFIER", PROVISIONING_PROFILE_SPECIFIER);
                project.SetTargetAttributes("ProvisioningStyle", "Manual");
            }
            else
            {
                project.SetTargetAttributes("ProvisioningStyle", "Automatic");
            }

            switch (IOS_CODE_INDENTITY)
            {
                case shacoEditor.BuildInspector.IOSCodeIndentity.Development:
                    {
                        project.SetBuildProperty(targetGUIID, "CODE_SIGN_IDENTITY", "iPhone Developer");
                        project.SetBuildProperty(targetGUIID, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", "iPhone Developer");
                        break;
                    }
                case shacoEditor.BuildInspector.IOSCodeIndentity.Distribution:
                    {
                        project.SetBuildProperty(targetGUIID, "CODE_SIGN_IDENTITY", "iPhone Distribution");
                        project.SetBuildProperty(targetGUIID, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", "iPhone Distribution");
                        break;
                    }
                default: Debug.LogError("XcodeBuildListerner ModifyProject error: unsupport ios code indentity type=" + IOS_CODE_INDENTITY); break;
            }

            project.SetBuildProperty(targetGUIID, "ENABLE_BITCODE", "NO");
            project.Save(projectRootPath);

            //modify capability
            // var projectPathUnityIphone = Path.Combine(projectRootPath, PBXProject.GetUnityTargetName()) + "/";
            // var entitlementsPath = GetEntitlementsPath(project, projectPathUnityIphone, targetGUIID);
            // var capabilityManager = new ProjectCapabilityManager(PBXProject.GetPBXProjectPath(projectRootPath), PBXProject.GetUnityTargetName() + "/" + Path.GetFileName(entitlementsPath), PBXProject.GetUnityTargetName());

            // capabilityManager.AddGameCenter();
            // capabilityManager.AddPushNotifications(true);
            // #if UNITY_5_6_OR_NEWER
            //             capabilityManager.AddKeychainSharing(new string[]{UnityEditor.PlayerSettings.applicationIdentifier});
            // #else
            //             capabilityManager.AddKeychainSharing(new string[]{UnityEditor.PlayerSettings.bundleIdentifier});
            // #endif
            // capabilityManager.AddInAppPurchase();

            // capabilityManager.WriteToFile();
        }

        static private string GetEntitlementsPath(PBXProject project, string projectPathUnityIphone, string targetGUIID)
        {
            var retValue = string.Empty;

            var productName = project.GetBuildProperty(targetGUIID, "PRODUCT_NAME");
            retValue = Path.Combine(projectPathUnityIphone, productName + ".entitlements");

            if (string.IsNullOrEmpty(productName))
            {
                Debug.LogError("XcodeBuildListener getOrCreateEntitlementsPath error: not find PRODUCT_NAME");
                return retValue;
            }
            return retValue;
        }

        static private void BuildMods(string pathToBuiltProject)
        {
            // Create a new project object from build target
            var project = new shaco.iOS.XcodeEditor.XCProject(pathToBuiltProject);

            // Find and run through all projmods files to patch the project.
            // Please pay attention that ALL projmods files in your project folder will be excuted!
            string[] files = Directory.GetFiles(Application.dataPath, "*.xcodemods", SearchOption.AllDirectories);

            Debug.Log("OnPostProcessBuild------------------files count=" + files.Length);

            foreach (string file in files)
            {
                UnityEngine.Debug.Log("ProjMod File: " + file);
                project.ApplyMod(file);
            }

            // Finally save the xcode project
            project.Save();
        }
    }
}