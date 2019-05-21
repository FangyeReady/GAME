#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace shaco.iOS.XcodeEditor
{
    public partial class XCProject : System.IDisposable
    {
        private PBXDictionary _datastore;
        public PBXDictionary _objects;
        //private PBXDictionary _configurations;

        private PBXGroup _rootGroup;
        //private string _defaultConfigurationName;
        private string _rootObjectKey;

        public string projectRootPath { get; private set; }
        private FileInfo projectFileInfo;

        public string filePath { get; private set; }
        //private string sourcePathRoot;
        private bool modified = false;

        #region Data

        // Objects
        private PBXSortedDictionary<PBXBuildFile> _buildFiles;
        private PBXSortedDictionary<PBXGroup> _groups;
        private PBXSortedDictionary<PBXFileReference> _fileReferences;
        private PBXDictionary<PBXNativeTarget> _nativeTargets;

        private PBXDictionary<PBXFrameworksBuildPhase> _frameworkBuildPhases;
        private PBXDictionary<PBXResourcesBuildPhase> _resourcesBuildPhases;
        private PBXDictionary<PBXShellScriptBuildPhase> _shellScriptBuildPhases;
        private PBXDictionary<PBXSourcesBuildPhase> _sourcesBuildPhases;
        private PBXDictionary<PBXCopyFilesBuildPhase> _copyBuildPhases;

        private PBXDictionary<PBXVariantGroup> _variantGroups;
        private PBXDictionary<XCBuildConfiguration> _buildConfigurations;
        private PBXSortedDictionary<XCConfigurationList> _configurationLists;

        private PBXProject _project;

        #endregion


        #region Constructor

        public XCProject()
        {

        }

        public XCProject(string filePath) : this()
        {
            if (!System.IO.Directory.Exists(filePath))
            {
                Debug.LogWarning("XCode project path does not exist: " + filePath);
                return;
            }

            if (filePath.EndsWith(".xcodeproj"))
            {
                Debug.Log("Opening project " + filePath);
                this.projectRootPath = Path.GetDirectoryName(filePath);
                this.filePath = filePath;
            }
            else
            {
                Debug.Log("Looking for xcodeproj files in " + filePath);
                string[] projects = System.IO.Directory.GetDirectories(filePath, "*.xcodeproj");
                if (projects.Length == 0)
                {
                    Debug.LogWarning("Error: missing xcodeproj file");
                    return;
                }

                this.projectRootPath = filePath;
                //if the path is relative to the project, we need to make it absolute
                if (!System.IO.Path.IsPathRooted(projectRootPath))
                    projectRootPath = Application.dataPath.Replace("Assets", "") + projectRootPath;
                //Debug.Log ("projectRootPath adjusted to " + projectRootPath);
                this.filePath = projects[0];
            }

            projectFileInfo = new FileInfo(combinePath(this.filePath, "project.pbxproj"));
            string contents = projectFileInfo.OpenText().ReadToEnd();

            PBXParser parser = new PBXParser();
            _datastore = parser.Decode(contents);
            if (_datastore == null)
            {
                throw new System.Exception("Project file not found at file path " + filePath);
            }

            if (!_datastore.ContainsKey("objects"))
            {
                Debug.Log("Errore " + _datastore.Count);
                return;
            }

            _objects = (PBXDictionary)_datastore["objects"];
            modified = false;

            _rootObjectKey = (string)_datastore["rootObject"];
            if (!string.IsNullOrEmpty(_rootObjectKey))
            {
                _project = new PBXProject(_rootObjectKey, (PBXDictionary)_objects[_rootObjectKey]);
                _rootGroup = new PBXGroup(_rootObjectKey, (PBXDictionary)_objects[_project.mainGroupID]);
            }
            else
            {
                Debug.LogWarning("error: project has no root object");
                _project = null;
                _rootGroup = null;
            }

        }

        #endregion


        #region Properties

        public PBXProject project
        {
            get
            {
                return _project;
            }
        }

        public PBXGroup rootGroup
        {
            get
            {
                return _rootGroup;
            }
        }

        public PBXSortedDictionary<PBXBuildFile> buildFiles
        {
            get
            {
                if (_buildFiles == null)
                {
                    _buildFiles = new PBXSortedDictionary<PBXBuildFile>(_objects);
                }
                return _buildFiles;
            }
        }

        public PBXSortedDictionary<PBXGroup> groups
        {
            get
            {
                if (_groups == null)
                {
                    _groups = new PBXSortedDictionary<PBXGroup>(_objects);
                }
                return _groups;
            }
        }

        public PBXSortedDictionary<PBXFileReference> fileReferences
        {
            get
            {
                if (_fileReferences == null)
                {
                    _fileReferences = new PBXSortedDictionary<PBXFileReference>(_objects);
                }
                return _fileReferences;
            }
        }

        public PBXDictionary<PBXVariantGroup> variantGroups
        {
            get
            {
                if (_variantGroups == null)
                {
                    _variantGroups = new PBXDictionary<PBXVariantGroup>(_objects);
                }
                return _variantGroups;
            }
        }

        public PBXDictionary<PBXNativeTarget> nativeTargets
        {
            get
            {
                if (_nativeTargets == null)
                {
                    _nativeTargets = new PBXDictionary<PBXNativeTarget>(_objects);
                }
                return _nativeTargets;
            }
        }

        public PBXDictionary<XCBuildConfiguration> buildConfigurations
        {
            get
            {
                if (_buildConfigurations == null)
                {
                    _buildConfigurations = new PBXDictionary<XCBuildConfiguration>(_objects);
                }
                return _buildConfigurations;
            }
        }

        public PBXSortedDictionary<XCConfigurationList> configurationLists
        {
            get
            {
                if (_configurationLists == null)
                {
                    _configurationLists = new PBXSortedDictionary<XCConfigurationList>(_objects);
                }
                return _configurationLists;
            }
        }

        public PBXDictionary<PBXFrameworksBuildPhase> frameworkBuildPhases
        {
            get
            {
                if (_frameworkBuildPhases == null)
                {
                    _frameworkBuildPhases = new PBXDictionary<PBXFrameworksBuildPhase>(_objects);
                }
                return _frameworkBuildPhases;
            }
        }

        public PBXDictionary<PBXResourcesBuildPhase> resourcesBuildPhases
        {
            get
            {
                if (_resourcesBuildPhases == null)
                {
                    _resourcesBuildPhases = new PBXDictionary<PBXResourcesBuildPhase>(_objects);
                }
                return _resourcesBuildPhases;
            }
        }

        public PBXDictionary<PBXShellScriptBuildPhase> shellScriptBuildPhases
        {
            get
            {
                if (_shellScriptBuildPhases == null)
                {
                    _shellScriptBuildPhases = new PBXDictionary<PBXShellScriptBuildPhase>(_objects);
                }
                return _shellScriptBuildPhases;
            }
        }

        public PBXDictionary<PBXSourcesBuildPhase> sourcesBuildPhases
        {
            get
            {
                if (_sourcesBuildPhases == null)
                {
                    _sourcesBuildPhases = new PBXDictionary<PBXSourcesBuildPhase>(_objects);
                }
                return _sourcesBuildPhases;
            }
        }

        public PBXDictionary<PBXCopyFilesBuildPhase> copyBuildPhases
        {
            get
            {
                if (_copyBuildPhases == null)
                {
                    _copyBuildPhases = new PBXDictionary<PBXCopyFilesBuildPhase>(_objects);
                }
                return _copyBuildPhases;
            }
        }

        #endregion


        #region PBXMOD

        public bool AddOtherCFlags(string flag)
        {
            return AddOtherCFlags(new PBXList(flag));
        }

        public bool AddOtherCFlags(PBXList flags)
        {
            foreach (KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddOtherCFlags(flags);
            }
            modified = true;
            return modified;
        }

        public bool AddOtherLinkerFlags(string flag)
        {
            return AddOtherLinkerFlags(new PBXList(flag));
        }

        public bool AddAppIcons(string appIconSetPath, IDictionary iconDic)
        {
            if (null == iconDic)
            {
                Debug.LogError("XCProject AddAppIcons error: iconDic is null");
                return false;
            }

            if (!iconDic.Contains("path")
                && !iconDic.Contains("size")
                && !iconDic.Contains("idiom")
                && !iconDic.Contains("scale"))
            {
                Debug.LogError("XCProject AddAppIcons error: missing parameter, appIconSetPath=" + appIconSetPath);
                return false;
            }

            string path = (string)iconDic["path"];
            var fullPath = Application.dataPath.ContactPath(path);

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!fullPath.EndsWith(".png"))
            {
                Debug.LogError("XCProject AddAppIcons error: must be 'png' file, fullPath=" + fullPath);
                return false;
            }

            if (!System.IO.File.Exists(fullPath))
            {
                Debug.LogError("XCProject AddAppIcons error: missing icon fullPath=" + fullPath);
                return false;
            }

            string fileName = shaco.Base.FileHelper.GetLastFileName(fullPath);
            string size = (string)iconDic["size"];
            string idiom = (string)iconDic["idiom"];
            string scale = (string)iconDic["scale"];

            //copy png file
            shaco.Base.FileHelper.CopyFileByUserPath(fullPath, Path.Combine(appIconSetPath, fileName));

            //set config json
            var contentsPath = appIconSetPath.ContactPath("Contents.json");
            var jsonContents = shaco.LitJson.JsonMapper.ToObject(System.IO.File.ReadAllText(contentsPath));
            var images = (IList)jsonContents["images"];

            bool isDuplicate = false;
            foreach (IDictionary entry in (IList)images)
            {
                if (entry.Contains("filename"))
                {
                    var fileNameTmp = entry["filename"].ToString();
                    if (fileNameTmp == fileName)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
            }
            if (isDuplicate)
            {
                Debug.LogWarning("XCProject AddAppIcon warning: have duplicate icon fileName=" + fileName + "\nfullPath=" + fullPath);
                return false;
            }
            else
            {
                var newImageInfo = new Dictionary<string, string>();
                newImageInfo["size"] = size;
                newImageInfo["idiom"] = idiom;
                newImageInfo["filename"] = fileName;
                newImageInfo["scale"] = scale;
                ((shaco.LitJson.JsonData)images).Add(newImageInfo);

                //resave contents json
                System.IO.File.WriteAllText(contentsPath, jsonContents.ToJson());
            }
            return true;
        }

        public bool AddOtherLinkerFlags(PBXList flags)
        {
            foreach (KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddOtherLinkerFlags(flags);
            }
            modified = true;
            return modified;
        }

        public bool overwriteBuildSetting(string settingName, string newValue, string buildConfigName = "all")
        {
            Debug.Log("overwriteBuildSetting " + settingName + " " + newValue + " " + buildConfigName);
            foreach (KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                //Debug.Log ("build config " + buildConfig);
                XCBuildConfiguration b = buildConfig.Value;
                if ((string)b.data["name"] == buildConfigName || (string)buildConfigName == "all")
                {
                    //Debug.Log ("found " + b.data["name"] + " config");
                    buildConfig.Value.overwriteBuildSetting(settingName, newValue);
                    modified = true;
                }
                else
                {
                    //Debug.LogWarning ("skipping " + buildConfigName + " config " + (string)b.data["name"]);
                }
            }
            return modified;
        }

        public bool AddHeaderSearchPaths(string path)
        {
            return AddHeaderSearchPaths(new PBXList(path));
        }

        public bool AddHeaderSearchPaths(PBXList paths)
        {
            Debug.Log("AddHeaderSearchPaths " + paths);
            foreach (KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddHeaderSearchPaths(paths);
            }
            modified = true;
            return modified;
        }

        public bool AddLibrarySearchPaths(string path)
        {
            return AddLibrarySearchPaths(new PBXList(path));
        }

        public bool AddLibrarySearchPaths(PBXList paths)
        {
            Debug.Log("AddLibrarySearchPaths " + paths);
            foreach (KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddLibrarySearchPaths(paths);
            }
            modified = true;
            return modified;
        }

        public bool AddFrameworkSearchPaths(string path)
        {
            return AddFrameworkSearchPaths(new PBXList(path));
        }

        public bool AddFrameworkSearchPaths(PBXList paths)
        {
            foreach (KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddFrameworkSearchPaths(paths);
            }
            modified = true;
            return modified;
        }

        public object GetObject(string guid)
        {
            return _objects[guid];
        }

        public PBXDictionary AddFile(string filePath, PBXGroup parent = null, string tree = "SOURCE_ROOT", bool createBuildFiles = true, bool weak = false)
        {
            Debug.Log("AddFile " + filePath + ", " + parent + ", " + tree + ", " + (createBuildFiles ? "TRUE" : "FALSE") + ", " + (weak ? "TRUE" : "FALSE"));

            PBXDictionary results = new PBXDictionary();
            if (filePath == null)
            {
                Debug.LogError("AddFile called with null filePath");
                return results;
            }

            string absPath = string.Empty;

            if (Path.IsPathRooted(filePath))
            {
                Debug.Log("Path is Rooted");
                absPath = filePath;
            }
            else if (tree.CompareTo("SDKROOT") != 0)
            {
                absPath = combinePath(Application.dataPath, filePath);
            }

            if (!(File.Exists(absPath) || Directory.Exists(absPath)) && tree.CompareTo("SDKROOT") != 0)
            {
                Debug.Log("Missing file: " + filePath);
                return results;
            }
            else if (tree.CompareTo("SOURCE_ROOT") == 0)
            {
                Debug.Log("Source Root File");
                System.Uri fileURI = new System.Uri(absPath);
                System.Uri rootURI = new System.Uri((projectRootPath + "/."));
                filePath = rootURI.MakeRelativeUri(fileURI).ToString();
            }
            else if (tree.CompareTo("GROUP") == 0)
            {
                Debug.Log("Group File");
                filePath = System.IO.Path.GetFileName(filePath);
            }

            if (parent == null)
            {
                parent = _rootGroup;
            }

            //Check if there is already a file
            PBXFileReference fileReference = GetFile(System.IO.Path.GetFileName(filePath));
            if (fileReference != null)
            {
                Debug.Log("File already exists: " + filePath); //not a warning, because this is normal for most builds!
                return null;
            }

            fileReference = new PBXFileReference(filePath, (TreeEnum)System.Enum.Parse(typeof(TreeEnum), tree));
            parent.AddChild(fileReference);
            fileReferences.Add(fileReference);
            results.Add(fileReference.guid, fileReference);

            //Create a build file for reference
            if (!string.IsNullOrEmpty(fileReference.buildPhase) && createBuildFiles)
            {

                switch (fileReference.buildPhase)
                {
                    case "PBXFrameworksBuildPhase":
                        foreach (KeyValuePair<string, PBXFrameworksBuildPhase> currentObject in frameworkBuildPhases)
                        {
                            BuildAddFile(fileReference, currentObject, weak);
                        }
                        if (!string.IsNullOrEmpty(absPath) && (tree.CompareTo("SOURCE_ROOT") == 0))
                        {
                            string libraryPath = combinePath("$(SRCROOT)", Path.GetDirectoryName(filePath));
                            if (File.Exists(absPath))
                            {
                                this.AddLibrarySearchPaths(new PBXList(libraryPath));
                            }
                            else
                            {
                                this.AddFrameworkSearchPaths(new PBXList(libraryPath));
                            }

                        }
                        break;
                    case "PBXResourcesBuildPhase":
                        foreach (KeyValuePair<string, PBXResourcesBuildPhase> currentObject in resourcesBuildPhases)
                        {
                            Debug.Log("Adding Resources Build File");
                            BuildAddFile(fileReference, currentObject, weak);
                        }
                        break;
                    case "PBXShellScriptBuildPhase":
                        foreach (KeyValuePair<string, PBXShellScriptBuildPhase> currentObject in shellScriptBuildPhases)
                        {
                            Debug.Log("Adding Script Build File");
                            BuildAddFile(fileReference, currentObject, weak);
                        }
                        break;
                    case "PBXSourcesBuildPhase":
                        foreach (KeyValuePair<string, PBXSourcesBuildPhase> currentObject in sourcesBuildPhases)
                        {
                            Debug.Log("Adding Source Build File");
                            BuildAddFile(fileReference, currentObject, weak);
                        }
                        break;
                    case "PBXCopyFilesBuildPhase":
                        foreach (KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject in copyBuildPhases)
                        {
                            Debug.Log("Adding Copy Files Build Phase");
                            BuildAddFile(fileReference, currentObject, weak);
                        }
                        break;
                    case null:
                        Debug.LogWarning("File Not Supported: " + filePath);
                        break;
                    default:
                        Debug.LogWarning("File Not Supported.");
                        return null;
                }
            }
            return results;
        }

        public PBXNativeTarget GetNativeTarget(string name)
        {
            PBXNativeTarget naviTarget = null;
            foreach (KeyValuePair<string, PBXNativeTarget> currentObject in nativeTargets)
            {
                string targetName = (string)currentObject.Value.data["name"];
                if (targetName == name)
                {
                    naviTarget = currentObject.Value;
                    break;
                }
            }
            return naviTarget;
        }

        public int GetBuildActionMask()
        {
            int buildActionMask = 0;
            foreach (var currentObject in copyBuildPhases)
            {
                buildActionMask = (int)currentObject.Value.data["buildActionMask"];
                break;
            }
            return buildActionMask;
        }

        public PBXCopyFilesBuildPhase AddEmbedFrameworkBuildPhase()
        {
            PBXCopyFilesBuildPhase phase = null;

            PBXNativeTarget naviTarget = GetNativeTarget("Unity-iPhone");
            if (naviTarget == null)
            {
                Debug.Log("Not found Correct NativeTarget.");
                return phase;
            }

            //check if embed framework buildPhase exist
            foreach (var currentObject in copyBuildPhases)
            {
                object nameObj = null;
                if (currentObject.Value.data.TryGetValue("name", out nameObj))
                {
                    string name = (string)nameObj;
                    if (name == "Embed Frameworks")
                        return currentObject.Value;
                }
            }

            int buildActionMask = this.GetBuildActionMask();
            phase = new PBXCopyFilesBuildPhase(buildActionMask);
            var buildPhases = (ArrayList)naviTarget.data["buildPhases"];
            buildPhases.Add(phase.guid);//add build phase
            copyBuildPhases.Add(phase);
            return phase;
        }

        public void AddEmbedFramework(string fileName)
        {
            Debug.Log("Add Embed Framework: " + fileName);

            //Check if there is already a file
            PBXFileReference fileReference = GetFile(System.IO.Path.GetFileName(fileName));
            if (fileReference == null)
            {
                Debug.Log("Embed Framework must added already: " + fileName);
                return;
            }

            var embedPhase = this.AddEmbedFrameworkBuildPhase();
            if (embedPhase == null)
            {
                Debug.Log("AddEmbedFrameworkBuildPhase Failed.");
                return;
            }

            //create a build file
            PBXBuildFile buildFile = new PBXBuildFile(fileReference);
            buildFile.AddCodeSignOnCopy();
            buildFiles.Add(buildFile);

            embedPhase.AddBuildFile(buildFile);
        }

        private void BuildAddFile(PBXFileReference fileReference, KeyValuePair<string, PBXFrameworksBuildPhase> currentObject, bool weak)
        {
            PBXBuildFile buildFile = new PBXBuildFile(fileReference, weak);
            buildFiles.Add(buildFile);
            currentObject.Value.AddBuildFile(buildFile);
        }
        private void BuildAddFile(PBXFileReference fileReference, KeyValuePair<string, PBXResourcesBuildPhase> currentObject, bool weak)
        {
            PBXBuildFile buildFile = new PBXBuildFile(fileReference, weak);
            buildFiles.Add(buildFile);
            currentObject.Value.AddBuildFile(buildFile);
        }
        private void BuildAddFile(PBXFileReference fileReference, KeyValuePair<string, PBXShellScriptBuildPhase> currentObject, bool weak)
        {
            PBXBuildFile buildFile = new PBXBuildFile(fileReference, weak);
            buildFiles.Add(buildFile);
            currentObject.Value.AddBuildFile(buildFile);
        }
        private void BuildAddFile(PBXFileReference fileReference, KeyValuePair<string, PBXSourcesBuildPhase> currentObject, bool weak)
        {
            PBXBuildFile buildFile = new PBXBuildFile(fileReference, weak);
            buildFiles.Add(buildFile);
            currentObject.Value.AddBuildFile(buildFile);
        }
        private void BuildAddFile(PBXFileReference fileReference, KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject, bool weak)
        {
            PBXBuildFile buildFile = new PBXBuildFile(fileReference, weak);
            buildFiles.Add(buildFile);
            currentObject.Value.AddBuildFile(buildFile);
        }

        public bool AddFolder(string folderPath, PBXGroup parent = null, string[] exclude = null, bool recursive = true, bool createBuildFile = true)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.Log("XCProject AddFolder erorr: missing folder=" + folderPath);
                return false;
            }

            Debug.Log("Folder PATH: " + folderPath);

            if (folderPath.EndsWith(".lproj"))
            {
                Debug.Log("Ended with .lproj");
                return AddLocFolder(folderPath, parent, exclude, createBuildFile);
            }

            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(folderPath);

            if (exclude == null)
            {
                Debug.Log("Exclude was null");
                exclude = new string[] { };
            }

            if (parent == null)
            {
                Debug.Log("Parent was null");
                parent = rootGroup;
            }

            // Create group
            PBXGroup newGroup = GetGroup(sourceDirectoryInfo.Name, null /*relative path*/, parent);
            Debug.Log("New Group created");

            foreach (string directory in Directory.GetDirectories(folderPath))
            {
                Debug.Log("DIR: " + directory);
                if (directory.EndsWith(".bundle") || directory.EndsWith(".xcdatamodeld"))
                {
                    // Treat it like a file and copy even if not recursive
                    Debug.LogWarning("This is a special folder: " + directory);
                    AddFile(directory, newGroup, "SOURCE_ROOT", createBuildFile);
                    continue;
                }

                if (recursive)
                {
                    Debug.Log("recursive");
                    AddFolder(directory, newGroup, exclude, recursive, createBuildFile);
                }
            }

            // Adding files.
            string regexExclude = string.Format(@"{0}", string.Join("|", exclude));
            foreach (string file in Directory.GetFiles(folderPath))
            {
                if (Regex.IsMatch(file, regexExclude))
                {
                    continue;
                }
                Debug.Log("Adding Files for Folder");
                AddFile(file, newGroup, "SOURCE_ROOT", createBuildFile);
            }

            modified = true;
            return modified;
        }

        // We support neither recursing into nor bundles contained inside loc folders
        public bool AddLocFolder(string folderPath, PBXGroup parent = null, string[] exclude = null, bool createBuildFile = true)
        {
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(folderPath);

            if (exclude == null)
                exclude = new string[] { };

            if (parent == null)
                parent = rootGroup;

            // Create group as needed
            System.Uri projectFolderURI = new System.Uri(projectFileInfo.DirectoryName);
            System.Uri locFolderURI = new System.Uri(folderPath);
            var relativePath = projectFolderURI.MakeRelativeUri(locFolderURI).ToString();
            PBXGroup newGroup = GetGroup(sourceDirectoryInfo.Name, relativePath, parent);

            // Add loc region to project
            string nom = sourceDirectoryInfo.Name;
            string region = nom.Substring(0, nom.Length - ".lproj".Length);
            project.AddRegion(region);

            // Adding files.
            string regexExclude = string.Format(@"{0}", string.Join("|", exclude));
            foreach (string file in Directory.GetFiles(folderPath))
            {
                if (Regex.IsMatch(file, regexExclude))
                {
                    continue;
                }

                // Add a variant group for the language as well
                var variant = new PBXVariantGroup(System.IO.Path.GetFileName(file), null, "GROUP");
                variantGroups.Add(variant);

                // The group gets a reference to the variant, not to the file itself
                newGroup.AddChild(variant);

                AddFile(file, variant, "GROUP", createBuildFile);
            }

            modified = true;
            return modified;
        }
        #endregion

        #region Getters
        public PBXFileReference GetFile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            foreach (KeyValuePair<string, PBXFileReference> current in fileReferences)
            {
                if (!string.IsNullOrEmpty(current.Value.name) && current.Value.name.CompareTo(name) == 0)
                {
                    return current.Value;
                }
            }

            return null;
        }

        public PBXGroup GetGroup(string name, string path = null, PBXGroup parent = null)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (parent == null) parent = rootGroup;

            foreach (KeyValuePair<string, PBXGroup> current in groups)
            {
                if (string.IsNullOrEmpty(current.Value.name))
                {
                    if (current.Value.path.CompareTo(name) == 0 && parent.HasChild(current.Key))
                    {
                        return current.Value;
                    }
                }
                else if (current.Value.name.CompareTo(name) == 0 && parent.HasChild(current.Key))
                {
                    return current.Value;
                }
            }

            PBXGroup result = new PBXGroup(name, path);
            groups.Add(result);
            parent.AddChild(result);

            modified = true;
            return result;
        }

        #endregion

        #region Mods		
        public void ApplyMod(string pbxmod)
        {
            XCMod mod = new XCMod(pbxmod);
            ApplyMod(mod);
        }

        public string combinePath(string path1, string path2)
        {
            return path1 + "/" + path2;
        }

        private string GetExtensions(string filename)
        {
            int findIndex = filename.LastIndexOf('.');
            if (findIndex == -1)
            {
                return string.Empty;
            }
            else
            {
                return filename.Substring(findIndex + 1, filename.Length - findIndex - 1);
            }
        }

        public void ApplyMod(XCMod mod)
        {
            PBXGroup modGroup = this.GetGroup(mod.group);

            Debug.Log("Adding libraries...");
            // var listLibs = new List<XCModFile>();
            // var listFrameworks = new List<string>();
            PBXGroup frameworkGroup = this.GetGroup("Frameworks");
            foreach (string filename in mod.libraries)
            {
                var extensionTmp = GetExtensions(filename);
                if (extensionTmp == "framework")
                {
                    string[] filenames = filename.Split(':');
                    bool isWeak = (filename.Length > 1) ? true : false;
                    string completePath = combinePath("System/Library/Frameworks", filenames[0]);
                    this.AddFile(completePath, frameworkGroup, "SDKROOT", true, isWeak);
                }
                else
                {
                    var libRef = new XCModFile(filename);
                    string completeLibPath = combinePath("usr/lib", libRef.filePath);
                    Debug.Log("Adding library " + completeLibPath);
                    this.AddFile(completeLibPath, modGroup, "SDKROOT", true, libRef.isWeak);
                }
            }

            Debug.Log("Adding files...");
            foreach (string filePath in mod.files)
            {
                string absoluteFilePath = combinePath(mod.path, filePath);
                if (!System.IO.File.Exists(absoluteFilePath) && !System.IO.Directory.Exists(absoluteFilePath))
                {
                    Debug.LogError("XCProject AddFile error: missisng file=" + filePath);
                }
                else
                {
                    this.AddFile(absoluteFilePath, modGroup, "SDKROOT");
                }
            }

            Debug.Log("Adding embed binaries...");
            if (mod.embed_binaries != null)
            {
                //1. Add LD_RUNPATH_SEARCH_PATHS for embed framework
                this.overwriteBuildSetting("LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks", "Release");
                this.overwriteBuildSetting("LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks", "Debug");

                foreach (string binary in mod.embed_binaries)
                {
                    string absoluteFilePath = combinePath(mod.path, binary);
                    this.AddEmbedFramework(absoluteFilePath);
                }
            }

            Debug.Log("Adding folders...");
            foreach (string folderPath in mod.folders)
            {
                string absoluteFolderPath = combinePath(mod.path, folderPath);
                this.AddFolder(absoluteFolderPath, modGroup, (string[])mod.excludes.ToArray(typeof(string)));
            }

            Debug.Log("Adding headerpaths...");
            foreach (string headerpath in mod.headerpaths)
            {
                if (headerpath.Contains("$(inherited)"))
                {
                    Debug.Log("not prepending a path to " + headerpath);
                    this.AddHeaderSearchPaths(headerpath);
                }
                else
                {
                    string absoluteHeaderPath = combinePath(mod.path, headerpath);
                    this.AddHeaderSearchPaths(absoluteHeaderPath);
                }
            }

            Debug.Log("Adding librariespaths...");
            foreach (string librariespath in mod.librariespaths)
            {
                if (librariespath.Contains("$(inherited)"))
                {
                    Debug.Log("not prepending a path to " + librariespath);
                    this.AddLibrarySearchPaths(librariespath);
                }
                else
                {
                    string absoluteHeaderPath = combinePath(mod.path, librariespath);
                    this.AddLibrarySearchPaths(absoluteHeaderPath);
                }
            }

            Debug.Log("Adding compiler flags...");
            foreach (string flag in mod.compiler_flags)
            {
                this.AddOtherCFlags(flag);
            }

            Debug.Log("Adding linker flags...");
            foreach (string flag in mod.linker_flags)
            {
                this.AddOtherLinkerFlags(flag);
            }

            Debug.Log("Adding plist items...");
            string plistPath = this.projectRootPath + "/Info.plist";
            XCPlist plist = new XCPlist(plistPath);
            plist.Process(mod.plist);

            Debug.Log("Adding app icons...");
            string xcodeIconPath = this.projectRootPath + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/";
            foreach (IDictionary iconDic in mod.app_icons)
            {
                this.AddAppIcons(xcodeIconPath, iconDic);
            }

            this.Consolidate();
        }

        #endregion

        #region Savings	
        public void Consolidate()
        {
            PBXDictionary consolidated = new PBXDictionary();
            consolidated.Append<PBXBuildFile>(this.buildFiles);//sort!
            consolidated.Append<PBXCopyFilesBuildPhase>(this.copyBuildPhases);
            consolidated.Append<PBXFileReference>(this.fileReferences);//sort!
            consolidated.Append<PBXFrameworksBuildPhase>(this.frameworkBuildPhases);
            consolidated.Append<PBXGroup>(this.groups);//sort!
            consolidated.Append<PBXNativeTarget>(this.nativeTargets);
            consolidated.Add(project.guid, project.data);//TODO this should be named PBXProject?
            consolidated.Append<PBXResourcesBuildPhase>(this.resourcesBuildPhases);
            consolidated.Append<PBXShellScriptBuildPhase>(this.shellScriptBuildPhases);
            consolidated.Append<PBXSourcesBuildPhase>(this.sourcesBuildPhases);
            consolidated.Append<PBXVariantGroup>(this.variantGroups);
            consolidated.Append<XCBuildConfiguration>(this.buildConfigurations);
            consolidated.Append<XCConfigurationList>(this.configurationLists);
            _objects = consolidated;
            consolidated = null;
        }

        public void Backup()
        {
            string backupPath = combinePath(this.filePath, "project.backup.pbxproj");

            // Delete previous backup file
            if (File.Exists(backupPath))
                File.Delete(backupPath);

            // Backup original pbxproj file first
            File.Copy(combinePath(this.filePath, "project.pbxproj"), backupPath);
        }

        private void DeleteExisting(string path)
        {
            // Delete old project file
            if (File.Exists(path))
                File.Delete(path);
        }

        private void CreateNewProject(PBXDictionary result, string path)
        {
            PBXParser parser = new PBXParser();
            StreamWriter saveFile = File.CreateText(path);
            saveFile.Write(parser.Encode(result, true));
            saveFile.Close();
        }

        /// <summary>
        /// Saves a project after editing.
        /// </summary>
        public void Save()
        {
            PBXDictionary result = new PBXDictionary();
            result.Add("archiveVersion", 1);
            result.Add("classes", new PBXDictionary());
            result.Add("objectVersion", 46);

            Consolidate();
            result.Add("objects", _objects);

            result.Add("rootObject", _rootObjectKey);

            string projectPath = combinePath(this.filePath, "project.pbxproj");

            // Delete old project file, in case of an IOException 'Sharing violation on path Error'
            DeleteExisting(projectPath);

            // Parse result object directly into file
            CreateNewProject(result, projectPath);
        }

        /**
		* Raw project data.
		*/
        public Dictionary<string, object> objects
        {
            get
            {
                return null;
            }
        }
        #endregion

        public void Dispose()
        {

        }
    }
}

#endif