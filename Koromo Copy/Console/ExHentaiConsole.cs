/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.EH;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Fs;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 익헨 콘솔 옵션입니다.
    /// </summary>
    public class ExHentaiConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-article", CommandType.ARGUMENTS, Help = "use -article <Exhentai address>",
            Info = "Download article page and parse html.")]
        public string[] Article;

        [CommandLine("-addr", CommandType.ARGUMENTS, Help = "use -addr <Hitomi Article>", Pipe = true,
            Info = "Get ex-hentai address using hitomi article.")]
        public string[] Address;

        [CommandLine("-paging", CommandType.ARGUMENTS, Help = "use -paging page")]
        public string[] Paging;
        [CommandLine("-expun", CommandType.ARGUMENTS, Help = "use -expun xxx")]
        public string[] Expun;
        [CommandLine("-hashing", CommandType.OPTION, Help = "use -hashing")]
        public bool Hashing;
        [CommandLine("-xxx", CommandType.OPTION, Help = "?")]
        public bool XXX;
        [CommandLine("-extract", CommandType.OPTION, Help = "?")]
        public bool Extract;
        [CommandLine("-extract2", CommandType.OPTION, Help = "?")]
        public bool Extract2;
        [CommandLine("-extract3", CommandType.OPTION, Help = "?")]
        public bool Extract3;
        [CommandLine("-extract4", CommandType.OPTION, Help = "?")]
        public bool Extract4;
    }

    /// <summary>
    /// 코로모 카피에 구현된 모든 익헨 도구를 사용할 수 있는 콘솔 명령 집합입니다.
    /// </summary>
    public class ExHentaiConsole : IConsole
    {
        /// <summary>
        /// 익헨 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            ExHentaiConsoleOption option = CommandLineParser<ExHentaiConsoleOption>.Parse(arguments, contents != "", contents);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.Instance.WriteLine(option.HelpMessage);
                return false;
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Article != null)
            {
                ProcessArticle(option.Article);
            }
            else if (option.Address != null)
            {
                ProcessAddress(option.Address);
            }
            else if (option.Paging != null)
            {
                ProcessPaging(option.Paging);
            }
            else if (option.Expun != null)
            {
                ProcessExpun(option.Expun);
            }
            else if (option.Hashing)
            {
                ProcessHashing();
            }
            else if (option.XXX)
            {
                ProcessXXX();
            }
            else if (option.Extract)
            {
                ProcessExtract();
            }
            else if (option.Extract2)
            {
                ProcessExtract2();
            }
            else if (option.Extract3)
            {
                ProcessExtract3();
            }
            else if (option.Extract4)
            {
                ProcessExtract4();
            }

            return true;
        }

        bool IConsole.Redirect(string[] arguments, string contents)
        {
            return Redirect(arguments, contents);
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "Ex-Hentai Console Core\r\n" +
                "\r\n" +
                " -article <Exhentai address> : Show article info.\r\n" +
                " -addr <Hitomi Article> : Get ExHentai Address from HitomiArticle. This option can using pipe."
                );
        }

        /// <summary>
        /// 아티클 정보를 다운로드하고 정보를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessArticle(string[] args)
        {
            string html_source = NetCommon.DownloadExHentaiString(args[0]);
            EHentaiArticle article = ExHentaiParser.ParseArticleData(html_source);
            Console.Instance.WriteLine(article);
        }

        /// <summary>
        /// 히토미아티클을 이용해 익헨 주소를 가져옵니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessAddress(string[] args)
        {
            HitomiArticle article = null;
            try { article = JsonConvert.DeserializeObject<HitomiArticle>(args[0]); } catch { }
            if (article == null)
            {
                Console.Instance.WriteErrorLine("Not valid 'HitomiArticle' objects. Check your pipe or arguments.");
                return;
            }

            string result = ExHentaiTool.GetAddressFromMagicTitle(article.Magic, article.Title);
            if (result != "")
                Console.Instance.WriteLine(result);
            else
                Console.Instance.WriteLine("Not found.");
        }

        /// <summary>
        /// 페이지 주소 정보를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessPaging(string[] args)
        {
            //string url = "https://exhentai.org/?inline_set=dm_l&page=" + args[0];
            //string url2 = "https://exhentai.org/?page=1&f_doujinshi=on&f_manga=on&f_artistcg=on&f_gamecg=on&f_search=language%3Akorean&f_apply=Apply+Filter&inline_set=dm_l";
            
            var result = new List<EHentaiResultArticle>();
            //var urlx = $"https://exhentai.org/?page=0&f_doujinshi=on&f_manga=on&f_artistcg=on&f_gamecg=on&&f_cats=0&f_sname=on&f_stags=on&f_sh=on&advsearch=1&f_srdd=2&f_sname=on&f_stags=on&f_sdesc=on&f_sh=on&inline_set=dm_e";
            //NetCommon.DownloadExHentaiString(urlx);

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    //var url = $"https://exhentai.org/?page={i}&f_doujinshi=on&f_manga=on&f_artistcg=on&f_gamecg=on&&f_cats=0&f_search=lang:korean&f_sname=on&f_stags=on&f_sh=on&advsearch=1&f_srdd=2&f_sname=on&f_stags=on&f_sdesc=on&f_sh=on&inline_set=dm_e";
                    //var url = $"https://exhentai.org/?page={i}&f_doujinshi=on&f_manga=on&f_artistcg=on&f_gamecg=on&f_search=language%3Akorean&f_apply=Apply+Filter&inline_set=dm_e";
                    //var url2 = $"https://exhentai.org/?page={i}&f_doujinshi=on&f_manga=on&f_artistcg=on&f_gamecg=on&advsearch=1&f_search=language%3Akorean&f_srdd=2&f_sname=on&f_stags=on&f_sdesc=on&f_sh=on&f_apply=Apply+Filter";
                    //           https://exhentai.org/?page=1&f_doujinshi=on&f_manga=on&f_artistcg=on&f_gamecg=on&advsearch=1&f_srdd=2&f_sname=on&f_stags=on&f_sh=on&f_apply=Apply+Filter
                    var url3 = $"https://exhentai.org/?page={i}&f_doujinshi=on&f_manga=on&f_artistcg=on&f_gamecg=on&&f_cats=0&f_sname=on&f_stags=on&f_sh=on&advsearch=1&f_srdd=2&f_sname=on&f_stags=on&f_sdesc=on&f_sh=on";
                    var html = NetCommon.DownloadExHentaiString(url3);
                    //result.AddRange(ExHentaiParser.ParseResultPageExtendedListView(html));
                    File.WriteAllText($"exhentai-page/exhentai-{i}.html", html);
                    Monitor.Instance.Push($"[Paging] {i+1}/1457");
                }
                catch (Exception e)
                {
                    Console.Instance.WriteErrorLine($"[Error] {i} {e.Message}");
                }
                Thread.Sleep(100);
            }
            
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream("exh.json", FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }

        static void ProcessHashing()
        {
            var result = new List<string>();

            for (int i = 2955; i < 13000; i++)
            {
                try
                {
                    var url = $"https://exhentai.org/?page={i}&f_sname=on&f_stags=on&f_sh=on&advsearch=1";
                    var html = NetCommon.DownloadExHentaiString(url);

                    var regex = new Regex(@"https://exhentai.org/g/\d+/\w+/");
                    var match = regex.Matches(html);

                    foreach (var m in match)
                    {
                        result.Add(((Match)m).Value);
                        Console.Instance.WriteLine(((Match)m).Value);
                    }
                }
                catch (Exception e)
                {
                    Console.Instance.WriteErrorLine($"[Error] {i} {e.Message}");
                }
            }

            var builder = new StringBuilder();
            result.ForEach(x => builder.Append(x + "\r\n"));
            File.WriteAllText("rr.txt", builder.ToString());
        }

        /// <summary>
        /// 익펀 데이터를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessExpun(string[] args)
        {
            var non_ef = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("exh.json"));
            var inc_ef = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("exh-ef.json"));

            HashSet<string> non_ef_urls = new HashSet<string>();
            non_ef.ForEach(x => non_ef_urls.Add(x.URL));

            HashSet<string> inc_ef_urls = new HashSet<string>();
            inc_ef.ForEach(x => inc_ef_urls.Add(x.URL));

            var build = new StringBuilder();
            build.Append("\r\n");
            build.Append("Koromo Copy Expunged Data Extractor\r\n");
            build.Append("Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.\r\n");
            build.Append("\r\n");
            build.Append("손실 작품\r\n");
            int ee_count = 0;
            non_ef.ForEach(x =>
            {
                if (!inc_ef_urls.Contains(x.URL))
                {
                    build.Append($"{x.URL.Substring("https://exhentai.org/g".Length)} : {x.Title} \r\n");
                    ee_count++;
                }
            });
            build.Append($"{(ee_count > 0 ? ee_count.ToString("#,#") : "0")} 개");
            build.Append("\r\n");
            build.Append("Expunged 데이터\r\n");
            int ef_count = 0;
            inc_ef.ForEach(x =>
            {
                if (!non_ef_urls.Contains(x.URL))
                {
                    build.Append($"{x.URL.Substring("https://exhentai.org/g".Length)} : {x.Title} \r\n");
                    ef_count++;
                }
            });
            build.Append($"{(ef_count > 0 ? ef_count.ToString("#,#") : "0")} 개");
            Console.Instance.WriteLine(build.ToString());
        }

        static void ProcessXXX()
        {
            var inc_ef = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("exh-ef.json"));

            var builder = new StringBuilder();

            Random rnd = new Random();
            inc_ef = inc_ef.OrderBy(x => rnd.Next()).ToList();

            int ppp = 0;
            foreach (var per in inc_ef)
            {
                var url = per.URL;
                var xi = Convert.ToInt64( url.Split('/')[4]);
                var yi = Convert.ToInt64(url.Split('/')[5],16);
                builder.Append(((xi << 40) + yi).ToString() + ", ");
                ppp++;
                if (ppp % 10 == 9)
                    builder.Append("\r\n        ");
            }
            Console.Instance.WriteLine(builder.ToString());
        }

        static void ProcessExtract()
        {
            var xxx = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("ex-hentai-archive.json"));

            const string archive = @"E:\2019\e-archive";
            var ix = new FileIndexor();
            Task.Run(async () => await ix.ListingDirectoryAsync(archive)).Wait();

            var htmls = new List<string>();
            ix.Enumerate((string path, List<FileInfo> files) =>
            {
                files.ForEach(x => htmls.Add(x.FullName));
            });

            var result = new List<EHentaiResultArticle>();

            using (var progressBar = new Console.ConsoleProgressBar())
            {
                int x = 0;
                foreach (var html in htmls)
                {
                    var content = File.ReadAllText(html);
                    try
                    {
                        var exh = ExHentaiParser.ParseResultPageExtendedListView(content);
                        //Console.Instance.WriteLine("[GET] " + exh.Count + " Articles! - " + html);
                        result.AddRange(exh);
                        if (exh.Count != 25)
                        {
                            Console.Instance.WriteLine("[Miss] " + html);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Instance.WriteLine("[Fail] " + html);
                    }
                    x++;
                    progressBar.SetProgress(x / (float)htmls.Count * 100);
                }
            }


            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push("Write file: ex-hentai-archive.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ex-hentai-archive.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, result);
            }
        }

        static void ProcessExtract2()
        {
            const string target = @"e-parse-fail.txt";
            var lines = File.ReadAllLines(target);

            var result = new List<EHentaiResultArticle>();

            using (var progressBar = new Console.ConsoleProgressBar())
            {
                int x = 0;
                foreach (var line in lines)
                {
                    var fn = line.Replace("[Fail] ", "");
                    var content = File.ReadAllText(fn);

                    try
                    {
                        var exh = ExHentaiParser.ParseResultPageMinimalListView(content);
                        Console.Instance.WriteLine("[GET] " + exh.Count + " Articles! - " + fn);
                        result.AddRange(exh);
                    }
                    catch (Exception e)
                    {
                        Console.Instance.WriteLine("[Fail] " + fn);
                    }

                    x++;
                    progressBar.SetProgress(x / (float)lines.Length * 100);
                }
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push("Write file: ex-hentai-archive2.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ex-hentai-archive2.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, result);
            }
        }

        static void ProcessExtract3()
        {
            var xxx = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("ex-hentai-archive.json"));
            //var yyy = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("ex-hentai-archive2.json"));
            var zzz = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("ex-hentai-archive2.json"));

            //xxx.AddRange(yyy);
            //xxx.AddRange(zzz);

            var exists = new HashSet<int>();
            xxx.ForEach(x => exists.Add(x.URL.Split('/')[4].ToInt32()));

            foreach (var z in zzz)
            {
                var nn = z.URL.Split('/')[4].ToInt32();

                if (exists.Contains(nn))
                    Console.Instance.WriteLine("[Duplicate] " + nn);
                else
                    xxx.Add(z);
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push("Write file: ex-hentai-archive3.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ex-hentai-archive3.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, xxx);
            }
        }

        static void ProcessExtract4()
        {
            const string archive = @"C:\Tools\koromo-copy\Koromo Copy UX\bin\Debug\exhentai-page";
            var htmls = new List<string>();

            foreach (var file in Directory.GetFiles(archive))
                htmls.Add(File.ReadAllText(file));

            var result = new List<EHentaiResultArticle>();

            using (var progressBar = new Console.ConsoleProgressBar())
            {
                int x = 0;
                foreach (var html in htmls)
                {
                    var content = html;
                    try
                    {
                        var exh = ExHentaiParser.ParseResultPageExtendedListView(content);
                        //Console.Instance.WriteLine("[GET] " + exh.Count + " Articles! - " + html);
                        result.AddRange(exh);
                        if (exh.Count != 25)
                        {
                            Console.Instance.WriteLine("[Miss] " + html);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Instance.WriteLine("[Fail] " + html);
                    }
                    x++;
                    progressBar.SetProgress(x / (float)htmls.Count * 100);
                }
            }


            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            Monitor.Instance.Push("Write file: ex-hentai-archive.json");
            using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ex-hentai-archive2.json")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, result);
            }
        }

    }
}
