/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        
        [CommandLine("--navigate", CommandType.ARGUMENTS, Help = "use '--navigate <url>'")]
        public string[] Navigate;
        [CommandLine("--wait", CommandType.ARGUMENTS)]
        public string[] Wait;

        [CommandLine("--printhtml", CommandType.OPTION, Help = "use '--printhtml'")]
        public bool PrintHtml;
        [CommandLine("--scroll", CommandType.OPTION, Help = "use '--scroll'")]
        public bool Scroll;


        [CommandLine("--tablist", CommandType.ARGUMENTS)]
        public string[] TabLists;
    }

    /// <summary>
    /// 셀레니움을 컨트롤하는 콘솔 명령어 집합입니다.
    /// </summary>
    public class SeleniumConsole : ILazy<SeleniumConsole>, IConsole
    {
        IWebDriver driver;

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
            else if (option.Navigate != null)
            {
                ProcessNavigate(option.Navigate, option.Wait);
            }
            else if (option.PrintHtml)
            {
                ProcessPrintHtml();
            }
            else if (option.TabLists != null)
            {
                ProcessTabLists(option.TabLists);
            }
            else if (option.Scroll)
            {
                ProcessScroll();
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

        static void CreateDriver()
        {
            if (Instance.driver == null)
            {
                var chromeDriverService = ChromeDriverService.CreateDefaultService($"{Directory.GetCurrentDirectory()}");
                chromeDriverService.HideCommandPromptWindow = true;
                var chrome = new ChromeOptions();
                //chrome.AddArgument("--headless");
                Instance.driver = new ChromeDriver(chromeDriverService, chrome);
            }
        }
        
        /// <summary>
        /// 특정 Url로 이동합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessNavigate(string[] args, string[] wait)
        {
            CreateDriver();

            int _wait = 3;
            if (wait != null)
            {
                if (!int.TryParse(wait[0], out _wait))
                {
                    Console.Instance.WriteErrorLine($"'{wait[0]}' is not correct wait time unit.");
                    return;
                }
            }

            Instance.driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_wait);
            Instance.driver.Url = args[0];
        }

        /// <summary>
        /// Html 소스를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessPrintHtml()
        {
            if (Instance.driver == null)
            {
                Console.Instance.WriteErrorLine($"Navigate before processing.");
                return;
            }

            Console.Instance.Write(Instance.driver.PageSource);
        }

        /// <summary>
        /// 탭 리스트를 출력합니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessTabLists(string[] args)
        {
            if (Instance.driver == null)
            {
                Console.Instance.WriteErrorLine($"Navigate before processing.");
                return;
            }

            foreach (var wh in Instance.driver.WindowHandles)
            {
                Instance.driver.SwitchTo().Window(wh);
                Console.Instance.WriteLine(Instance.driver.Url);
            }
        }

        static void ProcessScroll()
        {
            IJavaScriptExecutor js = Instance.driver as IJavaScriptExecutor;
            
            while(true)
            {
                object s = js.ExecuteScript("return document.body.scrollHeight");
                Console.Instance.WriteLine(s);
                js.ExecuteScript("window.scrollTo(0,document.body.scrollHeight);");

                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
