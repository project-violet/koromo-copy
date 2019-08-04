/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.Interface;
using Koromo_Copy.JS;
using Koromo_Copy.LP;
using Koromo_Copy.LP.Lang;
using System;
using System.Collections.Generic;
using System.IO;
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

        [CommandLine("-regex", CommandType.ARGUMENTS, ArgumentsCount = 2, Help = "use -regex <Pattern> <Target>",
            Info = "Verify that the target string can be derived from the pattern.")]
        public string[] Regex;

        [CommandLine("--pargen-sample", CommandType.OPTION, DefaultArgument = true, Help = "use --pargen-sample",
            Info = "Print parser generator sample result table.")]
        public bool PSGample;

        [CommandLine("-test", CommandType.ARGUMENTS, Pipe = true, DefaultArgument = true)]
        public string[] Test;
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
            else if (option.Regex != null)
            {
                ProcessRegex(option.Regex);
            }
            else if (option.PSGample)
            {
                ProcessPGSample();
            }
            else if (option.Test != null)
            {
                ProcessTest(option.Test);
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

        static void ProcessRegex(string[] args)
        {
            var sr = new SimpleRegex(args[0]);
            var cur = sr.Diagram.start_node;

            Console.Instance.WriteLine(sr.PrintDiagram());

            for (int i = 0; i < args[1].Length; i++)
            {
                var ch = args[1][i];
                if (cur.transition.Any(x => x.Item1 == ch))
                {
                    cur = cur.transition.Where(x => x.Item1 == ch).ElementAt(0).Item2;
                }
                else
                {
                    Console.Instance.WriteLine("Error! A token that can not be derived. Char='" + ch + "', POS=" + i);
                    return;
                }
            }

            if (cur.is_acceptable)
                Console.Instance.WriteLine("Matching pattern!");
            else
                Console.Instance.WriteLine("Partial matching pattern! End=" + cur.index);
        }

        static void ProcessPGSample()
        {
            var gen = new ParserGenerator();

            // Non-Terminals
            var E = gen.CreateNewProduction("E", false);
            //var T = gen.CreateNewProduction("T", false);
            //var F = gen.CreateNewProduction("F", false);
            //var func = gen.CreateNewProduction("func", false);
            //var arguments = gen.CreateNewProduction("args", false);

            // Terminals
            var plus = gen.CreateNewProduction("+");         // +
            var minus = gen.CreateNewProduction("-");       // -
            var multiple = gen.CreateNewProduction("*"); // *
            var divide = gen.CreateNewProduction("/");     // /
            //var id = gen.CreateNewProduction("id");             // [_$a-zA-Z][_$a-zA-Z0-9]*
            var op_open = gen.CreateNewProduction("(");   // (
            var op_close = gen.CreateNewProduction(")"); // )
            var num = gen.CreateNewProduction("num");           // [0-9]+
            //var split = gen.CreateNewProduction("split");       // ,

            //exp |= exp + plus + term;
            //exp |= exp + minus + term;
            //exp |= term;
            //term |= term + multiple + factor;
            //term |= term + divide + factor;
            //term |= factor;
            //factor |= op_open + exp + op_close;
            //factor |= num;
            //factor |= id;
            //factor |= func;
            //func |= id + op_open + arguments + op_close;
            //arguments |= id;
            //arguments |= arguments + split + id;
            //arguments |= ParserGenerator.EmptyString;

            E |= E + plus + E + ParserAction.Create(x => { }); ;
            E |= E + minus + E + ParserAction.Create(x => { }); ;
            E |= E + multiple + E + ParserAction.Create(x => { }); ;
            E |= E + divide + E + ParserAction.Create(x => { }); ;
            E |= minus + E + ParserAction.Create(x => { }); ;
            E |= op_open + E + op_close + ParserAction.Create(x => { }); ;
            E |= num + ParserAction.Create(x => { }); ;

            gen.PushConflictSolver(false, new Tuple<ParserProduction, int>(E, 4));
            gen.PushConflictSolver(true, multiple, divide);
            gen.PushConflictSolver(true, plus, minus);

            gen.PushStarts(E);
            gen.PrintProductionRules();
            gen.GenerateLALR2();
            gen.PrintStates();
            gen.PrintTable();

            Console.Instance.WriteLine(gen.GlobalPrinter.ToString());
            Console.Instance.WriteLine(gen.CreateShiftReduceParserInstance().ToCSCode("Calculator"));

            //////////////////////////////////////////////////////

            //var scanner_gen = new ScannerGenerator();
            //
            //scanner_gen.PushRule("", @"[\r\n ]");  // Skip characters
            //scanner_gen.PushRule("+", @"\+");
            //scanner_gen.PushRule("-", @"\-");
            //scanner_gen.PushRule("*", @"\*");
            //scanner_gen.PushRule("/", @"\/");
            //scanner_gen.PushRule("(", @"\(");
            //scanner_gen.PushRule(")", @"\)");
            //scanner_gen.PushRule("num", @"[0-9]+(\.[0-9]+)?([Ee][\+\-]?[0-9]+)?");
            //scanner_gen.Generate();
            //var ss = scanner_gen.CreateScannerInstance();
            //var pp = gen.CreateShiftReduceParserInstance();
            //
            //Action<string, string, int, int> insert = (string x, string y, int a, int b) =>
            //{
            //    pp.Insert(x, y);
            //    if (pp.Error()) throw new Exception($"[COMPILER] Parser error! L:{a}, C:{b}");
            //    while (pp.Reduce())
            //    {
            //        var l = pp.LatestReduce();
            //        Console.Instance.Write(l.Production.PadLeft(8) + " => ");
            //        Console.Instance.WriteLine(string.Join(" ", l.Childs.Select(z => z.Production)));
            //        Console.Instance.Write(l.Production.PadLeft(8) + " => ");
            //        Console.Instance.WriteLine(string.Join(" ", l.Childs.Select(z => z.Contents)));
            //        pp.Insert(x, y);
            //        if (pp.Error()) throw new Exception($"[COMPILER] Parser error! L:{a}, C:{b}");
            //    }
            //};
            //
            //try
            //{
            //    int ll = 0;
            //    var line = "5-(4+2*3-1)/(6+-5)";
            //        ss.AllocateTarget(line.Trim());
            //
            //        while (ss.Valid())
            //        {
            //            var tk = ss.Next();
            //            if (ss.Error())
            //                throw new Exception("[COMPILER] Tokenize error! '" + tk + "'");
            //            insert(tk.Item1, tk.Item2, ll, tk.Item4);
            //        }
            //
            //    if (pp.Error()) throw new Exception();
            //    insert("$", "$", -1, -1);
            //
            //    var tree = pp.Tree;
            //    CALtoCS.PrintTree(tree.root, "", true);
            //}
            //catch (Exception e)
            //{
            //    Console.Instance.WriteLine(e.Message);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        static void ProcessTest(string[] args)
        {
            switch (args[0].ToInt32())
            {
                case 1:
                    var c2c = new CALtoCS();
                    c2c.Compile(File.ReadAllLines("script/danbooru-pages.srcal"));
                    break;

                case 2:
                    var cal = new ESRCAL();
                    cal.Compile(new[] { "5+4-(-4*(2-4)*2)/3+-(-(2*2+3)-2)*(3+1)" });
                    //cal.Compile(new[] { "(2*2+3)" });
                    break;

                case 3:
                    var x = new HTMLParser();
                    
                    break;

                case 4:
                    var p = JSParserGenerator.Parser;

                    break;

                case 5:
                    var s = JSScannerGenerator.Scanner;
                    break;
            }
        }
        
    }
}
