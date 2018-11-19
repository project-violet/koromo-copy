/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy_UX3.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Koromo_Copy_UX3
{
    /// <summary>
    /// SearchPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchSpace : UserControl
    {
        public SearchSpace()
        {
            InitializeComponent();

            Loaded += SearchSpace_Loaded;
        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            TotalProgress.Visibility = Visibility.Collapsed;
        }

        private bool StartsLoading = false;
        private void SearchSpace_Loaded(object sender, RoutedEventArgs e)
        {
            bool loading = false;
            // Metadata 로딩
            Task.Run(async () => {
                if (IsMetadataLoaded || StartsLoading) return;
                StartsLoading = true;
                if (!HitomiData.Instance.CheckMetadataExist())
                {
//#if !DEBUG
//                    Koromo_Copy.Monitor.Instance.ControlEnable = true;
//                    Koromo_Copy.Monitor.Instance.Push("다운로드가 계속 진행되지 않는다면 이 창에서 Enter키를 눌러주세요");
//                    Koromo_Copy.Console.Console.Instance.Show();
//#endif
                    MainWindow.Instance.Fade_MiddlePopup(true, "데이터를 다운로드 중입니다...");
                    HitomiData.Instance.MetadataDownloadStatusEvent = UpdateDownloadText;
                    await HitomiData.Instance.DownloadMetadata();
                    MainWindow.Instance.ModifyText_MiddlePopup("히든 데이터를 다운로드 중입니다...");
                    await HitomiData.Instance.DownloadHiddendata();
                    MainWindow.Instance.FadeOut_MiddlePopup("데이터를 모두 다운로드했습니다!", false);
                    Koromo_Copy.Monitor.Instance.ControlEnable = false;
                }
                else
                {
                    HitomiData.Instance.LoadMetadataJson();
                    HitomiData.Instance.LoadHiddendataJson();
                    MainWindow.Instance.Fade_MiddlePopup(false);
                }
                HitomiData.Instance.RebuildTagData();
                if (HitomiData.Instance.metadata_collection != null)
                {
                    Koromo_Copy.Monitor.Instance.Push($"Loaded metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");

                    if (Settings.Instance.Hitomi.UsingOptimization)
                    {
                        HitomiData.Instance.OptimizeMetadata();
                        Koromo_Copy.Monitor.Instance.Push($"Optimize metadata: '{HitomiData.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
                    }
                }
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                loading = true;
            }).ContinueWith(t =>
            {
                if ((IsMetadataLoaded || StartsLoading) && !loading) return;
                TotalProgress.IsIndeterminate = false;
                TotalProgress.Value = 0;
                IsMetadataLoaded = true;
                Storyboard sb = TotalProgress.FindResource("FadeProgressStoryboard") as Storyboard;
                sb.Completed += Sb_Completed;
                if (sb != null) { BeginStoryboard(sb); }
                RecommendSpace.Instance.Update();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            Window w = Window.GetWindow(this);
            // 이거 지우면 디자이너 오류남
            if (w != null)
            {
                w.LocationChanged += (object obj, EventArgs args) =>
                {
                    var offset = AutoComplete.HorizontalOffset;
                    AutoComplete.HorizontalOffset = offset + 1;
                    AutoComplete.HorizontalOffset = offset;
                };
            }

            logic = new AutoCompleteLogic(SearchText, AutoComplete, AutoCompleteList);
        }

        private void UpdateDownloadText(string text)
        {
            MainWindow.Instance.ModifyText_MiddlePopup($"데이터를 다운로드 중입니다... {text}");
        }

        public bool IsMetadataLoaded = false;

        private void SearchSpace_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Koromo_Copy.Monitor.IsValueCreated)
            {
                Koromo_Copy.Monitor.Instance.Save();
                if (Koromo_Copy.Monitor.Instance.ControlEnable)
                    Koromo_Copy.Console.Console.Instance.Stop();
            }
        }

        private async void AppendAsync(string content)
        {
            try
            {
                List<HitomiMetadata> result;
                if (!Settings.Instance.Hitomi.UsingAdvancedSearch || content.Contains("recent:"))
                {
                    result = await HitomiDataParser.SearchAsync(content.Trim());

                    if (content.Contains("recent:"))
                    {
                        var elem = content.Split(' ').Where(x => x.StartsWith("recent:")).ElementAt(0);
                        int recent_count = 0;
                        int recent_start = 0;
                        if (elem.Substring("recent:".Length).Contains("-"))
                        {
                            recent_start = Convert.ToInt32(elem.Substring("recent:".Length).Split('-')[0]);
                            recent_count = Convert.ToInt32(elem.Substring("recent:".Length).Split('-')[1]);
                        }
                        else
                            recent_count = Convert.ToInt32(elem.Substring("recent:".Length));
                        SearchText.Text = "recent:" + (recent_start + recent_count) + "-" + recent_count;
                    }
                }
                else
                    result = await HitomiDataSearchAdvanced.Search(content.Trim());

                SearchCount.Text = $"검색된 항목: {result.Count.ToString("#,#")}개";
                _ = Task.Run(() => LoadThumbnail(result));
            }
            catch
            {
                SearchCount.Text = "검색 문법이 잘못되었습니다.";
            }
        }

        private void LoadThumbnail(List<HitomiMetadata> md)
        {
            List<Task> task = new List<Task>();
            foreach (var metadata in md)
            {
                Task.Run(() => LoadThumbnail(metadata));
                Thread.Sleep(100);
            }
        }

        private void LoadThumbnail(HitomiMetadata md)
        {
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                // Put code that needs to run on the UI thread here
                var se = new SearchElements(HitomiLegalize.MetadataToArticle(md));
                SearchPanel.Children.Add(se);
                SearchPanel.Children.Add(new Separator());
                Koromo_Copy.Monitor.Instance.Push("[AddSearchElements] Hitomi Metadata " + md.ID);
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();

            if (tag == "Search" && IsMetadataLoaded)
            {
                AppendAsync(SearchText.Text);
            }
            else if (tag == "Tidy")
            {
                int count = SearchPanel.Children.Count / 2;
                SearchPanel.Children.Clear();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                if (count > 0)
                MainWindow.Instance.FadeOut_MiddlePopup($"{count}개 항목을 정리했습니다!", false);
            }
            else if (tag == "SelectAll")
            {
                SearchPanel.Children.OfType<SearchElements>().ToList().ForEach(x => x.Select = true);
            }
            else if (tag == "DeSelectAll")
            {
                SearchPanel.Children.OfType<SearchElements>().ToList().ForEach(x => x.Select = false);
            }
            else if (tag == "Download")
            {
                int count = 0;
                SearchPanel.Children.OfType<SearchElements>().ToList().Where(x => x.Select).ToList().ForEach(x =>
                {
                    var prefix = HitomiCommon.MakeDownloadDirectory(x.Article as HitomiArticle, SearchText.Text);
                    Directory.CreateDirectory(prefix);
                    DownloadSpace.Instance.RequestDownload(x.Article.Title, 
                        x.Article.ImagesLink.Select(y => HitomiCommon.GetDownloadImageAddress((x.Article as HitomiArticle).Magic, y)).ToArray(), 
                        x.Article.ImagesLink.Select(y => Path.Combine(prefix, y)).ToArray(),
                        Koromo_Copy.Net.SemaphoreExtends.Default, prefix, x.Article);
                    count++;
                });
                if (count > 0) MainWindow.Instance.FadeOut_MiddlePopup($"{count}개 항목 다운로드 시작...");
            }
        }
        
        #region Search Helper
        AutoCompleteLogic logic;

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !logic.skip_enter)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(SearchButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
            logic.skip_enter = false;
        }
        
        private void SearchText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMetadataLoaded) return;
            logic.SearchText_PreviewKeyDown(sender, e);
        }
        
        private void SearchText_KeyUp(object sender, KeyEventArgs e)
        {
            if (!IsMetadataLoaded) return;
            logic.SearchText_KeyUp(sender, e);
        }
        
        private void AutoCompleteList_KeyUp(object sender, KeyEventArgs e)
        {
            logic.AutoCompleteList_KeyUp(sender, e);
        }

        private void AutoCompleteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            logic.AutoCompleteList_MouseDoubleClick(sender, e);
        }
#endregion

    }
}
