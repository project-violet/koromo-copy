/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Koromo_Copy.Component.Pinterest
{
    public class PinParser
    {
        public static List<string> ParseId(string html)
        {
            return Regex.Matches(html, @"<a href=""(.*?)""").OfType<Match>().Select(x => x.Groups[1].Value).Where(x => !string.IsNullOrEmpty(x) && x.Count(f => f== '/') >= 2).ToList();
        }

        public static List<string> ParseBoard(string html)
        {
            return Regex.Matches(html, @"(https://i.pinimg.com/.*?\.jpg)").OfType<Match>().ToList().Select(x => x.Groups[1].Value).ToList();
        }

        public static List<string> ParseImage(string html)
        {
            return Regex.Matches(html, @"(https://i.pinimg.com/originals/.*?) ").OfType<Match>().ToList().Select(x => x.Groups[1].Value).ToList();
        }
    }
}
