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
        public string tmp_path;

        public DPIB()
        {
#if !DEBUG
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                Monitor.Instance.Push($"[GoodbyeDPI] Not administrator mode.");
                return;
            }

            var dpib_zip_path = Path.Combine(Directory.GetCurrentDirectory(), "goodbyedpi.zip");

            NetCommon.GetDefaultClient().DownloadFile(DownloadURL, dpib_zip_path);

            var zip = ZipFile.Open(dpib_zip_path, ZipArchiveMode.Read);
            zip.ExtractToDirectory(tmp_path = Path.GetTempPath());
            zip.Dispose();
            File.Delete(dpib_zip_path);
#endif
        }

        Process proc;
        bool close = false;

        public void Start()
        {
#if !DEBUG
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                return;
            
            while (true)
            {
                var dpib_path = Path.Combine(tmp_path, "goodbyedpi-0.1.5/x86_64/goodbyedpi.exe");

                proc = new Process();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.FileName = dpib_path;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.Verb = "runas";
                proc.OutputDataReceived += (sender, args) => Monitor.Instance.Push($"[GoodbyeDPI] {args.Data}");
                proc.Start();
                proc.BeginOutputReadLine();
                proc.WaitForExit();

                if (close) break;
            }
#endif
        }

        public void Close()
        {
            if (proc != null)
            {
                close = true;
                proc.Kill();
                Directory.Delete(Path.Combine(tmp_path, "goodbyedpi-0.1.5"), true);
            }
        }
    }
}
