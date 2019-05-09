using System.Collections;
using System.IO;
using System.Text;

namespace shaco.Base
{
    public static partial class FileHelper
    {
        static public bool CheckFolderPathWithAutoCreate(string path)
        {
            var folder = FileHelper.GetFolderNameByPath(path, FileDefine.PATH_FLAG_SPLIT);
            if (!Directory.Exists(folder))
            {
                Log.Info("FileHelper CheckFolderPathWithAutoCreate folder=" + folder);

                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (System.Exception e)
                {
                    ExceptionHandlingWrite(path, e);
                    return false;
                }
            }
            return true;
        }

        static public bool WriteAllByPersistent(string path, string data)
        {
            var fullpath = GetFullpath(path);
            CheckFolderPathWithAutoCreate(fullpath);

            return WriteAllByUserPath(fullpath, data);
        }

        static public bool WriteAllByUserPath(string path, string data)
        {
            CheckFolderPathWithAutoCreate(path);
            try
            {
                File.WriteAllText(path, data);
            }
            catch (System.Exception e)
            {
                ExceptionHandlingWrite(path, e);
                return false;
            }

            Log.Info("FileHelper WriteAllByUserPath path=" + path);
            return true;
        }

        static public bool AppendByPersistent(string path, string data)
        {
            var fullpath = GetFullpath(path);
            return AppendByUserPath(fullpath, data);
        }

        static public bool AppendByUserPath(string path, string data)
        {
            CheckFolderPathWithAutoCreate(path);
            try
            {
                File.AppendAllText(path, data);
            }
            catch (System.Exception e)
            {
                ExceptionHandlingWrite(path, e);
                return false;
            }

            // Log.Info("FileHelper AppendByUserPath path=" + path);
            return true;
        }

        static public bool WriteAllByteByPersistent(string path, byte[] data)
        {
            var fullpath = GetFullpath(path);
            CheckFolderPathWithAutoCreate(fullpath);

            return WriteAllByteByUserPath(fullpath, data);
        }

        static public bool WriteAllByteByUserPath(string path, byte[] data)
        {
            CheckFolderPathWithAutoCreate(path);
            try
            {
                File.WriteAllBytes(path, data);
            }
            catch (System.Exception e)
            {
                ExceptionHandlingWrite(path, e);
                return false;
            }

            Log.Info("FileHelper WriteAllByteByUserPath path=" + path);
            return true;
        }

        static public bool CopyFileByUserPath(string pathOld, string pathNew, params string[] ignorePatterns)
        {
            try
            {
                Log.Info("FileHelper CopyFileByUserPath \npathOld=" + pathOld + "\npathNew=" + pathNew);
                return CopyFileByUserPathBase(pathOld, pathNew, ignorePatterns);
            }
            catch (System.Exception e)
            {
                Log.Error("FileHelper CopyFileByUserPath error: msg=" + e.ToString() + "\npathOld=" + pathOld + " pathNew=" + pathNew);
                return false;
            }
        }

        static public bool MoveFileByUserPath(string pathSource, string pathDest)
        {
            if (pathSource == pathDest)
            {
                Log.Warning("FileHelper MoveFileByUserPath warning: is same path=" + pathSource);
                return false;
            }

            try
            {
                CheckFolderPathWithAutoCreate(pathDest);

                if (FileHelper.ExistsDirectory(pathDest) || FileHelper.ExistsFile(pathDest))
                {
                    FileHelper.DeleteByUserPath(pathDest);
                }

                if (FileHelper.ExistsFile(pathSource))
                {
                    File.Move(pathSource, pathDest);
                    Log.Info("FileHelper MoveFileByUserPath file" + "\npathSource=" + pathSource + "\npathDest=" + pathDest);
                }
                else if (FileHelper.ExistsDirectory(pathSource))
                {
                    Directory.Move(pathSource, pathDest);
                    Log.Info("FileHelper MoveFileByUserPath directory" + "\npathSource=" + pathSource + "\npathDest=" + pathDest);
                }
                else
                {
                    Log.Error("FileHelper move error: not find source path=" + pathSource);
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Log.Error("FileHelper move error: msg=" + e + "\npathSource=" + pathSource + " pathDest=" + pathDest);
                return false;
            }
            return true;
        }

        static public string ReadAllByPersistent(string path, string defaultValue = "")
        {
            var fullpath = GetFullpath(path);
            return ReadAllByUserPath(fullpath, defaultValue);
        }

        static public byte[] ReadAllByteByPersistent(string path, byte[] defaultValue = null)
        {
            var fullpath = GetFullpath(path);
            return ReadAllByteByUserPath(fullpath, defaultValue);
        }

        static public string ReadAllByUserPath(string path, string defaultValue = "")
        {
            string ret = defaultValue;

            if (!ExistsFile(path))
            {
                Log.Error("FileHelper ReadAllByUserPath error: not find path=" + path);
                return ret;
            }

            try
            {
                ret = File.ReadAllText(path);
            }
            catch (System.Exception e)
            {
                ExceptionHandlingRead(path, e);
            }

            return ret;
        }

        static public byte[] ReadAllByteByUserPath(string path, byte[] defaultValue = null)
        {
            byte[] ret = defaultValue;

            if (!ExistsFile(path))
            {
                Log.Error("FileHelper ReadAllByteByUserPath error: not find path=" + path);
                return ret;
            }

            try
            {
                ret = File.ReadAllBytes(path);
            }
            catch (System.Exception e)
            {
                ExceptionHandlingRead(path, e);
            }

            return ret;
        }

        /// <summary>
        /// 判断文件是否在被占用
        /// <param name="path">文件路径</param>
        /// <return></return>
        /// </summary>
        static public bool IsFileInUse(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            bool retValue = true;
            // FileStream fs = null;
            try
            {
                using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    retValue = false;
                }
            }
            catch { }
            return retValue;
        }

        static public bool isNumber(char c)
        {
            bool ret = true;
            if (c < '0' || c > '9')
            {
                if (c != '.' && c != '-')
                {
                    ret = false;
                }
            }
            return ret;
        }

        static public string GetFullpath(string path)
        {
            string ret = string.Empty;
            if (string.IsNullOrEmpty(FileDefine.persistentDataPath))
            {
                Log.Error("FileHelper getFulPath error: FileDefine.persistentDataPath is empty, please set it at first");
            }
            if (!path.Contains(FileDefine.persistentDataPath))
            {
                ret = FileHelper.ContactPath(FileDefine.persistentDataPath, path);
            }
            else
                ret = path;
            return ret;
        }

        static public string MD5FromFile(string file)
        {
            try
            {
                if (!ExistsFile(file))
                {
                    return string.Empty;
                }

                //				System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                //				FileStream stream = File.OpenRead(file);
                //				byte[] data2 = md5.ComputeHash(stream);
                //				
                //				return MD5ToStringArray(data2);

                return MD5FromByte(ReadAllByteByUserPath(file));
            }
            catch (System.Exception ex)
            {
                Log.Error("md5file() fail, error:" + ex.Message);
            }
            return string.Empty;
        }

        static public string MD5FromString(string data)
        {
            return MD5FromByte(data.ToByteArray());
        }

        static public string MD5FromByte(byte[] data)
        {
            string ret = string.Empty;
            try
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                ret = MD5ToStringArray(md5.ComputeHash(data));
            }
            catch (System.Exception e)
            {
                Log.Error("MD5FromByte exception: error message=" + e);
                ret = string.Empty;
            }

            return ret;
        }

        static public string MD5ToStringArray(byte[] buf)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < buf.Length; i++)
            {
                sb.Append(buf[i].ToString("X2"));
            }
            return sb.ToString();
        }

        static public void GetSeekPath(string path, ref System.Collections.Generic.List<string> outFiles, params string[] patterns)
        {
            GetSeekPath(path, ref outFiles, false, patterns);
        }

        static public void GetSeekPath(string path, ref System.Collections.Generic.List<string> outFiles, System.Func<string, bool> callbackIsValidPath)
        {
            GetSeekPathBase(path, ref outFiles, callbackIsValidPath);
        }

        static public void GetSeekPath(string path, ref System.Collections.Generic.List<string> outFiles, bool ignorePattern, params string[] patterns)
        {
            GetSeekPathBase(path, ref outFiles, (string outFile) =>
            {
                bool checkFile = ignorePattern;
                if (patterns != null && patterns.Length > 0)
                {
                    for (int i = 0; i < patterns.Length; ++i)
                    {
                        var patternTmp = patterns[i];
                        if (patternTmp.Length >= 2 && patternTmp[0] == '*' && patternTmp[patternTmp.Length - 1] == '*')
                        {
                            patternTmp = patternTmp.Substring(1, patternTmp.Length - 2);
                            if (outFile.Contains(patternTmp))
                            {
                                checkFile = !ignorePattern;
                                break;
                            }
                        }
                        else
                        {
                            if (outFile.EndsWith(patternTmp))
                            {
                                checkFile = !ignorePattern;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    checkFile = true;
                }

                return checkFile;
            });
        }

        /// <summary>
        /// 遍历目录下所有文件
        /// <param name="path">需要被遍历的目录路径</param>
        /// <param name="outFiles">遍历到的文件列表</param>
        /// <param name="callbackIsValidPath">遍历过程中，判断文件是否过滤的回调方法，允许为空</param>
        /// <return></return>
        /// </summary>
        static public void GetSeekPathBase(string path, ref System.Collections.Generic.List<string> outFiles, System.Func<string, bool> callbackIsValidPath)
        {
            if (!string.IsNullOrEmpty(path) && FileHelper.ExistsDirectory(path))
            {
                string[] files = FileHelper.GetFiles(path, "*");
                string outFile = string.Empty;

                for (int j = 0; j < files.Length; ++j)
                {
                    outFile = files[j];

                    bool checkFile = false;

                    if (callbackIsValidPath == null)
                        checkFile = true;
                    else
                    {
                        checkFile = callbackIsValidPath(outFile);
                    }

                    if (checkFile)
                    {
                        outFiles.Add(files[j]);
                    }
                }
                try
                {
                    string[] paths = FileHelper.GetDirectories(path);
                    for (int i = 0; i < paths.Length; ++i)
                    {
                        try
                        {
                            GetSeekPathBase(paths[i], ref outFiles, callbackIsValidPath);
                        }
                        catch (System.Exception e)
                        {
                            Log.Info("FileHelper GetSeekPathBase catch a exception: e" + e);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Log.Info("FileHelper GetSeekPathBase catch a exception: e" + e);
                }
            }
        }

        static public string GetLastFileName(string str, string flag, bool isKeepExtension)
        {
            string ret = str;
            int indexFind = str.LastIndexOf(flag);
            if (indexFind == -1)
            {
                if (!isKeepExtension)
                {
                    RemoveExtension(ret);
                }
                return ret;
            }

            ret = str.Substring(indexFind + flag.Length, str.Length - indexFind - flag.Length);

            if (!isKeepExtension)
            {
                ret = RemoveExtension(ret);
            }

            return ret;
        }
        static public string GetLastFileName(string str, bool isKeepExtension)
        {
            return GetLastFileName(str, FileDefine.PATH_FLAG_SPLIT, isKeepExtension);
        }

        static public string GetLastFileName(string str)
        {
            return GetLastFileName(str, FileDefine.PATH_FLAG_SPLIT, true);
        }

        static public string[] GetDirectories(string path)
        {
            var ret = System.IO.Directory.GetDirectories(path);

            for (int i = 0; i < ret.Length; ++i)
            {
                if (ret[i].Contains('\\'.ToString()))
                    ret[i] = ret[i].Replace('\\', '/');

                //兼容mac的虚拟机路径
                if (ret[i].StartsWith("//"))
                {
                    ret[i] = ret[i].ReplaceFromBegin("//", "\\\\", 1);
                }
            }

            return ret;
        }

        static public string[] GetFiles(string path, string searchPattern)
        {
            var ret = System.IO.Directory.GetFiles(path, searchPattern);

            for (int i = 0; i < ret.Length; ++i)
            {
                if (ret[i].Contains('\\'.ToString()))
                    ret[i] = ret[i].Replace('\\', '/');

                //兼容mac的虚拟机路径
                if (ret[i].StartsWith("//"))
                {
                    ret[i] = ret[i].ReplaceFromBegin("//", "\\\\", 1);
                }
            }

            return ret;
        }

        /// <summary>
        /// Removes the file name extension.
        /// </summary>
        /// <param name="filename">Filename.</param>
        static public string RemoveExtension(string filename)
        {
            int indexFind = filename.LastIndexOf(FileDefine.DOT_SPLIT);
            if (indexFind != -1)
            {
                filename = filename.Remove(indexFind, filename.Length - indexFind);
            }
            else
            {
                Log.Warning("FileHelper RemoveExtension warning: missing extension ! filename=" + filename);
            }
            return filename;
        }

        static public string ReplaceExtension(string filename, string extension)
        {
            filename = RemoveExtension(filename);
            return AddExtensions(filename, extension);
        }

        static public string AddExtensions(string str, params string[] extensions)
        {
            string ret = str;
            if (!string.IsNullOrEmpty(ret) && ret[ret.Length - 1].ToString() != FileDefine.DOT_SPLIT)
            {
                for (int i = 0; i < extensions.Length; ++i)
                {
                    var strTmp = extensions[i];
                    if (!string.IsNullOrEmpty(strTmp) && strTmp[0].ToString() != FileDefine.DOT_SPLIT)
                    {
                        extensions[i] = extensions[i].Insert(0, FileDefine.DOT_SPLIT);
                    }
                }
            }
            for (int i = 0; i < extensions.Length; ++i)
            {
                var extensionTmp = extensions[i];
                if (!ret.Contains(extensionTmp))
                {
                    //                  ret.Insert(findIndex, extensionTmp);
                    ret += extensionTmp;
                }
            }

            return ret;
        }

        //RemoveSubStringByFind("1/??2$/3", "??", "/") -> "1/3"
        static public string RemoveSubStringByFind(string filename, string find, string flag)
        {
            bool isChangedFiledName = false;
            do
            {
                int indexFind = filename.IndexOf(find);
                if (indexFind < 0)
                    break;
                else
                {
                    int indexSeekStart = indexFind;
                    int indexSeekEnd = indexFind;

                    bool isFindFlagStart = false;
                    bool isFindFlagEnd = false;
                    isChangedFiledName = true;

                    while (--indexSeekStart > 0)
                    {
                        isFindFlagStart = true;
                        for (int i = flag.Length - 1; i >= 0; --i)
                        {
                            if (flag[i] != filename[indexSeekStart - i])
                            {
                                isFindFlagStart = false;
                                break;
                            }
                        }
                        if (isFindFlagStart)
                        {
                            break;
                        }
                    }
                    while (++indexSeekEnd < filename.Length)
                    {
                        isFindFlagEnd = true;
                        for (int i = 0; i < flag.Length; ++i)
                        {
                            if (flag[i] != filename[indexSeekEnd + i])
                            {
                                isFindFlagEnd = false;
                                break;
                            }
                        }
                        if (isFindFlagEnd)
                        {
                            break;
                        }
                    }

                    if (!isFindFlagStart || !isFindFlagEnd)
                    {
                        break;
                    }
                    else
                    {
                        filename = filename.Remove(indexSeekStart, indexSeekEnd - indexSeekStart);
                    }
                }

            } while (true);

            if (isChangedFiledName)
            {
                var filenameTmp = GetLastFileName(filename, flag, true);
                if (filenameTmp.Contains(find))
                {
                    filename = filename.Remove(filename.Length - filenameTmp.Length, filenameTmp.Length);
                }

                if (!string.IsNullOrEmpty(filename) && filename[filename.Length - 1].ToString() == flag)
                {
                    filename = filename.Remove(filename.Length - 1);
                }
            }

            return filename;
        }
        static public string RemoveSubStringByFind(string filename, string find)
        {
            return RemoveSubStringByFind(filename, find, FileDefine.PATH_FLAG_SPLIT);
        }

        static public int GetPathLevel(string path, string flag)
        {
            int ret = 0;
            int startIndex = 0;
            do
            {
                int indexFind = path.IndexOf(flag, startIndex);
                if (indexFind < 0)
                    break;
                else
                {
                    startIndex = indexFind + flag.Length;
                    ++ret;
                }

            } while (true);

            return ret;
        }
        static public int GetPathLevel(string path)
        {
            return GetPathLevel(path, FileDefine.PATH_FLAG_SPLIT);
        }

        static public string RemoveLastPathByLevel(string path, string flag, int pathLevel)
        {
            string ret = path;
            for (int i = 0; i < pathLevel; ++i)
            {
                int indexFind = ret.LastIndexOf(flag);
                if (indexFind == -1)
                {
                    return ret;
                }

                ret = ret.Remove(indexFind, ret.Length - indexFind);
            }

            return ret;
        }
        static public string RemoveLastPathByLevel(string path, int pathLevel)
        {
            return RemoveLastPathByLevel(path, FileDefine.PATH_FLAG_SPLIT, pathLevel);
        }

        static public string GetLastFileNameLevel(string str, string flag, bool isKeepExtension, int pathLevel)
        {
            string ret = string.Empty;
            var strTmp = str;

            for (int i = 0; i < pathLevel; ++i)
            {
                int indexFind = strTmp.LastIndexOf(flag);
                if (indexFind == -1)
                {
                    ret = strTmp + flag + ret;
                    break;
                }

                var strLast = GetLastFileName(strTmp, flag, i == 0 ? isKeepExtension : false);

                if (i != 0)
                {
                    strLast += flag;
                }
                strLast += ret;
                ret = strLast;

                strTmp = strTmp.Substring(0, indexFind);
            }

            return ret;
        }
        static public string GetLastFileNameLevel(string str, bool isKeepExtension, int pathLevel)
        {
            return GetLastFileNameLevel(str, FileDefine.PATH_FLAG_SPLIT, isKeepExtension, pathLevel);
        }

        static public string GetFolderNameByPath(string str, string flag)
        {
            if (FileHelper.ExistsDirectory(str))
            {
                return str;
            }

            //no folder, just a file name
            if (!str.Contains(flag))
            {
                return string.Empty;
            }

            string ret = str;
            int indexFind = ret.LastIndexOf(flag);
            if (indexFind != -1)
            {
                ret = ret.Substring(0, indexFind);
            }

            if (!string.IsNullOrEmpty(ret) && ret[ret.Length - 1].ToString() != flag)
            {
                ret += flag;
            }

            return ret;
        }
        static public string GetFolderNameByPath(string str)
        {
            return GetFolderNameByPath(str, FileDefine.PATH_FLAG_SPLIT);
        }

        static public string GetFilNameExtension(string str)
        {
            int findIndex = str.LastIndexOf('.');
            if (findIndex == -1)
            {
                Log.Warning("GetExtensions error: missing flag '.', it is a file name ?");
                return string.Empty;
            }
            return str.Substring(findIndex + 1, str.Length - findIndex - 1);
        }

        static public bool HasFileNameExtension(string str)
        {
            return str.Contains(FileDefine.DOT_SPLIT);
        }

        static public string AddFolderNameByPath(string str, string newFolder, string flag)
        {
            if (null == str)
                return str;

            var fileName = string.Empty;
            string folderName = string.Empty;

            if (!ExistsDirectory(str) && str.LastIndexOf(FileDefine.DOT_SPLIT) >= 0)
            {
                fileName = GetLastFileName(str, flag, true);
                folderName = GetFolderNameByPath(str, flag);
            }
            else
            {
                fileName = string.Empty;
                folderName = str;
            }

            return ContactPath(ContactPath(folderName, newFolder), fileName);
        }
        static public string AddFolderNameByPath(string str, string newFolder)
        {
            return AddFolderNameByPath(str, newFolder, FileDefine.PATH_FLAG_SPLIT);
        }

        static public string ContactPath(string path1, string path2, string flag)
        {
            if (string.IsNullOrEmpty(path2))
                return path1;
            if (string.IsNullOrEmpty(path1))
                return path2;

            string ret = string.Empty;
            bool b1 = !string.IsNullOrEmpty(path1) && path1[path1.Length - 1].ToString() == flag;
            bool b2 = !string.IsNullOrEmpty(path2) && path2[0].ToString() == flag;
            if (b1 && b2)
            {
                path1 = path1.Remove(path1.Length - 1);
            }
            else if (!b1 && !b2)
            {
                path1 += flag;
            }
            ret = path1 + path2;
            return ret;
        }
        static public string ContactPath(string path1, string path2)
        {
            return ContactPath(path1, path2, FileDefine.PATH_FLAG_SPLIT);
        }

        static public bool DeleteByUserPath(string path)
        {
            try
            {
                if (ExistsFile(path))
                {
                    File.Delete(path);
                    Log.Info("FileHelper delete file path=" + path);
                }
                else if (ExistsDirectory(path))
                {
                    Directory.Delete(path, true);

                    Log.Info("FileHelper delete directory path=" + path);
                }
                else
                {
                    Log.Warning("FileHelper DeleteByUserPath warning: not find path=" + path);
                }
            }
            catch (System.Exception e)
            {
                ExceptionHandlingDelete(path, e);
                return false;
            }
            return true;
        }

        static public bool DeleteByPersistent(string fileName)
        {
            string pathTmp = GetFullpath(fileName);
            return DeleteByUserPath(pathTmp);
        }

        /// <summary>
        /// 递归删除空文件
        /// <param name="path">递归开始的路径，从该文件夹开始往上层依次删除空文件夹</param>
        /// <param name="ignorePatterns">判断文件数量的时候需要过滤的文件后缀名，如果文件数量为0则会删除该文件夹</param>
        /// </summary>
        static public bool DeleteEmptyFolder(string path, params string[] ignorePatterns)
        {
            var retValue = true;

            //如果文件夹内容空，则删除它
            var filesTmp = new System.Collections.Generic.List<string>();
            var folderName = string.Empty;

            //本身是文件夹
            if (FileHelper.ExistsDirectory(path))
            {
                folderName = path;
            }
            //可能是文件路径
            else
            {
                folderName = FileHelper.GetFolderNameByPath(path);
            }

            //获取文件夹下所有文件
            FileHelper.GetSeekPathBase(folderName, ref filesTmp, (string value) =>
            {
                for (int i = 0; i < ignorePatterns.Length; ++i)
                {
                    if (value.Contains(ignorePatterns[i]))
                    {
                        return false;
                    }
                }
                return true;
            });

            //文件内容为空
            if (filesTmp.Count == 0)
            {
                //删除文件
                retValue &= FileHelper.DeleteByUserPath(folderName);

                //获取上级目录
                if (!string.IsNullOrEmpty(folderName) && folderName[folderName.Length - 1].ToString() == FileDefine.PATH_FLAG_SPLIT)
                    folderName = folderName.Remove(folderName.Length - 1);
                folderName = FileHelper.RemoveLastPathByLevel(folderName, 1);

                DeleteEmptyFolder(folderName, ignorePatterns);
            }

            return retValue;
        }

        static public bool ExistsFile(string path)
        {
            return File.Exists(path);
        }

        static public bool ExistsDirectory(string path)
        {
            return Directory.Exists(path);
        }

        static public string GetExtension(FileDefine.FileExtension extension)
        {
            string ret = string.Empty;
            switch (extension)
            {
                case FileDefine.FileExtension.None: break;
                case FileDefine.FileExtension.AssetBundle: ret = "assetbundle"; break;
                case FileDefine.FileExtension.Prefab: ret = "prefab"; break;
                case FileDefine.FileExtension.Png: ret = "png"; break;
                case FileDefine.FileExtension.Jpg: ret = "jpg"; break;
                case FileDefine.FileExtension.Txt: ret = "txt"; break;
                case FileDefine.FileExtension.Json: ret = "json"; break;
                case FileDefine.FileExtension.Xml: ret = "xml"; break;
                case FileDefine.FileExtension.Lua: ret = "lua"; break;
                case FileDefine.FileExtension.Bytes: ret = "bytes"; break;
                default: Log.Error("unsupport extension type !"); break;
            }

            return ret;
        }

        /// <summary>
        /// 删除文件夹目录中，所有包含标记文件名字的文件
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="tag">名字标记</param>
        /// </summary>
        static public void DeleteFileByTag(string folderPath, params string[] tag)
        {
            if (tag == null || tag.Length == 0)
            {
                Log.Error("FileHelper DeleteFileByTag error: tag is empty");
                return;
            }

            var listPath = new System.Collections.Generic.List<string>();
            FileHelper.GetSeekPathBase(folderPath, ref listPath, (string path) =>
            {
                bool ret = false;
                if (string.IsNullOrEmpty(path))
                    return ret;
                for (int i = tag.Length - 1; i >= 0; --i)
                {
                    if (path.Contains(tag[i]))
                    {
                        ret = true;
                        break;
                    }
                }
                return ret;
            });

            for (int i = 0; i < listPath.Count; ++i)
            {
                FileHelper.DeleteByUserPath(listPath[i]);
            }
        }

        /// <summary>
        /// 添加文件名字后缀标记，例如Folder/1.png 添加标记_test后-> Folder/1_test.png
        /// <param name=""> </param>
        /// <return></return>
        /// </summary>
        static public string AddFileNameTag(string path, string stringTag)
        {
            var ret = path;
            int findIndex = ret.LastIndexOf(FileDefine.DOT_SPLIT);
            if (findIndex == -1)
            {
                Log.Error("HotUpdate AddFileTag error: missing extension");
                return ret;
            }

            if (!ret.Contains(stringTag))
                ret = ret.Insert(findIndex, stringTag);

            return ret;
        }

        static public string GetFileSizeFormatString(long size)
        {
            string ret = string.Empty;

            if (size < FileDefine.ONE_KB)
                ret = size + " B";
            else if (size >= FileDefine.ONE_KB && size < FileDefine.ONE_MB)
                ret = (long)(size / FileDefine.ONE_KB) + " KB";
            else if (size >= FileDefine.ONE_MB && size < FileDefine.ONE_GB)
                ret = ((double)size / (double)FileDefine.ONE_MB).Round(2) + " MB";
            else if (size >= FileDefine.ONE_GB)
                ret = ((double)size / (double)FileDefine.ONE_GB).Round(2) + " GB";
            else
                ret = size.ToString();
            return ret;
        }

        static public int FindLastStatckLevelWhereCallLocated(string findTag)
        {
            int retValue = -1;
            var stackTmp = new System.Diagnostics.StackTrace(true);
            var statckFramesTmp = stackTmp.GetFrames();

            for (int i = 0; i < statckFramesTmp.Length; ++i)
            {
                var statckInfoTmp = statckFramesTmp[i].GetFileName();
                if (!string.IsNullOrEmpty(statckInfoTmp))
                {
                    statckInfoTmp = statckInfoTmp.Replace("\\", FileDefine.PATH_FLAG_SPLIT);

                    if (string.IsNullOrEmpty(findTag))
                    {
                        retValue = i - 1;
                        break;
                    }
                    else if (statckInfoTmp.Contains(findTag))
                    {
                        retValue = i - 1;
                    }
                }
                else
                {
                    if (retValue >= 0)
                        break;
                }
            }
            return retValue;
        }

        /// <summary>
        /// Gets the file path where the code that calls the method is located
        /// </summary>
        static public string GetPathWhereCallLocated(int statckIndex = 0)
        {
            return GetStackFrameWhereCallLocated(statckIndex).GetFileName();
        }

        /// <summary>
        /// Gets the file column number where the code that calls the method is located
        /// </summary>
        static public int GetFileColumnNumberWhereCallLocated(int statckIndex = 0)
        {
            return GetStackFrameWhereCallLocated(statckIndex).GetFileColumnNumber();
        }

        /// <summary>
        /// Gets the file line number where the code that calls the method is located
        /// </summary>
        static public int GetFileLineNumberWhereCallLocated(int statckIndex = 0)
        {
            return GetStackFrameWhereCallLocated(statckIndex).GetFileLineNumber();
        }

        static public string GetCurrentSourceFolderPath()
        {
            return FileHelper.GetFolderNameByPath(GetCurrentSourceFilePath(2));
        }

        static public string GetCurrentSourceFilePath(int skipFrames = 1)
        {
            var stackTmp = new System.Diagnostics.StackTrace(skipFrames, true);
            if (null == stackTmp)
                return "FileHelper GetCurrentSourcePath error: no statck";

            var frameTmp = stackTmp.GetFrame(0);
            return null == frameTmp || string.IsNullOrEmpty(frameTmp.GetFileName()) ? "FileHelper GetCurrentSourcePath error: no frame" : frameTmp.GetFileName().Replace("\\", FileDefine.PATH_FLAG_SPLIT);
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="absolutePath">绝对路径</param>
        /// <return>文件大小</return>
        static public long GetFileSize(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                Log.Error("FileHelper GetFileSize error: missing path=" + absolutePath);
                return 0;
            }

            var fileInfo = new FileInfo(absolutePath);
            return fileInfo.Length;
        }

        /// <summary>
        /// 获取文件夹大小
        /// <param name="absolutePath">绝对路径</param>
        /// <param name="ignorePatterns">需要被过滤的文件后缀名</param>
        /// <return>文件夹大小</return>
        /// </summary>
        static public long GetDirectorySize(string absolutePath, params string[] ignorePatterns)
        {
            if (!Directory.Exists(absolutePath))
            {
                Log.Error("FileHelper GetDirectorySize error: missing path=" + absolutePath);
                return 0;
            }

            long retValue = 0;
            var allFiles = new System.Collections.Generic.List<string>();
            FileHelper.GetSeekPath(absolutePath, ref allFiles, true, ignorePatterns);

            for (int i = allFiles.Count - 1; i >= 0; --i)
            {
                retValue += GetFileSize(allFiles[i]);
            }
            return retValue;
        }

        /// <summary>
        /// 判断文件是否为文本文件
        /// </summary>
        static public bool IsTextFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            bool isTextFile = true;
            try
            {
                int i = 0;
                int length = (int)fs.Length;
                byte data;
                while (i < length && isTextFile)
                {
                    data = (byte)fs.ReadByte();
                    isTextFile = (data != 0);
                    ++i;
                }
                return isTextFile;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        static private System.Diagnostics.StackFrame GetStackFrameWhereCallLocated(int statckIndex)
        {
            System.Diagnostics.StackFrame retFrame = new System.Diagnostics.StackFrame();
            var stackTmp = new System.Diagnostics.StackTrace(true);
            var statckFramesTmp = stackTmp.GetFrames();

            //skip FileHelper GetStackFrameWhereCallLocated statck level
            statckIndex += 2;

            // if (quicklyLocated)
            // {
            if (statckFramesTmp.Length > 0)
            {
                if (statckIndex < 0)
                {
                    retFrame = statckFramesTmp[statckFramesTmp.Length - 1];
                }
                else if (statckIndex < statckFramesTmp.Length)
                    retFrame = statckFramesTmp[statckIndex];

                if (retFrame == null)
                {
                    retFrame = statckFramesTmp[statckFramesTmp.Length - 1];
                }
            }
            return retFrame;
        }

        static private bool CopyFileByUserPathBase(string pathOld, string pathNew, params string[] ignorePatterns)
        {
            try
            {
                CheckFolderPathWithAutoCreate(pathNew);

                if (FileHelper.ExistsFile(pathOld))
                    File.Copy(pathOld, pathNew, true);
                else if (FileHelper.ExistsDirectory(pathOld))
                {
                    var listSeekPath = new System.Collections.Generic.List<string>();
                    FileHelper.GetSeekPath(pathOld, ref listSeekPath, true, ignorePatterns);
                    for (int i = 0; i < listSeekPath.Count; ++i)
                    {
                        var filename = listSeekPath[i].Remove(pathOld);
                        CopyFileByUserPathBase(listSeekPath[i], FileHelper.ContactPath(pathNew, filename));
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error("FileHelper CopyFileByUserPathBase error: msg=" + e.ToString() + "\npathOld=" + pathOld + " pathNew=" + pathNew);
                return false;
            }
            return true;
        }

        static private void ExceptionHandlingWrite(string path, System.Exception exception)
        {
            Log.Error("FileHelper write exception: path=" + path + "\nerror message=" + exception);
            FileHelper.DeleteByUserPath(path);
        }

        static private void ExceptionHandlingRead(string path, System.Exception exception)
        {
            Log.Error("FileHelper read exception: path=" + path + "\nerror message=" + exception);
        }

        static private void ExceptionHandlingDelete(string path, System.Exception exception)
        {
            Log.Error("FileHelper delete exception: path=" + path + "\nerror message=" + exception);
        }
    }
}
