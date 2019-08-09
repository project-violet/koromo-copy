/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component;
using Koromo_Copy.Component.DC;
using Koromo_Copy.Component.EH;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Component.Manazero;
using Koromo_Copy.Component.Mangashow;
using Koromo_Copy.Component.Pinterest;
using Koromo_Copy.Component.Pixiv;
using Koromo_Copy.Net;
using Koromo_Copy.Net.Driver;
using Koromo_Copy.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Koromo_Copy_UX.Domain
{
    public class DownloaderHelper
    {
        public static async void LoadOthersAsync()
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

        static int unstable_request = 0;
        public static void ProcessUnstable(HArticleModel commander)
        {
            Interlocked.Increment(ref unstable_request);
            if (commander.ArticleType == HArticleType.Hiyobi)
            {
                ProcessHiyobi(commander.URL, true);
            }
            else if (commander.ArticleType == HArticleType.EHentai)
            {

            }
            else if (commander.ArticleType == HArticleType.EXHentai)
            {
                ProcessEXHentai(commander, true);
            }
        }

        public static void ProcessOthers(string url)
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
            else if (url.Contains("manazero009i.blogspot.com"))
            {
                ProcessManazero(url);
            }
            else if (url.Contains("hiyobi.me"))
            {
                ProcessHiyobi(url);
            }
            else if (url.Contains("e-hentai.org"))
            {

            }
            else if (url.Contains("exhentai.org"))
            {

            }
            //else if (url.Contains("mangashow.me"))
            //{
            //    ProcessMangashowme(url);
            //}
            //else if (ScriptEngine.Instance.TestScript(url))
            //{
            //    ProcessScript(url);
            //}
            else if (ScriptManager.Instance.SpecifyScript(url))
            {
                ProcessScriptAdvanced(url);
            }
        }

        public static string DeleteInvalid(string path)
        {
            var split = path.Split(new[] { '/', '\\' });
            var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var builder = new StringBuilder();
            foreach (var tkn in split)
            {
                var result = tkn;
                foreach (char c in invalid)
                    result = result.Replace(c.ToString(), "");
                builder.Append(result + "\\");
            }
            builder.Remove(builder.Length-1, 1);
            return builder.ToString();
        }

        public static async void ProcessPixivAsync(string url)
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

        public static void ProcessPinterest(string url)
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

        public static void ProcessDC(string url)
        {
            var article = DCParser.ParseBoardView(NetCommon.DownloadString(url));
            string dir = Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "dcinside"), $"[{article.GalleryName}] {DeleteInvalid(article.Title)}");
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

        public static void ProcessManazero(string url)
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

        public static void ProcessHiyobi(string url, bool unstable = false)
        {
            Task.Run(() =>
            {
                if (url.StartsWith("https://hiyobi.me/manga/info/"))
                {
                    MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                    var html = NetCommon.DownloadString(url);
                    var articles = HiyobiParser.ParseNonHArticles(html);
                    var title = HiyobiParser.ParseNonHTitle(html);

                    MainWindow.Instance.ModifyText_MiddlePopup($"가져오는중...[0/{articles.Count}]");
                    for (int i = 0; i < articles.Count; i++)
                    {
                        articles[i].ImagesLink = HitomiParser.GetImageLink(NetCommon.DownloadString(HiyobiCommon.GetDownloadMangaImageAddress(articles[i].Magic)));
                        MainWindow.Instance.ModifyText_MiddlePopup($"가져오는중...[{i + 1}/{articles.Count}]");
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
                            article.ImagesLink.Select(x => x.StartsWith("http://") || x.StartsWith("https://") ? x : $"https://aa.hiyobi.me/data_m/{article.Magic}/{x}").ToArray(),
                            article.ImagesLink.Select(x => Path.Combine(dir, !x.StartsWith("http://images-blogger-opensocial.googleusercontent.com/") ? 
                            HttpUtility.UrlDecode(HttpUtility.UrlDecode(x.Split('/').Last())) : 
                            HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpUtility.ParseQueryString(new Uri(x).Query).Get("url").Split('/').Last())))).ToArray(),
                            se,
                            dir + '\\',
                            null
                            );
                    }

                    MainWindow.Instance.FadeOut_MiddlePopup($"{count}개({articles.Count} 작품) 항목 다운로드 시작...");
                }
                else if (url.StartsWith("https://hiyobi.me/info/"))
                {
                    if (unstable) MainWindow.Instance.Fade_MiddlePopup(true, $"불안정한 작업 진행중...[{unstable_request}개]");
                    else MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                    var wc = NetCommon.GetDefaultClient();
                    wc.Headers.Add(System.Net.HttpRequestHeader.Referer, "https://xn--9w3b15m8vo.asia/reader/" + url.Split('/').Last());
                    var imagelink = HitomiParser.GetImageLink(wc.DownloadString(HiyobiCommon.GetDownloadImageAddress(url.Split('/').Last())));
                    var article = HitomiLegalize.MetadataToArticle(HitomiLegalize.GetMetadataFromMagic(url.Split('/').Last()).Value); //HiyobiParser.ParseGalleryConents(NetCommon.DownloadString(url));
                    string dir = HitomiCommon.MakeDownloadDirectory(article);
                    var se = Koromo_Copy.Interface.SemaphoreExtends.Default;
                    se.Referer = "https://xn--9w3b15m8vo.asia/reader/" + url.Split('/').Last();
                    article.ImagesLink = imagelink;
                    Directory.CreateDirectory(dir);
                    DownloadSpace.Instance.RequestDownload(article.Title,
                        imagelink.Select(y => $"https://xn--9w3b15m8vo.asia/data/{article.Magic}/{y}").ToArray(),
                        imagelink.Select(y => Path.Combine(dir, y)).ToArray(),
                        se, dir, article);
                    Directory.CreateDirectory(dir);
                    if (unstable) Interlocked.Decrement(ref unstable_request);
                    if (unstable && unstable_request != 0) MainWindow.Instance.Fade_MiddlePopup(true, $"불안정한 작업 진행중...[{unstable_request}개]");
                    else MainWindow.Instance.FadeOut_MiddlePopup($"{imagelink.Count}개 이미지 다운로드 시작...");
                }
            });
        }

        public static void ProcessEXHentai(HArticleModel commander, bool unstable = false)
        {
            Task.Run(() =>
            {
                if (unstable) MainWindow.Instance.Fade_MiddlePopup(true, $"불안정한 작업 진행중...[{unstable_request}개]");
                var pages = ExHentaiParser.GetPagesUri(NetCommon.DownloadExHentaiString(commander.URL));
                var pages_html = EmiliaJobEXH.Instance.AddJob(pages.ToList(), x => { }, Enumerable.Range(0,pages.Length).OfType<object>().ToList()).Select(x => new Tuple<object, string[]> (x.Item1, ExHentaiParser.GetImagesUri(x.Item2))).ToList();
                pages_html.Sort((x, y) => ((int)x.Item1).CompareTo((int)y.Item1));
                List<string> pages_all = new List<string>();
                pages_html.ToList().ForEach(x => pages_all.AddRange(x.Item2));
                var imagelink = EmiliaJobEXH.Instance.AddJob(pages_all, x => { }, Enumerable.Range(0, pages_all.Count).OfType<object>().ToList()).Select(x => new Tuple<object, string>(x.Item1, ExHentaiParser.GetImagesAddress(x.Item2))).ToList();
                List<string> tags = new List<string>();
                if (commander.male != null) tags.AddRange(commander.male.Select(x => "male:" + x.Replace(' ', '_')));
                if (commander.female != null) tags.AddRange(commander.female.Select(x => "female:" + x.Replace(' ', '_')));
                if (commander.misc != null) tags.AddRange(commander.misc.Select(x => x.Replace(' ', '_')));
                HitomiArticle article = new HitomiArticle
                {
                    Magic = commander.Magic,
                    Title = commander.Title,
                    Artists = commander.artist,
                    Groups = commander.group,
                    Series = commander.parody,
                    Tags = tags.ToArray(),
                    Language = commander.language != null ? commander.language[0] : "",
                    Characters = commander.character
                };
                string dir = HitomiCommon.MakeDownloadDirectory(article);
                imagelink.Sort((x, y) => ((int)x.Item1).CompareTo((int)y.Item1));
                article.ImagesLink = imagelink.Select(x => x.Item2).ToList();
                Directory.CreateDirectory(dir);
                var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
                se.Cookie = "igneous=30e0c0a66;ipb_member_id=2742770;ipb_pass_hash=6042be35e994fed920ee7dd11180b65f;";
                DownloadSpace.Instance.RequestDownload(article.Title,
                    imagelink.Select(x => x.Item2).ToArray(),
                    imagelink.Select(y => Path.Combine(dir, y.Item2.Split('/').Last())).ToArray(),
                    se, dir, article);
                if (unstable) Interlocked.Decrement(ref unstable_request);
                if (unstable && unstable_request != 0) MainWindow.Instance.Fade_MiddlePopup(true, $"불안정한 작업 진행중...[{unstable_request}개]");
                else MainWindow.Instance.FadeOut_MiddlePopup($"{imagelink.Count()}개 이미지 다운로드 시작...");
            });
        }

        public static void ProcessMangashowme(string url)
        {
            Task.Run(() =>
            {
                MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                var html = NetCommon.DownloadString(url);
                var articles = MangashowmeParser.ParseSeries(html);
                var title = MangashowmeParser.ParseTitle(html);

                MainWindow.Instance.ModifyText_MiddlePopup($"가져오는중...[0/{articles.Count}]");
                for (int i = 0; i < articles.Count; i++)
                {
                    articles[i].ImagesLink = MangashowmeParser.ParseImages(NetCommon.DownloadString(MangashowmeCommon.GetDownloadMangaImageAddress(articles[i].ArticleLink)));
                    MainWindow.Instance.ModifyText_MiddlePopup($"가져오는중...[{i + 1}/{articles.Count}]");
                }

                int count = 0;
                foreach (var article in articles)
                {
                    string dir = Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "mangashowme"), DeleteInvalid(title)), DeleteInvalid(article.Title));
                    Directory.CreateDirectory(dir);

                    var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
                    se.Referer = url;

                    count += article.ImagesLink.Count;
                    DownloadSpace.Instance.RequestDownload($"mangashow-me: {article.Title}",
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

        public static void ProcessScript(string url)
        {
            Task.Run(() =>
            {
                try
                {
                    var model = ScriptEngine.Instance.FindModel(url);
                    MainWindow.Instance.FadeOut_MiddlePopup($"스크립트: {model.ScriptName} ({model.ScriptVersion})\r\n작성자: {model.ScriptAuthor}");
                    System.Threading.Thread.Sleep(3000);

                    MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                    var result = ScriptEngine.Instance.Run(url, (x) => MainWindow.Instance.ModifyText_MiddlePopup(x));

                    int count = 0;
                    foreach (var article in result.Item2)
                    {
                        string dir = Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), model.ScriptFolderName), DeleteInvalid(result.Item1)), DeleteInvalid(article.Title));
                        Directory.CreateDirectory(dir);

                        var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
                        se.Referer = article.URL;

                        count += article.Images.Count;
                        DownloadSpace.Instance.RequestDownload($"{model.ScriptRequestName}: {article.Title}",
                            article.Images.ToArray(),
                            article.Images.Select(x => Path.Combine(dir, HttpUtility.UrlDecode(HttpUtility.UrlDecode(x.Split('/').Last())))).ToArray(),
                            se,
                            dir + '\\',
                            null
                            );
                    }
                    MainWindow.Instance.FadeOut_MiddlePopup($"{count}개({result.Item2.Count} 작품) 항목 다운로드 시작...");
                }
                catch (Exception e)
                {
                    MessageBox.Show("스크립트 실행 중 오류가 발생했습니다.\r\n" + e.Message + "\r\n" + e.StackTrace, 
                        "Script Engine", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public static void ProcessScriptAdvanced(string url)
        {
            Task.Run(() =>
            {
                var script = ScriptManager.Instance.GetScript(url);
                MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                var err = script.Run(url, (x, y) =>
                {
                    string dir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), x.ScriptFolderName);

                    var dic = new Dictionary<string, List<Tuple<string,string>>>();
                    
                    foreach (var ddd in y)
                    {
                        var tidy = DeleteInvalid(ddd.Item2);
                        var sub_dir = Path.GetDirectoryName(Path.Combine(dir, tidy));
                        if (dic.ContainsKey(sub_dir))
                            dic[sub_dir].Add(Tuple.Create(ddd.Item1, tidy));
                        else
                            dic.Add(sub_dir, new List<Tuple<string, string>>() { Tuple.Create(ddd.Item1, tidy) });
                    }

                    foreach (var list in dic)
                    {
                        Directory.CreateDirectory(list.Key);
                        var se = Koromo_Copy.Interface.SemaphoreExtends.MakeDefault();
                        se.Referer = url;

                        DownloadSpace.Instance.RequestDownload($"{x.ScriptRequestName}: {url}",
                            list.Value.Select(z => z.Item1).ToArray(),
                            list.Value.Select(z => z.Item2).Select(k => Path.Combine(dir, k)).ToArray(),
                            se, list.Key + "\\", null);
                    }

                    MainWindow.Instance.FadeOut_MiddlePopup($"{y.Count}개 항목 다운로드 시작...");
                });
                if (err)
                {
                    Koromo_Copy.Monitor.Instance.Push($"[Script Runner] '{script.Attribute().ScriptName}' 스크립트 실행중 오류가 발생했습니다.");
                    MainWindow.Instance.FadeOut_MiddlePopup($"'{script.Attribute().ScriptName}' 스크립트 실행 중 오류가 발생했습니다.");
                }
            });
        }
    }
}
