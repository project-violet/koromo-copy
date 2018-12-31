/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Koromo_Copy.Net
{
    public class DownloadGroup : ILazy<DownloadGroup>
    {
        ISemaphore queue;
        int remain_contents;
        int index_count;
        int mutex_count;

        object add_lock = new object();
        object job_lock = new object();

        /// <summary>
        /// 모든 다운로드가 끝났을 때 발생합니다.
        /// </summary>
        public event EventHandler DownloadComplete;

        /// <summary>
        /// 다운로드 시작 전 사이즈를 알립니다.
        /// </summary>
        public event EventHandler<Tuple<string, long, object>> NotifySize;

        /// <summary>
        /// 다운로드한 바이트 수를 알립니다.
        /// </summary>
        public event EventHandler<Tuple<string, int, object>> DownloadStatus;

        /// <summary>
        /// 재시도한 파일을 알립니다.
        /// </summary>
        public event EventHandler<Tuple<string, object>> Retry;

        /// <summary>
        /// 어떤 파일의 다운로드가 끝났을 때 발생합니다.
        /// </summary>
        public event EventHandler<Tuple<string, string, object>> Complete;

        /// <summary>
        /// 어떤 그룹(작품)의 다운로드가 끝났을 때 발생합니다.
        /// </summary>
        public event EventHandler<Tuple<string, object>> CompleteGroup;

        List<Tuple<int, object, SemaphoreCallBack, 
            DownloadQueue.DownloadSizeCallBack, DownloadQueue.DownloadStatusCallBack, DownloadQueue.RetryCallBack>> jobs;
        List<int> download_file_count;
        List<int> file_count;
        
        public ISemaphore Queue { get { return queue; } }

        public DownloadGroup()
        {
            queue = new EmiliaQueue(downloadSizeCallback, downloadStatusCallback, downloadRetryCallback, downloadErrorCallback);
            remain_contents = 0;
            index_count = 0;
            jobs = new List<Tuple<int, object, SemaphoreCallBack, DownloadQueue.DownloadSizeCallBack, DownloadQueue.DownloadStatusCallBack, DownloadQueue.RetryCallBack>>();
            download_file_count = new List<int>();
            file_count = new List<int>();
        }

        private void downloadErrorCallback(string uri, string msg, object obj)
        {
        }

        private void downloadSizeCallback(string uri, long size, object obj)
        {
            if (NotifySize != null)
                NotifySize.Invoke(null, Tuple.Create(uri, size, jobs[(int)obj].Item2));
            jobs[(int)obj].Item4?.Invoke(uri, size, jobs[(int)obj].Item2);
        }

        private void downloadStatusCallback(string uri, int size, object obj)
        {
            if (DownloadStatus != null)
                DownloadStatus.Invoke(null, Tuple.Create(uri, size, jobs[(int)obj].Item2));
            jobs[(int)obj].Item5?.Invoke(uri, size, jobs[(int)obj].Item2);
        }

        private void downloadRetryCallback(string uri, object obj)
        {
            if (Retry != null)
                Retry.Invoke(null, Tuple.Create(uri, jobs[(int)obj].Item2));
            jobs[(int)obj].Item6?.Invoke(uri, jobs[(int)obj].Item2);
        }

        private void downloadCallback(string url, string filename, object obj)
        {
            if (Complete != null)
            Complete.Invoke(null, Tuple.Create(url, filename, jobs[(int)obj].Item2));

            lock (add_lock)
            {
                remain_contents--;
                if (remain_contents == 0)
                    DownloadComplete.Invoke(null, null);
            }

            download_file_count[(int)obj]++;
            if (download_file_count[(int)obj] == file_count[(int)obj])
                if (CompleteGroup != null)
                    CompleteGroup.Invoke(null, Tuple.Create("", jobs[(int)obj].Item2));
        }
        
        /// <summary>
        /// 큐를 일시정지합니다.
        /// </summary>
        public void Preempt()
        {
            queue.Preempt();
            Interlocked.Increment(ref mutex_count);
        }
        
        /// <summary>
        /// 큐를 재활성화합니다.
        /// </summary>
        public void Reactivation()
        {
            if (Interlocked.Decrement(ref mutex_count) == 0)
                queue.Reactivation();
        }

        /// <summary>
        /// 모든 작업을 취소하고, 다운로드 중인 파일을 삭제합니다.
        /// </summary>
        public void Abort()
        {
            queue.Abort();
        }

        /// <summary>
        /// 특정 작업을 취소합니다.
        /// </summary>
        public void Abort(string url)
        {
            queue.Abort(url);
        }

        /// <summary>
        /// 새 작업을 추가합니다.
        /// </summary>
        /// <param name="urls">다운로드할 파일의 URL입니다.</param>
        /// <param name="paths">다운로드 경로를 지정합니다.</param>
        /// <param name="obj">callback에서 전달될 객체입니다.</param>
        /// <param name="callback">파일의 다운로드가 끝나면 이 함수가 호출됩니다.</param>
        /// <param name="se">리퀘스트에 추가할 추가 옵션입니다.</param>
        /// <param name="size_callback">리퀘스트 응답을 성공적으로 받을 시 파일의 크기가 전달됩니다.</param>
        /// <param name="status_callback">파일의 바이트 블록(131,072 바이트)이나 맨 마지막 바이트 블록을 전달받으면 이 함수가 호출됩니다.</param>
        /// <param name="retry_callback">리퀘스트 도중 응답이 끊기거나, 정의되지 않은 오류로인해 다운로드가 취소되어 파일을 재다운로드할 경우 이 함수가 호출됩니다.</param>
        public void Add(string[] urls, string[] paths, object obj, SemaphoreCallBack callback, SemaphoreExtends se, 
            DownloadQueue.DownloadSizeCallBack size_callback = null, DownloadQueue.DownloadStatusCallBack status_callback = null, DownloadQueue.RetryCallBack retry_callback = null)
        {
            lock (job_lock)
            {
                jobs.Add(new Tuple<int, object, SemaphoreCallBack, DownloadQueue.DownloadSizeCallBack, DownloadQueue.DownloadStatusCallBack, DownloadQueue.RetryCallBack>(
                    index_count, obj, callback,
                    size_callback, status_callback, retry_callback));
                download_file_count.Add(0);
                file_count.Add(urls.Length);
            }

            lock (add_lock)
            {
                for (int i = 0; i < urls.Length; i++)
                {
                    queue.Add(urls[i], paths[i], index_count, downloadCallback, se);
                }
                index_count++;
                remain_contents += urls.Length;
            }
        }
    }
}
