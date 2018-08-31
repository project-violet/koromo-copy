/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Net;

namespace Koromo_Copy.Net
{
    public class DownloadQueue : ISemaphore
    {
        public int capacity = 32;
        public int mtx = 0;
        public List<Tuple<string, string, object>> queue = new List<Tuple<string, string, object>>();
        public List<Tuple<string, HttpWebRequest>> requests = new List<Tuple<string, HttpWebRequest>>();
        public List<string> aborted = new List<string>();
        public IWebProxy proxy { get; set; }

        /// <summary>
        /// Request 타임아웃 관련 설정입니다.
        /// </summary>
        public bool timeout_infinite = true;
        public int timeout_ms = 10000;

        /// <summary>
        /// 이 플래그가 true면 모든 다운로드과정을 정지합니다.
        /// </summary>
        public bool shutdown = false;

        /// <summary>
        /// 세마포어가 허용하는 총 스레드 수를 가져옵니다.
        /// </summary>
        public int Capacity { get { return capacity; } set { capacity = value; } }

        public bool Abort(string url)
        {
            return true;
        }

        public void Abort()
        {
        }

        public void Add(string url, string path, object obj, SemaphoreCallBack callback, SemaphoreExtends se = null)
        {
        }
    }
}
