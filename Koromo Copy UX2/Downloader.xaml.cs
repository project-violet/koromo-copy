/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer
   
***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX2.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public void TogglePanel()
        {
            //SearchPanel.Effect = Effect;
        }

        private async void AppendAsync(string content)
        {
            var result = await HitomiDataParser.SearchAsync(content);
            SearchCount.Text = "검색된 항목 : " + result.Count + "개";

            if (result.Count > 100)
            {
                var sampleMessageDialog = new MessageDialog
                {
                    DataContext = new SampleDialogViewModel(),
                    Message = { Text = $"검색결과가 {result.Count.ToString("#,#")}개로 너무 많습니다. 그래도 로딩할까요?" }
                };

                var xx = await DialogHost.Show(sampleMessageDialog, "RootDialog");

                if ((bool)xx == false)
                    return;
            }
            
            Task.Run(() => LoadThumbnail(result));
        }

        private void LoadThumbnail(List<HitomiMetadata> md)
        {
            List<Task> task = new List<Task>();
            foreach (var metadata in md)
            {

                Application.Current.Dispatcher.Invoke(new Action(
                delegate
                {
                    Task.Run(() => LoadThumbnail(metadata));
                }));
                Thread.Sleep(100);
            }
            //await Task.Run(() => Task.WaitAll(task.ToArray()));
        }
        
        private void LoadThumbnail(HitomiMetadata md)
        {
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                // Put code that needs to run on the UI thread here
                var se = new SearchElements(HitomiLegalize.MetadataToArticle(md));
                SearchResult.Children.Add(se);
                SearchResult.Children.Add(new Separator());
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();

            if (tag == "Tidy")
            {
                SearchResult.Children.Clear();
            }
            else if (tag == "SelectAll")
            {
                SearchResult.Children.OfType<SearchElements>().ToList().ForEach(x => x.Select = true);
            }
            else if (tag == "SelectAllCancle") // deselect all
            {
                SearchResult.Children.OfType<SearchElements>().ToList().ForEach(x => x.Select = false);
            }
            else if (tag == "Download")
            {

            }
        }
    }
}
