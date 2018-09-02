/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.EH;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 익헨 콘솔 옵션입니다.
    /// </summary>
    public class ExHentaiConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-article", CommandType.ARGUMENTS, Help = "use -article <Hitomi Number>")]
        public string[] Article;
    }

    /// <summary>
    /// 코로모 카피에 구현된 모든 익헨 도구를 사용할 수 있는 콘솔 명령 집합입니다.
    /// </summary>
    public class ExHentaiConsole : IConsole
    {
        /// <summary>
        /// 익헨 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments)
        {
            HitomiConsoleOption option = CommandLineParser<HitomiConsoleOption>.Parse(arguments);

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

            return true;
        }

        bool IConsole.Redirect(string[] arguments)
        {
            return Redirect(arguments);
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "Ex-Hentai Console Core\r\n" +
                "\r\n" +
                " -article <Hitomi Number> : Show article info.\r\n" +
                " -image <Hitomi Number> [-type=small | big]: Get Image Link."
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
    }
}
