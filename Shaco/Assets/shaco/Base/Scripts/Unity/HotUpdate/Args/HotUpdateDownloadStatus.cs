using UnityEngine;
using System.Collections;

namespace shaco
{
    public class HotUpdateDownloadStatus
    {
        public enum Status
        {
            //默认状态
            None = 0x00,
            //发生错误，调用HotUpdateWWW的GetLastError()方法获取详细错误信息
            HasError = 0x01,
            //需要联网更新资源
            ErrorNeedUpdateResourceWithNetWork = 0x02,
            //url地址失效，未找到指定资源
            ErrorNotFoundResource = 0x04,
            //不需要更新资源配置
            NoNeedToUpdateConfig = 0x08,
            //不需要下载资源
            NoNeedToDownloadResource = 0x10,
            //只下载配置文件，不下载其他内容(包含manifest和资源文件))
            OnlyDownloadConfig = 0x20,
            
            //更新完成，但是可能会有其他状态错误，建议使用HasStatus检查状态
            UpdateCompleted = 0x40
        }

        private int status = (int)HotUpdateDownloadStatus.Status.None;

        public void ResetDownloadStatus()
        {
            status = (int)HotUpdateDownloadStatus.Status.None;
        }

        public void SetStatus(HotUpdateDownloadStatus.Status status)
        {
            this.status |= (int)status;
        }

        public bool HasStatus(HotUpdateDownloadStatus.Status status)
        {
            return (this.status & (int)status) != 0;
        }

        public string GetStatusDescription()
        {
            var retValue = new System.Text.StringBuilder();

            retValue.Append("[status: ");

            var allStatus = new HotUpdateDownloadStatus.Status[]
            {
                HotUpdateDownloadStatus.Status.None, HotUpdateDownloadStatus.Status.HasError,
                HotUpdateDownloadStatus.Status.NoNeedToUpdateConfig, HotUpdateDownloadStatus.Status.NoNeedToDownloadResource,
                HotUpdateDownloadStatus.Status.ErrorNeedUpdateResourceWithNetWork, HotUpdateDownloadStatus.Status.ErrorNotFoundResource,
                HotUpdateDownloadStatus.Status.UpdateCompleted
            };

            bool hasStatusTmp = false;
            string splitTmp = ", ";

            for (int i = 0; i < allStatus.Length; ++i)
            {
                var statusTmp = allStatus[i];
                if (HasStatus(statusTmp))
                {
                    retValue.Append(statusTmp.ToString());
                    retValue.Append(splitTmp);
                    hasStatusTmp = true;
                }
            }

            if (hasStatusTmp)
            {
                retValue.Remove(retValue.Length - splitTmp.Length, splitTmp.Length);
            }
            retValue.Append("]"); 

            return retValue.ToString();
        }
    }
}
