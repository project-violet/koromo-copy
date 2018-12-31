/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Fs;
using Koromo_Copy.Interface;
using System.IO;
using System.Threading.Tasks;

namespace Koromo_Copy.Console.Utility
{
    /// <summary>
    /// Scan 콘솔 옵션입니다.
    /// </summary>
    public class ScanConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--dir", CommandType.ARGUMENTS, Pipe = true, DefaultArgument = true, Help = "--dir <directory> : Target directory")]
        public string[] Directory;

        [CommandLine("-r", CommandType.OPTION, Help = "-r : Recursive")]
        public bool Recursive;

        [CommandLine("-f", CommandType.OPTION, Help = "-f : Scan with files")]
        public bool WithFiles;
    }

    /// <summary>
    /// 파일시스템 열거자입니다.
    /// </summary>
    public class ScanConsole : IConsole
    {
        /// <summary>
        /// Scan 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            arguments = CommandLineUtil.InsertWeirdArguments<ScanConsoleOption>(arguments, true, "--dir");
            ScanConsoleOption option = CommandLineParser<ScanConsoleOption>.Parse(arguments);

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
            else if (option.Directory != null)
            {
                ProcessDirectory(option.Directory, option.Recursive, option.WithFiles);
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
                "Scan Console - File system enumberator\r\n" +
                "\r\n" +
                " -dir <directory> : Target directory\r\n" +
                " -r : Recursive\r\n" +
                " -f : Scan with files\r\n"
                );
        }

        /// <summary>
        /// 특정된 옵션에 따라 파일 또는 폴더를 나열합니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="recursive"></param>
        /// <param name="with_files"></param>
        static void ProcessDirectory(string[] args, bool recursive, bool with_files)
        {
            Console.Instance.GlobalTask.Add(Task.Run(async () =>
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.Instance.WriteErrorLine($"'{args[0]}' is not valid directory name.");
                    return;
                }

                if (!recursive)
                {
                    foreach (DirectoryInfo d in new DirectoryInfo(args[0]).GetDirectories())
                        Console.Instance.WriteLine(d.FullName);

                    if (with_files)
                    {
                        foreach (FileInfo f in new DirectoryInfo(args[0]).GetFiles())
                            Console.Instance.WriteLine(f.FullName);
                    }
                }
                else
                {
                    var indexor = new FileIndexor();
                    await indexor.ListingDirectoryAsync(args[0]);

                    FileIndexorNode node = indexor.GetRootNode();
                    foreach (FileIndexorNode n in node.Nodes)
                    {
                        Console.Instance.WriteLine(n.Path);
                        internal_listing(n, with_files);
                    }
                    if (with_files)
                    {
                        foreach (FileInfo f in new DirectoryInfo(node.Path).GetFiles())
                            Console.Instance.WriteLine(f.FullName);
                    }
                }
            }));
        }

        static void internal_listing(FileIndexorNode node, bool with_files)
        {
            foreach (FileIndexorNode n in node.Nodes)
            {
                Console.Instance.WriteLine(n.Path);
                internal_listing(n, with_files);
            }
            if (with_files)
            {
                foreach (FileInfo f in new DirectoryInfo(node.Path).GetFiles())
                    Console.Instance.WriteLine(f.FullName);
            }
        }
    }
}
