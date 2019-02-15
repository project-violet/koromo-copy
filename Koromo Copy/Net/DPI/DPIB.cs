/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Net.DPI
{
    /// <summary>
    /// https://github.com/ValdikSS/GoodbyeDPI 를 사용해 DPI를 우회합니다.
    /// </summary>
    public class DPIB : ILazy<DPIB>
    {
        public const string DownloadURL = "https://github.com/ValdikSS/GoodbyeDPI/releases/download/0.1.5/goodbyedpi-0.1.5.zip";

        public DPIB()
        {
            var dpib_path = Path.Combine(Directory.GetCurrentDirectory(), "goodbyedpi/goodbyedpi-0.1.5/x86_64/goodbyedpi.exe");
            var dpib_zip_path = Path.Combine(Directory.GetCurrentDirectory(), "goodbyedpi.zip");

            if (!File.Exists(dpib_path))
            {
                NetCommon.GetDefaultClient().DownloadFile(DownloadURL, dpib_zip_path);

                var zip = ZipFile.Open(dpib_zip_path, ZipArchiveMode.Read);
                zip.ExtractToDirectory(Path.Combine(Directory.GetCurrentDirectory(), "goodbyedpi"));
                zip.Dispose();
                File.Delete(dpib_zip_path);
            }
            
        }

        Process proc;
        bool close = false;

        public void Start()
        {
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                Monitor.Instance.Push($"[GoodbyeDPI] Not administrator mode.");
                return;
            }
            
            while (true)
            {
                var dpib_path = Path.Combine(Directory.GetCurrentDirectory(), "goodbyedpi/goodbyedpi-0.1.5/x86_64/goodbyedpi.exe");

                proc = new Process();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.FileName = dpib_path;
                proc.StartInfo.Verb = "runas";
                proc.OutputDataReceived += (sender, args) => Monitor.Instance.Push($"[GoodbyeDPI] {args.Data}");
                proc.Start();
                proc.BeginOutputReadLine();
                proc.WaitForExit();

                if (close) break;
            }
        }

        public void Close()
        {
            if (proc != null)
            {
                close = true;
                proc.Kill();
            }
        }
    }
}
