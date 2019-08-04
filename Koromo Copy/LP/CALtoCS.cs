/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.LP.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.LP
{
    /// <summary>
    /// SRCAL문법을 C#문법으로 컴파일하는 도구집합입니다.
    /// </summary>
    public class CALtoCS
    {
        /// <summary>
        /// SRCAL문법을 C#으로 바꿉니다.
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
            }
            catch (Exception e)
            {
                Console.Console.Instance.WriteLine(e.Message);
            }

            Console.Console.Instance.WriteLine(pp.ToCSCode("CALCS"));

            return "";
        }

        public static void PrintTree(ParsingTree.ParsingTreeNode node, string indent, bool last)
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
                Console.Console.Instance.WriteLine(node.Production + " " + node.Contents);
            }
            else
            {
                Console.Console.Instance.WriteLine(node.Production);
            }
            for (int i = 0; i < node.Childs.Count; i++)
                PrintTree(node.Childs[i], indent, i == node.Childs.Count - 1);
        }

        static Scanner scanner;
        static ExtendedShiftReduceParser pargen;

        /// <summary>
        /// SRCAL의 파서제너레이터를 생성합니다.
        /// </summary>
        /// <returns></returns>
        private ExtendedShiftReduceParser get_pargen()
        {
            if (pargen != null) return pargen;

            var gen = new ParserGenerator();

            // Non-Terminals
            var script = gen.CreateNewProduction("script", false);
            var line = gen.CreateNewProduction("line", false);
            var lines = gen.CreateNewProduction("lines", false);
            var expr = gen.CreateNewProduction("expr", false);
            var block = gen.CreateNewProduction("block", false);
            var iblock = gen.CreateNewProduction("iblock", false);
            var index = gen.CreateNewProduction("index", false);
            var variable = gen.CreateNewProduction("variable", false);
            var argument = gen.CreateNewProduction("argument", false);
            var function = gen.CreateNewProduction("function", false);
            var runnable = gen.CreateNewProduction("runnable", false);

            // Terminals
            var name = gen.CreateNewProduction("name");
            var _const = gen.CreateNewProduction("const"); // number | string
            var loop = gen.CreateNewProduction("loop");
            var op_open = gen.CreateNewProduction("op_open");
            var op_close = gen.CreateNewProduction("op_close");
            var pp_open = gen.CreateNewProduction("pp_open"); // [
            var pp_close = gen.CreateNewProduction("pp_close"); // ]
            var equal = gen.CreateNewProduction("equal");
            var to = gen.CreateNewProduction("to");
            var scolon = gen.CreateNewProduction("scolon");
            var comma = gen.CreateNewProduction("comma");
            var plus = gen.CreateNewProduction("plus");         // +
            var minus = gen.CreateNewProduction("minus");       // -
            var multiple = gen.CreateNewProduction("multiple"); // *
            var divide = gen.CreateNewProduction("divide");     // /
            var _foreach = gen.CreateNewProduction("foreach");
            var _if = gen.CreateNewProduction("if");
            var _else = gen.CreateNewProduction("else");

            script |= lines + ParserAction.Create((m, f, b, x) => {
                var module = new LPModule();
                var sfunc = module.CreateFunction("start");
                var bb = sfunc.CreateBasicBlock();
                x.Childs[0].Action(module, sfunc, bb, x.Childs[0]);
                x.UserContents = module;
            });
            script |= ParserGenerator.EmptyString + ParserAction.Create((m, f, b, x) => {
                x.UserContents = new LPModule();
            });

            block |= pp_open + iblock + pp_close + ParserAction.Create((m, f, b, x) => { });
            block |= line + ParserAction.Create((m, f, b, x) => { });

            iblock |= block + ParserAction.Create((m, f, b, x) => { });
            iblock |= lines + ParserAction.Create((m, f, b, x) => { });
            iblock |= ParserGenerator.EmptyString + ParserAction.Create((m, f, b, x) => { });

            line |= expr + ParserAction.Create((m, f, b, x) => { });

            lines |= expr + ParserAction.Create((m, f, b, x) => {
                x.Childs[0].Action(m, f, b, x.Childs[0]);
            });
            lines |= expr + lines + ParserAction.Create((m, f, b, x) => {
                x.Childs[0].Action(m, f, b, x.Childs[0]);
                x.Childs[1].Action(m, f, b, x.Childs[1]);
            });

            expr |= function + ParserAction.Create((m, f, b, x) => {
                x.Childs[0].Action(m, f, b, x.Childs[0]);
            });
            expr |= name + equal + index + ParserAction.Create((m, f, b, x) => { });
            expr |= runnable + ParserAction.Create((m, f, b, x) => { });

            function |= name + op_open + op_close + ParserAction.Create((m, f, b, x) => {
                var caller = m.CreateFunction(x.Childs[0].Contents);
                caller.IsExtern = true;
                var ci = LPCallOperator.Create(caller, new List<LPUser>());
                b.Insert(ci);
                x.UserContents = ci;
            });
            function |= name + op_open + argument + op_close + ParserAction.Create((m, f, b, x) => {
                var caller = m.CreateFunction(x.Childs[0].Contents);
                caller.IsExtern = true;
                x.Childs[2].Action(m, f, b, x);
                var ci = LPCallOperator.Create(caller, x.Childs[2].UserContents as List<LPUser>);
                b.Insert(ci);
                x.UserContents = ci;
            });

            argument |= index + ParserAction.Create((m, f, b, x) => {
                x.Childs[0].Action(m, f, b, x);
                x.UserContents = new List<LPUser> { x.Childs[0].UserContents as LPUser };
            });
            argument |= index + comma + argument + ParserAction.Create((m, f, b, x) => { });

            index |= variable + ParserAction.Create((m, f, b, x) => { });
            index |= variable + pp_open + variable + pp_close + ParserAction.Create((m, f, b, x) => { });
            index |= index + plus + index + ParserAction.Create((m, f, b, x) => { });
            index |= index + minus + index + ParserAction.Create((m, f, b, x) => { });
            index |= index + multiple + index + ParserAction.Create((m, f, b, x) => { });
            index |= index + divide + index + ParserAction.Create((m, f, b, x) => { });
            index |= minus + index + ParserAction.Create((m, f, b, x) => { });
            index |= op_open + index + op_close + ParserAction.Create((m, f, b, x) => { });

            variable |= name + ParserAction.Create((m, f, b, x) => { });
            variable |= function + ParserAction.Create((m, f, b, x) => { });
            variable |= _const + ParserAction.Create((m, f, b, x) => { x.UserContents = LPConstant.Create(x.Childs[0].Contents); });

            runnable |= loop + op_open + name + equal + index + to + index + op_close + block + ParserAction.Create((m, f, b, x) => { });
            runnable |= _foreach + op_open + name + scolon + index + op_close + block + ParserAction.Create((m, f, b, x) => { });
            runnable |= _if + op_open + index + op_close + block + ParserAction.Create((m, f, b, x) => { });
            runnable |= _if + op_open + index + op_close + block + _else + block + ParserAction.Create((m, f, b, x) => { });

            gen.PushConflictSolver(true, _else);
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(runnable, 2));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(index, 6));
            gen.PushConflictSolver(false, multiple, divide);
            gen.PushConflictSolver(false, plus, minus);

            //gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(index, 1));
            gen.PushConflictSolver(false, pp_open);
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(index, 0));

            try
            {
                gen.PushStarts(script);
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

            return pargen = gen.CreateExtendedShiftReduceParserInstance();
        }

        private Scanner get_scanner()
        {
            if (scanner != null) return scanner;

            var sg = new ScannerGenerator();

            sg.PushRule("", @"[\r\n ]");  // Skip characters
            sg.PushRule("loop", "loop");
            sg.PushRule("op_open", @"\(");
            sg.PushRule("op_close", @"\)");
            sg.PushRule("pp_open", @"\[");
            sg.PushRule("pp_close", @"\]");
            sg.PushRule("equal", @"\=");
            sg.PushRule("to", "to");
            sg.PushRule("scolon", ":");
            sg.PushRule("comma", ",");
            sg.PushRule("foreach", "foreach");
            sg.PushRule("if", "if");
            sg.PushRule("else", "else");
            sg.PushRule("name", @"[_$a-zA-Z][_$a-zA-Z0-9]*");
            //sg.PushRule("const", @"[\-\+]?[0-9]+(\.[0-9]+)?([Ee][\+\-]?[0-9]+)?|""[_$a-zA-Z0-9\/\?:,\[\]\\#\=&\+\-\*\|\(\)\<\>\.{}! ]*""");
            //sg.PushRule("const", @"[\-\+]?[0-9]+(\.[0-9]+)?([Ee][\+\-]?[0-9]+)?|""([^\\""]|\\"")*""");
            sg.PushRule("const", @"[\-\+]?[0-9]+(\.[0-9]+)?([Ee][\+\-]?[0-9]+)?|""([^""\\]|\\.)*""");

            sg.Generate();
            
            Console.Console.Instance.WriteLine(sg.PrintDiagram());
            
            return scanner = sg.CreateScannerInstance();
        }
    }
}
