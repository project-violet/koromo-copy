/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;

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
    }

    /// <summary>
    /// 파이프 처리를 총괄하는 클래스입니다.
    /// </summary>
    public class PipeConsole
    {
        /// <summary>
        /// 파이프 콘솔 리다이렉트
        /// </summary>
        public static void Redirect(string[] arguments, string contentss)
        {
            PipeConsoleOption option = CommandLineParser<PipeConsoleOption>.Parse(arguments);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
            }
            else if (option.Help)
            {
                PrintHelp();
            }
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "Pipe\r\n" +
                "\r\n" +
                " out <text file> : Write contents to text file.\r\n"
                );
        }

    }
}
