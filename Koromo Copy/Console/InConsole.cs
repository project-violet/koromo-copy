/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System.IO;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// In 콘솔 옵션입니다.
    /// </summary>
    public class InConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-i", CommandType.ARGUMENTS, Pipe = true, DefaultArgument = true,
            Info = "Set load file name.")]
        public string[] Input;
    }

    /// <summary>
    /// 파일의 내용을 가져옵니다.
    /// </summary>
    public class InConsole : IConsole
    {
        /// <summary>
        /// In 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.InsertWeirdArguments<InConsoleOption>(arguments, true, "-i");
            InConsoleOption option = CommandLineParser<InConsoleOption>.Parse(arguments);

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
            else if (option.Input != null)
            {
                ProcessIn(option.Input);
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
                "In Console\r\n" +
                "\r\n" +
                " -i <path>:"
                );
        }

        /// <summary>
        /// 파일을 텍스트 형태로 읽어옵니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="contents"></param>
        static void ProcessIn(string[] args, bool overwrite = false)
        {
            if (!File.Exists(args[0]))
            {
                Console.Instance.WriteErrorLine(
                    $"File not found. '{args[0]}'");
                return;
            }

            Console.Instance.WriteLine(File.ReadAllText(args[0]));
        }
    }
}
