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
    public class EmiliaJob : ILazy<EmiliaJob>
    {
        public int capacity = 32;
        public int job_count = 0;
        public List<Tuple<string, object, SemaphoreCallBack>> queue = new List<Tuple<string, object, SemaphoreCallBack>>();
        public List<Thread> threads = new List<Thread>();
        public List<ManualResetEvent> interrupt = new List<ManualResetEvent>();
        public List<List<string>> results = new List<List<string>>();
        public List<Action<int>> count_events = new List<Action<int>>();
        
        public EmiliaJob()
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
        public List<string> AddJob(List<string> urls, Action<int> action)
        {
            int job = -1;
            lock (queue)
            {
                job = job_count;
                results.Add(new List<string>());
                count_events.Add(action);
                urls.ForEach(url => queue.Add(new Tuple<string, object, SemaphoreCallBack>(url, job_count, callback)));
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

            return results[job];
        }

        private void callback(string url, string filename, object obj)
        {
            int count = -1;
            lock (results)
            {
                results[(int)obj].Add(filename);
                count = results[(int)obj].Count;
            }

            lock (count_events[(int)obj]) count_events[(int)obj](count);
        }
        
        private void remote_download_thread_handler(object i)
        {
            int index = (int)i;
            while (true)
            {
                interrupt[index].WaitOne();

                Tuple<string, object, SemaphoreCallBack> job;

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
                object obj = job.Item2;
                SemaphoreCallBack callback = job.Item3;
                
                try
                {
                    lock (callback) callback(uri, NetCommon.DownloadString(uri), obj);
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
