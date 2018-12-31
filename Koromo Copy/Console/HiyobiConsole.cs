/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using System.Linq;

namespace Koromo_Copy.Console
{
    public class HiyobiConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-article", CommandType.ARGUMENTS, Help = "use -article <Hitomi Number>",
            Info = "Download article page and parse html.")]
        public string[] Article;
        [CommandLine("-image", CommandType.ARGUMENTS, Help = "use -image <Hitomi Number>",
            Info = "Download article images link list.")]
        public string[] ImageLink;
    }

    public class HiyobiConsole : ILazy<HiyobiConsole>, IConsole
    {
        static bool Redirect(string[] arguments, string contents)
        {
            HiyobiConsoleOption option = CommandLineParser<HiyobiConsoleOption>.Parse(arguments);

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
            else if (option.ImageLink != null)
            {
                ProcessImage(option.ImageLink);
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
                "Hiyobi Console Core\r\n" +
                "\r\n" +
                " -article <Hitomi Number> : Show article info.\r\n" +
                " -image <Hitomi Number> : Get Image Link."
                );
        }

        /// <summary>
        /// 아티클 정보를 다운로드하고 정보를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessArticle(string[] args)
        {
            Console.Instance.WriteLine(HiyobiDispatcher.Collect(args[0]));
        }

        /// <summary>
        /// 이미지 링크를 다운로드하고 정보를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="dl">다운로드 가능한 이미지 링크를 출력할지의 여부를 설정합니다.</param>
        static void ProcessImage(string[] args)
        {
            string json_source = NetCommon.DownloadString(HiyobiCommon.GetDownloadImageAddress(args[0]));
            var image_link = HitomiParser.GetImageLink(json_source);
            
            Console.Instance.WriteLine(image_link);
        }

    }
}
