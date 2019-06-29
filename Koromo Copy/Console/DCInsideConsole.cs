/***

   Copyright (C) 2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.DC;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Console
{
    public class DCInsideConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-find-gallery", CommandType.ARGUMENTS, Help = "use -find-gallery <Gallery Name>",
            Info = "Search gallery by name.")]
        public string[] FindGallery;

        [CommandLine("-parse-gallery", CommandType.ARGUMENTS, ArgumentsCount = 3, Help = "use -parse-gallery <Gallery Id> <Start Page> <End Page>",
            Info = "Parse gallery pages.")]
        public string[] ParseGallery;
        [CommandLine("--filter-rem", CommandType.OPTION, Help = "use --filter-rem",
            Info = "Filtering recommendation article list.")]
        public bool FilterRecommend;
        [CommandLine("--filter-title", CommandType.ARGUMENTS, Help = "use --filter-title <Title>",
            Info = "Filtering by title.")]
        public string[] FilterTitle;
        [CommandLine("--filter-nick", CommandType.ARGUMENTS, ArgumentsCount = 1, Help = "use --filter-nick <Nickname>",
            Info = "Filtering by nickname.")]
        public string[] FilterNick;
        [CommandLine("--filter-ip", CommandType.ARGUMENTS, ArgumentsCount = 1, Help = "use --filter-ip <Ip>",
            Info = "Filtering by ip-address.")]
        public string[] FilterIp;
        [CommandLine("--filter-id", CommandType.ARGUMENTS, ArgumentsCount = 1, Help = "use --filter-id <Id>",
            Info = "Filtering by user id.")]
        public string[] FilterId;
        [CommandLine("--filter-login", CommandType.OPTION, Help = "use --filter-login",
            Info = "Filtering by logined user.")]
        public bool FilterLogined;
        [CommandLine("--filter-fix", CommandType.OPTION, Help = "use --filter-fix",
            Info = "Filtering by fixed logined user.")]
        public bool FilterFixed;

        [CommandLine("-collect-articles", CommandType.ARGUMENTS, ArgumentsCount = 3, Help = "use -collect-articles <Gallery Id> <Start Page> <End Page>",
            Info = "Parse gallery pages.")]
        public string[] CollectArticles;

        [CommandLine("-test", CommandType.ARGUMENTS, Help = "use -test <Number>",
            Info = "Test dcinside methods.")]
        public string[] Test;
    }
    
    public class DCInsideConsole : IConsole
    {
        static bool Redirect(string[] arguments, string contents)
        {
            DCInsideConsoleOption option = CommandLineParser<DCInsideConsoleOption>.Parse(arguments, contents != "", contents);

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
            else if (option.FindGallery != null)
            {
                ProcessFindGallery(option.FindGallery);
            }
            else if (option.ParseGallery != null)
            {
                ProcessParseGallery(option.ParseGallery, option.FilterRecommend, option.FilterTitle,
                    option.FilterNick, option.FilterIp, option.FilterId, option.FilterLogined, option.FilterFixed);
            }
            else if (option.CollectArticles != null)
            {
                ProcessCollectArticles(option.CollectArticles, option.FilterRecommend);
            }
            else if (option.Test != null)
            {
                ProcessTest(option.Test);
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
                "DCInside Console Core\r\n" +
                "\r\n"
                );

            var builder = new StringBuilder();
            CommandLineParser<DCInsideConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} ({x.Value.Item2.Help}) : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                    else
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                });
            Console.Instance.WriteLine(builder.ToString());
        }


        static SortedDictionary<string, string> galleries;
        static SortedDictionary<string, string> minor_galleries;

        static void LoadGalleryList()
        {
            if (galleries == null)
                galleries = DCCommon.GetGalleryList();
            if (minor_galleries == null)
                minor_galleries = DCCommon.GetMinorGalleryList();
        }

        static void ProcessFindGallery(string[] args)
        {
            LoadGalleryList();

            foreach (var gall in galleries)
                if (gall.Key.Contains(args[0]))
                    Console.Instance.WriteLine($"{gall.Key} ({gall.Value})");

            foreach (var gall in minor_galleries)
                if (gall.Key.Contains(args[0]))
                    Console.Instance.WriteLine($"[Minor Gallery] {gall.Key} ({gall.Value})");
        }

        static void ProcessParseGallery(string[] args, bool rem, string[] title, string[] nick,
            string[] ip, string[] id, bool login, bool fix)
        {
            var starts = Convert.ToInt32(args[1]);
            var ends = Convert.ToInt32(args[2]);

            LoadGalleryList();

            bool is_minorg = minor_galleries.ContainsValue(args[0]);

            for (; starts <= ends; starts++)
            {
                var url = "";
                if (is_minorg)
                    url = $"https://gall.dcinside.com/mgallery/board/lists/?id={args[0]}&page={starts}";
                else
                    url = $"https://gall.dcinside.com/board/lists/?id={args[0]}&page={starts}";

                if (rem)
                    url += "&exception_mode=recommend";

                var html = NetCommon.DownloadString(url);
                DCGallery gall = null;

                if (is_minorg)
                    gall = DCParser.ParseMinorGallery(html);
                else
                    gall = DCParser.ParseGallery(html);

                foreach (var article in gall.articles)
                {
                    if (title != null && !article.title.Contains(title[0]))
                        continue;
                    if (nick != null && !article.nick.Contains(nick[0]))
                        continue;
                    if (ip != null && !article.ip.Contains(ip[0]))
                        continue;
                    if (id != null && !article.uid.Contains(id[0]))
                        continue;
                    if (login && !article.islogined)
                        continue;
                    if (fix && !article.isfixed)
                        continue;

                    Console.Instance.Write(Monitor.SerializeObject(article));
                    Console.Instance.WriteLine(",");
                }
            }
        }

        static void ProcessCollectArticles(string[] args, bool rem)
        {
            var rstarts = Convert.ToInt32(args[1]);
            var starts = Convert.ToInt32(args[1]);
            var ends = Convert.ToInt32(args[2]);

            LoadGalleryList();

            bool is_minorg = minor_galleries.ContainsValue(args[0]);

            var result = new DCGalleryModel();
            var articles = new List<DCPageArticle>();

            using (var progressBar = new Console.ConsoleProgressBar())
            {
                for (; starts <= ends; starts++)
                {
                    X:
                    try
                    {
                        var url = "";
                        if (is_minorg)
                            url = $"https://gall.dcinside.com/mgallery/board/lists/?id={args[0]}&page={starts}";
                        else
                            url = $"https://gall.dcinside.com/board/lists/?id={args[0]}&page={starts}";

                        if (rem)
                            url += "&exception_mode=recommend";

                        Console.Instance.WriteLine($"Download URL: {url}");

                        var html = NetCommon.DownloadString(url);
                        DCGallery gall = null;

                        if (is_minorg)
                            gall = DCParser.ParseMinorGallery(html);
                        else
                            gall = DCParser.ParseGallery(html);

                        articles.AddRange(gall.articles);
                    }
                    catch { goto X; }
                    progressBar.SetProgress((((ends - rstarts + 1) - (ends - starts)) / (float)(ends - rstarts + 1)) * 100);
                }

                result.is_minor_gallery = is_minorg;
                result.gallery_id = args[0];
                result.articles = articles.ToArray();

                File.WriteAllText($"list-{args[0]}-{DateTime.Now.Ticks}.txt", JsonConvert.SerializeObject(result));
            }
        }

        static void ProcessTest(string[] args)
        {
            switch (args[0])
            {
                case "cmt":
                    var x = DCCommon.GetComments(DCParser.ParseBoardView(Net.NetCommon.DownloadString("https://gall.dcinside.com/board/view/?id=maplestory&no=6712726&exception_mode=recommend&page=1")), "0");
                    Console.Instance.WriteLine(x);
                    break;

                case "board":
                    DCParser.ParseBoardView(Net.NetCommon.DownloadString("https://gall.dcinside.com/board/view/?id=maplestory&no=6712726&exception_mode=recommend&page=1"));
                    break;

                case "gall":
                    var url = "https://gall.dcinside.com/board/lists?id=watch";
                    var g = DCParser.ParseGallery(Net.NetCommon.DownloadString(url));
                    break;

                case "galllist":
                    Console.Instance.WriteLine(DCCommon.GetGalleryList());
                    break;

                case "mgalllist":
                    Console.Instance.WriteLine(DCCommon.GetMinorGalleryList());
                    break;
            }
        }
    }
}
