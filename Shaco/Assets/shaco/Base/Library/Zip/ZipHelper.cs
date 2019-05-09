using shaco.ICSharpCode.SharpZipLib;
using shaco.ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System;

namespace shaco.Base
{
    public static class ZipHelper
    {
        //解压过滤的文件或者文件夹，模糊匹配
        static public string[] ignoreFiles = new string[] { };
        static private readonly string[] defaultIgnoreFiles = new string[] { "__MACOSX" };

        #region 压缩

        #region 公开压缩方法
        /// <summary>   
        /// 压缩文件或文件夹   ----带密码
        /// </summary>   
        /// <param name="fileToZip">要压缩的路径-文件夹或者文件</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码</param>
        /// <returns>如果失败返回失败信息，成功返回空字符串</returns>   
        public static string Zip(string fileToZip, string zipedFile, string password)
        {
            shaco.ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = System.Text.Encoding.GetEncoding("UTF-8").CodePage;
            var result = string.Empty;
            try
            {
                if (Directory.Exists(fileToZip))
                    ZipDirectory(fileToZip, zipedFile, password);
                else if (File.Exists(fileToZip))
                    ZipFile(fileToZip, zipedFile, password);
            }
            catch (Exception ex)
            {
                Log.Error("ZipHelper Zip erorr: " + ex);
                result = ex.Message;
            }
            return result;
        }

        /// <summary>   
        /// 压缩文件或文件夹 ----无密码 
        /// </summary>   
        /// <param name="fileToZip">要压缩的路径-文件夹或者文件</param>   
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <returns>如果失败返回失败信息，成功返回空字符串</returns>   
        public static string Zip(string fileToZip, string zipedFile)
        {
            shaco.ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = System.Text.Encoding.GetEncoding("UTF-8").CodePage;
            var result = string.Empty;
            try
            {
                if (Directory.Exists(fileToZip))
                    ZipDirectory(fileToZip, zipedFile, null);
                else if (File.Exists(fileToZip))
                    ZipFile(fileToZip, zipedFile, null);
                else 
                {
                    Log.Error("ZipHelper Zip error: invalid path=" + fileToZip);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        #endregion

        #region 内部处理方法
        /// <summary>   
        /// 压缩文件   
        /// </summary>   
        /// <param name="fileToZip">要压缩的文件全名</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码</param>   
        /// <returns>压缩结果</returns>   
        private static bool ZipFile(string fileToZip, string zipedFile, string password)
        {
            shaco.ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = System.Text.Encoding.GetEncoding("UTF-8").CodePage; 
            bool result = true;
            ZipOutputStream zipStream = null;
            FileStream fs = null;
            ZipEntry ent = null;

            if (!File.Exists(fileToZip))
                return false;

            try
            {
                fs = File.OpenRead(fileToZip);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                fs = File.Create(zipedFile);
                zipStream = new ZipOutputStream(fs);
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                ent = new ZipEntry(Path.GetFileName(fileToZip));
                zipStream.PutNextEntry(ent);
                zipStream.SetLevel(6);

                zipStream.Write(buffer, 0, buffer.Length);

            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Finish();
                    zipStream.Close();
                }
                if (ent != null)
                {
                    ent = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            GC.Collect();
            GC.Collect(1);

            return result;
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="strFile">带压缩的文件夹目录</param>
        /// <param name="strZip">压缩后的文件名</param>
        /// <param name="password">压缩密码</param>
        /// <returns>是否压缩成功</returns>
        private static bool ZipDirectory(string strFile, string strZip, string password)
        {
            bool result = false;
            if (!Directory.Exists(strFile)) return false;
            if (strFile[strFile.Length - 1] != Path.DirectorySeparatorChar)
                strFile += Path.DirectorySeparatorChar;
            ZipOutputStream s = new ZipOutputStream(File.Create(strZip));
            s.SetLevel(6); // 0 - store only to 9 - means best compression
            if (!string.IsNullOrEmpty(password)) s.Password = password;
            try
            {
                result = zip(strFile, s, strFile);
            }
            catch (Exception ex)
            {
                Log.Error("ZipHelper ZipDirectory error: " + ex);
                throw ex;
            }
            finally
            {
                s.Finish();
                s.Close();
            }
            return result;
        }

        /// <summary>
        /// 压缩文件夹内部方法
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="s"></param>
        /// <param name="staticFile"></param>
        /// <returns></returns>
        private static bool zip(string strFile, ZipOutputStream s, string staticFile)
        {
            bool result = true;
            if (strFile[strFile.Length - 1] != Path.DirectorySeparatorChar) strFile += Path.DirectorySeparatorChar;
            try
            {
                string[] filenames = Directory.GetFileSystemEntries(strFile);
                foreach (string file in filenames)
                {
                    if (Directory.Exists(file))
                    {
                        zip(file, s, staticFile);
                    }
                    else // 否则直接压缩文件
                    {
                        //打开压缩文件
                        FileStream fs = File.OpenRead(file);

                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        string tempfile = file.Substring(staticFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                        ZipEntry entry = new ZipEntry(tempfile);

                        entry.DateTime = DateTime.Now;
                        entry.Size = fs.Length;
                        fs.Close();
                        entry.Crc = shaco.ICSharpCode.SharpZipLib.Checksums.Crc32.GetCRC32(buffer);
                        s.PutNextEntry(entry);

                        s.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }
            return result;

        }
        #endregion

        #endregion


        #region 解压  

        #region 公开解压方法
        /// <summary>   
        /// 解压功能(解压压缩文件到指定目录)---->不需要密码
        /// </summary>   
        /// <param name="fileToUnZip">待解压的文件</param>   
        /// <param name="zipedFolder">指定解压目标目录</param>  
        /// <param name="callbackProgress">解压进度回调，回调为空的时候是同步解压，反之为异步解压</param>
        /// <returns>如果失败返回失败信息，成功返回空字符串</returns>   
        public static string UnZip(string fileToUnZip, string zipedFolder, System.Action<float> callbackProgress = null)
        {
            string result = string.Empty;
            try
            {
                UnZipByPassword(fileToUnZip, zipedFolder, null, callbackProgress);
            }
            catch (Exception ex)
            {
                Log.Error("ZipHelper Zip erorr: " + ex);
                result = ex.Message;
            }

            //重置文件过滤设置
            ignoreFiles = new string[0];
            return result;
        }

        /// <summary>   
        /// 解压功能(解压压缩文件到指定目录)---->需要密码
        /// </summary>   
        /// <param name="fileToUnZip">待解压的文件</param>   
        /// <param name="zipedFolder">指定解压目标目录</param>
        /// <param name="password">密码</param> 
        /// <returns>如果失败返回失败信息，成功返回空字符串</returns>   
        public static string UnZip(string fileToUnZip, string zipedFolder, string password, System.Action<float> callbackProgress = null)
        {
            var result = string.Empty;
            try
            {
                UnZipByPassword(fileToUnZip, zipedFolder, password, callbackProgress);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }
        #endregion

        #region 内部处理方法
        /// <summary>
        /// 解压功能 内部处理方法
        /// </summary>
        /// <param name="TargetFile">待解压的文件</param>
        /// <param name="fileDir">指定解压目标目录</param>
        /// <param name="password">密码</param>
        /// <param name="callbackProgress">解压进度回调，回调为空的时候是同步解压，反之为异步解压</param>
        /// <returns>成功返回true</returns>
        private static bool UnZipByPassword(string TargetFile, string fileDir, string password, System.Action<float> callbackProgress)
        {
            shaco.ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = System.Text.Encoding.GetEncoding("UTF-8").CodePage; 
            bool rootFile = true;
            try
            {
                //读取压缩文件(zip文件)，准备解压缩
                ZipInputStream zipStream = new ZipInputStream(File.OpenRead(TargetFile.Trim()));
                ZipEntry theEntry = null;
                long currentPosition = zipStream.Position;
                long currentFileSize = zipStream.FileSize;
                float currentProgress = 0;

                // string rootDir = " ";
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

                if (callbackProgress == null)
                {
                    while (null != (theEntry = zipStream.GetNextEntry()))
                    {
                        UnZipOneFile(zipStream, theEntry, fileDir, ref currentPosition, ref currentProgress, callbackProgress);
                    }
                }
                else 
                {
                    shaco.Base.Coroutine.WhileAsync((() =>
                    {
                        theEntry = zipStream.GetNextEntry();
                        if (theEntry != null)
                        {
                            return true;
                        }
                        else
                        {
                            if (theEntry != null)
                            {
                                theEntry = null;
                            }
                            if (zipStream != null)
                            {
                                zipStream.Close();
                            }
                            currentProgress = 1;
                            return false;
                        }
                    }), () =>
                    {
                        UnZipOneFile(zipStream, theEntry, fileDir, ref currentPosition, ref currentProgress, callbackProgress);
                    });

                    //将子线程中的进度回调方法合并到主线程安全使用
                    shaco.Base.WaitFor.Run(()=>
                    {
                        if (currentProgress < 1.0f)
                        {
                            callbackProgress(currentProgress);
                        }
                        return currentProgress >= 1.0f;
                    }, ()=>
                    {
                        callbackProgress(1);
                    });
                }
            }
            catch (Exception ex)
            {
                rootFile = false;
                throw ex;
            }
            finally
            {
                GC.Collect();
                GC.Collect(1);
            }
            return rootFile;
        }

        static private bool IsIgnoreFile(string[] ignoreFilesTmp, string filename)
        {
            for (int i = 0; i < ignoreFilesTmp.Length; ++i)
            {
                if (filename.Contains(ignoreFilesTmp[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static private void UnZipOneFile(ZipInputStream zipStream, ZipEntry theEntry, string fileDir, ref long currentPosition, ref float currentProgress, System.Action<float> callbackProgress)
        {
            string path = fileDir;

            var rootDir = Path.GetDirectoryName(theEntry.Name);
            if (rootDir.IndexOf(Path.DirectorySeparatorChar) >= 0)
            {
                rootDir = rootDir.Substring(0, rootDir.IndexOf(Path.DirectorySeparatorChar) + 1);
            }
            string dir = Path.GetDirectoryName(theEntry.Name);

            if (IsIgnoreFile(ignoreFiles, theEntry.Name) || IsIgnoreFile(defaultIgnoreFiles, theEntry.Name))
            {
                return;
            }

            string fileName = Path.GetFileName(theEntry.Name);
            if (dir != " ")
            {
                path = fileDir + Path.DirectorySeparatorChar + dir;
                if (!Directory.Exists(fileDir + Path.DirectorySeparatorChar + dir))
                {
                    Directory.CreateDirectory(path);
                }
            }
            else if (dir == " " && fileName != "")
            {
                path = fileDir;
            }
            else if (dir != " " && fileName != "")
            {
                if (dir.IndexOf(Path.DirectorySeparatorChar) > 0)
                {
                    path = fileDir + Path.DirectorySeparatorChar + dir;
                }
            }

            if (dir == rootDir)
            {
                path = fileDir + Path.DirectorySeparatorChar + rootDir;
            }

            //以下为解压缩zip文件的基本步骤
            //基本思路就是遍历压缩文件里的所有文件，创建一个相同的文件。
            if (fileName != String.Empty)
            {
                FileStream streamWriter = File.Create(path + Path.DirectorySeparatorChar + fileName);

                int size = 2048;
                byte[] data = new byte[2048];
                while (true)
                {
                    try 
                    {
                        size = zipStream.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Log.Error(e.ToString());
                        throw e;
                    }
                }

                streamWriter.Close();
            }

            if (null != callbackProgress)
            {
                currentPosition = zipStream.Position;
                currentProgress = (float)((double)zipStream.Position / (double)zipStream.FileSize);
                if (currentProgress < 0.0001f)
                    currentProgress = 0;
            }
        }
        #endregion

        #endregion
    }
}