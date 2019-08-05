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
using Koromo_Copy.Script;
using Koromo_Copy_UX.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Koromo_Copy_UX
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
            Koromo_Copy_UX.Language.Lang.ApplyLanguageDictionary(this);

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
                Profiler.Push("Check metadata exists");
                if (!HitomiIndex.Instance.CheckMetadataExist() || Settings.Instance.Hitomi.AutoSync)
                {
//#if !DEBUG
//                    Koromo_Copy.Monitor.Instance.ControlEnable = true;
//                    Koromo_Copy.Monitor.Instance.Push("다운로드가 계속 진행되지 않는다면 이 창에서 Enter키를 눌러주세요");
//                    Koromo_Copy.Console.Console.Instance.Show();
//#endif
                    Profiler.Push("Start download metadata");
                    MainWindow.Instance.Fade_MiddlePopup(true, (string)FindResource("msg_download_metadata"));
#if true
                    //HitomiIndex.Instance.MetadataDownloadStatusEvent = UpdateDownloadText;
                    await HitomiIndex.Instance.DownloadMetadata();
#endif
                    MainWindow.Instance.FadeOut_MiddlePopup((string)FindResource("msg_download_data_complete"), false);
                    Koromo_Copy.Monitor.Instance.ControlEnable = false;
                }
                else
                {
                    Profiler.Push("Load metadata, hiddendata");
                    try
                    {
                        HitomiIndex.Instance.Load();
                        //MainWindow.Instance.Fade_MiddlePopup(false);
                    }
                    catch (Exception ex)
                    {
                        Koromo_Copy.Monitor.Instance.Push($"[Hitomi DataLoad] {ex.Message}\r\n{ex.StackTrace}");
                    }
                }
                if (Settings.Instance.Hitomi.UsingOriginalTitle)
                {
                    Profiler.Push("Check titles exists");
                    if (!HitomiTitle.Instance.CheckExist())
                    {
                        MainWindow.Instance.ModifyText_MiddlePopup((string)FindResource("msg_download_title"));
                        await HitomiTitle.Instance.DownloadTitle();
                        MainWindow.Instance.FadeOut_MiddlePopup((string)FindResource("msg_download_data_complete"), false);
                    }
                    HitomiTitle.Instance.Load();
                    HitomiTitle.Instance.ReplaceToOriginTitle();
                    Koromo_Copy.Monitor.Instance.Push($"Loaded titles: '{HitomiTitle.Instance.Count.ToString("#,#")}' articles.");
                }
                MainWindow.Instance.Fade_MiddlePopup(false);
                Profiler.Push("Rebuild tag data");
                HitomiIndex.Instance.RebuildTagData();
                if (HitomiIndex.Instance.metadata_collection != null)
                {
                    Koromo_Copy.Monitor.Instance.Push($"Loaded metadata: '{HitomiIndex.Instance.metadata_collection.Count.ToString("#,#")}' articles.");

                    if (Settings.Instance.Hitomi.UsingOptimization)
                    {
                        HitomiIndex.Instance.OptimizeMetadata();
                        Koromo_Copy.Monitor.Instance.Push($"Optimize metadata: '{HitomiIndex.Instance.metadata_collection.Count.ToString("#,#")}' articles.");
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
                    //Process.Start("https://github.com/dc-koromo/koromo-copy/blob/master/Document/Update.md");
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
            MainWindow.Instance.ModifyText_MiddlePopup($"{(string)FindResource("msg_download_metadata")} {text}");
        }

        private void CheckUpdate()
        {
            if (Koromo_Copy.Version.UpdateRequired())
                MainWindow.Instance.FadeOut_MiddlePopup((string)FindResource("msg_new_update"), false);
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
                List<HitomiIndexMetadata> result;

                int start_element = 0;
                int count_element = 0;

                if (content.Split(' ').Any(x => x.StartsWith("/")))
                {
                    var elem = content.Split(' ').Where(x => x.StartsWith("/")).ElementAt(0);
                    start_element = Convert.ToInt32(elem.Substring(1));
                    content = content.Replace(elem, " ");
                }

                if (content.Split(' ').Any(x => x.StartsWith("?")))
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
                
                SearchCount.Text = $"{FindResource("searched")}: {(result.Count != 0 ? result.Count.ToString("#,#") : "0")}{(FindResource("count_postfix"))}";
                SearchButton.Content = $"{FindResource("stop")}";
                _ = Task.Run(() => LoadThumbnail(result));
            }
            catch
            {
                SearchCount.Text = (string)FindResource("msg_incorret_search_grammar");
            }
        }

        int load_count = 0;
        object load_lock = new object();
        bool load_cancel = false;
        bool loading = false;
        private void LoadThumbnail(List<HitomiIndexMetadata> md)
        {
            List<Task> task = new List<Task>();
            load_count = 0;
            lock (load_lock)
            {
                load_cancel = false;
                loading = true;
            }
            foreach (var metadata in md)
            {
                Task.Run(() => LoadThumbnail(metadata));
                Interlocked.Increment(ref load_count);
                Application.Current.Dispatcher.Invoke(new Action(
                delegate
                {
                    SearchCount.Text = $"{FindResource("searched")}: {load_count}/{(md.Count != 0 ? md.Count.ToString("#,#") : "0")}{(FindResource("count_postfix"))}";
                }));
                lock (load_lock) if (load_cancel) break;
                Thread.Sleep(100);
                lock (load_lock) if (load_cancel) break;
            }
            lock (load_lock)
                loading = false;
            Application.Current.Dispatcher.Invoke(new Action(
            delegate
            {
                SearchButton.Content = $"{FindResource("search")}";
            }));
        }

        private void LoadThumbnail(HitomiIndexMetadata md)
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

        private void StopLoad()
        {
            lock (load_lock)
                load_cancel = true;

            var search = SearchText.Text;
            if (search.Split(' ').Any(x => x.StartsWith("/")))
            {
                var elem = search.Split(' ').Where(x => x.StartsWith("/")).ElementAt(0);
                var start_element = Convert.ToInt32(elem.Substring(1));
                SearchText.Text = string.Join(" ", search.Split(' ').Where(x => !x.StartsWith("/"))) + $" /{start_element + load_count}";
            }
            else
            {
                SearchText.Text += $" /{load_count}";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();

            if (tag == "Search" && IsMetadataLoaded)
            {
                lock (load_lock)
                {
                    if (!loading)
                        AppendAsync(SearchText.Text);
                    else
                        StopLoad();
                }
            }
            else if (tag == "Tidy")
            {
                if (!Settings.Instance.UXSetting.UsingThumbnailSearchElements)
                {
                    int count = SearchPanel.Children.Count / 2;
                    SearchPanel.Children.Clear();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    if (count > 0)
                        MainWindow.Instance.FadeOut_MiddlePopup($"{count}{(string)FindResource("msg_tidy")}", false);
                }
                else
                {
                    int count = SearchMaterialPanel.Children.Count / 2;
                    SearchMaterialPanel.Children.Clear();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    if (count > 0)
                        MainWindow.Instance.FadeOut_MiddlePopup($"{count}{(string)FindResource("msg_tidy")}", false);
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
                    if (count > 0) MainWindow.Instance.FadeOut_MiddlePopup($"{count}{(string)FindResource("msg_download_start")}");
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
                    if (count > 0) MainWindow.Instance.FadeOut_MiddlePopup($"{count}{(string)FindResource("msg_download_start")}");
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
