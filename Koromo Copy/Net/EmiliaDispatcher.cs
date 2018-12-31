/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koromo_Copy.Net
{
    /// <summary>
    /// 파일단위의 세그먼트입니다.
    /// </summary>
    public class EmiliaFileSegment
    {
        /// <summary>
        /// 다운로드할 파일의 고유번호입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 파일이 속한 아티클의 고유번호입니다.
        /// </summary>
        public int ArticleIndex;

        /// <summary>
        /// 파일이 속한 시리즈의 고유번호입니다.
        /// </summary>
        public int SeriesIndex;

        /// <summary>
        /// 다운로드 Url입니다.
        /// </summary>
        public string Url;

        /// <summary>
        /// 다운로드할 Url의 부가정보입니다.
        /// </summary>
        public SemaphoreExtends Extends;

        /// <summary>
        /// 다운로드할 파일의 이름입니다.
        /// </summary>
        public string FileName;

        /// <summary>
        /// 다운로드할 파일의 크기입니다.
        /// </summary>
        public long Size;
    }

    public class EmiliaFileStatusSegment
    {
        /// <summary>
        /// 다운로드할 파일의 고유번호입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 파일이 속한 아티클의 고유번호입니다.
        /// </summary>
        public int ArticleIndex;

        /// <summary>
        /// 다운로드한 크기입니다.
        /// 이 크기는 파일의 전체크기가 아닌 다운로드 블록당 다운로드한 크기입니다.
        /// </summary>
        public long DownloadSize;
    }

    /// <summary>
    /// 아티클단위의 세그먼트입니다.
    /// </summary>
    public class EmiliaArticleSegment
    {
        /// <summary>
        /// 다운로드할 아티클의 고유번호입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 아티클이 속한 시리즈의 고유번호입니다.
        /// </summary>
        public int SereisIndex;

        /// <summary>
        /// 아티클의 폴더이름입니다.
        /// </summary>
        public string FolderName;

        /// <summary>
        /// 아티클의 제목입니다.
        /// </summary>
        public string Name;

        /// <summary>
        /// 다운로드할 파일들의 정보입니다.
        /// </summary>
        public List<EmiliaFileSegment> Files;
    }

    /// <summary>
    /// 시리즈단위의 세그먼트입니다.
    /// </summary>
    public class EmiliaSeriesSegment
    {
        /// <summary>
        /// 다운로드할 시리즈의 고유번호입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 시리즈를 다운로드할 최상위 폴더입니다.
        /// </summary>
        public string Path;

        /// <summary>
        /// 시리즈의 제목입니다.
        /// </summary>
        public string Title;

        /// <summary>
        /// 아티클들의 정보입니다.
        /// </summary>
        public List<EmiliaArticleSegment> Articles;
    }

    /// <summary>
    /// 오류 정보를 담는 세그먼트입니다.
    /// </summary>
    public class EmiliaErrorSegment
    {
        /// <summary>
        /// 다운로드 Url입니다.
        /// </summary>
        public string Url;

        /// <summary>
        /// 오류내용입니다.
        /// </summary>
        public string Message;
    }

    /// <summary>
    /// 디스패쳐에 맡길 정보들 입니다.
    /// </summary>
    public class DispatchInformation
    {
        /// <summary>
        /// 다운로드할 파일의 사이즈를 얻었을 때 발생합니다.
        /// </summary>
        public Action<EmiliaFileSegment> DownloadSize;

        /// <summary>
        /// 다운로드한 바이트 수를 알립니다.
        /// </summary>
        public Action<EmiliaFileStatusSegment> DownloadStatus;

        /// <summary>
        /// 재시도한 파일을 알립니다.
        /// </summary>
        public Action<EmiliaFileSegment> DownloadRetry;

        /// <summary>
        /// 어떤 파일의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public Action<EmiliaFileSegment> CompleteFile;

        /// <summary>
        /// 어떤 아티클의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public Action<EmiliaArticleSegment> CompleteArticle;

        /// <summary>
        /// 어떤 아티클의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public Action<EmiliaErrorSegment> ErrorOccured;

        /// <summary>
        /// 시리즈 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public Action CompleteSeries;
    }

    /// <summary>
    /// 단일 시리즈에 대한 다운로드관리를 지원하는 다운로드 큐 관리자 입니다.
    /// </summary>
    public class EmiliaDispatcher : ILazy<EmiliaDispatcher>
    {
        ISemaphore queue;

        /// <summary>
        /// 선점 연속 요청 횟수
        /// </summary>
        int mutex_count;

        /// <summary>
        /// 남은 다운로드할 파일 수
        /// </summary>
        int remain_contents;

        /// <summary>
        /// 전체 파일 수
        /// </summary>
        int total_contents;

        object add_lock = new object();
        object job_lock = new object();
        object complete_lock = new object();

        public ISemaphore Queue { get { return queue; } }

        public int TotalContents { get { return total_contents; } }

        public EmiliaDispatcher()
        {
            queue = new EmiliaQueue(downloadSizeCallback, downloadStatusCallback, downloadRetryCallback, downloadErrorCallback);
            series_dictionary = new Dictionary<int, EmiliaSeriesSegment>();
            dispatcher_dictionary = new Dictionary<int, DispatchInformation>();
            check_dictionary = new Dictionary<int, List<bool[]>>();
            downloaded_count_dictionary = new Dictionary<int, int[]>();
            downloaded_articles_count_dictionary = new Dictionary<int, int>();
            series_count = -1;
        }

        Dictionary<int, EmiliaSeriesSegment> series_dictionary;
        Dictionary<int, DispatchInformation> dispatcher_dictionary;
        Dictionary<int, List<bool[]>> check_dictionary;
        Dictionary<int, int[]> downloaded_count_dictionary;
        Dictionary<int, int> downloaded_articles_count_dictionary;

        int series_count;

        /// <summary>
        /// 시리즈들을 구별할 수 있는 인덱스를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public int GetSeriesIndex()
        {
            return Interlocked.Increment(ref series_count);
        }

        #region Global Event

        /// <summary>
        /// 다운로드할 파일의 사이즈를 얻었을 때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> DownloadSize;

        /// <summary>
        /// 다운로드한 바이트 수를 알립니다.
        /// </summary>
        public event EventHandler<EmiliaFileStatusSegment> DownloadStatus;

        /// <summary>
        /// 재시도한 파일을 알립니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> DownloadRetry;

        /// <summary>
        /// 어떤 파일의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaFileSegment> CompleteFile;

        /// <summary>
        /// 어떤 아티클의 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaArticleSegment> CompleteArticle;

        /// <summary>
        /// 어떤 시리즈 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler<EmiliaSeriesSegment> CompleteSeries;

        /// <summary>
        /// 모든 다운로드가 끝났을때 발생합니다.
        /// </summary>
        public event EventHandler DownloadComplete;

        #endregion

        private void downloadSizeCallback(string uri, long size, object obj)
        {
            var file_seg = (EmiliaFileSegment)obj;
            DownloadSize?.Invoke(null, file_seg);
            dispatcher_dictionary[file_seg.SeriesIndex].DownloadSize.Invoke(file_seg);
        }

        private void downloadStatusCallback(string uri, int size, object obj)
        {
            var file_seg = (EmiliaFileSegment)obj;
            var status_seg = new EmiliaFileStatusSegment
            {
                ArticleIndex = file_seg.ArticleIndex,
                DownloadSize = size,
                Index = file_seg.Index
            };
            DownloadStatus?.Invoke(null, status_seg);
            dispatcher_dictionary[file_seg.SeriesIndex].DownloadStatus.Invoke(status_seg);
        }

        private void downloadRetryCallback(string uri, object obj)
        {
            var file_seg = (EmiliaFileSegment)obj;
            DownloadRetry?.Invoke(null, file_seg);
            dispatcher_dictionary[file_seg.SeriesIndex].DownloadRetry.Invoke(file_seg);
        }

        private void downloadCallback(string url, string filename, object obj)
        {
            var file_seg = (EmiliaFileSegment)obj;
            CompleteFile?.Invoke(null, file_seg);
            dispatcher_dictionary[file_seg.SeriesIndex].CompleteFile.Invoke(file_seg);

            lock (complete_lock)
            {
                int article_status = downloaded_count_dictionary[file_seg.SeriesIndex][file_seg.ArticleIndex] += 1;
                check_dictionary[file_seg.SeriesIndex][file_seg.ArticleIndex][file_seg.Index] = true;

                // 아티클 다운로드 완료
                if (article_status == series_dictionary[file_seg.SeriesIndex].Articles[file_seg.ArticleIndex].Files.Count)
                {
                    int series_status = downloaded_articles_count_dictionary[file_seg.SeriesIndex] += 1;
                    dispatcher_dictionary[file_seg.SeriesIndex].CompleteArticle.Invoke(series_dictionary[file_seg.SeriesIndex].Articles[file_seg.ArticleIndex]);
                    CompleteArticle?.Invoke(null, series_dictionary[file_seg.SeriesIndex].Articles[file_seg.ArticleIndex]);

                    // 시리즈 다운로드 완료
                    if (series_status == series_dictionary[file_seg.SeriesIndex].Articles.Count)
                    {
                        dispatcher_dictionary[file_seg.SeriesIndex].CompleteSeries();
                        CompleteSeries?.Invoke(null, series_dictionary[file_seg.SeriesIndex]);
                    }
                }
            }
            
            lock (add_lock)
            {
                remain_contents--;
                if (remain_contents == 0 && DownloadComplete != null)
                    DownloadComplete.Invoke(null, null);
            }
        }

        private void downloadErrorCallback(string url, string msg, object obj)
        {
            var file_seg = (EmiliaFileSegment)obj;
            dispatcher_dictionary[file_seg.SeriesIndex].ErrorOccured.Invoke(new EmiliaErrorSegment { Message = msg, Url = url });
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
        /// 시리즈를 다운로드하는 새 작업을 추가합니다.
        /// </summary>
        /// <param name="series"></param>
        /// <param name="dispatcher"></param>
        public void Add(EmiliaSeriesSegment series, DispatchInformation dispatcher)
        {
            lock (job_lock)
            {
                series_dictionary.Add(series.Index, series);
                dispatcher_dictionary.Add(series.Index, dispatcher);

                var check_list = new List<bool[]>();
                for (int i = 0; i < series.Articles.Count; i++)
                    check_list.Add(new bool[series.Articles[i].Files.Count]);
                check_dictionary.Add(series.Index, check_list);

                downloaded_count_dictionary.Add(series.Index, new int[series.Articles.Count]);
                downloaded_articles_count_dictionary.Add(series.Index, 0);
            }

            lock (add_lock)
            {
                foreach (var article in series.Articles)
                {
                    var article_folder = Path.Combine(series.Path, article.FolderName);
                    remain_contents += article.Files.Count;
                    total_contents += article.Files.Count;
                    foreach (var file in article.Files)
                    {
                        var file_path = Path.Combine(article_folder, file.FileName);
                        queue.Add(file.Url, file_path, file, downloadCallback, file.Extends);
                    }
                }
            }
        }
    }
}
