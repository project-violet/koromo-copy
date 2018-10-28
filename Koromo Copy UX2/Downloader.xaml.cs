/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer
   
***/

using Koromo_Copy.Component.Hitomi;
using System;
using System.Collections.Generic;
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

namespace Koromo_Copy_UX2
{
    /// <summary>
    /// Downloader.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Downloader : UserControl
    {
        public Downloader()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (HitomiData.IsValueCreated == false)
                {
                    //Task.Factory.StartNew(t =>
                    //{
                    //    //note you can use the message queue from any thread, but just for the demo here we 
                    //    //need to get the message queue from the snackbar, so need to be on the dispatcher
                    //    MainSnackbar.MessageQueue.Enqueue("Koromo Copy에 오신것을 환영합니다!");
                    //}, TaskScheduler.FromCurrentSynchronizationContext());
                    MainWindow.Snackbar.MessageQueue.Enqueue("메타데이터가 로딩되지 않아 검색할 수 없습니다.");
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(
                    delegate
                    {
                        AppendAsync((sender as TextBox).Text);
                    }));
                }
            }
        }

        private async void AppendAsync(string content)
        {
            var result = await HitomiDataParser.SearchAsync(content);
            SearchCount.Text = "검색된 항목 : " + result.Count + "개";
            //result.Reverse();
            
            List<Task> task = new List<Task>();
            foreach (var metadata in result)
            {
                task.Add(Task.Run(() => LoadThumbnail(metadata)));
            }

            await Task.Run(() => Task.WaitAll(task.ToArray()));
        }

        private void LoadThumbnail(HitomiMetadata md)
        {
            HitomiArticle ha = HitomiLegalize.MetadataToArticle(md);
            ha.Thumbnail = HitomiCommon.HitomiThumbnail + HitomiParser.ParseGalleryBlock(Koromo_Copy.Net.NetCommon.DownloadString(
                $"{HitomiCommon.HitomiGalleryBlock}{md.ID}.html")).Thumbnail;
            ha.ImagesLink = HitomiParser.GetImageLink(Koromo_Copy.Net.NetCommon.DownloadString(HitomiCommon.GetImagesLinkAddress(ha.Magic)));
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                // Put code that needs to run on the UI thread here
                var se = new SearchElements(ha);
                SearchResult.Children.Add(se);
                SearchResult.Children.Add(new Separator());
            }));
        }
    }
}
