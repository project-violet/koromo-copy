/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koromo_Copy.Net
{
    /// <summary>
    /// C#의 저수준 Thread를 이용하는 다운로드 큐입니다.
    /// </summary>
    public class EmiliaQueue : ISemaphore
    {
        public int capacity = 32;
        public List<Tuple<string, string, object, SemaphoreCallBack, SemaphoreExtends>> queue = new List<Tuple<string, string, object, SemaphoreCallBack, SemaphoreExtends>>();
        public List<Tuple<string, HttpWebRequest>> requests = new List<Tuple<string, HttpWebRequest>>();
        public List<string> aborted = new List<string>();
        public List<Thread> threads = new List<Thread>();
        public List<ManualResetEvent> interrupt = new List<ManualResetEvent>();
        public IWebProxy proxy { get; set; }

        public delegate void DownloadSizeCallBack(string uri, long size, object obj);
        public delegate void DownloadStatusCallBack(string uri, int size, object obj);
        public delegate void RetryCallBack(string uri, object obj);
        public delegate void ErrorCallBack(string uri, string msg, object obj);

        DownloadSizeCallBack download_callback;
        DownloadStatusCallBack status_callback;
        RetryCallBack retry_callback;
        ErrorCallBack err_callback;

        object notify_lock = new object();
        SpinLock shutdown_lock = new SpinLock();
        object task_lock = new object();
        volatile bool preempt_take = false;
        bool shutdown_lock_taken = false;

        /// <summary>
        /// 다운로드 큐의 생성자 입니다.
        /// </summary>
        /// <param name="notify_size">다운로드가 시작되기전 파일의 크기를 알릴때 호출됩니다.</param>
        /// <param name="notify_status">다운로드 중인 파일의 </param>
        /// <param name="retry"></param>
        public EmiliaQueue(DownloadSizeCallBack notify_size, DownloadStatusCallBack notify_status, RetryCallBack retry, ErrorCallBack err)
        {
            capacity = Settings.Instance.Model.Thread;
            ServicePointManager.DefaultConnectionLimit = Settings.Instance.Net.ServicePointConnectionLimit;
            timeout_infinite = Settings.Instance.Net.TimeoutInfinite;
            timeout_ms = Settings.Instance.Net.TimeoutMillisecond;
            buffer_size = Settings.Instance.Net.DownloadBufferSize;
            download_callback = notify_size;
            status_callback = notify_status;
            retry_callback = retry;
            err_callback = err;
            proxy = null;

            for (int i = 0; i < capacity; i++)
            {
                interrupt.Add(new ManualResetEvent(false));
                threads.Add(new Thread(new ParameterizedThreadStart(remote_download_thread_handler)));
                threads.Last().Start(i);
            }
        }

        /// <summary>
        /// Request 타임아웃 관련 설정입니다.
        /// </summary>
        public bool timeout_infinite = false;
        public int timeout_ms = 10000;
        public int buffer_size = 131072;

        /// <summary>
        /// 이 플래그가 true면 모든 다운로드과정을 정지합니다.
        /// </summary>
        public bool shutdown = false;

        /// <summary>
        /// 세마포어가 허용하는 총 스레드 수를 가져옵니다.
        /// </summary>
        public int Capacity { get { return capacity; } set { capacity = value; } }

        /// <summary>
        /// 특정 작업을 취소합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool Abort(string url)
        {
            lock (queue)
            {
                for (int i = 0; i < queue.Count; i++)
                    if (queue[i].Item1 == url)
                    {
                        queue.RemoveAt(i);
                        lock (notify_lock) Notify();
                        break;
                    }
            }
            lock (requests)
            {
                foreach (var i in requests)
                    if (i.Item1 == url)
                        lock (i.Item2) i.Item2.Abort();
            }
            aborted.Add(url);
            return false;
        }

        /// <summary>
        /// 모든 작업을 취소하고, 다운로드 중인 파일을 삭제합니다.
        /// </summary>
        public void Abort()
        {
            lock (requests)
            {
                shutdown_lock_taken = false;
                shutdown_lock.Enter(ref shutdown_lock_taken);
                shutdown = true;
                shutdown_lock.Exit();

                lock (queue)
                {
                    foreach (var vp in queue) try { File.Delete(vp.Item2); } catch { }
                    queue.Clear();
                }
                for (int i = requests.Count - 1; i >= 0; i--)
                    requests[i].Item2.Abort();
            }
        }

        /// <summary>
        /// 새로운 작업을 큐에 추가합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <param name="callback"></param>
        /// <param name="se"></param>
        public void Add(string url, string path, object obj, SemaphoreCallBack callback, SemaphoreExtends se = null)
        {
            lock (queue) queue.Add(new Tuple<string, string, object, SemaphoreCallBack, SemaphoreExtends>(url, path, obj, callback, se));
            lock (notify_lock) Notify();
        }

        /// <summary>
        /// 큐를 일시정지합니다.
        /// </summary>
        public void Preempt()
        {
            preempt_take = true;
        }

        /// <summary>
        /// 큐를 재활성화합니다.
        /// </summary>
        public void Reactivation()
        {
            preempt_take = false;
        }
        
        private void Notify()
        {
            interrupt.ForEach(x => x.Set());
        }

        private void remote_download_thread_handler(object i)
        {
            int index = (int)i;
            //Monitor.Instance.Push($"[Emilia Queue] Starts download thread [{i}]");
            while (true)
            {
                interrupt[index].WaitOne();

                Tuple<string, string, object, SemaphoreCallBack, SemaphoreExtends> job;

                lock (queue)
                {
                    if (queue.Count > 0)
                    {
                        job = queue[0];
                        queue.RemoveAt(0);
                    }
                    else
                    {
                        //Monitor.Instance.Push($"[Emilia Queue] Suspends download thread [{i}]");
                        interrupt[index].Reset();
                        continue;
                    }
                }

                string uri = job.Item1;
                string fileName = job.Item2;
                object obj = job.Item3;
                SemaphoreCallBack callback = job.Item4;
                SemaphoreExtends se = job.Item5;

                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Monitor.Instance.Push($"[Directory Not Found] {uri} is auto deleted in download queue.");
                    goto END;
                }
                
                int retry_count = 0;
                bool lock_donwload_size = false;
            RETRY:
                if (retry_count > Settings.Instance.Net.RetryCount)
                {
                    Monitor.Instance.Push($"[Many Retry] {uri} is auto deleted in download queue.");
                    lock (err_callback) err_callback(uri, "[Emilia Queue] Many retry. auto deleted in download queue.", obj);
                    lock (callback) callback(uri, fileName, obj);
                    return;
                }

                if (!uri.StartsWith("http"))
                {
                    Monitor.Instance.Push($"[Url Error] {uri} is not corret url");
                    lock (err_callback) err_callback(uri, "[Emilia Queue] Url Error. not corret url.", obj);
                    lock (callback) callback(uri, fileName, obj);
                    return;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                se.RunPass(ref request);

                // Temporary Assignments
                if (uri.Contains("hitomi.la"))
                {
                    request.Referer = $"https://hitomi.la/galleries/{uri.Split('/')[5]}.html";
                }

                request.Timeout = timeout_infinite ? Timeout.Infinite : timeout_ms;
                request.KeepAlive = true;
                request.Proxy = proxy;

                lock (requests) requests.Add(new Tuple<string, HttpWebRequest>(uri, request));

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            Monitor.Instance.Push($"404 Not Found {uri}");
                        }
                        else if (response.StatusCode == HttpStatusCode.OK ||
                            response.StatusCode == HttpStatusCode.Moved ||
                            response.StatusCode == HttpStatusCode.Redirect)
                        {
                            using (Stream inputStream = response.GetResponseStream())
                            using (Stream outputStream = File.OpenWrite(fileName))
                            {
                                byte[] buffer = new byte[buffer_size];
                                int bytesRead;
                                if (!lock_donwload_size)
                                    lock (download_callback)
                                        download_callback(uri, response.ContentLength, obj);
                                lock_donwload_size = true;
                                do
                                {
                                    bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                    outputStream.Write(buffer, 0, bytesRead);
                                    lock (status_callback) status_callback(uri, bytesRead, obj);
                                    shutdown_lock_taken = false;
                                    shutdown_lock.Enter(ref shutdown_lock_taken);
                                    if (shutdown) { shutdown_lock.Exit(); break; }
                                    shutdown_lock.Exit(); 
                                    if (preempt_take)
                                    {
                                        Monitor.Instance.Push($"[Preempt Queue] {uri}");
                                        while (preempt_take)
                                            Thread.Sleep(500);
                                        Monitor.Instance.Push($"[Exit Preempt] {uri}");
                                    }
                                } while (bytesRead != 0);
                            }
                            shutdown_lock_taken = false;
                            shutdown_lock.Enter(ref shutdown_lock_taken);
                            if (shutdown)
                            {
                                shutdown_lock.Exit();
                                File.Delete(fileName);
                                Monitor.Instance.Push($"[Shutdown] {uri}");
                                return;
                            }
                            shutdown_lock.Exit();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is WebException we)
                    {
                        HttpWebResponse webResponse = (HttpWebResponse)we.Response;
                        if (webResponse != null && webResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            Monitor.Instance.Push($"[Emilia Queue] 404 error {uri}");
                            lock (err_callback) err_callback(uri, "[Emilia Queue] 404 error. auto deleted in download queue.", obj);
                            goto END;
                        }
                    }
                    
                    Monitor.Instance.Push($"[{retry_count}] {e.Message}");
                    lock (aborted)
                        if (!aborted.Contains(uri))
                        {
                            lock (retry_callback) retry_callback(uri, obj);
                            request.Abort();
                            Thread.Sleep(1000);
                            retry_count++;
                            goto RETRY;
                        }
                        else
                        {
                            File.Delete(fileName);
                            lock (callback) callback(uri, fileName, obj);
                            return;
                        }
                }
                END:

                lock (callback) callback(uri, fileName, obj);
            }
        }
    }
}
