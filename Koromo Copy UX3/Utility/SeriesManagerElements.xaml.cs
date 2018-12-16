/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component;
using Koromo_Copy.Component.Mangashow;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace Koromo_Copy_UX3.Utility
{
    /// <summary>
    /// SeriesManagerElements.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SeriesManagerElements : UserControl
    {
        string url;
        IManager manager;
        ISeries series;
        IArticle article;

        public SeriesManagerElements(SeriesInfo info)
        {
            InitializeComponent();

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
                        }));
                        return;
                    }

                    string title = "";
                    string thumbnail = "";
                    string top_html = "";
                    string inner_counts = "";
                    
                    var wc = manager.GetWebClient();
                    if (wc != null)
                        top_html = wc.DownloadString(url);
                    else
                        top_html = NetCommon.DownloadString(url);

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
                                article = manager.ParseArticle(top_html);
                                title = article.Title;
                                thumbnail = article.Thumbnail;
                                inner_counts = $"사진 {article.ImagesLink.Count}장";
                            }
                            break;

                        case ManagerType.SingleSeriesMultipleArticles:
                            {
                                series = manager.ParseSeries(top_html);
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
                        LatestSyncDate.Text = MakeSyncDate(new TimeSpan(0, 1, 0, 0));
                        InnerSitesCount.Text = inner_counts;
                    }));
                    
                    Task.Run(() => StartFirstDownloads());
                }
            });
        }

        private void StartFirstDownloads()
        {
            DispatchInformation dispatch_info = new DispatchInformation();

            switch (manager.EngineType)
            {
                case ManagerEngineType.None:
                    
                    // Collect 시작

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

                        int file_count = 0;

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
                                    bitmap.UriSource = new Uri(series.Articles[0].ImagesLink[0]);
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
                    }));

                    // 다운로드 시작

                    break;

                case ManagerEngineType.UsingDriver:

                    break;
            }
        }

        private string MakeSyncDate(TimeSpan gap)
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
    }
}
