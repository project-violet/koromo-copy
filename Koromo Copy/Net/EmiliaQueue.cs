/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Net
{
    /// <summary>
    /// C#의 저수준 Thread를 이용하는 다운로드 큐입니다.
    /// </summary>
    public class EmiliaQueue : ISemaphore
    {
        public int Capacity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Abort(string url)
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public void Add(string url, string path, object obj, SemaphoreCallBack callback, SemaphoreExtends se = null)
        {
            throw new NotImplementedException();
        }
    }
}
