/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy
{
    public class Profiler
    {
        public const bool UsingProfiler = false;
        public const string FileName = "profile.log";

        public static void Push(string str)
        {
            if (!UsingProfiler) return;
            CultureInfo en = new CultureInfo("en-US");
            File.AppendAllText(FileName, $"[{DateTime.Now.ToString(en)}] " + str + "\r\n");
        }
    }
}
