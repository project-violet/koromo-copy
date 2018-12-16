/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.Mangashow;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Koromo_Copy.Net.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// SeriesManagerElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SeriesManagerElements : UserControl
    {
        string url;
        string title = "";
        string thumbnail = "";
        IManager manager;
        ISeries series;
        IArticle article;
        SeriesLogModel series_log;
        SeleniumWrapper wrapper;

        public SeriesManagerElements(SeriesLogModel series_log)
        {
            InitializeComponent();

            this.series_log = series_log;
            Loaded += SeriesManagerElements_Loaded1;
        }

        private void SeriesManagerElements_Loaded1(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    CollectStatusPanel.Visibility = Visibility.Collapsed;
                    SyncPanel.Visibility = Visibility.Visible;
                    url = series_log.URL;
                    manager = SeriesInfo.Instance.SelectManager(url);
                    title = Title.Text = series_log.Title;
                    SiteName.Text = manager.Name;
                    thumbnail = series_log.Thumbnail;

                    switch (manager.Type)
                    {
                        case ManagerType.SingleArticleSingleImage:
                            break;

                        case ManagerType.SingleArticleSingleMovie:
                            break;

                        case ManagerType.SingleArticleMultipleImages:
                            InnerSitesCount.Text = $"사진 {series_log.Archive.Length}장";
                            break;

                        case ManagerType.SingleSeriesMultipleArticles:
                            InnerSitesCount.Text = $"작품 {series_log.Archive.Length}개";
                            break;
                    }

                    LatestSyncDate.Text = MakeSyncDate(DateTime.Now - series_log.LatestUpdateTime);
                    SiteName.Text = manager.Name;
                    DownloadState.Text = "완료됨";
                    if (!string.IsNullOrEmpty(thumbnail))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(thumbnail);
                        bitmap.EndInit();
                        Image.Source = bitmap;
                    }

                    Task.Run(() => PrepareSync());
                }));
            });
        }

        public SeriesManagerElements(string url)
        {
            InitializeComponent();

            this.url = url;
            Title.Text = url;
            Loaded += SeriesManagerElements_Loaded;
        }

        private void SeriesManagerElements_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(url))
                {
                    manager = SeriesInfo.Instance.SelectManager(url);

                    if (manager == null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            Title.Foreground = Brushes.Red;
                            Title.Text = "오류 - 만족하는 URL 유형을 찾을 수 없습니다.\r\n옳바른 URL인지 확인해주세요.\r\nURL: " + url;
                            CollectStatusPanel.Visibility = Visibility.Collapsed;
                        }));
                        return;
                    }

                    if (manager.EngineType == ManagerEngineType.UsingDriver)
                    {
                        wrapper = new SeleniumWrapper();
                    }

                    string top_html = "";
                    string inner_counts = "";
                    
                    var wc = manager.GetWebClient();

                    if (wrapper == null)
                    {
                        if (wc != null)
                            top_html = wc.DownloadString(url);
                        else
                            top_html = NetCommon.DownloadString(url);
                    }

                    switch (manager.Type)
                    {
                        case ManagerType.SingleArticleSingleImage:
                            {

                            }
                            break;

                        case ManagerType.SingleArticleSingleMovie:
                            {

                            }
                            break;

                        case ManagerType.SingleArticleMultipleImages:
                            {
                                if (wrapper == null)
                                {
                                    article = manager.ParseArticle(top_html);
                                }
                                else
                                {
                                    wrapper.Navigate(url);
                                    try { wrapper.ClickXPath("//a[@class='maia-button maia-button-primary']"); } catch { }

                                    article = manager.ParseArticle(wrapper.GetHtml());
                                }
                                title = article.Title;
                                thumbnail = article.Thumbnail;
                                inner_counts = $"사진 {article.ImagesLink.Count}장";
                            }
                            break;

                        case ManagerType.SingleSeriesMultipleArticles:
                            {
                                if (wrapper == null)
                                {
                                    series = manager.ParseSeries(top_html);
                                }
                                else
                                {
                                    wrapper.Navigate(url);
                                    try { wrapper.ClickXPath("//a[@class='maia-button maia-button-primary']"); } catch { }

                                    series = manager.ParseSeries(wrapper.GetHtml());
                                }
                                title = series.Title;
                                thumbnail = series.Thumbnail;
                                inner_counts = $"작품 {series.Articles.Count}개";
                            }
                            break;
                    }
                    
                    Application.Current.Dispatcher.BeginInvoke(new Action(
                    delegate
                    {
                        if (!string.IsNullOrEmpty(thumbnail))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(thumbnail);
                            //bitmap.DecodePixelHeight = 160;
                            bitmap.EndInit();
                            Image.Source = bitmap;
                        }

                        Title.Text = title;
                        SiteName.Text = manager.Name;
                        LatestSyncDate.Text = MakeSyncDate(new TimeSpan(0, 0, 0, 0));
                        InnerSitesCount.Text = inner_counts;
                        DownloadState.Text = "다운로드 중";
                    }));
                    
                    Task.Run(() => StartFirstDownloads());
                }
            });
        }

        private void StartFirstDownloads()
        {
            DispatchInformation dispatch_info = new DispatchInformation();

            dispatch_info.DownloadSize = DownloadSize;
            dispatch_info.DownloadStatus = DownloadStatus;
            dispatch_info.DownloadRetry = DownloadRetry;
            dispatch_info.CompleteFile = CompleteFile;
            dispatch_info.CompleteArticle = CompleteArticle;
            dispatch_info.CompleteSeries = CompleteSeries;

            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                DownloadState.Text = $"수집 중";
            }));

            switch (manager.EngineType)
            {
                case ManagerEngineType.None:
                    {
                        //
                        // Collect 시작
                        //

                        int file_count = 0;

                        if (manager.Type == ManagerType.SingleArticleMultipleImages)
                        {
                            article.ImagesLink = manager.ParseImages(NetCommon.DownloadString(article.Archive), article);
                        }
                        else if (manager.Type == ManagerType.SingleSeriesMultipleArticles)
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(
                            delegate
                            {
                                ProgressText.Text = $"가져오는 중... [0/{series.Articles.Count}]";
                            }));

                            for (int i = 0; i < series.Articles.Count; i++)
                            {
                                series.Articles[i].ImagesLink = manager.ParseImages(NetCommon.DownloadString(series.Archive[i]), series.Articles[i]);
                                file_count += series.Articles[i].ImagesLink.Count;

                                int k = i;
                                Application.Current.Dispatcher.BeginInvoke(new Action(
                                delegate
                                {
                                    ProgressText.Text = $"가져오는 중... [{i}/{series.Articles.Count}] (파일 {file_count}개)";
                                    if (k == 0 && string.IsNullOrEmpty(series.Thumbnail))
                                    {
                                        var bitmap = new BitmapImage();
                                        bitmap.BeginInit();
                                        bitmap.UriSource = new Uri(thumbnail = series.Articles[0].ImagesLink[0]);
                                        bitmap.EndInit();
                                        Image.Source = bitmap;
                                    }
                                }));
                            }
                        }

                        Application.Current.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            CollectStatusPanel.Visibility = Visibility.Collapsed;
                            DownloadStatusPanel.Visibility = Visibility.Visible;
                            Progress.Maximum = file_count;
                            ProgressStatus.Text = $"[0/{file_count}]";
                        }));

                        //
                        // 다운로드 시작
                        //

                        EmiliaSeriesSegment series_seg = new EmiliaSeriesSegment();
                        series_seg.Index = EmiliaDispatcher.Instance.GetSeriesIndex();
                        series_seg.Title = series.Title;
                        download_folder = series_seg.Path = Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), manager.Name.Trim()), DeleteInvalid(series.Title));

                        List<EmiliaArticleSegment> article_segs = new List<EmiliaArticleSegment>();
                        HashSet<string> folder_names = new HashSet<string>();
                        int ov = 0;
                        for (int i = 0; i < series.Articles.Count; i++)
                        {
                            EmiliaArticleSegment article_seg = new EmiliaArticleSegment();
                            article_seg.Index = i;
                            article_seg.Name = series.Articles[i].Title;

                            string folder_name = DeleteInvalid(series.Articles[i].Title).Trim();
                            if (!folder_names.Contains(folder_name))
                                article_seg.FolderName = DeleteInvalid(series.Articles[i].Title).Trim();
                            else
                                article_seg.FolderName = DeleteInvalid(series.Articles[i].Title).Trim() + $" [OV{++ov}]";
                            folder_names.Add(article_seg.FolderName);
                            article_seg.SereisIndex = series_seg.Index;

                            Directory.CreateDirectory(Path.Combine(series_seg.Path, article_seg.FolderName));

                            List<EmiliaFileSegment> file_segs = new List<EmiliaFileSegment>();
                            List<string> file_names = manager.GetDownloadFileNames(series.Articles[i]);
                            for (int j = 0; j < series.Articles[i].ImagesLink.Count; j++)
                            {
                                EmiliaFileSegment file_seg = new EmiliaFileSegment();
                                file_seg.Index = j;
                                file_seg.ArticleIndex = i;
                                file_seg.SeriesIndex = series_seg.Index;
                                file_seg.FileName = file_names[j];
                                file_seg.Url = series.Articles[i].ImagesLink[j];

                                SemaphoreExtends se = SemaphoreExtends.MakeDefault();
                                se.Referer = url;

                                file_seg.Extends = se;
                                file_segs.Add(file_seg);
                            }

                            article_seg.Files = file_segs;
                            article_segs.Add(article_seg);
                        }
                        series_seg.Articles = article_segs;

                        EmiliaDispatcher.Instance.Add(series_seg, dispatch_info);
                    }
                    break;

                case ManagerEngineType.UsingDriver:
                    {
                        int file_count = 0;

                        if (manager.Type == ManagerType.SingleArticleMultipleImages)
                        {
                            wrapper.Navigate(article.Archive);
                            try { wrapper.ClickXPath("//a[@class='maia-button maia-button-primary']"); } catch { }
                            article.ImagesLink = manager.ParseImages(wrapper.GetHtml(), article);
                        }
                        else if (manager.Type == ManagerType.SingleSeriesMultipleArticles)
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(
                            delegate
                            {
                                ProgressText.Text = $"가져오는 중... [0/{series.Articles.Count}]";
                            }));

                            for (int i = 0; i < series.Articles.Count; i++)
                            {
                                wrapper.Navigate(series.Archive[i]);
                                try { wrapper.ClickXPath("//a[@class='maia-button maia-button-primary']"); } catch { }
                                series.Articles[i].ImagesLink = manager.ParseImages(wrapper.GetHtml(), series.Articles[i]);
                                file_count += series.Articles[i].ImagesLink.Count;

                                int k = i;
                                Application.Current.Dispatcher.BeginInvoke(new Action(
                                delegate
                                {
                                    ProgressText.Text = $"가져오는 중... [{i}/{series.Articles.Count}] (파일 {file_count}개)";
                                    if (k == 0 && string.IsNullOrEmpty(series.Thumbnail))
                                    {
                                        var bitmap = new BitmapImage();
                                        bitmap.BeginInit();
                                        bitmap.UriSource = new Uri(thumbnail = series.Articles[0].ImagesLink[0]);
                                        bitmap.EndInit();
                                        Image.Source = bitmap;
                                    }
                                }));
                            }
                        }

                        Application.Current.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            CollectStatusPanel.Visibility = Visibility.Collapsed;
                            DownloadStatusPanel.Visibility = Visibility.Visible;
                            Progress.Maximum = file_count;
                            ProgressStatus.Text = $"[0/{file_count}]";
                        }));

                        //
                        // 다운로드 시작
                        //

                        EmiliaSeriesSegment series_seg = new EmiliaSeriesSegment();
                        series_seg.Index = EmiliaDispatcher.Instance.GetSeriesIndex();
                        series_seg.Title = series.Title;
                        download_folder = series_seg.Path = Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), manager.Name.Trim()), DeleteInvalid(series.Title));

                        List<EmiliaArticleSegment> article_segs = new List<EmiliaArticleSegment>();
                        for (int i = 0; i < series.Articles.Count; i++)
                        {
                            EmiliaArticleSegment article_seg = new EmiliaArticleSegment();
                            article_seg.Index = i;
                            article_seg.Name = series.Articles[i].Title;
                            article_seg.FolderName = DeleteInvalid(series.Articles[i].Title).Trim();
                            article_seg.SereisIndex = series_seg.Index;

                            Directory.CreateDirectory(Path.Combine(series_seg.Path, article_seg.FolderName));

                            List<EmiliaFileSegment> file_segs = new List<EmiliaFileSegment>();
                            List<string> file_names = manager.GetDownloadFileNames(series.Articles[i]);
                            for (int j = 0; j < series.Articles[i].ImagesLink.Count; j++)
                            {
                                EmiliaFileSegment file_seg = new EmiliaFileSegment();
                                file_seg.Index = j;
                                file_seg.ArticleIndex = i;
                                file_seg.SeriesIndex = series_seg.Index;
                                file_seg.FileName = file_names[j];
                                file_seg.Url = series.Articles[i].ImagesLink[j];

                                SemaphoreExtends se = SemaphoreExtends.MakeDefault();
                                se.Referer = url;

                                file_seg.Extends = se;
                                file_segs.Add(file_seg);
                            }

                            article_seg.Files = file_segs;
                            article_segs.Add(article_seg);
                        }
                        series_seg.Articles = article_segs;

                        EmiliaDispatcher.Instance.Add(series_seg, dispatch_info);
                    }
                    break;
            }
        }

        private void DownloadSize(EmiliaFileSegment efs)
        {
        }

        long downloaded_size = 0;
        private void DownloadStatus(EmiliaFileStatusSegment efss)
        {
            downloaded_size += efss.DownloadSize;
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                ProgressSize.Text = ((double)downloaded_size / 1000 / 1000).ToString("#,#.#") + " MB";
            }));
        }

        private void DownloadRetry(EmiliaFileSegment efs)
        {
            Monitor.Instance.Push($"[Retry SME] {manager.Name} {series.Title} {series.Articles[efs.ArticleIndex].Title} {efs.Url}");

            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                ErrorMessageGrid.Visibility = Visibility.Visible;
                ErrorMessage.Text = $"재시도 : {efs.Url}";
            }));
        }

        private void CompleteFile(EmiliaFileSegment efs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                CollectStatusPanel.Visibility = Visibility.Collapsed;
                DownloadStatusPanel.Visibility = Visibility.Visible;
                Progress.Value += 1;
                ProgressStatus.Text = $"[{Progress.Value}/{Progress.Maximum}]";
            }));
        }

        int zip_count = 0;
        string download_folder;
        private void CompleteArticle(EmiliaArticleSegment efs)
        {
            if (Settings.Instance.Model.AutoZip)
            {
                Monitor.Instance.Push("[Zip Start] " + Path.Combine(download_folder, efs.FolderName));
                MainWindow.Instance.ZipCountUp();
                System.Threading.Interlocked.Increment(ref zip_count);
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    DownloadState.Text = $"{zip_count}개 압축 중";
                }));
                Task.Run(() => Zip(Path.Combine(download_folder, efs.FolderName)));
            }
        }

        private void Zip(string address)
        {
            if (address.EndsWith("\\"))
                address = address.Remove(address.Length - 1);
            if (File.Exists($"{address}.zip"))
                File.Delete($"{address}.zip");

            try
            {
                ZipFile.CreateFromDirectory(address, $"{address}.zip");
                Directory.Delete(address, true);
                Monitor.Instance.Push("[Zip End] " + address);
            }
            catch
            {
                Monitor.Instance.Push("[Zip Error] " + address);
            }
            MainWindow.Instance.ZipCountDown();

            int v = System.Threading.Interlocked.Decrement(ref zip_count);

            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                DownloadState.Text = $"{v}개 압축 중";
            }));

            if (v == 0 && complete_download)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    DownloadState.Text = "압축 완료";
                }));
            }
        }

        bool complete_download = false;
        private void CompleteSeries()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                DownloadStatusPanel.Visibility = Visibility.Collapsed;
                ErrorMessageGrid.Visibility = Visibility.Collapsed;

                if (!Settings.Instance.Model.AutoZip)
                {
                    DownloadState.Text = "다운로드 완료";
                }
                complete_download = true;
            }));

            List<string> archive = new List<string>();
            List<string> subtitle = new List<string>();

            switch (manager.Type)
            {
                case ManagerType.SingleArticleSingleImage:
                    {

                    }
                    break;

                case ManagerType.SingleArticleSingleMovie:
                    {

                    }
                    break;

                case ManagerType.SingleArticleMultipleImages:
                    {
                        archive.AddRange(article.ImagesLink);
                        subtitle.Add(title);
                    }
                    break;

                case ManagerType.SingleSeriesMultipleArticles:
                    {
                        archive.AddRange(series.Archive);
                        series.Articles.ForEach(x => subtitle.Add(x.Title));
                    }
                    break;
            }
            
            if (series_log == null)
            {
                SeriesLog.Instance.Add(new SeriesLogModel
                {
                    URL = url,
                    Title = title,
                    Thumbnail = thumbnail,
                    Archive = archive.ToArray(),
                    Subtitle = subtitle.ToArray(),
                    LatestUpdateTime = DateTime.Now
                });
                SeriesLog.Instance.Save();
            }
            else
            {
                SeriesLog.Instance.Model.Remove(series_log);

                var v = series_log.Archive.ToList();
                v.AddRange(archive);
                series_log.Archive = v.ToArray();

                var s = series_log.Subtitle.ToList();
                s.AddRange(subtitle);
                series_log.Subtitle = s.ToArray();

                series_log.LatestUpdateTime = DateTime.Now;

                SeriesLog.Instance.Add(series_log);
                SeriesLog.Instance.Save();

                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    SyncButton.IsEnabled = false;
                }));
            }
        }

        private static string DeleteInvalid(string path)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
                path = path.Replace(c.ToString(), "");
            return path;
        }

        private static string MakeSyncDate(TimeSpan gap)
        {
            if (gap.Days >= 730) return "2년 전";
            if (gap.Days >= 365) return "1년 전";
            if (gap.Days >= 30) return $"{gap.Days / 30} 개월 전";
            if (gap.Days >= 2) return $"{gap.Days}일 전";
            if (gap.Days >= 1) return $"하루 전";
            if (gap.Hours >= 1) return $"{gap.Hours}시간 전";
            if (gap.Minutes >= 1) return $"{gap.Minutes}분 전";
            return "방금";
        }
        
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            ImageToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ImageToolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ImageToolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y;
        }

        private void SiteName_MouseEnter(object sender, MouseEventArgs e)
        {
            SiteName.Foreground = Brushes.White;
        }

        private void SiteName_MouseLeave(object sender, MouseEventArgs e)
        {
            SiteName.Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
        }

        private void SiteName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(url);
        }

        private void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button.Tag.ToString() == "Sync")
            {
                Task.Run(() => Sync());
            }
            else if (button.Tag.ToString() == "Detail")
            {
            }
            else if (button.Tag.ToString() == "Delete")
            {
                SeriesLog.Instance.Model.Remove(series_log);
                SeriesLog.Instance.Save();
                (Parent as Panel).Children.Remove(this);
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
        }

        private void PrepareSync()
        {
            string top_html = "";
            string inner_counts = "";

            var wc = manager.GetWebClient();

            if (wrapper == null)
            {
                if (wc != null)
                    top_html = wc.DownloadString(url);
                else
                    top_html = NetCommon.DownloadString(url);
            }

            bool require_sync = false;

            switch (manager.Type)
            {
                case ManagerType.SingleSeriesMultipleArticles:
                    {
                        if (wrapper == null)
                        {
                            series = manager.ParseSeries(top_html);
                        }
                        else
                        {
                            wrapper.Navigate(url);
                            try { wrapper.ClickXPath("//a[@class='maia-button maia-button-primary']"); } catch { }

                            series = manager.ParseSeries(wrapper.GetHtml());
                        }
                        title = series.Title;
                        thumbnail = series.Thumbnail;
                        inner_counts = $"작품 {series.Articles.Count}개";

                        if (series_log.Archive.Length < series.Articles.Count)
                        {
                            require_sync = true;
                            Application.Current.Dispatcher.BeginInvoke(new Action(
                            delegate
                            {
                                SyncPanel.Visibility = Visibility.Collapsed;
                                SyncButton.IsEnabled = true;
                                RequireSyncPanel.Visibility = Visibility.Visible;
                                SyncText.Text = $"{series.Articles.Count - series_log.Archive.Length}개의 새로운 항목";
                            }));
                        }
                    }
                    break;
            }

            if (!require_sync)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    SyncPanel.Visibility = Visibility.Collapsed;
                }));
            }
        }

        private void Sync()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(
            delegate
            {
                RequireSyncPanel.Visibility = Visibility.Collapsed;
            }));
            List<string> archive = series.Archive.ToList();
            List<IArticle> articles = series.Articles;
            for (int j = 0; j < series_log.Archive.Length; j++)
            {
                for (int i = 0; i < archive.Count; i++)
                {
                    if (archive[i] == series_log.Archive[j])
                    {
                        archive.RemoveAt(i);
                        articles.RemoveAt(i);
                        break;
                    }
                }
            }
            series.Archive = archive.ToArray();
            series.Articles = articles;
            StartFirstDownloads();
        }

        private void Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (new ZipViewer(Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), manager.Name.Trim()), DeleteInvalid(series.Title)))).Show();
        }
    }
}
