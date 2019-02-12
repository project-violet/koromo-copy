/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.DC;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Component.Manazero;
using Koromo_Copy.Component.Pinterest;
using Koromo_Copy.Component.Pixiv;
using Koromo_Copy.Console;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Koromo_Copy.Net.DPI;
using Koromo_Copy.Net.Driver;
using Koromo_Copy.Plugin;
using Koromo_Copy.Script;
using Koromo_Copy_UX3.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
        public static SearchSpace Instance;

        public SearchSpace()
        {
            InitializeComponent();

            if (!Settings.Instance.UXSetting.UsingThumbnailSearchElements)
                SearchMaterialPanel.Visibility = Visibility.Collapsed;
            else
                SearchPanel.Visibility = Visibility.Collapsed;
            Loaded += SearchSpace_Loaded;
            Instance = this;
            InstanceMonitor.Instances.Add("searchspace", Instance);
        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            TotalProgress.Visibility = Visibility.Collapsed;
        }

        private bool StartsLoading = false;
        private void SearchSpace_Loaded(object sender, RoutedEventArgs e)
        {
#if true
            bool loading = false;
            // Metadata 로딩
            Task.Run(async () => {
                if (IsMetadataLoaded || StartsLoading) return;
                StartsLoading = true;
                if (!HitomiData.Instance.CheckMetadataExist() || Settings.Instance.Hitomi.AutoSync)
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
                if (sb != null)
                {
                    sb.Completed += Sb_Completed;
                    BeginStoryboard(sb);
                }
                RecommendSpace.Instance.Update();
                if (Koromo_Copy.Version.RequireTidy(System.Reflection.Assembly.GetExecutingAssembly().Location))
                {
                    (new PatchNoteWindow()).Show();
                }
                Task.Run(() => CheckUpdate()).ContinueWith((x) => ScriptManager.Instance.Initialization()).ContinueWith((x) => SettingViewScript.Instance.Init());
                Task.Run(() => DownloaderHelper.LoadOthersAsync());
                Task.Run(() => DPIB.Instance.Start());
            }, TaskScheduler.FromCurrentSynchronizationContext());
#endif
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

        private void CheckUpdate()
        {
            if (Koromo_Copy.Version.UpdateRequired())
                MainWindow.Instance.FadeOut_MiddlePopup("새로운 업데이트가 있습니다! 설정->업데이트에서 확인해주세요!", false);
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
            if (content.Trim().StartsWith("http://") || content.Trim().StartsWith("https://"))
            {
                DownloaderHelper.ProcessOthers(content.Trim());
                return;
            }

            try
            {
                List<HitomiMetadata> result;

                int start_element = 0;
                int count_element = 0;

                if (content.Contains('/'))
                {
                    var elem = content.Split(' ').Where(x => x.StartsWith("/")).ElementAt(0);
                    start_element = Convert.ToInt32(elem.Substring(1));
                    content = content.Replace(elem, " ");
                }

                if (content.Contains('?'))
                {
                    var elem = content.Split(' ').Where(x => x.StartsWith("?")).ElementAt(0);
                    count_element = Convert.ToInt32(elem.Substring(1));
                    content = content.Replace(elem, " ");
                }

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
                {
                    result = await HitomiDataSearchAdvanced.Search(content.Trim());
                }
                
                if (start_element != 0 && start_element <= result.Count) result.RemoveRange(0, start_element);
                if (count_element != 0 && count_element < result.Count) result.RemoveRange(count_element, result.Count - count_element);

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
                if (!Settings.Instance.UXSetting.UsingThumbnailSearchElements)
                {
                    var se = new SearchElements(HitomiLegalize.MetadataToArticle(md));
                    SearchPanel.Children.Add(se);
                    SearchPanel.Children.Add(new Separator());
                }
                else
                {
                    var sme = new SearchMaterialElements(HitomiLegalize.MetadataToArticle(md));
                    SearchMaterialPanel.Children.Add(sme);
                    Koromo_Copy.Monitor.Instance.Push("[AddSearchElements] Hitomi Metadata " + md.ID);
                }
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
                if (!Settings.Instance.UXSetting.UsingThumbnailSearchElements)
                {
                    int count = SearchPanel.Children.Count / 2;
                    SearchPanel.Children.Clear();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    if (count > 0)
                        MainWindow.Instance.FadeOut_MiddlePopup($"{count}개 항목을 정리했습니다!", false);
                }
                else
                {
                    int count = SearchMaterialPanel.Children.Count / 2;
                    SearchMaterialPanel.Children.Clear();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    if (count > 0)
                        MainWindow.Instance.FadeOut_MiddlePopup($"{count}개 항목을 정리했습니다!", false);
                }
            }
            else if (tag == "SelectAll")
            {
                if (!Settings.Instance.UXSetting.UsingThumbnailSearchElements)
                    SearchPanel.Children.OfType<SearchElements>().ToList().ForEach(x => x.Select = true);
                else
                    SearchMaterialPanel.Children.OfType<SearchMaterialElements>().ToList().ForEach(x => x.Select = true);
            }
            else if (tag == "DeSelectAll")
            {
                if (!Settings.Instance.UXSetting.UsingThumbnailSearchElements)
                    SearchPanel.Children.OfType<SearchElements>().ToList().ForEach(x => x.Select = false);
                else
                    SearchMaterialPanel.Children.OfType<SearchMaterialElements>().ToList().ForEach(x => x.Select = false);
            }
            else if (tag == "Download")
            {
                if (!Settings.Instance.UXSetting.UsingThumbnailSearchElements)
                {
                    int count = 0;
                    SearchPanel.Children.OfType<SearchElements>().ToList().Where(x => x.Select).ToList().ForEach(x =>
                    {
                        var ha = x.Article as HitomiArticle;
                        var prefix = HitomiCommon.MakeDownloadDirectory(ha, SearchText.Text);
                        Directory.CreateDirectory(prefix);
                        if (!ha.IsUnstable)
                        {
                            DownloadSpace.Instance.RequestDownload(x.Article.Title,
                                x.Article.ImagesLink.Select(y => HitomiCommon.GetDownloadImageAddress((x.Article as HitomiArticle).Magic, y)).ToArray(),
                                x.Article.ImagesLink.Select(y => Path.Combine(prefix, y)).ToArray(),
                                Koromo_Copy.Interface.SemaphoreExtends.Default, prefix, x.Article);
                        }
                        else
                        {
                            DownloaderHelper.ProcessUnstable(ha.UnstableModel);
                        }
                        count++;
                    });
                    if (count > 0) MainWindow.Instance.FadeOut_MiddlePopup($"{count}개 항목 다운로드 시작...");
                }
                else
                {
                    int count = 0;
                    SearchMaterialPanel.Children.OfType<SearchMaterialElements>().ToList().Where(x => x.Select).ToList().ForEach(x =>
                    {
                        var ha = x.Article as HitomiArticle;
                        var prefix = HitomiCommon.MakeDownloadDirectory(x.Article as HitomiArticle, SearchText.Text);
                        Directory.CreateDirectory(prefix);
                        if (!ha.IsUnstable)
                        {
                            DownloadSpace.Instance.RequestDownload(x.Article.Title,
                                x.Article.ImagesLink.Select(y => HitomiCommon.GetDownloadImageAddress((x.Article as HitomiArticle).Magic, y)).ToArray(),
                                x.Article.ImagesLink.Select(y => Path.Combine(prefix, y)).ToArray(),
                                Koromo_Copy.Interface.SemaphoreExtends.Default, prefix, x.Article);
                        }
                        else
                        {
                            DownloaderHelper.ProcessUnstable(ha.UnstableModel);
                        }
                        count++;
                    });
                    if (count > 0) MainWindow.Instance.FadeOut_MiddlePopup($"{count}개 항목 다운로드 시작...");
                }
            }
        }
        
        #region Search Helper
        AutoCompleteLogic logic;

        public object StringAlgorithms { get; private set; }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !logic.skip_enter)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(SearchButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
                logic.ClosePopup();
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((MenuItem)sender).Tag.ToString();

            if (tag == "DeSelectSimilar")
            {
                List<string> titles = new List<string>();
                if (!Settings.Instance.UXSetting.UsingThumbnailSearchElements)
                {
                    for (int i = 0; i < SearchPanel.Children.Count; i+=2)
                    {
                        string ttitle = (SearchPanel.Children[i] as SearchElements).Article.Title.Split('|')[0];
                        if (titles.Count > 0 && !titles.TrueForAll((title) => Strings.ComputeLevenshteinDistance(ttitle, title) > Settings.Instance.Hitomi.TextMatchingAccuracy))
                        {
                            (SearchPanel.Children[i] as SearchElements).Select = false;
                            continue;
                        }

                        titles.Add(ttitle);
                    }
                }
                else
                {
                    for (int i = 0; i < SearchMaterialPanel.Children.Count; i++)
                    {
                        string ttitle = (SearchMaterialPanel.Children[i] as SearchMaterialElements).Article.Title.Split('|')[0];
                        if (titles.Count > 0 && !titles.TrueForAll((title) => Strings.ComputeLevenshteinDistance(ttitle, title) > Settings.Instance.Hitomi.TextMatchingAccuracy))
                        {
                            (SearchMaterialPanel.Children[i] as SearchMaterialElements).Select = false;
                            continue;
                        }

                        titles.Add(ttitle);
                    }
                }
            }
        }
    }
}
