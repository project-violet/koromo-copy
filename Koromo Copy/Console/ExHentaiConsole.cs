/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.EH;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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

        [CommandLine("-paging", CommandType.ARGUMENTS, Help = "use -paging page", Pipe = true,
            Info = "Get ex-hentai address using hitomi article.")]
        public string[] Paging;
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

            for (int i = 0; i < 1398; i++)
            {
                try
                {
                    var url = $"https://exhentai.org/?page={i}&f_doujinshi=on&f_manga=on&f_artistcg=on&f_gamecg=on&f_search=language%3Akorean&f_apply=Apply+Filter&inline_set=dm_l";
                    var html = NetCommon.DownloadExHentaiString(url);
                    result.AddRange(ExHentaiParser.ParseResultPageListView(html));
                    Monitor.Instance.Push($"[Paging] {i+1}/1398");
                }
                catch (Exception e)
                {
                    Console.Instance.WriteErrorLine($"[Error] {i} {e.Message}");
                }
                Thread.Sleep(5000);
            }
            
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream("exh.json", FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }
    }
}
