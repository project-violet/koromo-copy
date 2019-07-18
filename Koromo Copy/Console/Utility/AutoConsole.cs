/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Fs;
using Koromo_Copy.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Console.Utility
{
    /// <summary>
    /// 다운로드 콘솔 옵션입니다.
    /// </summary>
    public class AutoConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-load-dir", CommandType.ARGUMENTS, Help = "use -load-dir <Directory Name>",
            Info = "Load directory information.")]
        public string[] LoadDir;

        [CommandLine("-overlap", CommandType.OPTION, Help = "use -overlap",
            Info = "Find overlapping directory names.")]
        public bool Overlap;

        [CommandLine("-move", CommandType.ARGUMENTS, Help = "use -move <From Directory Name>",
            Info = "Load directory information.")]
        public string[] Move;
    }

    /// <summary>
    /// Auto 콘솔입니다.
    /// </summary>
    public class AutoConsole : ILazy<AutoConsole>, IConsole
    {
        /// <summary>
        /// Auto 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            AutoConsoleOption option = CommandLineParser<AutoConsoleOption>.Parse(arguments);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.Instance.WriteLine(option.HelpMessage);
                return false;
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.LoadDir != null)
            {
                ProcessLoadDir(option.LoadDir);
            }
            else if (option.Overlap)
            {
                ProcessOverlap();
            }
            else if (option.Move != null)
            {
                ProcessMove(option.Move);
            }

            return true;
        }

        bool IConsole.Redirect(string[] arguments, string contents)
        {
            return Redirect(arguments, contents);
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "Auto Console - Helper for File Management\r\n" + 
                "\r\n"
                );

            var builder = new StringBuilder();
            CommandLineParser<AutoConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} ({x.Value.Item2.Help}) : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                    else
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                });
            Console.Instance.WriteLine(builder.ToString());
        }

        static List<string> dirs;
        static void ProcessLoadDir(string[] args)
        {
            Console.Instance.GlobalTask.Add(Task.Run(async () =>
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.Instance.WriteErrorLine($"'{args[0]}' is not valid directory name.");
                    return;
                }

                var indexor = new FileIndexor();
                indexor.OnlyListing = true;
                await indexor.ListingDirectoryAsync(args[0]);

                dirs = indexor.GetDirectories();
            }));
        }

        static void ProcessOverlap()
        {
            var overlap = new Dictionary<string, List<string>>(); 

            foreach (var dir in dirs)
            {
                var d = Path.GetFileName(Path.GetDirectoryName(dir));
                if (!overlap.ContainsKey(d))
                    overlap.Add(d, new List<string>());
                overlap[d].Add(dir);
            }

            foreach (var ov in overlap)
            {
                if (ov.Value.Count > 1)
                {
                    foreach (var dd in ov.Value)
                        Console.Instance.WriteLine(dd);
                    Console.Instance.WriteLine("");
                }
            }
        }

        /// <summary>
        /// 폴더를 옮깁니다.
        /// </summary>
        /// <param name="args"></param>
        static void ProcessMove(string[] args)
        {
            var overlap = new Dictionary<string, List<string>>();
            var valid = new HashSet<string>();

            foreach (var dir in dirs)
            {
                var d = Path.GetFileName(Path.GetDirectoryName(dir));
                if (!overlap.ContainsKey(d))
                    overlap.Add(d, new List<string>());
                overlap[d].Add(dir);
                valid.Add(d);
            }

            var indexor = new FileIndexor();
            indexor.OnlyListing = true;
            Task.Run(async () => await indexor.ListingDirectoryAsync(args[0])).Wait();

            foreach (var dir in indexor.GetDirectories())
            {
                //if (Directory.GetDirectories(dir).Length == 0 && Directory.GetFiles(dir).Length == 0)
                //{
                //    Directory.Delete(dir);
                //    Console.Instance.WriteLine("[DELETE] " + dir);
                //    continue;
                //}

                var d = Path.GetFileName(Path.GetDirectoryName(dir));

                if (valid.Contains(d))
                {
                    if (overlap[d].Count == 1)
                    {
                        var del = true;
                        foreach (string fi in Directory.GetFiles(dir))
                        {
                            var filename = Path.GetFileName(fi);

                            if (File.Exists(Path.Combine(overlap[d][0], filename)))
                            {
                                Console.Instance.WriteLine("File already exists " + d + " " + filename);
                                if (new FileInfo(fi).Length <= new FileInfo(Path.Combine(overlap[d][0], filename)).Length * 1.1)
                                {
                                    Console.Instance.WriteLine("Same length " + new FileInfo(fi).Length);
                                    File.Delete(fi);
                                    Console.Instance.WriteLine("[DELETE FILE] " + fi);
                                    continue;
                                }
                                del = false;
                                continue;
                            }

                            File.Move(fi, Path.Combine(overlap[d][0], filename));
                            Console.Instance.WriteLine("[MOVE] " + fi + " => " + Path.Combine(overlap[d][0], filename));
                        }

                        if (del && Directory.GetDirectories(dir).Length == 0)
                        {
                            Directory.Delete(dir);
                            Console.Instance.WriteLine("[DELETE] " + dir);
                        }
                    }
                    else
                    {
                        Console.Instance.WriteLine("Cannot move " + d);
                    }
                }
                else
                {
                    Console.Instance.WriteLine("Not found " + d);
                }
            }
        }
    }
}
