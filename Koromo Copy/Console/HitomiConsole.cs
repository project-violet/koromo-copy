/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy.Net;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 히토미 콘솔 옵션입니다.
    /// </summary>
    public class HitomiConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-at", CommandType.ARGUMENTS)]
        public string[] At;

        [CommandLine("-il", CommandType.ARGUMENTS)]
        public string[] Il;
    }

    /// <summary>
    /// 코로모 카피에 구현된 모든 히토미 도구를 사용할 수 있는 콘솔 명령 집합입니다.
    /// </summary>
    public class HitomiConsole : IConsole
    {
        /// <summary>
        /// 히토미 콘솔 리다이렉트
        /// </summary>
        static void Redirect(string[] arguments)
        {
            HitomiConsoleOption option = CommandLineParser<HitomiConsoleOption>.Parse(arguments);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.At != null)
            {
                ProcessAt(option.At);
            }
            else if (option.Il != null)
            {
                ProcessIl(option.Il);
            }
        }

        void IConsole.Redirect(string[] arguments)
        {
            Redirect(arguments);
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "Hitomi Console Core\r\n" + 
                "\r\n" +
                " -at <Hitomi Number> : Show article info.\r\n" +
                " -il <Hitomi Number> : Get Image Link."
                );
        }

        /// <summary>
        /// 아티클 정보를 다운로드하고 정보를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessAt(string[] args)
        {
            string html_source = NetCommon.DownloadString($"{HitomiCommon.HitomiGalleryBlock}{args[0]}.html");
            HitomiArticle article = HitomiParser.ParseGalleryBlock(html_source);
            Console.Instance.WriteLine(Monitor.SerializeObject(article));
        }

        /// <summary>
        /// 이미지 링크를 다운로드하고 정보를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessIl(string[] args)
        {
            string html_source = NetCommon.DownloadString($"{HitomiCommon.HitomiGalleryAddress}{args[0]}.js");
            var image_link = HitomiParser.GetImageLink(html_source);
            Console.Instance.WriteLine(Monitor.SerializeObject(image_link));
        }
    }
}
