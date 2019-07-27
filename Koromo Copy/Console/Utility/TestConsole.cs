/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Algorithm;
using Koromo_Copy.Component.DC;
using Koromo_Copy.Component.Hitomi;
using Koromo_Copy.Component.Hiyobi;
using Koromo_Copy.Component.Mangashow;
using Koromo_Copy.Html;
using Koromo_Copy.Interface;
using Koromo_Copy.LP;
using Koromo_Copy.Script;
using Koromo_Copy.Utility.Develop;
using Koromo_Copy.Wrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

                case "dc":
                    var html2 = Net.NetCommon.DownloadString("http://gall.dcinside.com/board/view/?id=comic_new1&no=7118416&page=1");
                    DCParser.ParseBoardView(html2);
                    break;

                case "non-h":
                    var html3 = Net.NetCommon.DownloadString("https://hiyobi.me/manga/info/1848");
                    HiyobiParser.ParseNonHArticles(html3);
                    break;

                case "msm":
                    var html4 = Net.NetCommon.DownloadString("http://mangashow.me/detail.php?manga_id=4969");
                    MangashowmeParser.ParseSeries(html4);
                    var html5 = Net.NetCommon.DownloadString("http://mangashow.me/viewer.php?chapter_id=79688");
                    MangashowmeParser.ParseImages(html5);
                    break;

                case "tree":
                    var html6 = Net.NetCommon.DownloadString("https://www.google.com/search?q=anime&newwindow=1&rlz=1C1GIGM_enKR776KR776&source=lnms&tbm=isch&sa=X&ved=0ahUKEwjS-J2cxejfAhWT-mEKHSpkCqsQ_AUIDigB&biw=1573&bih=814");
                    var tree = new HtmlTree(html6);
                    tree.BuildTree();
                    Console.Instance.Write(Monitor.SerializeObject(tree.GetLevelImages(11)));
                    break;

                case "ls":
                    HtmlLocalServer.Instance.Start();
                    break;

                case "lpkor":
                    var x = "진화론의 시야에서 벗어난 생물학은 아무 의미가 없다.";
                    var xl = string.Join("", x.Select(y => LPKor.IsHangul(y) ? LPKor.Disassembly(y) : y.ToString()));
                    Console.Instance.WriteLine(xl);
                    var xr = LPKor.Assembly(xl);
                    Console.Instance.WriteLine(xr);

                    xl = string.Join("", x.Select(y => LPKor.IsHangul3(y) ? LPKor.Disassembly3(y) : y.ToString()));
                    Console.Instance.WriteLine(xl);
                    xr = LPKor.Assembly3(xl);
                    Console.Instance.WriteLine(xr);
                    break;

                case "heap":
                    var rl = new List<int>();

                    var r = new Random();
                    for (int i = 0; i < 20; i++)
                        rl.Add(r.Next() % 100);

                    var h = new Heap<int>();
                    foreach (var xx in rl)
                        h.Push(xx);

                    rl.Sort();
                    rl.Reverse();

                    for (int i = 0; i < 20; i++)
                    {
                        Console.Instance.WriteLine($"{rl[i]}, {h.Front}");
                        h.Pop();
                    }

                    var hh = new UpdatableHeap<int>();
                    
                    break;

                case "ydl":
                    ydlWrapper.download();
                    ydlWrapper.test();
                    break;

                case "stack":
                    StackTrace st = new StackTrace();

                    Console.Instance.WriteLine(st.ToString());
                    break;
            }
        }
    }
}
