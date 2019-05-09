using System.Collections;
using System.IO;
using System.Text;

namespace shaco.Base
{
    public static partial class FileHelper
    {
        //每帧写入文件字节(B)，不要超过int.MaxValue(2147483647)
        private const int MAX_WRITE_COUNT_PER_LOOP = (int)FileDefine.ONE_MB;

        //每帧读取文件字节(B)，不要超过int.MaxValue(2147483647)
        private const int MAX_READ_COUNT_PER_LOOP = (int)FileDefine.ONE_MB;

        /// <summary>
        /// 写入数据到文件中，指定路径为FileDefine.persistentDataPath(该路径可读可写)
        /// <param name="path">文件路径</param>
        /// <param name="data">数据</param>
        /// <param name="callbackEnd">写入完毕回调，允许为空</param>
        /// <param name="callbackProgress">写入进度回调，允许为空</param>
        /// </summary>
        static public void WriteAllByteByPersistentAsync(string path, byte[] data, System.Action<bool> callbackEnd, System.Action<float> callbackProgress = null)
        {
            var fullpath = GetFullpath(path);
            CheckFolderPathWithAutoCreate(fullpath);
            WriteAllByteByUserPathAsync(fullpath, data, true, callbackEnd, callbackProgress);
        }

        /// <summary>
        /// 写入数据到文件中，自定义路径
        /// <param name="path">文件路径</param>
        /// <param name="data">数据</param>
        /// <param name="callbackEnd">写入完毕回调，允许为空</param>
        /// <param name="callbackProgress">写入进度回调，允许为空</param>
        /// </summary>
        static public void WriteAllByteByUserPathAsync(string path, byte[] data, System.Action<bool> callbackEnd, System.Action<float> callbackProgress = null)
        {
            WriteAllByteByUserPathAsync(path, data, true, callbackEnd, callbackProgress);
        }

        /// <summary>
        /// 写入数据到文件中，自定义路径
        /// <param name="path">文件路径</param>
        /// <param name="data">数据</param>
        /// <param name="overrite">是否覆盖原文件，如果为false不覆盖原文件，则会在原文件基础上新增数据</param>
        /// <param name="callbackEnd">写入完毕回调，允许为空，非线程安全</param>
        /// <param name="callbackProgress">写入进度回调，允许为空，线程安全</param>
        /// </summary>
        static public void WriteAllByteByUserPathAsync(string path, byte[] data, bool overrite, System.Action<bool> callbackEnd, System.Action<float> callbackProgress = null)
        {
            bool writeResult = false;
            FileStream writeStream = null;
            long writeOffset = 0;
            int writedSize = 0;

            if (data.IsNullOrEmpty())
            {
                if (null != callbackProgress)
                {
                    callbackProgress(1);
                }
                if (null != callbackEnd)
                {
                    callbackEnd(false);
                }
                return;
            }

            //检查并自动创建文件夹
            if (!FileHelper.CheckFolderPathWithAutoCreate(path))
            {
                return;
            }

            //进度回调
            float progressTmp = 0;
            if (null != callbackProgress)
            {
                WaitFor.Run(() =>
                {
                    if (progressTmp < 1.0f)
                    {
                        callbackProgress(progressTmp);
                    }
                    return progressTmp >= 1.0f;
                },
                () =>
                {
                    callbackProgress(1.0f);
                });
            }

            ThreadPool.RunThreadSafeCallBack(() =>
            {
                bool hasErorr = false;

                //为了覆盖原文件，如果文件已存在，则删除文件
                if (overrite && File.Exists(path))
                {
                    File.Delete(path);
                }

                //准备打开文件写入流
                try
                {
                    writeStream = File.OpenWrite(path);
                    writeOffset = overrite ? 0 : writeStream.Length;
                }
                catch (System.Exception e)
                {
                    Log.Error("FileHelper WriteAllByUserPathAsync error: can't open write, \ne=" + e);
                    hasErorr = true;
                }

                //开始循环写入文件
                while (!hasErorr && writedSize != data.Length)
                {
                    System.Threading.Thread.Sleep(10);

                    if (writedSize > data.Length)
                    {
                        Log.Error("FileHelper WriteAllByUserPathAsync error: data error length writedSize=" + writedSize + " data.Length=" + data.Length);
                        hasErorr = true;
                    }
                    else
                    {
                        try
                        {
                            int currentWriteCount = MAX_WRITE_COUNT_PER_LOOP;
                            int writeLength = (int)writeStream.Length;
                            if (currentWriteCount > data.Length - writedSize)
                            {
                                currentWriteCount = data.Length - writedSize;
                            }

                            writeStream.Position = writeOffset;
                            writeStream.Write(data, writedSize, currentWriteCount);
                            writeOffset += currentWriteCount;
                            writedSize += currentWriteCount;

                            //写入文件进度回调
                            if (null != callbackProgress)
                            {
                                progressTmp = (float)((double)writedSize / (double)data.Length);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("FileHelper WriteAllByUserPathAsync error: write error, \ne=" + e);
                            hasErorr = true;
                        }
                    }
                }

                writeStream.Close();
                writeStream.Dispose();
                writeResult = !hasErorr;

            }, () =>
            {
                if (null != callbackEnd) callbackEnd(writeResult);
            });
        }

        /// <summary>
        /// 添加数据到文件中，指定路径为FileDefine.persistentDataPath(该路径可读可写)
        /// <param name="path">文件路径</param>
        /// <param name="data">数据</param>
        /// <param name="callbackEnd">写入完毕回调，允许为空</param>
        /// <param name="callbackProgress">写入进度回调，允许为空</param>
        /// </summary>
        static public void AppendByPersistentAsync(string path, string data, System.Action<bool> callbackEnd, System.Action<float> callbackProgress = null)
        {
            var fullpath = GetFullpath(path);
            CheckFolderPathWithAutoCreate(fullpath);
            WriteAllByteByUserPathAsync(fullpath, data.ToByteArray(), false, callbackEnd, callbackProgress);
        }

        /// <summary>
        /// 添加数据到文件中，自定义路径
        /// <param name="path">文件路径</param>
        /// <param name="data">数据</param>
        /// <param name="callbackEnd">写入完毕回调，允许为空</param>
        /// <param name="callbackProgress">写入进度回调，允许为空</param>
        /// </summary>
        static public void AppendByUserPathAsync(string path, string data, System.Action<bool> callbackEnd, System.Action<float> callbackProgress = null)
        {
            WriteAllByteByUserPathAsync(path, data.ToByteArray(), false, callbackEnd, callbackProgress);
        }

        /// <summary>
        /// 写入数据到文件中，指定路径为FileDefine.persistentDataPath(该路径可读可写)
        /// <param name="path">文件路径</param>
        /// <param name="data">数据</param>
        /// <param name="callbackEnd">写入完毕回调，允许为空</param>
        /// <param name="callbackProgress">写入进度回调，允许为空</param>
        /// </summary>
        static public void WriteAllByPersistentAsync(string path, string data, System.Action<bool> callbackEnd, System.Action<float> callbackProgress = null)
        {
            WriteAllByteByUserPathAsync(path, data.ToByteArray(), true, callbackEnd, callbackProgress);
        }

        /// <summary>
        /// 写入数据到文件中，自定义路径
        /// <param name="path">文件路径</param>
        /// <param name="data">数据</param>
        /// <param name="callbackEnd">写入完毕回调，允许为空</param>
        /// <param name="callbackProgress">写入进度回调，允许为空</param>
        /// </summary>
        static public void WriteAllByUserPathAsync(string path, string data, System.Action<bool> callbackEnd, System.Action<float> callbackProgress = null)
        {
            WriteAllByteByUserPathAsync(path, data.ToByteArray(), true, callbackEnd, callbackProgress);
        }

        /// <summary>
        /// 拷贝文件或目录到自定义路径中
        /// <param name="pathOld">被拷贝的旧路径</param>
        /// <param name="pathNew">拷贝到的新路径</param>
        /// <param name="callbackEnd">拷贝完毕回调，允许为空</param>
        /// <param name="ignorePatterns">拷贝文件或者目录时候忽略(不拷贝)的文件后缀名</param>
        /// </summary>
        static public void CopyFileByUserPathAsync(string pathOld, string pathNew, System.Action<bool> callbackEnd, params string[] ignorePatterns)
        {
            bool isSuccess = false;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                isSuccess = CopyFileByUserPathBase(pathOld, pathNew, ignorePatterns);
            }, () =>
            {
                callbackEnd(isSuccess);
            });
        }

        /// <summary>
        /// 移动文件或目录到自定义路径中，也可以用于修改文件或目录名字
        /// <param name="pathSource">被移动的原始路径</param>
        /// <param name="pathDest">移动到的目标路径</param>
        /// <param name="callbackEnd">拷贝完毕回调，允许为空</param>
        /// </summary>
        static public void MoveFileByUserPathAsync(string pathSource, string pathDest, System.Action<bool> callbackEnd)
        {
            bool isSuccess = false;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                isSuccess = MoveFileByUserPath(pathSource, pathDest);
            }, () =>
            {
                callbackEnd(isSuccess);
            });
        }

        /// <summary>
        /// 异步从文件中读取数据，指定路径为FileDefine.persistentDataPath(该路径可读可写)
        /// <param name="path">数据路径</param>
        /// <param name="callbackEnd">数据读取完毕的回调</param>
        /// <param name="callbackProgress">数据读取的百分比回调</param>
        /// <param name="defaultValue">数据读取失败的默认返回参数</param>
        /// </summary>
        static public void ReadAllByteByPersistentAsync(string path, System.Action<byte[]> callbackEnd, System.Action<float> callbackProgress = null, byte[] defaultValue = null)
        {
            var fullpath = GetFullpath(path);
            ReadAllByteByUserPathAsync(fullpath, 0, 0, callbackEnd, callbackProgress, defaultValue);
        }

        /// <summary>
        /// 异步从文件中读取数据，指定路径为FileDefine.persistentDataPath(该路径可读可写)
        /// <param name="path">数据路径</param>
        /// <param name="callbackEnd">数据读取完毕的回调</param>
        /// <param name="callbackProgress">数据读取的百分比回调</param>
        /// <param name="defaultValue">数据读取失败的默认返回参数</param>
        /// </summary>
        static public void ReadAllByPersistentAsync(string path, System.Action<string> callbackEnd, System.Action<float> callbackProgress = null, string defaultValue = "")
        {
            var fullpath = GetFullpath(path);
            ReadAllByUserPathAsync(fullpath, callbackEnd, callbackProgress, defaultValue);
        }

        /// <summary>
        /// 异步从文件中读取数据，自定义路径
        /// <param name="path">数据路径</param>
        /// <param name="callbackEnd">数据读取完毕的回调</param>
        /// <param name="callbackProgress">数据读取的百分比回调</param>
        /// <param name="defaultValue">数据读取失败的默认返回参数</param>
        /// </summary>
        static public void ReadAllByUserPathAsync(string path, System.Action<string> callbackEnd, System.Action<float> callbackProgress = null, string defaultValue = "")
        {
            ReadAllByteByUserPathAsync(path, 0, 0, (byte[] bytes) =>
            {
                if (null != callbackEnd) callbackEnd(bytes.ToStringArray());
            }, callbackProgress, defaultValue.ToByteArray());
        }

        /// <summary>
        /// 异步从文件中读取数据，自定义路径
        /// <param name="path">数据路径</param>
        /// <param name="callbackEnd">数据读取完毕的回调</param>
        /// <param name="callbackProgress">数据读取的百分比回调</param>
        /// <param name="defaultValue">数据读取失败的默认返回参数</param>
        /// </summary>
        static public void ReadAllByteByUserPathAsync(string path, System.Action<byte[]> callbackEnd, System.Action<float> callbackProgress = null, byte[] defaultValue = null)
        {
            ReadAllByteByUserPathAsync(path, 0, 0, callbackEnd, callbackProgress, defaultValue);
        }

        /// <summary>
        /// 异步从文件中读取数据，自定义路径，当一次性读取文件内容大于1MB的时候会自动调用一次System.GC.Collect()来回收使用内存
        /// <param name="path">数据路径</param>
        /// <param name="readOffset">读取数据的起始偏移下标，不能超过文件本身大小和readSize大小</param>
        /// <param name="readSize">读取数据的字节大小，不能超过文件本身大小，当readSize为0的时候，默认读取文件本身大小的数据</param>
        /// <param name="callbackEnd">数据读取完毕的回调，非线程安全</param>
        /// <param name="callbackProgress">数据读取的百分比回调，线程安全</param>
        /// <param name="defaultValue">数据读取失败的默认返回参数</param>
        /// </summary>
        static public void ReadAllByteByUserPathAsync(string path, long readOffset, int readSize, System.Action<byte[]> callbackEnd, System.Action<float> callbackProgress = null, byte[] defaultValue = null)
        {
            byte[] readResult = defaultValue;
            FileStream readStream = null;
            int readedSize = 0;

            //进度回调
            float progressTmp = 0;
            if (null != callbackProgress)
            {
                WaitFor.Run(() =>
                {
                    if (progressTmp < 1.0f)
                    {
                        callbackProgress(progressTmp);
                    }
                    return progressTmp >= 1.0f;
                },
                () =>
                {
                    callbackProgress(1.0f);
                });
            }

            ThreadPool.RunThreadSafeCallBack(() =>
            {
                bool hasErorr = false;

                //如果文件不存在，则立即返回
                if (!File.Exists(path))
                {
                    Log.Error("FileHelper ReadAllByUserPathAsync error: missing path=" + path);
                    if (null != callbackEnd) callbackEnd(readResult);
                    return;
                }

                //准备打开文件读取流
                try
                {
                    readStream = File.OpenRead(path);
                }
                catch (System.Exception e)
                {
                    Log.Error("FileHelper ReadAllByUserPathAsync error: can't open write, \ne=" + e);
                    hasErorr = true;
                }

                //检查可读文件字节数与要求读的字节数据是否匹配
                if (readSize > readStream.Length - readOffset)
                {
                    Log.Error("FileHelper ReadAllByUserPathAsync error: over range of remain size readSize=" + readSize + " remain size=" + (readStream.Length - readOffset));
                    hasErorr = true;
                }

                //创建读取数组
                if (readSize <= 0)
                {
                    if (readStream.Length > int.MaxValue)
                    {
                        Log.Error("FileHelper ReadAllByUserPathAsync error: over range of int.MaxValue readStream.Length=" + readStream.Length);
                        hasErorr = true;
                    }
                    else
                    {
                        readSize = (int)readStream.Length;
                    }
                }
                if (!hasErorr)
                {
                    readResult = new byte[readSize];
                }

                //开始循环写入文件
                while (!hasErorr && readedSize != readSize)
                {
                    System.Threading.Thread.Sleep(10);

                    if (readedSize > readSize)
                    {
                        Log.Error("FileHelper ReadAllByUserPathAsync error: data error length readedSize=" + readedSize + " readSize=" + readSize);
                        hasErorr = true;
                    }
                    else
                    {
                        try
                        {
                            int currentReadCount = MAX_READ_COUNT_PER_LOOP;
                            if (currentReadCount > readSize - readedSize)
                            {
                                currentReadCount = (int)(readSize - readedSize);
                            }

                            readStream.Position = readOffset;
                            readStream.Read(readResult, readedSize, currentReadCount);
                            readOffset += currentReadCount;
                            readedSize += currentReadCount;

                            //读取文件回调
                            if (null != callbackProgress)
                            {
                                progressTmp = (float)((double)readedSize / (double)readSize);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("FileHelper ReadAllByUserPathAsync error: write error, \ne=" + e);
                            hasErorr = true;
                        }
                    }
                }

                readStream.Close();
                readStream.Dispose();

            }, () =>
            {
                if (null != callbackEnd) callbackEnd(readResult);

                //如果使用的内存大于了1MB则回收一次
                if (readSize > FileDefine.ONE_MB)
                {
                    System.GC.Collect();
                }
            });
        }

        /// <summary>
        /// 删除文件，自定义目录
        /// <param name="path">文件路径</param>
        /// <param name="callbackEnd">删除完毕回调</param>
        /// </summary>
        static public void DeleteByUserPathAsync(string path, System.Action<bool> callbackEnd)
        {
            bool isSuccess = false;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                isSuccess = FileHelper.DeleteByUserPath(path);
            }, () =>
            {
                callbackEnd(isSuccess);
            });
        }

        /// <summary>
        /// 删除文件，指定路径为FileDefine.persistentDataPath(该路径可读可写)
        /// <param name="path">文件路径</param>
        /// <param name="callbackEnd">删除完毕回调</param>
        /// </summary>
        static public void DeleteByPersistentAsync(string path, System.Action<bool> callbackEnd)
        {
            bool isSuccess = false;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                isSuccess = FileHelper.DeleteByPersistent(path);
            }, () =>
            {
                callbackEnd(isSuccess);
            });
        }

        /// <summary>
        /// 递归删除空文件，自定义目录
        /// <param name="path">递归开始的路径，从该文件夹开始往上层依次删除空文件夹</param>
        /// <param name="callbackEnd">删除完毕回调</param>
        /// <param name="ignorePatterns">判断文件数量的时候需要过滤的文件后缀名，如果文件数量为0则会删除该文件夹</param>
        /// </summary>
        static public void DeleteEmptyFolderAsync(string path, System.Action<bool> callbackEnd, params string[] ignorePatterns)
        {
            bool isSuccess = false;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                isSuccess = FileHelper.DeleteEmptyFolder(path, ignorePatterns);
            }, () =>
            {
                callbackEnd(isSuccess);
            });
        }

        /// <summary>
        /// 获取文件的md5，自定义目录
        /// <param name="path">文件路径</param>
        /// <param name="callbackEnd">md5转换完毕回调</param>
        /// </summary>
        static public void MD5FromFileAsync(string path, System.Action<string> callbackEnd)
        {
            string resultMD5 = string.Empty;
            ReadAllByteByUserPathAsync(path, (byte[] bytes) =>
            {
                if (bytes.IsNullOrEmpty())
                {
                    if (null != callbackEnd) callbackEnd(resultMD5);
                }
                else
                {
                    MD5FromByteAsync(bytes, (string md5) =>
                    {
                        resultMD5 = md5;
                        if (null != callbackEnd) callbackEnd(resultMD5);
                    });
                }
            });
        }

        /// <summary>
        /// 获取字符串的md5，自定义目录
        /// <param name="data">字符串数据</param>
        /// <param name="callbackEnd">md5转换完毕回调</param>
        /// </summary>
        static public void MD5FromStringAsync(string data, System.Action<string> callbackEnd)
        {
            MD5FromByteAsync(data.ToByteArray(), callbackEnd);
        }

        /// <summary>
        /// 获取二进制数据的md5，自定义目录
        /// <param name="data">字符串数据</param>
        /// <param name="callbackEnd">md5转换完毕回调</param>
        /// </summary>
        static public void MD5FromByteAsync(byte[] data, System.Action<string> callbackEnd)
        {
            string resultMD5 = string.Empty;
            ThreadPool.RunThreadSafeCallBack(() =>
            {
                resultMD5 = FileHelper.MD5FromByte(data);
            }, () =>
            {
                callbackEnd(resultMD5);
            });
        }
    }
}
