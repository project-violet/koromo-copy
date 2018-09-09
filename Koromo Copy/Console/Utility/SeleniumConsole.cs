/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;

namespace Koromo_Copy.Console.Utility
{
    /// <summary>
    /// Selenium 콘솔 옵션입니다.
    /// </summary>
    public class SeleniumConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--newsession", CommandType.ARGUMENTS, Help = "Create new session using chrome driver.")]
        public string[] Session;
        [CommandLine("--closesession", CommandType.ARGUMENTS, Help = "Close session.")]
        public string[] CloseSession;

        [CommandLine("--navigate", CommandType.ARGUMENTS, ArgumentsCount = 2, Help = "--navigate <session> <url>")]
        public string[] Navigate;
        [CommandLine("--wait", CommandType.ARGUMENTS)]
        public string[] Wait;

        [CommandLine("--printhtml", CommandType.ARGUMENTS, Help = "--printhtml <session>")]
        public string[] PrintHtml;

        [CommandLine("--tablist", CommandType.ARGUMENTS)]
        public string[] TabLists;
    }

    /// <summary>
    /// 셀레니움을 컨트롤하는 콘솔 명령어 집합입니다.
    /// </summary>
    public class SeleniumConsole : ILazy<SeleniumConsole>, IConsole
    {
        Dictionary<string, IWebDriver> drivers = new Dictionary<string, IWebDriver>();

        /// <summary>
        /// Selenium 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            SeleniumConsoleOption option = CommandLineParser<SeleniumConsoleOption>.Parse(arguments);

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
            else if (option.Session != null)
            {
                ProcessNewSession(option.Session);
            }
            else if (option.CloseSession != null)
            {
                ProcessCloseSession(option.CloseSession);
            }
            else if (option.Navigate != null)
            {
                ProcessNavigate(option.Navigate, option.Wait);
            }
            else if (option.PrintHtml != null)
            {
                ProcessPrintHtml(option.PrintHtml);
            }
            else if (option.TabLists != null)
            {
                ProcessTabLists(option.TabLists);
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
                "Selenium Console\r\n"
                );
        }

        /// <summary>
        /// 새로운 세션을 만듭니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessNewSession(string[] args)
        {
            if (Instance.drivers.ContainsKey(args[0]))
            {
                Console.Instance.WriteErrorLine($"'{args[0]}' session already exists.");
                return;
            }
            
            var chromeDriverService = ChromeDriverService.CreateDefaultService($"{Directory.GetCurrentDirectory()}");
            chromeDriverService.HideCommandPromptWindow = true;
            Instance.drivers.Add(args[0], new ChromeDriver(chromeDriverService, new ChromeOptions()));
        }

        /// <summary>
        /// 세션을 닫습니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessCloseSession(string[] args)
        {
            if (!Instance.drivers.ContainsKey(args[0]))
            {
                Console.Instance.WriteErrorLine($"'{args[0]}' session not found.");
                return;
            }

            Instance.drivers[args[0]].Quit();
            Instance.drivers.Remove(args[0]);
        }

        /// <summary>
        /// 특정 Url로 이동합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessNavigate(string[] args, string[] wait)
        {
            if (!Instance.drivers.ContainsKey(args[0]))
            {
                Console.Instance.WriteErrorLine($"'{args[0]}' session not found.");
                return;
            }

            int _wait = 3;
            if (wait != null)
            {
                if (!int.TryParse(wait[0], out _wait))
                {
                    Console.Instance.WriteErrorLine($"'{wait[0]}' is not correct wait time unit.");
                    return;
                }
            }

            Instance.drivers[args[0]].Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_wait);
            Instance.drivers[args[0]].Url = args[1];
        }

        /// <summary>
        /// Html 소스를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessPrintHtml(string[] args)
        {
            if (!Instance.drivers.ContainsKey(args[0]))
            {
                Console.Instance.WriteErrorLine($"'{args[0]}' session not found.");
                return;
            }

            Console.Instance.Write(Instance.drivers[args[0]].PageSource);
        }

        /// <summary>
        /// 탭 리스트를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessTabLists(string[] args)
        {
            if (!Instance.drivers.ContainsKey(args[0]))
            {
                Console.Instance.WriteErrorLine($"'{args[0]}' session not found.");
                return;
            }

            foreach (var wh in Instance.drivers[args[0]].WindowHandles)
            {
                Instance.drivers[args[0]].SwitchTo().Window(wh);
                Console.Instance.WriteLine(Instance.drivers[args[0]].Url);
            }
        }
    }
}
