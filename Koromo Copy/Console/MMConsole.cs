/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.MM;
using Koromo_Copy.Interface;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// MM 콘솔 옵션입니다.
    /// </summary>
    public class MMConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-series", CommandType.ARGUMENTS)]
        public string[] Series;

        [CommandLine("-articles", CommandType.ARGUMENTS)]
        public string[] Articles;
    }

    /// <summary>
    /// MM 콘솔입니다.
    /// </summary>
    public class MMConsole : IConsole
    {
        /// <summary>
        /// MM 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            MMConsoleOption option = CommandLineParser<MMConsoleOption>.Parse(arguments);

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
            else if (option.Series != null)
            {
                ProcessSeries(option.Series);
            }
            else if (option.Articles != null)
            {
                ProcessArticles(option.Articles);
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
                "Marumaru Console\r\n"
                );
        }

        /// <summary>
        /// 시리즈 정보를 가져옵니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessSeries(string[] args)
        {
            string html = Net.NetCommon.DownloadString(args[0]);

            MMSeries mms = new MMSeries();
            mms.Archive = MMParser.ParseManga(html).ToArray();
            mms.Title = MMParser.GetTitle(html);
            mms.Thumbnail = MMParser.GetThumbnailAddress(html);

            Console.Instance.Write(Monitor.SerializeObject(mms));
        }

        /// <summary>
        /// 아티클(아카이브) 정보를 가져옵니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessArticles(string[] args)
        {
            string html = Net.NetCommon.DownloadString(args[0]);

            MMSeries mms = new MMSeries();
            mms.Archive = MMParser.ParseManga(html).ToArray();
            mms.Title = MMParser.GetTitle(html);
            mms.Thumbnail = MMParser.GetThumbnailAddress(html);

            List<MMArticle> articles = new List<MMArticle>();

            for (int i = 0; i < mms.Archive.Length; i++)
            {
                WebClient wc = new WebClient();

                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36");
                //wc.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                wc.Headers.Add(HttpRequestHeader.AcceptLanguage, "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
                wc.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
                //wc.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
                wc.Headers.Add(HttpRequestHeader.Cookie, "__cfduid=d46fd6709d735a04a08fd60d89582a3911525265471; _ga=GA1.2.335086797.1525265472; _gid=GA1.2.928930778.1525265472; __gads=ID=fe459c0742f63207:T=1525265474:S=ALNI_Mb08qlp3nTYBBz1WptFsP7GviAwEw; impx={%22imp_usy%22:{%22capCount%22:5%2C%22capExpired%22:1525351873}}; PHPSESSID=4bae062279cf21003588d75744ba4ed1");
                wc.Headers.Add(HttpRequestHeader.Host, "wasabisyrup.com");
                wc.Headers.Add(HttpRequestHeader.Referer, "http://203.233.24.233/tm/?a=CR&b=WIN&c=799001634617&d=10003&e=2013&f=d2FzYWJpc3lydXAuY29tL2FyY2hpdmVzLzQyODA2MQ==&g=1525401005814&h=1525401004232&y=0&z=0&x=1&w=2018-02-12&in=2013_00009301&id=20180504");
                wc.Headers.Add(HttpRequestHeader.Upgrade, "1");

                string archive = wc.DownloadString(mms.Archive[i]);
                MMArticle article = new MMArticle();
                article.Title = MMParser.GetArchive(archive);
                article.ImagesLink = MMParser.ParseArchives(archive);
                articles.Add(article);
                Monitor.Instance.Push($"Load complete: [{i + 1}/{mms.Archive.Length}] {article.Title} {mms.Archive[i]}");
                //Thread.Sleep(1000);
            }

            articles.ForEach(x => Console.Instance.Write(Monitor.SerializeObject(articles)));
        }
    }
}
