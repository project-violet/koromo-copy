/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Hitomi_Copy_3;
using Hitomi_Copy_3._403;
using Koromo_Copy.Interface;
using Koromo_Copy.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Koromo_Copy.Console.Utility
{
    /// <summary>
    /// Run 콘솔 옵션입니다.
    /// </summary>
    public class RunConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--name", CommandType.ARGUMENTS, DefaultArgument = true, 
            Help = "--name <Utility>|<Program>", 
            Info = "Set sepecific program to run.")]
        public string[] RunProgramName;
    }
    
    /// <summary>
    /// 특정 유틸리티나 프로그램을 실행하는 콘솔 명령집합입니다.
    /// </summary>
    class RunConsole : IConsole
    {
        /// <summary>
        /// Run 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            arguments = CommandLineUtil.InsertWeirdArguments<RunConsoleOption>(arguments, true, "--name");
            RunConsoleOption option = CommandLineParser<RunConsoleOption>.Parse(arguments);

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
            else if (option.RunProgramName != null)
            {
                ProcessRun(option.RunProgramName);
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
                "Run Console\r\n"
                );

            foreach (var pair in run_dic)
                Console.Instance.WriteLine($"{pair.Key.PadRight(12)} [{pair.Value.ToString()}]");
        }

        static Dictionary<string, Type> run_dic = new Dictionary<string, Type>() {
            { "fsenum", typeof(FsEnumerator) },
            { "mddown", typeof(MetadataDownloader) },
            { "strings", typeof(StringTools) },
            { "explorer", typeof(HitomiExplorer) },
            { "tagtest", typeof(RelatedTagsTest) },
            { "record", typeof(Record) },
            { "counter", typeof(FileLineCounter) },
            { "pgs", typeof(PGS) },
            { "stat", typeof(Hitomi_Copy_3.Statistics) },
#if DEBUG
            { "gbt", typeof (GalleryBlockTester) },
#endif
        };

        /// <summary>
        /// 특정 유틸리니타 프로그램을 실행합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessRun(string[] args)
        {
#if !DEBUG
            if (args[0] == "gbt")
            {
                Console.Instance.WriteErrorLine("Forbidden. You cannot use gbt tool.");
                return;
            }
#endif

            if (!run_dic.ContainsKey(args[0]))
            {
                Console.Instance.WriteErrorLine($"'{args[0]}' program not found.");
                return;
            }

            Global.UXInvoke(() => ((Form)Activator.CreateInstance(run_dic[args[0]])).Show());
        }
    }
}
