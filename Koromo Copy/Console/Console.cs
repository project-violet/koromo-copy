/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        
        public Console()
        {
            // https://stackoverflow.com/questions/4362111/how-do-i-show-a-console-output-window-in-a-forms-application
            AllocConsole();
            // https://stackoverflow.com/questions/15578540/allocconsole-not-printing-when-in-visual-studio
            OverrideRedirection();

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

            System.Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            System.Console.Out.WriteLine($"Koromo Copy {Version.Text}");
            System.Console.Out.WriteLine("Copyright (C) 2018. Koromo Copy Developer");
            System.Console.Out.WriteLine("E-Mail: koromo.software@gmail.com");
            System.Console.Out.WriteLine("Source-code : https://github.com/dc-koromo/koromo-copy");
            System.Console.Out.WriteLine("");
        }

        /// <summary>
        /// Control + C로 강제 종료되는 것을 막기위해 추가하였습니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                e.Cancel = true;
            }
        }

        public void Start()
        {
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

        /// <summary>
        /// 커맨드 라인을 분석합니다.
        /// 가령, 커맨드 라인에 "~"가 포함된 경우, ~에 공백이 포함여부에 상관없이 하나의 원소로 취급됩니다.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public static string[] ParseArgument(string cl)
        {
            List<string> argv = new List<string>();

            for (int i = 0; i < cl.Length; i++)
            {
                // trim
                for (; char.IsWhiteSpace(cl[i]); i++) ;
                if (i >= cl.Length) break;

                bool path = false;
                if (cl[i] == '"') { i++; path = true; }

                StringBuilder builder = new StringBuilder();
                for (; i < cl.Length && ((!path && !char.IsWhiteSpace(cl[i])) || (path && cl[i] != '"')); i++)
                    builder.Append(cl[i]);

                argv.Add(builder.ToString());
            }

            return argv.ToArray();
        }
        
        /// <summary>
        /// 콘솔 스레드의 메인 루프입니다.
        /// </summary>
        public void Loop()
        {
            var redirections = new Dictionary<string, IConsole>()
            {
                {"hitomi", new HitomiConsole()}
            };

            System.Console.Out.WriteLine("");

            while (true)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Out.Write("dc-koromo");
                System.Console.ResetColor();
                System.Console.Out.Write("@");
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.Out.Write("koromo-copy");
                System.Console.ResetColor();
                System.Console.Out.Write(":");
                System.Console.ForegroundColor = ConsoleColor.Blue;
                System.Console.Out.Write("~");
                System.Console.ResetColor();
                System.Console.Out.Write("$ ");

                try
                {
                    string[] command = ParseArgument(System.Console.In.ReadLine());
                    if (command.Length == 0) continue;
                    if (command[0] == "help")
                    {
                        PrintHelp();
                    }
                    else if (redirections.ContainsKey(command[0]))
                    {
                        if (command.Length == 1)
                        {
                            redirections[command[0]].Redirect(Array.Empty<string>());
                        }
                        else
                        {
                            var list = command.ToList();
                            list.RemoveAt(0);
                            redirections[command[0]].Redirect(list.ToArray());
                        }
                    }
                    else
                    {
                        System.Console.Out.WriteLine($"{command[0]}: command not found");
                        System.Console.Out.WriteLine($"try 'help' command!");
                    }
                }
                catch (Exception e)
                {
                    System.Console.Out.WriteLine($"Error occurred on processing!");
                    System.Console.Out.WriteLine($"Message: {e.Message}");
                    System.Console.Out.WriteLine($"StackTrace: {e.StackTrace}");
                }
            }
        }

        public void PrintHelp()
        {

        }

        public void Push(DateTime dt, string contents)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            //System.Console.Out.WriteLine("");
            CultureInfo en = new CultureInfo("en-US");
            System.Console.Out.Write($"[{DateTime.Now.ToString(en)}]");
            System.Console.ResetColor();
            System.Console.Out.WriteLine($" {contents}");
        }

        public void WriteLine(string contents)
        {
            System.Console.WriteLine(contents);
        }
    }
}
