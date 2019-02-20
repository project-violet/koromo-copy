/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System.Net;

namespace Koromo_Copy.Interface
{
    public class SemaphoreExtends
    {
        public static SemaphoreExtends Default = MakeDefault();
        public static SemaphoreExtends MakeDefault()
        {
            return new SemaphoreExtends()
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36"
            };
        }

        public string Accept = null;
        public string UserAgent = null;
        public string Referer = null;
        public string Cookie = null;

        /// <summary>
        /// 리퀘스트 헤더 정보를 사용자 설정에 맞게 재설정합니다.
        /// </summary>
        /// <param name="request"></param>
        public virtual void RunPass(ref HttpWebRequest request)
        {
            try
            {
                if (Accept != null) request.Accept = Accept;
                if (UserAgent != null) request.UserAgent = UserAgent;

                // Referer의 URL이 접속가능한 정상적인 URL인데도,
                // 옳바른 형식의 URL이 아니라며 예외를 던질때가 있다.
                // 현재 해결방법을 찾지 못하여 try-catch로 감싸놓은 상태이다.
                if (Referer != null) request.Referer = Referer;
                if (Cookie != null) request.Headers.Add(HttpRequestHeader.Cookie, Cookie);
            }
            catch { }
        }
    }

    public delegate void SemaphoreCallBack(string url, string filename, object obj);

    /// <summary>
    /// 다운로드 큐를 구현하기 위한 세마포어 인터페이스입니다.
    /// </summary>
    public interface ISemaphore
    {
        /// <summary>
        /// 세마포어의 크기입니다.
        /// </summary>
        int Capacity { get; set; }

        /// <summary>
        /// 특정 url 다운로드를 취소합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool Abort(string url);

        /// <summary>
        /// 모든 작업을 멈추고, 종료 신호를 보냅니다.
        /// </summary>
        void Abort();

        /// <summary>
        /// 모든 작업을 일시중지합니다.
        /// </summary>
        void Preempt();

        /// <summary>
        /// 일시정지된 작업을 재개합니다.
        /// </summary>
        void Reactivation();

        /// <summary>
        /// 다운로드 작업을 세마포어에 추가합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="obj">작업을 구분할 수 있는 정보입니다.</param>
        /// <param name="callback">다운로드 작업이 끝났을때 호출됩니다.</param>
        /// <param name="se">Referer와 같이 추가적인 작업이 필요할 때 사용됩니다.</param>
        void Add(string url, string path, object obj, SemaphoreCallBack callback, SemaphoreExtends se = null);
    }
}
