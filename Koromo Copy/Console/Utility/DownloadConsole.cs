/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Koromo_Copy.Net;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Koromo_Copy.Console.Utility
{
    /// <summary>
    /// 다운로드 콘솔 옵션입니다.
    /// </summary>
    public class DownloadConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--url", CommandType.ARGUMENTS, DefaultArgument = true,
            Help = "--url <image url> : Download image.")]
        public string[] Url;
        
        [CommandLine("--out", CommandType.ARGUMENTS,
            Help = "--out <out pattern> : Download images file name.")]
        public string[] Out;
    }

    /// <summary>
    /// 다운로드 콘솔입니다.
    /// </summary>
    public class DownloadConsole : ILazy<DownloadConsole>, IConsole
    {
        /// <summary>
        /// 다운로드 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.InsertWeirdArguments<DownloadConsoleOption>(arguments, contents == "", "--url");
            DownloadConsoleOption option = CommandLineParser<DownloadConsoleOption>.Parse(arguments);

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
            else if (option.Url != null)
            {
                ProcessUrl(option.Url, option.Out);
            }
            else if (contents != "")
            {
                ProcessUrls(contents, option.Out);
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
                "Download Console\r\n"
                );
        }

        public DownloadQueue queue;
        
        static void AllocQueue()
        {
            if (Instance.queue == null)
            {
                Instance.queue = new DownloadQueue((x, y, z) => { }, (x, y, z) => { }, (x, y) => { });
            }
        }

        static void ResultCallback(string url, string filename, object obj)
        {
            Monitor.Instance.Push($"[{Instance.queue.queue.Count}] {url} {filename}");
        }

        /// <summary>
        /// 특정 이미지 url을 다운로드 큐에 넣습니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessUrl(string[] args, string[] outs)
        {
            if (outs == null)
            {
                Console.Instance.WriteErrorLine("Write output filename.");
                return;
            }
            AllocQueue();

            Instance.queue.Add(args[0], outs[0], null, ResultCallback, new SemaphoreExtends());
            Console.Instance.GlobalTask = Instance.queue.tasks;
        }

        /// <summary>
        /// 특정 이미지 url들을 다운로드 큐에 넣습니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessUrls(string args, string[] outs)
        {
            var list = JsonConvert.DeserializeObject<string[]>(args).ToList();

            if (outs == null)
            {
                Console.Instance.WriteErrorLine("Write output filename.");
                return;
            }

            AllocQueue();

            list.ForEach(x => Instance.queue.Add(x, Path.Combine(outs[0], x.Split('/').Last()), null, ResultCallback, new SemaphoreExtends()));
            Console.Instance.GlobalTask = Instance.queue.tasks;
        }
    }
}
