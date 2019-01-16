/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using Koromo_Copy.Component.DC;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Component.Manazero;
using Koromo_Copy.Component.Mangashow;
using Koromo_Copy.Component.Pinterest;
using Koromo_Copy.Component.Pixiv;
using Koromo_Copy.Net;
using Koromo_Copy.Net.Driver;
using Koromo_Copy.Plugin;
using Koromo_Copy.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Koromo_Copy_UX3.Domain
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
            else if (ScriptEngine.Instance.TestScript(url))
            {
                ProcessScript(url);
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

        public static string DeleteInvalid(string path)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
                path = path.Replace(c.ToString(), "");
            return path;
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

        public static void ProcessHiyobi(string url)
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
                    MainWindow.Instance.Fade_MiddlePopup(true, "접속중...");
                    var imagelink = HitomiParser.GetImageLink(NetCommon.DownloadString(HiyobiCommon.GetDownloadImageAddress(url.Split('/').Last())));
                    var article = HiyobiParser.ParseGalleryConents(NetCommon.DownloadString(url));
                    string dir = HitomiCommon.MakeDownloadDirectory(article);
                    article.ImagesLink = imagelink;
                    Directory.CreateDirectory(dir);
                    DownloadSpace.Instance.RequestDownload(article.Title,
                        imagelink.Select(y => HitomiCommon.GetDownloadImageAddress(article.Magic, y)).ToArray(),
                        imagelink.Select(y => Path.Combine(dir, y)).ToArray(),
                        Koromo_Copy.Interface.SemaphoreExtends.Default, dir, article);
                    Directory.CreateDirectory(dir);
                    MainWindow.Instance.FadeOut_MiddlePopup($"{imagelink.Count}개 이미지 다운로드 시작...");
                    //MainWindow.Instance.FadeOut_MiddlePopup("해당 hiyobi.me 주소는 다운로드를 지원하지 않아요");
                }
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

        public static async void ProcessPlugInAsync(DownloadPlugIn plugin, string url)
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

    }
}
