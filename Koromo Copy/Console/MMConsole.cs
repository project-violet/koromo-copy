/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Koromo_Copy.MM;

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
    }
}
