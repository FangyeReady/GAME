using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace shaco.Base
{
    public class HttpTest
    {
        public class HttpInfo
        {
            public string url = string.Empty;
            public string statusCode = string.Empty;
        }

        private int _currentLoadIndex = 0;
        private readonly int _threadCount = 2;
        private static readonly object _threadLock = new System.Object();

        //批量测试http请求，并将结果写入文件
        public void TestConnect(List<string> urls, string outputPath)
        {
            TestConnect(urls, (List<HttpInfo> urlsSuccess, List<HttpInfo> urlsFailed) =>
            {
                var outputSuccessPath = outputPath + "/test_http_success.txt";
                var outputFailedPath = outputPath + "/test_http_failed.txt";

                FileHelper.WriteAllByUserPath(outputSuccessPath, string.Empty);
                FileHelper.WriteAllByUserPath(outputFailedPath, string.Empty);

                foreach (var iter in urlsSuccess)
                {
                    FileHelper.AppendByUserPath(outputSuccessPath, iter.url + "\n");
                }

                foreach (var iter in urlsFailed)
                {
                    FileHelper.AppendByUserPath(outputFailedPath, iter.url + "\n");
                }
            });
        }

        //批量测试http请求，并返回结果
        public void TestConnect(List<string> urls, System.Action<List<HttpInfo>, List<HttpInfo>> callback)
        {
            List<HttpInfo> urlsSuccess = new List<HttpInfo>();
            List<HttpInfo> urlsFailed = new List<HttpInfo>();
            _currentLoadIndex = 0;

            for (int i = 0; i < _threadCount; ++i)
            {
                if (!StartOnceTest(urls, urlsSuccess, urlsFailed, urls.Count))
                {
                    break;
                }
            }

            shaco.WaitFor.Run(() =>
            {
                lock (_threadLock)
                {
                    return urlsSuccess.Count + urlsFailed.Count >= urls.Count;
                }
            }, () =>
            {
                if (null != callback)
                {
                    callback(urlsSuccess, urlsFailed);
                }
            });
        }

        private bool StartOnceTest(List<string> urls, List<HttpInfo> urlsSuccess, List<HttpInfo> urlsFailed, int totalCount)
        {
            if (_currentLoadIndex < 0 || _currentLoadIndex > urls.Count - 1)
                return false;

            var threadStart = new LoadURLThread();
            threadStart.url = urls[_currentLoadIndex++];
            threadStart.urlsSuccess = urlsSuccess;
            threadStart.urlsFailed = urlsFailed;
            threadStart.totalCount = totalCount;
            threadStart.callbackOnceCompleted = () =>
            {
                StartOnceTest(urls, urlsSuccess, urlsFailed, totalCount);
            };
            ThreadPool.RunThread(threadStart.Run);
            return true;
        }

        private class LoadURLThread
        {
            public string url = string.Empty;
            public int totalCount = 0;
            public List<HttpInfo> urlsSuccess = null;
            public List<HttpInfo> urlsFailed = null;
            public System.Action callbackOnceCompleted = null;
            private readonly int _retryTimes = 5;
            private int _currentRetryTimes = 0;

            public void Run()
            {
                System.Net.HttpWebRequest request = null;
                System.Net.HttpWebResponse response = null;
                string errorResult = string.Empty;

                try
                {
                    request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                    request.Method = "GET";
                    int timeoutSeconds = 10 * 1000;
                    request.Timeout = timeoutSeconds;
                    using (response = (System.Net.HttpWebResponse)request.GetResponse())
                    {
                        if (System.Net.HttpStatusCode.NotFound == response.StatusCode)
                        {
                            errorResult = "not found";
                        }
                        else
                        {
                            if (System.Net.HttpStatusCode.OK != response.StatusCode)
                            {
                                //准备重连
                                if (_currentRetryTimes++ < _retryTimes)
                                {
                                    Log.Info("HttpTest will retry test, current retry times=" + _currentRetryTimes);
                                    Run();
                                    return;
                                }
                                //达到重连次数上限
                                else
                                {
                                    errorResult = response.StatusCode.ToString();
                                }
                            }
                            else
                            {
                                errorResult = string.Empty;
                            }
                        }
                    }

                }
                catch (System.Exception e)
                {
                    if (null != response)
                    {
                        errorResult = e.ToString() + " code=" + response.StatusCode;
                    }
                }

                lock (_threadLock)
                {
                    if (string.IsNullOrEmpty(errorResult))
                    {
                        urlsSuccess.Add(new HttpInfo()
                        {
                            url = url,
                            statusCode = errorResult
                        });
                    }
                    else
                    {
                        urlsFailed.Add(new HttpInfo()
                        {
                            url = url,
                            statusCode = errorResult
                        });
                    }
                    var currentTotalCount = urlsSuccess.Count + urlsFailed.Count;

                    var logTmp = "HttpTest progress=" + ((float)currentTotalCount / (float)totalCount) + " current=" + currentTotalCount + " total count=" + totalCount + " url=" + url + "\nerrorResult=" + errorResult;
                    if (string.IsNullOrEmpty(errorResult))
                    {
                        Log.Info(logTmp);
                    }
                    else
                    {
                        Log.Error(logTmp);
                    }
                }

                if (null != response)
                {
                    response.Close();
                    response = null;
                }
                if (null != request)
                {
                    request.Abort();
                    request = null;
                }
                Thread.Sleep(1);
                callbackOnceCompleted();
            }
        }
    }
}