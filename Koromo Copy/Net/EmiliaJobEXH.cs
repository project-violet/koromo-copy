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
    /// 동시 다운로드를 지원하는 작업처리반입니다.
    /// 모든 작업은 짧은 시간안에 끝나야 합니다.
    /// </summary>
    public class EmiliaJobEXH : ILazy<EmiliaJobEXH>
    {
        public int capacity = 32;
        public int job_count = 0;
        public List<Tuple<string, int, object, SemaphoreCallBack>> queue = new List<Tuple<string, int, object, SemaphoreCallBack>>();
        public List<Thread> threads = new List<Thread>();
        public List<ManualResetEvent> interrupt = new List<ManualResetEvent>();
        public List<List<string>> results = new List<List<string>>();
        public List<List<Tuple<object, string>>> results2 = new List<List<Tuple<object, string>>>();
        public List<Action<int>> count_events = new List<Action<int>>();

        public EmiliaJobEXH()
        {
            capacity = Settings.Instance.Model.Thread;

            for (int i = 0; i < capacity; i++)
            {
                interrupt.Add(new ManualResetEvent(false));
                threads.Add(new Thread(new ParameterizedThreadStart(remote_download_thread_handler)));
                threads.Last().Start(i);
            }
        }

        /// <summary>
        /// 작업을 추가하고 끝날때까지 기다립니다.
        /// </summary>
        /// <param name="url"></param>
        public List<Tuple<object, string>> AddJob(List<string> urls, Action<int> action, List<object> obj = null)
        {
            int job = -1;
            lock (queue)
            {
                job = job_count;
                results.Add(new List<string>());
                results2.Add(new List<Tuple<object, string>>());
                count_events.Add(action);
                if (obj == null)
                    urls.ForEach(url => queue.Add(new Tuple<string, int, object, SemaphoreCallBack>(url, job_count, null, callback)));
                else
                {
                    for (int i = 0; i < urls.Count; i++)
                    {
                        queue.Add(new Tuple<string, int, object, SemaphoreCallBack>(urls[i], job_count, obj[i], callback));
                    }
                }
                interrupt.ForEach(x => x.Set());
                job_count++;
            }

            while (true)
            {
                lock (results)
                {
                    if (results[job].Count == urls.Count)
                        break;
                }
                Thread.Sleep(500);
            }

            return results2[job];
        }

        private void callback(string url, string filename, object obj)
        {
            Tuple<int, object> io = (Tuple<int, object>)obj;
            int count = -1;
            lock (results)
            {
                results[io.Item1].Add(filename);
                results2[io.Item1].Add(new Tuple<object, string>(io.Item2, filename));
                count = results[io.Item1].Count;
            }

            lock (count_events[io.Item1]) count_events[io.Item1](count);
        }

        private void remote_download_thread_handler(object i)
        {
            int index = (int)i;
            while (true)
            {
                interrupt[index].WaitOne();

                Tuple<string, int, object, SemaphoreCallBack> job;

                lock (queue)
                {
                    if (queue.Count > 0)
                    {
                        job = queue[0];
                        queue.RemoveAt(0);
                    }
                    else
                    {
                        interrupt[index].Reset();
                        continue;
                    }
                }

                string uri = job.Item1;
                int job_count = job.Item2;
                object obj = job.Item3;
                SemaphoreCallBack callback = job.Item4;

                try
                {
                    lock (callback) callback(uri, NetCommon.DownloadExHentaiString(uri), new Tuple<int, object> (job_count, obj));
                }
                catch (Exception e)
                {
                    Monitor.Instance.Push($"[Emilia Job] {uri} {e.Message}");
                    lock (callback) callback(uri, "", obj);
                }
            }
        }
    }
}
