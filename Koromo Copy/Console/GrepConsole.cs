/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using System;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// Grep 콘솔 옵션입니다.
    /// </summary>
    public class GrepConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--input", CommandType.ARGUMENTS, Pipe = true, Info = "Set input text.")]
        public string[] Input;

        [CommandLine("--p", CommandType.ARGUMENTS, DefaultArgument = true, Info = "Set pattern text.")]
        public string[] Pattern;

        [CommandLine("-i", CommandType.OPTION, Info = "Ignore case. Case insensitive search.")]
        public bool IgnoreCase;
        [CommandLine("-r", CommandType.OPTION, Info = "Search using regular expression.")]
        public bool UsingRegex;
        [CommandLine("-n", CommandType.OPTION, Info = "Show line numbers on results.")]
        public bool ShowNumber;
    }
    
    /// <summary>
    /// 특정 구문을 찾기위한 Grep 콘솔 클래스입니다.
    /// </summary>
    public class GrepConsole : IConsole
    {
        /// <summary>
        /// Grep 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            arguments = CommandLineUtil.InsertWeirdArguments<GrepConsoleOption>(arguments, contents != "", "--p");
            GrepConsoleOption option = CommandLineParser<GrepConsoleOption>.Parse(arguments, contents != "", contents);

            if (option.Input == null)
            {
                option.Input = new[] { contents };
            }

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
            else if (option.Input == null)
            {
                Console.Instance.WriteErrorLine("Input is empty.");
            }
            else if (option.Pattern == null)
            {
                Console.Instance.WriteErrorLine("Pattern is empty.");
            }
            else
            {
                ProcessGrep(option.Input[0], option.Pattern[0], option.UsingRegex, option.ShowNumber, option.IgnoreCase);
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
                "Grep Pipeline\r\n" +
                "This command must be used with pipe.\r\n" +
                "\r\n" +
                " -i : Ignore case. Case insensitive search.\r\n" +
                " -r : Search using regular expression.\r\n" +
                " -n : Show number on results\r\n" +
                "You can combine -i, -r, -n options like -ir, -nr, -rni."
                );
        }

        static void ProcessGrep(string contents, string pattern, bool using_regex, bool show_number, bool ignore_case)
        {
            string[] lines = contents.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "") continue;
                string match_string = pattern;
                if (using_regex)
                {
                    var match = Regex.Match(lines[i], pattern, ignore_case ? RegexOptions.IgnoreCase : RegexOptions.None);
                    if (match.Success == false) continue;
                    match_string = match.Value;
                }
                else
                {
                    if (!ignore_case && !(lines[i]?.IndexOf(pattern) >= 0)) continue;
                    if (ignore_case && !(lines[i]?.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                }

                int offset;
                if (!ignore_case)
                    offset = lines[i].IndexOf(match_string);
                else
                    offset = lines[i].IndexOf(match_string, StringComparison.OrdinalIgnoreCase);

                string p1 = lines[i].Remove(offset);
                string p2 = lines[i].Substring(offset, match_string.Length);
                string p3 = lines[i].Substring(offset + match_string.Length);

                if (show_number)
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Instance.Write($"{i+1}");
                    System.Console.ResetColor();
                    Console.Instance.Write($":");
                }

                Console.Instance.Write(p1);
                System.Console.ForegroundColor = ConsoleColor.Red;
                Console.Instance.Write(p2);
                System.Console.ResetColor();
                Console.Instance.WriteLine(p3);
            }
        }
    }
}
