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
    /// SRCAL문법을 C#문법으로 컴파일하는 도구집합입니다.
    /// </summary>
    public class CALtoCS
    {
        public CALtoCS(string cal)
        {
        }

        /// <summary>
        /// SRCAL문법을 C#으로 바꿉니다.
        /// </summary>
        /// <returns></returns>
        public string Compile()
        {
            var pp = get_pargen();
            return "";
        }

        ShiftReduceParser pargen;

        /// <summary>
        /// SRCAL의 파서제너레이터를 생성합니다.
        /// </summary>
        /// <returns></returns>
        private ShiftReduceParser get_pargen()
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
            var _foreach = gen.CreateNewProduction("foreach");
            var _if = gen.CreateNewProduction("if");
            var _else = gen.CreateNewProduction("else");

            script |= block;

            block |= pp_open + iblock + pp_close;
            block |= line;

            iblock |= block;
            iblock |= lines;
            iblock |= ParserGenerator.EmptyString;

            line |= expr;

            lines |= expr;
            lines |= expr + lines;

            expr |= function;
            expr |= name + equal + index;
            expr |= runnable;

            function |= name + op_open + op_close;
            function |= name + op_open + argument + op_close;

            argument |= index;
            argument |= argument + comma + index;

            index |= variable;
            index |= variable + pp_open + variable + pp_close;

            variable |= name;
            variable |= function;
            variable |= _const;

            runnable |= loop + op_open + name + equal + index + to + index + op_close + block;
            runnable |= _foreach + op_open + name + scolon + index + op_close + block;
            runnable |= _if + op_open + index + op_close + block;
            runnable |= _if + op_open + index + op_close + block + _else + block;
            
            gen.PushConflictSolver(true, _else);
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(runnable, 2));

            try
            {
                gen.PushStarts(script);
                gen.GenerateLALR();
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
    }
}
