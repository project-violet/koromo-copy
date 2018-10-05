/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;

namespace Koromo_Copy.Net
{
    public class DownloadGroup
    {
        DownloadQueue queue;
        int remain_contents;
        int index_count;

        object add_lock = new object();
        object job_lock = new object();

        public event EventHandler DownloadComplete;
        public event EventHandler<Tuple<string, long, object>> NotifySize;
        public event EventHandler<Tuple<string, int, object>> DownloadStatus;
        public event EventHandler<Tuple<string, object>> Retry;
        public event EventHandler<Tuple<string, string, object>> Complete;

        List<Tuple<int, object, SemaphoreCallBack, 
            DownloadQueue.DownloadSizeCallBack, DownloadQueue.DownloadStatusCallBack, DownloadQueue.RetryCallBack>> jobs;

        List<bool> completes;
        
        public DownloadGroup()
        {
            queue = new DownloadQueue(downloadSizeCallback, downloadStatusCallback, downloadRetryCallback);
            remain_contents = 0;
            index_count = 0;
            jobs = new List<Tuple<int, object, SemaphoreCallBack, DownloadQueue.DownloadSizeCallBack, DownloadQueue.DownloadStatusCallBack, DownloadQueue.RetryCallBack>>();
            completes = new List<bool>();
        }

        private void downloadSizeCallback(string uri, long size, object obj)
        {
            if (NotifySize != null)
                NotifySize.Invoke(null, Tuple.Create(uri, size, obj));
            lock (jobs)
                jobs[(int)obj].Item4?.Invoke(uri, size, obj);
        }

        private void downloadStatusCallback(string uri, int size, object obj)
        {
            if (DownloadStatus != null)
                DownloadStatus.Invoke(null, Tuple.Create(uri, size, obj));
            lock (jobs)
                jobs[(int)obj].Item5?.Invoke(uri, size, obj);
        }

        private void downloadRetryCallback(string uri, object obj)
        {
            if (Retry != null)
                Retry.Invoke(null, Tuple.Create(uri, obj));
            lock (jobs)
                jobs[(int)obj].Item6?.Invoke(uri, obj);
        }

        private void downloadCallback(string url, string filename, object obj)
        {
            if (Complete != null)
                Complete.Invoke(null, Tuple.Create(url, filename, obj));
            lock(job_lock)
            {
                completes[(int)obj] = true;
                if (completes.TrueForAll(x => x))
                    Complete.Invoke(null, Tuple.Create(url, filename, obj));
            }
        }

        /// <summary>
        /// 새 작업을 추가합니다.
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="paths"></param>
        /// <param name="obj"></param>
        /// <param name="callback"></param>
        /// <param name="se"></param>
        public void Add(string[] urls, string[] paths, object obj, SemaphoreCallBack callback, SemaphoreExtends se = null, 
            DownloadQueue.DownloadSizeCallBack size_callback = null, DownloadQueue.DownloadStatusCallBack statuc_callback = null, DownloadQueue.RetryCallBack retry_callback = null)
        {
            lock (add_lock)
            {
                lock (job_lock)
                {
                    jobs.Add(new Tuple<int, object, SemaphoreCallBack, DownloadQueue.DownloadSizeCallBack, DownloadQueue.DownloadStatusCallBack, DownloadQueue.RetryCallBack>(
                        index_count, obj, callback,
                        size_callback, statuc_callback, retry_callback));
                    completes.Add(false);
                }
                for (int i = 0; i < urls.Length; i++)
                {
                    queue.Add(urls[i], paths[i], index_count, downloadCallback, se);
                }
                index_count++;
            }
        }
    }
}
