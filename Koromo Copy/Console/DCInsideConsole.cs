/***

   Copyright (C) 2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.DC;
using Koromo_Copy.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Console
{
    public class DCInsideConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-test", CommandType.ARGUMENTS, Help = "use -test <Number>",
            Info = "Test dcinside methods.")]
        public string[] Test;
    }
    
    public class DCInsideConsole : IConsole
    {
        static bool Redirect(string[] arguments, string contents)
        {
            DCInsideConsoleOption option = CommandLineParser<DCInsideConsoleOption>.Parse(arguments, contents != "", contents);

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
            else if (option.Test != null)
            {
                ProcessTest(option.Test);
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
                "DCInside Console Core\r\n" +
                "\r\n"
                );

            var builder = new StringBuilder();
            CommandLineParser<DCInsideConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} ({x.Value.Item2.Help}) : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                    else
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                });
            Console.Instance.WriteLine(builder.ToString());
        }
        
        static void ProcessTest(string[] args)
        {
            switch (args[0])
            {
                case "cmt":
                    var x = DCCommon.GetComments(DCParser.ParseBoardView(Net.NetCommon.DownloadString("https://gall.dcinside.com/board/view/?id=maplestory&no=6712726&exception_mode=recommend&page=1")), "0");
                    Console.Instance.WriteLine(x);
                    break;

                case "board":
                    DCParser.ParseBoardView(Net.NetCommon.DownloadString("https://gall.dcinside.com/board/view/?id=maplestory&no=6712726&exception_mode=recommend&page=1"));
                    break;

                case "gall":
                    var url = "https://gall.dcinside.com/board/lists?id=watch";
                    var g = DCParser.ParseGallery(Net.NetCommon.DownloadString(url));
                    break;

                case "galllist":
                    Console.Instance.WriteLine(DCCommon.GetGalleryList());
                    break;

                case "mgalllist":
                    Console.Instance.WriteLine(DCCommon.GetMinorGalleryList());
                    break;
            }
        }
    }
}
