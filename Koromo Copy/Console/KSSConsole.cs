/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Koromo_Copy.KSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// KSS 콘솔 옵션입니다.
    /// </summary>
    public class KSSConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-connect", CommandType.OPTION, Info = "Connect to server.")]
        public bool Connect;
        [CommandLine("-myip", CommandType.OPTION, Info = "Get my ip address.")]
        public bool MyIp;
    }

    public class KSSConsole : IConsole
    {
        /// <summary>
        /// KSS 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            KSSConsoleOption option = CommandLineParser<KSSConsoleOption>.Parse(arguments);

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
            else if (option.Connect)
            {
                ProcessConnect();
            }
            else if (option.MyIp)
            {
                ProcessMyIP();
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
                "KSS Console\r\n" +
                "\r\n"
                );

            var builder = new StringBuilder();
            CommandLineParser<KSSConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} ({x.Value.Item2.Help}) : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                    else
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                });
            Console.Instance.WriteLine(builder.ToString());
        }

        /// <summary>
        /// KSS에 접속합니다.
        /// </summary>
        static void ProcessConnect()
        {
            KSSCore.Instance.Start();
        }

        /// <summary>
        /// KSS 서버를 통해 내 IP를 가져옵니다.
        /// </summary>
        static void ProcessMyIP()
        {
            KSSCore.Instance.MyIP();
        }
    }
}
