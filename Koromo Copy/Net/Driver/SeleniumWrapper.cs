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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Net.Driver
{
    public class SeleniumWrapper
    {
        ChromeDriver driver;

        public SeleniumWrapper()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService($"{Directory.GetCurrentDirectory()}");
            chromeDriverService.HideCommandPromptWindow = true;
            var chrome = new ChromeOptions();
            chrome.AddArgument("--headless");
            driver = new ChromeDriver("chromedriver.exe", chrome);
        }

        public void Navigate(string url, int _wait = 3)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_wait);
            driver.Url = url;
        }

        public string GetHeight()
        {
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            return js.ExecuteScript("return document.body.scrollHeight").ToString();
        }

        public void ScrollDown()
        {
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            js.ExecuteScript("window.scrollTo(0,document.body.scrollHeight);");
        }

        public string GetHtml()
        {
            return driver.PageSource;
        }

        public void Close()
        {
            driver.Quit();
        }
    }
}
