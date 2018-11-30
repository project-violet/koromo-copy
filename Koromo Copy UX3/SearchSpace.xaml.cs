/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.DC;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Component.Manazero;
using Koromo_Copy.Component.Pinterest;
using Koromo_Copy.Component.Pixiv;
using Koromo_Copy.Net;
using Koromo_Copy.Net.Driver;
using Koromo_Copy.Plugin;
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

            Loaded += SearchSpace_Loaded;
            Instance = this;
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
                sb.Completed += Sb_Completed;
                if (sb != null) { BeginStoryboard(sb); }
                RecommendSpace.Instance.Update();
                Task.Run(() => CheckUpdate());
                Task.Run(() => LoadOthersAsync());
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
                ProcessOthers(content.Trim());
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
                        Koromo_Copy.Interface.SemaphoreExtends.Default, prefix, x.Article);
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

        #region Other Downloader

        private async void LoadOthersAsync()
        {
            if (!string.IsNullOrEmpty(Settings.Instance.Pixiv.Id) && !string.IsNullOrEmpty(Settings.Instance.Pixiv.Password))
            {
                try
                {
                    await PixivTool.Instance.Login(Settings.Instance.Pixiv.Id, Settings.Instance.Pixiv.Password);
                    Koromo_Copy.Monitor.Instance.Push($"[Pixiv Login] Access Token: {PixivTool.Instance.GetAccessToken()}");
                }
                catch
                {
                    MainWindow.Instance.FadeOut_MiddlePopup("Pixiv 로그인 오류!", false);
                }
            }
        }

        private void ProcessOthers(string url)
        {
            if (url.Contains("pixiv.net"))
            {
                ProcessPixivAsync(url);
            }
            else if (url.Contains("pinterest.co.kr"))
            {
                ProcessPinterest(url);
            }
            else if (url.Contains("gall.dcinside.com"))
            {
                ProcessDC(url);
            }
            else if (url.Contains("manazero008h.blogspot.com"))
            {
                ProcessManazero(url);
            }
            else if (url.Contains("hiyobi.me"))
            {
                ProcessHiyobi(url);
            }
            else
            {
                // Plugins
                foreach (var plugin in PlugInManager.Instance.GetDownloadPlugins())
                {
                    if (plugin.SpecifyUrl(url))
                    {
                        ProcessPlugInAsync(plugin, url);
                        break;
                    }
                }
            }
        }
        
        private string DeleteInvalid(string path)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
                path = path.Replace(c.ToString(), "");
            return path;
        }

        private async void ProcessPixivAsync(string url)
        {
            if (!PixivTool.Instance.IsLogin)
            {
                MainWindow.Instance.FadeOut_MiddlePopup("로그인이 필요합니다", false);
                return;
            }

            string id = Regex.Split(Regex.Split(url, @"\?id\=")[1], @"\&")[0];

            try
            {
                string name = await PixivTool.Instance.GetUserAsync(id);
                string dir = Path.Combine(Settings.Instance.Pixiv.Path, DeleteInvalid(name));
                Directory.CreateDirectory(dir);

                var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
                se.Referer = "https://www.pixiv.net/member_illust.php?";

                var list = await PixivTool.Instance.GetDownloadUrlsAsync(id);
                DownloadSpace.Instance.RequestDownload(name,
                    list.ToArray(),
                    list.Select(x => Path.Combine(dir, x.Split('/').Last())).ToArray(),
                    se,
                    dir + '\\',
                    null
                    );
                MainWindow.Instance.FadeOut_MiddlePopup($"{list.Count}개 항목 다운로드 시작...");
            }
            catch (Exception e)
            {
                MainWindow.Instance.FadeOut_MiddlePopup("오류가 발생했습니다", false);
                Koromo_Copy.Monitor.Instance.Push("[Pixiv Error] " + e.Message + "\r\n" + e.StackTrace);
            }
        }

        private void ProcessPinterest(string url)
        {
            Task.Run(async () =>
            {
                string user = Regex.Match(url, "pinterest.co.kr/(.*?)/").Groups[1].Value;
                var sw = new SeleniumWrapper();

                MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                sw.Navigate(url);
                sw.WaitComplete();
                sw.ClickXPath("//div[@data-test-id='loginButton']");
                MainWindow.Instance.ModifyText_MiddlePopup("로그인중...");
                sw.SendKeyId("email", Settings.Instance.Pinterest.Id);
                sw.SendKeyId("password", Settings.Instance.Pinterest.Password);
                sw.ClickXPath("//div[@data-test-id='registerFormSubmitButton']");
                await Task.Delay(10000);
                sw.WaitComplete();
                sw.Navigate($"https://www.pinterest.co.kr/{user}/pins/");
                await Task.Delay(5000);
                sw.WaitComplete();

                List<string> images = new List<string>();
                string last;
                do
                {
                    last = sw.GetHeight();
                    sw.ScrollDown();
                    await Task.Delay(2000);

                    foreach (var image in PinParser.ParseImage(sw.GetHtml()))
                        if (!images.Contains(image))
                        {
                            images.Add(image);
                            MainWindow.Instance.ModifyText_MiddlePopup($"가져오는중... [{images.Count}]");
                        }
                } while (last != sw.GetHeight());

                int height = Convert.ToInt32(sw.GetHeight());
                for (int i = 0; i < height; i += 50)
                {
                    sw.Scroll(i);
                    await Task.Delay(1);
                    foreach (var image in PinParser.ParseImage(sw.GetHtml()))
                        if (!images.Contains(image))
                            if (!images.Contains(image))
                            {
                                images.Add(image);
                                MainWindow.Instance.ModifyText_MiddlePopup($"재 수집중... [{images.Count}]");
                            }
                }

                sw.Close();

                string dir = Path.Combine(Settings.Instance.Pinterest.Path, DeleteInvalid(user));
                Directory.CreateDirectory(dir);

                var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
                se.Referer = "https://www.pinterest.co.kr/";

                DownloadSpace.Instance.RequestDownload($"pinterest: {user}",
                    images.ToArray(),
                    images.Select(x => Path.Combine(dir, x.Split('/').Last())).ToArray(),
                    se,
                    dir + '\\',
                    null
                    );
                MainWindow.Instance.FadeOut_MiddlePopup($"{images.Count}개 항목 다운로드 시작...");
            });
        }

        private void ProcessDC(string url)
        {
            var article = DCParser.ParseBoardView(NetCommon.DownloadString(url));
            string dir = Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"dcinside"), $"[{article.GalleryName}] {DeleteInvalid(article.Title)}");
            Directory.CreateDirectory(dir);

            var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
            se.Referer = url;
            
            DownloadSpace.Instance.RequestDownload(article.Title,
                article.ImagesLink.ToArray(),
                article.FilesName.Select(x => Path.Combine(dir, DeleteInvalid(x))).ToArray(),
                se,
                dir + '\\',
                null
                );
            MainWindow.Instance.FadeOut_MiddlePopup($"{article.ImagesLink.Count}개 항목 다운로드 시작...");
        }

        private void ProcessManazero(string url)
        {
            Task.Run(() =>
            {
                var sw = new SeleniumWrapper();

                MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                sw.Navigate(url);
                try { sw.ClickXPath("//a[@class='maia-button maia-button-primary']"); } catch { }

                var title = ManazeroParser.ParseTitle(sw.GetHtml());
                var articles = ManazeroParser.ParseArticles(sw.GetHtml());
                MainWindow.Instance.ModifyText_MiddlePopup($"가져오는중...[0/{articles.Count}]");

                for (int i = 0; i < articles.Count; i++)
                {
                    sw.Navigate(articles[i].ArticleLink);
                    if (i == 0)
                    {
                        try { sw.ClickXPath("//a[@class='maia-button maia-button-primary']"); } catch { }
                    }
                    articles[i].ImagesLink = ManazeroParser.ParseImages(sw.GetHtml());
                    MainWindow.Instance.ModifyText_MiddlePopup($"가져오는중...[{i + 1}/{articles.Count}]");
                }
                sw.Close();

                int count = 0;
                foreach (var article in articles)
                {
                    string dir = Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "manazero"), DeleteInvalid(title)), DeleteInvalid(article.Title));
                    Directory.CreateDirectory(dir);

                    var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
                    se.Referer = url;
                    
                    count += article.ImagesLink.Count;
                    DownloadSpace.Instance.RequestDownload($"manazero: {article.Title}",
                        article.ImagesLink.ToArray(),
                        article.ImagesLink.Select(x => Path.Combine(dir, HttpUtility.UrlDecode(HttpUtility.UrlDecode(x.Split('/').Last())))).ToArray(),
                        se,
                        dir + '\\',
                        null
                        );
                }

                MainWindow.Instance.FadeOut_MiddlePopup($"{count}개({articles.Count} 작품) 항목 다운로드 시작...");
            });
        }

        private void ProcessHiyobi(string url)
        {
            Task.Run(() =>
            {
                if (url.StartsWith("https://hiyobi.me/manga/info/"))
                {
                    MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                    var html = NetCommon.DownloadString(url);
                    var articles = HiyobiParser.ParseNonHArticles(html);
                    var title = HiyobiParser.ParseNonHTitle(html);

                    MainWindow.Instance.Fade_MiddlePopup(true, $"가져오는중...[0/{articles.Count}]");
                    for (int i = 0; i < articles.Count; i++)
                    {
                        articles[i].ImagesLink = HiyobiParser.ParseNonHImageList(NetCommon.DownloadString(HiyobiCommon.GetDownloadMangaImageAddress(articles[i].Magic)));
                        MainWindow.Instance.Fade_MiddlePopup(true, $"가져오는중...[{i+1}/{articles.Count}]");
                    }

                    int count = 0;
                    foreach (var article in articles)
                    {
                        string dir = Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "hiyobi"), DeleteInvalid(title)), DeleteInvalid(article.Title));
                        Directory.CreateDirectory(dir);

                        var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
                        se.Referer = url;

                        count += article.ImagesLink.Count;
                        DownloadSpace.Instance.RequestDownload($"hiyobi-nonh: {article.Title}",
                            article.ImagesLink.ToArray(),
                            article.ImagesLink.Select(x => Path.Combine(dir, HttpUtility.UrlDecode(HttpUtility.UrlDecode(x.Split('/').Last())))).ToArray(),
                            se,
                            dir + '\\',
                            null
                            );
                    }

                    MainWindow.Instance.FadeOut_MiddlePopup($"{count}개({articles.Count} 작품) 항목 다운로드 시작...");
                }
                else
                {
                    MainWindow.Instance.FadeOut_MiddlePopup("해당 hiyobi.me 주소는 다운로드를 지원하지 않아요");
                }
            });
        }

        private async void ProcessPlugInAsync(DownloadPlugIn plugin, string url)
        {
            try
            {
                var article = await Task.Run(() => plugin.GetImageLink(url));
                DownloadSpace.Instance.RequestDownload(article.Title,
                    article.ImagesLink.ToArray(),
                    plugin.GetDownloadPaths(),
                    plugin.GetSemaphoreExtends(),
                    plugin.GetFolderName() + '\\',
                    null
                    );
                MainWindow.Instance.FadeOut_MiddlePopup($"{article.ImagesLink.Count}개 항목 다운로드 시작...");
            }
            catch (Exception e)
            {
                MainWindow.Instance.FadeOut_MiddlePopup("오류가 발생했습니다", false);
                Koromo_Copy.Monitor.Instance.Push($"[{plugin.Name} {plugin.Version} Error] " + e.Message + "\r\n" + e.StackTrace);
            }
        }

        #endregion
    }
}
