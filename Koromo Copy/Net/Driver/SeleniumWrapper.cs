/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Net.Driver
{
    public class SeleniumWrapper
    {
        public ChromeDriver Driver;

        public SeleniumWrapper()
        {
            var chrome = new ChromeOptions();
            chrome.AddArgument("--headless");
            Driver = new ChromeDriver("chromedriver.exe", chrome);
        }
    }
}
