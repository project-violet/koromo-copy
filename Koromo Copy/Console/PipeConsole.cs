/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 파이프 콘솔 옵션입니다.
    /// </summary>
    public class PipeConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;
        
        [CommandLine("out", CommandType.ARGUMENTS)]
        public string[] Out;

        [CommandLine("-i", CommandType.OPTION)]
        public bool Ignore;
    }

    /// <summary>
    /// 파이프 처리를 총괄하는 클래스입니다.
    /// </summary>
    public class PipeConsole
    {
        /// <summary>
        /// 파이프 콘솔 리다이렉트
        /// </summary>
        public static void Redirect(string[] arguments, string contents)
        {
            //
            //  옵션은 뒤로 빼고 나머지는 그대로
            //
            List<string> arrages = new List<string>();
            List<string> options = new List<string>();

            foreach (var arg in arguments)
                if (arg.StartsWith("-"))
                    arrages.Add(arg);
                else
                    arrages.Add(arg);

            arguments = arrages.Concat(options).ToArray();

            PipeConsoleOption option = CommandLineParser<PipeConsoleOption>.Parse(arguments);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.Instance.WriteLine(option.HelpMessage);
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Out != null)
            {
                ProcessOut(option.Out, contents, option.Ignore);
            }
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "Pipe\r\n" +
                "\r\n" +
                " out <text file> [-i]: Write contents to text file.\r\n"
                );
        }
        
        /// <summary>
        /// 콘텐츠를 파일에 씁니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="contents"></param>
        static void ProcessOut(string[] args, string contents, bool overwrite = false)
        {
            if (!overwrite && File.Exists(args[0]))
            {
                Console.Instance.WriteLine(
                    $"'{args[0]}' file already exists.");
                return;
            }

            Monitor.Instance.Push($"Write file: {args[0]}");
            File.WriteAllText(args[0], contents);
        }

    }
}
