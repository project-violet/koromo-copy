/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Interface;
using Koromo_Copy.Plugin;
using Koromo_Copy.Utility.Develop;
using System.Threading;
using System.Windows.Forms;

namespace Koromo_Copy.Console.Utility
{
    /// <summary>
    /// Run 콘솔 옵션입니다.
    /// </summary>
    public class TestConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--cmd", CommandType.ARGUMENTS, DefaultArgument = true)]
        public string[] Commands;
    }
    
    class TestConsole : IConsole
    {
        /// <summary>
        /// Test 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            arguments = CommandLineUtil.InsertWeirdArguments<TestConsoleOption>(arguments, true, "--cmd");
            TestConsoleOption option = CommandLineParser<TestConsoleOption>.Parse(arguments);

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
            else if (option.Commands != null)
            {
                ProcessTest(option.Commands);
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
                "Test Console\r\n"
                );
        }
        
        static void ProcessTest(string[] args)
        {
            switch (args[0])
            {
                case "version":
                    Version.ExportVersion();
                    break;

                case "manage":
                    var t = new Thread(() => {
                        Application.Run(new Manage());
                    });
                    t.Start();
                    break;

                case "plugin":
                    PlugInManager.Instance.GetLoadedPlugins().ForEach(x => Console.Instance.WriteLine(x));
                    break;

                case "preempt_dq":
                    DownloadConsole.Instance.queue.Preempt();
                    break;

                case "unpreempt_dq":
                    DownloadConsole.Instance.queue.Reactivation();
                    break;

                case "hiyobi":
                    var html = Net.NetCommon.DownloadString("https://hiyobi.me/info/1305694");
                    var article = Component.Hiyobi.HiyobiParser.ParseGalleryConents(html);
                    break;

                case "hiyobi2":
                    var html1 = Net.NetCommon.DownloadString("https://hiyobi.me/list/1");
                    var article1 = Component.Hiyobi.HiyobiParser.ParseGalleryArticles(html1);
                    break;

                case "query":
                    HitomiDataSearchAdvanced.to_linear(HitomiDataSearchAdvanced.make_tree("(a b) - (c d) - (e f)"));
                    break;

                case "version_check":
                    Version.UpdateRequired();
                    break;
            }
        }
    }
}
