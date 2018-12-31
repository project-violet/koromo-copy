/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 익헨 콘솔 옵션입니다.
    /// </summary>
    public class YoutubeConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-info", CommandType.ARGUMENTS, Help = "use -article <Exhentai address>",
            Info = "Get information data")]
        public string[] Info;
    }

    /// <summary>
    /// 코로모 카피에 구현된 모든 익헨 도구를 사용할 수 있는 콘솔 명령 집합입니다.
    /// </summary>
    public class YoutubeConsole : IConsole
    {
        /// <summary>
        /// 익헨 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            YoutubeConsoleOption option = CommandLineParser<YoutubeConsoleOption>.Parse(arguments, contents != "", contents);

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
            else if (option.Info != null)
            {
                ProcessInfo(option.Info);
            }

            return true;
        }

        bool IConsole.Redirect(string[] arguments, string contents)
        {
            return Redirect(arguments, contents);
        }

        static void PrintHelp()
        {
            //Console.Instance.WriteLine(
            //    "Ex-Hentai Console Core\r\n" +
            //    "\r\n" +
            //    " -article <Exhentai address> : Show article info.\r\n" +
            //    " -addr <Hitomi Article> : Get ExHentai Address from HitomiArticle. This option can using pipe."
            //    );
        }

        static void ProcessInfo(string[] args)
        {

            // Our test youtube link
            string link = args[0];

            /*
             * Get the available video formats.
             * We'll work with them in the video and audio download examples.
             */
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            /*
             * Select the first .mp4 video with 360p resolution
             */
            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            var videoDownloader = new VideoDownloader(video, Path.Combine("", "asdf" + video.VideoExtension));

            // Register the ProgressChanged event and print the current progress
            videoDownloader.DownloadProgressChanged += (sender, arg) => Console.Instance.WriteLine(arg.ProgressPercentage);

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            videoDownloader.Execute();
        }
    }
}
