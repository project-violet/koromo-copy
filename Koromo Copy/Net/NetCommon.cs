/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System.Net;
using System.Text;

namespace Koromo_Copy.Net
{
    /// <summary>
    /// 자주 쓰이는 간단한 네트워크 IO 집합입니다.
    /// </summary>
    public class NetCommon
    {
        public static WebClient GetDefaultClient()
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            return wc;
        }

        /// <summary>
        /// JSon, js, html 등 문서를 다운로드합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string DownloadString(string url)
        {
            Monitor.Instance.Push($"Download string: {url}");
            return GetDefaultClient().DownloadString(url);
        }
    }
}
