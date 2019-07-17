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
using System.Runtime.InteropServices;
using System.Security.Permissions;
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
        static extern bool FreeConsole();

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

            System.Console.Out.WriteLine(@" _  __                                                _____                         ");
            System.Console.Out.WriteLine(@"| |/ /                                               / ____|                        ");
            System.Console.Out.WriteLine(@"| ' /    ___    _ __    ___    _ __ ___     ___     | |        ___    _ __    _   _ ");
            System.Console.Out.WriteLine(@"|  <    / _ \  | '__|  / _ \  | '_ ` _ \   / _ \    | |       / _ \  | '_ \  | | | |");
            System.Console.Out.WriteLine(@"| . \  | (_) | | |    | (_) | | | | | | | | (_) |   | |____  | (_) | | |_) | | |_| |");
            System.Console.Out.WriteLine(@"|_|\_\  \___/  |_|     \___/  |_| |_| |_|  \___/     \_____|  \___/  | .__/   \__, |");
            System.Console.Out.WriteLine(@"                                                                     | |       __/ |");
            System.Console.Out.WriteLine(@"                                                                     |_|      |___/ ");

            System.Console.Title = "Koromo Copy Console";
            System.Console.Out.WriteLine($"Koromo Copy {Version.Text}");
            System.Console.Out.WriteLine("Copyright (C) 2018-2019. Koromo Copy Developer");
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
                Instance.PromptToken?.Cancel();
                Instance.ConsoleToken?.Cancel();
                e.Cancel = true;
            }
        }

        Thread console_thread;
        public void Start()
        {
            console_thread = new Thread(Loop);
            console_thread.Start();
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void Stop()
        {
            PromptToken?.Cancel();
            ConsoleToken?.Cancel();
            PromptThread?.Abort();
            console_thread?.Abort();
            FreeConsole();
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
        public List<Task> GlobalTask = new List<Task>();

        /// <summary>
        /// 명령 히스토리 입니다.
        /// </summary>
        public List<string> History = new List<string>();

        /// <summary>
        /// 프롬프트입니다.
        /// </summary>
        /// <returns></returns>
        public void Prompt()
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

            commandLine = System.Console.In.ReadLine();
        }

        public string commandLine;
        public Task PromptTask;
        public CancellationTokenSource PromptToken;
        public Thread PromptThread;
        public Task ConsoleTask;
        public CancellationTokenSource ConsoleToken;

        public Dictionary<string, IConsole> redirections;

        public Action RedirectionAfterLoopInit;

        /// <summary>
        /// 콘솔 스레드의 메인 루프입니다.
        /// </summary>
        public void Loop()
        {
            redirections = new Dictionary<string, IConsole>()
            {
                // normal command
                {"hitomi", new HitomiConsole()},
                {"exh", new ExHentaiConsole()},
                {"pixiv", new PixivConsole()},
                {"in", new InConsole()},
                {"internal", new InternalConsole()},
                {"hiyobi", new HiyobiConsole()},
                {"youtube", new YoutubeConsole()},
                {"script", new ScriptConsole()},
                {"dcinside", new DCInsideConsole()},
                {"lp", new LPConsole()},
                {"kss", new KSSConsole()},

                // utility command
                {"scan", new Utility.ScanConsole()},
                {"run", new Utility.RunConsole()},
                {"selenium", new Utility.SeleniumConsole()},
                {"down", new Utility.DownloadConsole()},
                {"test", new Utility.TestConsole()},
                {"auto", new Utility.AutoConsole()},

                // pipeline command
                {"grep", new GrepConsole()},
                {"out", new OutConsole()}
            };
            
            RedirectionAfterLoopInit?.Invoke();

            System.Console.Out.WriteLine("");

            while (true)
            {
                //PromptToken = new CancellationTokenSource();
                //PromptTask = Task.Factory.StartNew(() => Prompt(), PromptToken.Token);
                //PromptTask.Wait();
                PromptThread = new Thread(Prompt);
                PromptThread.Start();
                PromptThread.Join();
                PromptThread = null;

                ConsoleToken = new CancellationTokenSource();
                ConsoleTask = Task.Factory.StartNew(() =>
                {
                    if (commandLine == null)
                    {
                        System.Console.Out.WriteLine("");
                        return;
                    }

                    try
                    {
                        History.Add(commandLine);
                        string[] command = ParseArgument(commandLine);
                        PipeContents.Clear();

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
                            else if (command.Contains("|>"))
                            {
                                Pipe = true;
                                LoopPipe = true;

                                int meet_pipe = Array.FindIndex(command, w => w == "|>");
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
                            else if (command_argument[0] == "history")
                            {
                                PrintHistory();
                            }
                            else if (command_argument[0] == "exit")
                            {
                                Environment.Exit(0);
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
                            //  모든 태스크가 끝날때까지 기다림
                            //
                            if (GlobalTask.Count != 0)
                            {
                                while (GlobalTask.Count > 0)
                                {
                                    GlobalTask.RemoveAll(x => x.Status == TaskStatus.RanToCompletion || x.Status == TaskStatus.Faulted);
                                    for (int i = 0; i < GlobalTask.Count; i++)
                                    {
                                        Task task = GlobalTask[i];
                                        if (task != null && task.Status != TaskStatus.Faulted && task.Status != TaskStatus.RanToCompletion)
                                            task.Wait();
                                    }
                                }
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
                }, ConsoleToken.Token);
                try { ConsoleTask.Wait(); } catch { }

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
        }

        #region 출력부 - 기본 텍스트 출력

        public void PrintHelp()
        {
            System.Console.Out.WriteLine($"Koromo Copy Console");
            System.Console.Out.WriteLine($"Copyright (C) 2018-2019. Koromo Copy Developer");
            System.Console.Out.WriteLine($"");
            System.Console.Out.Write($"For more information, please check ");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Out.WriteLine("https://github.com/dc-koromo/koromo-copy/blob/master/Document/Development.md\r\n");
            System.Console.ResetColor();

            foreach (var pair in redirections)
                System.Console.Out.WriteLine($"{pair.Key.PadRight(12)} [{pair.Value.ToString()}]");
        }

        public void PrintHistory()
        {
            for (int i = 0; i < History.Count; i++)
            {
                WriteLine($"{i}: {History[i]}");
            }
        }

        public void Push(DateTime dt, string contents)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            //System.Console.Out.WriteLine("");
            CultureInfo en = new CultureInfo("en-US");
            System.Console.Out.Write($"[{dt.ToString(en)}]");
            System.Console.ResetColor();
            System.Console.Out.WriteLine($" {contents}");
        }

        public bool Pipe = false;
        public bool LoopPipe = false;
        public StringBuilder PipeContents = new StringBuilder();

        public void WriteLine(string contents, bool crlf = true)
        {
            if (Pipe == true)
            {
                if (crlf)
                    PipeContents.Append(contents + "\r\n");
                else
                    PipeContents.Append(contents);
            }
            else
                System.Console.WriteLine(contents);
        }

        public void Write(string contents)
        {
            if (Pipe == true)
                PipeContents.Append(contents);
            else
                System.Console.Write(contents);
        }

        public void WriteLine(object contents)
        {
            WriteLine(Monitor.SerializeObject(contents), false);
        }

        public void WriteErrorLine(string contents)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Out.Write("error");
            System.Console.ResetColor();
            System.Console.Out.Write(": ");
            System.Console.Out.WriteLine(contents);
        }

        public class ConsoleProgressBar : IDisposable
        {

            public float CurrentProgress => writer.CurrentProgress;

            private TextWriter OriginalWriter;
            private ProgressWriter writer;

            public ConsoleProgressBar()
            {
                OriginalWriter = System.Console.Out;
                writer = new ProgressWriter(OriginalWriter);
                System.Console.SetOut(writer);
            }

            public void Dispose()
            {
                System.Console.SetOut(OriginalWriter);
                writer.ClearProgressBar();
            }

            public void SetProgress(float f)
            {
                writer.CurrentProgress = f;
                writer.RedrawProgress();
            }
            public void SetProgress(int i)
            {
                SetProgress((float)i);
            }

            public void Increment(float f)
            {
                writer.CurrentProgress += f;
                writer.RedrawProgress();
            }

            public void Increment(int i)
            {
                Increment((float)i);
            }

            private class ProgressWriter : TextWriter
            {

                public override Encoding Encoding => Encoding.UTF8;
                public float CurrentProgress
                {
                    get { return _currentProgress; }
                    set
                    {
                        _currentProgress = value;
                        if (_currentProgress > 100)
                        {
                            _currentProgress = 100;
                        }
                        else if (CurrentProgress < 0)
                        {
                            _currentProgress = 0;
                        }
                    }
                }

                private float _currentProgress = 0;
                private TextWriter consoleOut;
                private const int AllocatedTemplateSpace = 21;
                private object SyncLock = new object();
                public ProgressWriter(TextWriter _consoleOut)
                {
                    consoleOut = _consoleOut;
                    RedrawProgress();
                }

                private void DrawProgressBar()
                {
                    lock (SyncLock)
                    {
                        int avalibleSpace = System.Console.BufferWidth - AllocatedTemplateSpace;
                        int percentAmmount = (int)(avalibleSpace * (CurrentProgress / 100));
                        var col = System.Console.ForegroundColor;
                        var bak = System.Console.BackgroundColor;
                        System.Console.BackgroundColor = ConsoleColor.Green;
                        System.Console.ForegroundColor = ConsoleColor.Black;
                        string progressBar = string.Concat(new string('#', percentAmmount), new string('.', avalibleSpace - percentAmmount));
                        consoleOut.Write($"Progress: [{((int)(CurrentProgress)).ToString().PadLeft(3, ' ')}%]");
                        System.Console.ForegroundColor = col;
                        System.Console.BackgroundColor = bak;
                        consoleOut.Write($" [{progressBar}]");
                    }
                }

                public void RedrawProgress()
                {
                    lock (SyncLock)
                    {
                        int LastLineWidth = System.Console.CursorLeft;
                        var consoleH = System.Console.WindowTop + System.Console.WindowHeight - 1;
                        System.Console.SetCursorPosition(0, consoleH);
                        DrawProgressBar();
                        System.Console.SetCursorPosition(LastLineWidth, consoleH - 1);
                    }
                }

                private void ClearLineEnd()
                {
                    lock (SyncLock)
                    {
                        int lineEndClear = System.Console.BufferWidth - System.Console.CursorLeft - 1;
                        consoleOut.Write(new string(' ', lineEndClear));
                    }
                }

                public void ClearProgressBar()
                {
                    lock (SyncLock)
                    {
                        int LastLineWidth = System.Console.CursorLeft;
                        var consoleH = System.Console.WindowTop + System.Console.WindowHeight - 1;
                        System.Console.SetCursorPosition(0, consoleH);
                        ClearLineEnd();
                        System.Console.SetCursorPosition(LastLineWidth, consoleH - 1);
                    }
                }

                public override void Write(char value)
                {
                    lock (SyncLock)
                    {
                        consoleOut.Write(value);
                    }
                }

                public override void Write(string value)
                {
                    lock (SyncLock)
                    {
                        consoleOut.Write(value);
                    }
                }

                public override void WriteLine(string value)
                {
                    lock (SyncLock)
                    {
                        consoleOut.Write(value);
                        consoleOut.Write(Environment.NewLine);
                        ClearLineEnd();
                        consoleOut.Write(Environment.NewLine);
                        RedrawProgress();
                    }
                }

                public override void WriteLine(string format, params object[] arg)
                {
                    WriteLine(string.Format(format, arg));
                }

                public override void WriteLine(int i)
                {
                    WriteLine(i.ToString());
                }

            }

        }
        #endregion
    }
}
