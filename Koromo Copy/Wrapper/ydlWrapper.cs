/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koromo_Copy.Wrapper
{
    public class ydlWrapper
    {
        public static void download()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "youtube-dl/youtube-dl.exe");
            Directory.CreateDirectory("youtube-dl");
            var html = NetCommon.DownloadString("https://github.com/ytdl-org/youtube-dl/releases");
            var file = "https://github.com" + Regex.Matches(html, @"""(.*?youtube\-dl\.exe)""").Cast<Match>().First().Groups[1];
            NetCommon.GetDefaultClient().DownloadFile(file, path);
        }

        public static void test()
        {
            var ydl_path = Path.Combine( Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "youtube-dl/youtube-dl.exe");
            
            var proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.FileName = ydl_path;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.Verb = "runas";
            proc.StartInfo.Arguments = "-F https://www.yoxutube.com/watch?v=zJgA-y96UPc&t=969s"; // -g
            proc.OutputDataReceived += (sender, args) => Monitor.Instance.Push($"[youtube-dl] {args.Data}");
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            
        }

        //public string[] get_urls(string )
    }
}
