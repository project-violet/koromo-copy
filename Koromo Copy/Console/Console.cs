/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// 모든 현황을 보여주는 콘솔창입니다.
    /// </summary>
    public class Console : ILazy<Console>
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_INPUT_HANDLE = -10;
        public const int STD_ERROR_HANDLE = -12;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,
                                               [MarshalAs(UnmanagedType.U4)]     uint access,
                                               [MarshalAs(UnmanagedType.U4)]     FileShare share,
                                                                                 IntPtr securityAttributes,
                                               [MarshalAs(UnmanagedType.U4)]     FileMode creationDisposition,
                                               [MarshalAs(UnmanagedType.U4)]     FileAttributes flagsAndAttributes,
                                                                                 IntPtr templateFile);

        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_READ = 0x80000000;

        private static void OverrideRedirection()
        {
            var hOut = GetStdHandle(STD_OUTPUT_HANDLE);
            var hRealOut = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE, FileShare.Write, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);
            if (hRealOut != hOut)
            {
                SetStdHandle(STD_OUTPUT_HANDLE, hRealOut);
                System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput(), System.Console.OutputEncoding) { AutoFlush = true });
                System.Console.SetIn(new StreamReader(System.Console.OpenStandardInput(), System.Console.InputEncoding));
            }
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public Console()
        {
            // https://stackoverflow.com/questions/4362111/how-do-i-show-a-console-output-window-in-a-forms-application
            AllocConsole();
            // https://stackoverflow.com/questions/15578540/allocconsole-not-printing-when-in-visual-studio
            OverrideRedirection();

            Thread thread = new Thread(Loop);
            thread.Start();
        }

        public void Hide()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        public void Show()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_SHOW);
        }
        
        public void Loop()
        {
            System.Console.Out.WriteLine($"Koromo Copy {Version.Text}");
            System.Console.Out.WriteLine("Copyright (C) 2018. Koromo Copy Developer");
            System.Console.Out.WriteLine("E-Mail: koromo.software@gmail.com");
            System.Console.Out.WriteLine("Source-code : https://github.com/dc-koromo/koromo-copy");
            System.Console.Out.WriteLine("");

            while (true)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Out.Write("dc-koromo@hitomi-copy");
                System.Console.ResetColor();
                System.Console.Out.Write(":");
                System.Console.ForegroundColor = ConsoleColor.Blue;
                System.Console.Out.Write("~");
                System.Console.ResetColor();
                System.Console.Out.Write("$ ");

                string command = System.Console.In.ReadLine();
            }
        }

        public void Push(DateTime dt, string contents)
        {

        }
    }
}
