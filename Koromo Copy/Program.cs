/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace Koromo_Copy
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Global Exception 핸들러 설정
            Application.ThreadException += ApplicationThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            
            // GC 설정
            GCLatencyMode oldMode = GCSettings.LatencyMode;
            RuntimeHelpers.PrepareConstrainedRegions();
            GCSettings.LatencyMode = GCLatencyMode.Batch;

            // 메인스레드 시작
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());

            Environment.Exit(0);
        }
        
        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("프로그램 내부에서 예외처리되지 않은 오류가 발생했습니다. 오류가 계속된다면 개발자에게 문의하십시오. " + (e.ExceptionObject as Exception).Source + "\nStackTrace: " + (e.ExceptionObject as Exception).StackTrace);
        }

        private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("프로그램 내부에서 예외처리되지 않은 오류가 발생했습니다. 오류가 계속된다면 개발자에게 문의하십시오. " + e.Exception.Source + "\nStackTrace: " + (e.Exception as Exception).StackTrace);
        }
    }
}
