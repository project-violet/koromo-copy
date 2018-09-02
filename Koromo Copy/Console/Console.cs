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
using System.Threading.Tasks;

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
        internal const UInt32 MF_GRAYED = 0x00000001;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("User32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int WS_EX_APPWINDOW = 0x40000;
        private const int GWL_EXSTYLE = -0x14;
        private const int WS_EX_TOOLWINDOW = 0x0080;
        
        public Console()
        {
            // https://stackoverflow.com/questions/4362111/how-do-i-show-a-console-output-window-in-a-forms-application
            AllocConsole();
            // https://stackoverflow.com/questions/15578540/allocconsole-not-printing-when-in-visual-studio
            OverrideRedirection();

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
            System.Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            var Handle = GetConsoleWindow();
            ShowWindow(Handle, SW_HIDE);
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
            ShowWindow(Handle, SW_SHOW);

            System.Console.Title = "Koromo Copy Console";
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

        public static string[] SliceArray(string[] array, int starts, int ends)
        {
            List<string> result = new List<string>();
            for (int i = starts; i < ends; i++)
                result.Add(array[i]);
            return result.ToArray();
        }

        /// <summary>
        /// 비동기로 실행되고 있는 태스크를 등록합니다.
        /// 이 태스크가 끝나야 Loop가 진행됩니다.
        /// </summary>
        public Task GlobalTask;
        
        /// <summary>
        /// 콘솔 스레드의 메인 루프입니다.
        /// </summary>
        public async void Loop()
        {
            var redirections = new Dictionary<string, IConsole>()
            {
                // normal command
                {"hitomi", new HitomiConsole()},
                {"exh", new ExHentaiConsole()},

                // pipeline command
                {"out", new OutConsole()}
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

                    while (command.Length > 0)
                    {
                        string[] command_argument = command;
                        string pipe_contents = PipeContents.ToString();
                        PipeContents.Clear();
                        
                        //
                        //  파이프 전처리
                        //
                        if (command.Contains(">"))
                        {
                            Pipe = true;

                            int meet_pipe = Array.FindIndex(command, w => w == ">");
                            command_argument = command.Take(meet_pipe).ToArray();
                            command = SliceArray(command, meet_pipe + 1, command.Length);
                        }
                        else
                        {
                            command = Array.Empty<string>();
                        }

                        //
                        //  커맨드 처리
                        //
                        bool success = false;
                        if (command_argument[0] == "help")
                        {
                            PrintHelp();
                        }
                        else if (redirections.ContainsKey(command_argument[0]))
                        {
                            if (command_argument.Length == 1)
                            {
                                success = redirections[command_argument[0]].Redirect(Array.Empty<string>(), pipe_contents);
                            }
                            else
                            {
                                var list = command_argument.ToList();
                                list.RemoveAt(0);
                                success = redirections[command_argument[0]].Redirect(list.ToArray(), pipe_contents);
                            }
                        }
                        else if (pipe_contents != "")
                        {
                            success = redirections["pipe"].Redirect(command_argument, pipe_contents);
                        }
                        else
                        {
                            System.Console.Out.WriteLine($"{command_argument[0]}: command not found");
                            System.Console.Out.WriteLine($"try 'help' command!");
                        }

                        //
                        //  파이프 후처리
                        //
                        if (!success)
                        {
                            if (Pipe)
                            {
                                System.Console.Out.WriteLine(PipeContents.ToString());
                            }
                            command = Array.Empty<string>();
                        }
                        Pipe = false;
                    }
                }
                catch (Exception e)
                {
                    System.Console.Out.WriteLine($"Error occurred on processing!");
                    System.Console.Out.WriteLine($"Message: {e.Message}");
                    System.Console.Out.WriteLine($"StackTrace: {e.StackTrace}");
                }

                if (GlobalTask != null)
                {
                    await GlobalTask;
                    GlobalTask = null;
                }

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
        }

        public void PrintHelp()
        {
            System.Console.Out.WriteLine($"Koromo Copy Console");
            System.Console.Out.WriteLine($"Copyright (C) 2018. Koromo Copy Developer");
            System.Console.Out.WriteLine($"");
            System.Console.Out.Write($"For more information, please check ");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Out.WriteLine("https://github.com/dc-koromo/koromo-copy/blob/master/Document/Development.md");
            System.Console.ResetColor();
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

        public bool Pipe = false;
        public StringBuilder PipeContents = new StringBuilder();

        public void WriteLine(string contents)
        {
            if (Pipe == true)
                PipeContents.Append(contents);
            else
                System.Console.WriteLine(contents);
        }

        public void WriteLine(object contents)
        {
            WriteLine(Monitor.SerializeObject(contents));
        }

        public void WriteErrorLine(string contents)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Out.Write("error");
            System.Console.ResetColor();
            System.Console.Out.Write(": ");
            System.Console.Out.WriteLine(contents);
        }
    }
}
