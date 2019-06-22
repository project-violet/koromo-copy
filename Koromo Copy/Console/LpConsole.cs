/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Koromo_Copy.LP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// Script 콘솔 옵션입니다.
    /// </summary>
    public class LPConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-kor-to-eng", CommandType.ARGUMENTS, DefaultArgument = true, Help = "use -kor-to-eng <Korean Sentence>",
            Info = "Replace Hangul with alphabetic characters in the form of a english-typing.")]
        public string[] Kor2Eng;
        [CommandLine("-eng-to-kor", CommandType.ARGUMENTS, DefaultArgument = true, Help = "use -eng-to-kor <English Sentence>",
            Info = "Replace English-Typing with alphabetic characters in the form of a korean.")]
        public string[] Eng2Kor;

        [CommandLine("--pargen-sample", CommandType.OPTION, DefaultArgument = true, Help = "use --pargen-sample",
            Info = "Print parser generator sample result table.")]
        public bool PSGample;

        [CommandLine("--test", CommandType.OPTION, Pipe = true, DefaultArgument = true)]
        public bool Test;
    }

    /// <summary>
    /// </summary>
    public class LPConsole : IConsole
    {
        /// <summary>
        /// Script 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            LPConsoleOption option = CommandLineParser<LPConsoleOption>.Parse(arguments);

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
            else if (option.Kor2Eng != null)
            {
                ProcessKor2Eng(option.Kor2Eng);
            }
            else if (option.Eng2Kor != null)
            {
                ProcessEng2Kor(option.Eng2Kor);
            }
            else if (option.PSGample)
            {
                ProcessPGSample();
            }
            else if (option.Test)
            {
                ProcessTest();
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
                "Langluage Processing Console\r\n" +
                "\r\n"
                );

            var builder = new StringBuilder();
            CommandLineParser<LPConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} ({x.Value.Item2.Help}) : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                    else
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                });
            Console.Instance.WriteLine(builder.ToString());
        }

        static void ProcessKor2Eng(string[] args)
        {
            var builder = new StringBuilder();
            foreach (var alp in args[0])
                if (LPKor.IsHangul(alp) || LPKor.IsHangulJamo11(alp) || LPKor.IsHangulJamo31(alp))
                    builder.Append(LPKor.Disassembly(alp));
                else
                    builder.Append(alp);
            Console.Instance.WriteLine(builder.ToString());
        }

        static void ProcessEng2Kor(string[] args)
        {
            Console.Instance.WriteLine(LPKor.Assembly(args[0]));
        }

        static void ProcessPGSample()
        {
            var gen = new ParserGenerator();

            // Non-Terminals
            var exp = gen.CreateNewProduction("exp", false);
            var term = gen.CreateNewProduction("term", false);
            var factor = gen.CreateNewProduction("factor", false);
            var func = gen.CreateNewProduction("func", false);
            var arguments = gen.CreateNewProduction("args", false);

            // Terminals
            var plus = gen.CreateNewProduction("plus");         // +
            var minus = gen.CreateNewProduction("minus");       // -
            var multiple = gen.CreateNewProduction("multiple"); // *
            var divide = gen.CreateNewProduction("divide");     // /
            var id = gen.CreateNewProduction("id");             // [_$a-zA-Z][_$a-zA-Z0-9]*
            var op_open = gen.CreateNewProduction("op_open");   // (
            var op_close = gen.CreateNewProduction("op_close"); // )
            var num = gen.CreateNewProduction("num");           // [0-9]+
            var split = gen.CreateNewProduction("split");       // ,

            exp |= exp + plus + term;
            exp |= exp + minus + term;
            exp |= term;
            term |= term + multiple + factor;
            term |= term + divide + factor;
            term |= factor;
            factor |= op_open + exp + op_close;
            factor |= num;
            factor |= id;
            factor |= func;
            func |= id + op_open + arguments + op_close;
            arguments |= id;
            arguments |= arguments + split + id;
            arguments |= ParserGenerator.EmptyString;

            gen.PushStarts(exp);
            gen.GenerateLALR();
            gen.PrintStates();
            gen.PrintTable();

            Console.Instance.WriteLine(gen.GlobalPrinter.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        static void ProcessTest()
        {
        }
        
    }
}
