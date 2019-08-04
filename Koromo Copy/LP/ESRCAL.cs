/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.LP
{
    /// <summary>
    /// Parser for Extended-SRCAL - Powerful Crawler Descript Language
    /// </summary>
    public class ESRCAL
    {
        static Scanner scanner;
        static ShiftReduceParser pargen;

        private Scanner get_scanner()
        {
            if (scanner != null) return scanner;

            var sg = new ScannerGenerator();

            sg.PushRule("", @"[\r\n ]");
            sg.PushRule("op_open", @"\(");
            sg.PushRule("op_close", @"\)");
            sg.PushRule("pp_open", @"\[");
            sg.PushRule("pp_close", @"\]");
            sg.PushRule("equal", @"\=");
            sg.PushRule("plus", @"\+");
            sg.PushRule("minus", @"\-");
            sg.PushRule("multiple", @"\*");
            sg.PushRule("divide", @"\/");
            sg.PushRule("to", "to");
            sg.PushRule("scolon", ":");
            sg.PushRule("comma", ",");
            sg.PushRule("loop", "loop");
            sg.PushRule("foreach", "foreach");
            sg.PushRule("if", "if");
            sg.PushRule("else", "else");
            sg.PushRule("id", @"[_$a-zA-Z][_$a-zA-Z0-9]*");
            sg.PushRule("num", @"[0-9]+(\.[0-9]+)?([Ee][\+\-]?[0-9]+)?");
            sg.PushRule("str", @"""[_$a-zA-Z0-9\/\?:,\[\]\\#\=&\+\-\*\|\(\)\<\>\.{}! ]*""");

            sg.Generate();

            Console.Console.Instance.WriteLine(sg.PrintDiagram());

            return scanner = sg.CreateScannerInstance();
        }

        /// <summary>
        /// ESRCAL의 파서제너레이터를 생성합니다.
        /// </summary>
        /// <returns></returns>
        private ShiftReduceParser get_pargen()
        {
            if (pargen != null) return pargen;

            var gen = new ParserGenerator();

            // Non-Terminals
            var expr = gen.CreateNewProduction("expr", false);

            // Terminals
            //var id = gen.CreateNewProduction("id");
            var num = gen.CreateNewProduction("num");
            //var str = gen.CreateNewProduction("str");
            var plus = gen.CreateNewProduction("plus");
            var minus = gen.CreateNewProduction("minus");
            var multiple = gen.CreateNewProduction("multiple");
            var divide = gen.CreateNewProduction("divide");
            //var loop = gen.CreateNewProduction("loop");
            var op_open = gen.CreateNewProduction("op_open");
            var op_close = gen.CreateNewProduction("op_close");
            //var pp_open = gen.CreateNewProduction("pp_open"); // [
            //var pp_close = gen.CreateNewProduction("pp_close"); // ]
            //var equal = gen.CreateNewProduction("equal");
            //var to = gen.CreateNewProduction("to");
            //var scolon = gen.CreateNewProduction("scolon");
            //var comma = gen.CreateNewProduction("comma");
            //var _foreach = gen.CreateNewProduction("foreach");
            //var _if = gen.CreateNewProduction("if");
            //var _else = gen.CreateNewProduction("else");
            
            expr |= num + ParserAction.Create(x => x.UserContents = double.Parse(x.Contents));
            expr |= expr + plus + expr + ParserAction.Create(x => x.UserContents = (double)x.Childs[0].UserContents + (double)x.Childs[2].UserContents);
            expr |= expr + minus + expr + ParserAction.Create(x => x.UserContents = (double)x.Childs[0].UserContents - (double)x.Childs[2].UserContents);
            expr |= expr + multiple + expr + ParserAction.Create(x => x.UserContents = (double)x.Childs[0].UserContents * (double)x.Childs[2].UserContents);
            expr |= expr + divide + expr + ParserAction.Create(x => x.UserContents = (double)x.Childs[0].UserContents / (double)x.Childs[2].UserContents);
            expr |= minus + expr + ParserAction.Create(x => x.UserContents = -(double)x.Childs[1].UserContents);
            expr |= op_open + expr + op_close + ParserAction.Create(x => x.UserContents = x.Childs[1].UserContents);

            // right associativity, -
            gen.PushConflictSolver(false, new Tuple<ParserProduction, int>(expr, 5));
            // left associativity, *, /
            gen.PushConflictSolver(true, multiple, divide);
            // left associativity, +, -
            gen.PushConflictSolver(true, plus, minus);

            try
            {
                gen.PushStarts(expr);
                gen.PrintProductionRules();
                gen.GenerateLALR2();
                gen.PrintStates();
                gen.PrintTable();
            }
            catch (Exception e)
            {
                Console.Console.Instance.WriteLine(e.Message);
            }

            Console.Console.Instance.WriteLine(gen.GlobalPrinter.ToString());

            return pargen = gen.CreateShiftReduceParserInstance();
        }


        public void PrintTree(ParsingTree.ParsingTreeNode node, string indent, bool last)
        {
            Console.Console.Instance.Write(indent);
            if (last)
            {
                Console.Console.Instance.Write("+-");
                indent += "  ";
            }
            else
            {
                Console.Console.Instance.Write("|-");
                indent += "| ";
            }

            if (node.Childs.Count == 0)
            {
                Console.Console.Instance.WriteLine(node.Production + " " + node.UserContents);
            }
            else
            {
                Console.Console.Instance.WriteLine(node.Production + " " + node.UserContents);
            }
            for (int i = 0; i < node.Childs.Count; i++)
                PrintTree(node.Childs[i], indent, i == node.Childs.Count - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Compile(string[] lines)
        {
            var pp = get_pargen();
            var ss = get_scanner();

            pp.Clear();

            Action<string, string, int, int> insert = (string x, string y, int a, int b) =>
            {
                pp.Insert(x, y);
                if (pp.Error()) throw new Exception($"[COMPILER] Parser error! L:{a}, C:{b}");
                while (pp.Reduce())
                {
                    var l = pp.LatestReduce();
                    Console.Console.Instance.Write(l.Production.PadLeft(8) + " => ");
                    Console.Console.Instance.WriteLine(string.Join(" ", l.Childs.Select(z => z.Production)));
                    Console.Console.Instance.Write(l.Production.PadLeft(8) + " => ");
                    Console.Console.Instance.WriteLine(string.Join(" ", l.Childs.Select(z => z.Contents)));
                    pp.Insert(x, y);
                    if (pp.Error()) throw new Exception($"[COMPILER] Parser error! L:{a}, C:{b}");
                }
            };

            try
            {
                int ll = 0;
                foreach (var line in lines)
                {
                    ll++;
                    if (line.Trim().StartsWith("##") || line.Trim() == "") continue;
                    ss.AllocateTarget(line.Trim());

                    while (ss.Valid())
                    {
                        var tk = ss.Next();
                        if (ss.Error())
                            throw new Exception("[COMPILER] Tokenize error! '" + tk + "'");
                        insert(tk.Item1, tk.Item2, ll, tk.Item4);
                    }
                }
                if (pp.Error()) throw new Exception();
                insert("$", "$", -1, -1);

                var tree = pp.Tree;
                PrintTree(tree.root, "", true);

                Console.Console.Instance.WriteLine($"Query: {lines[0]}");
                Console.Console.Instance.WriteLine($"Answer: {(double)(tree.root.UserContents)}");
            }
            catch (Exception e)
            {
                Console.Console.Instance.WriteLine(e.Message);
            }

            return "";
        }

    }
}
