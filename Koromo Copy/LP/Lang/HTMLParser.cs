/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.LP.Lang
{
    public class HTMLParser
    {
        static Scanner scanner;
        static ShiftReduceParser pargen;

        public HTMLParser()
        {
            get_pargen();
        }

        /// <summary>
        /// HTML Parser를 생성합니다.
        /// </summary>
        /// <returns></returns>
        private ShiftReduceParser get_pargen()
        {
            if (pargen != null) return pargen;

            var gen = new ParserGenerator();

            // Non-Terminals
            var html = gen.CreateNewProduction("HTML", false);
            var annote = gen.CreateNewProduction("ANNOTATE", false); // <!-- ~ -->
            var annote2 = gen.CreateNewProduction("ANNOTATE2", false); // <!~>
            var tag_closing = gen.CreateNewProduction("CLOSING", false); // <~> ~ </~>
            var tag_aio = gen.CreateNewProduction("ALLINONE", false); // <~/>

            // Terminals
            var open = gen.CreateNewProduction("open");
            var empty_open = gen.CreateNewProduction("empty-open");
            var close = gen.CreateNewProduction("close");
            var empty_close = gen.CreateNewProduction("empty-close");
            var annoate_open = gen.CreateNewProduction("annoate-open");
            var annoate_close = gen.CreateNewProduction("annoate-close");
            var annoate2_open = gen.CreateNewProduction("annoate2-open");
            var tag_name = gen.CreateNewProduction("tag-name");
            var attr_name = gen.CreateNewProduction("attr-name");
            
            html |= annoate_open + annote + annoate_close;
            html |= annoate_open + annote2;
            html |= tag_closing;
            html |= tag_aio;
            html |= html + html;
            html |= ParserGenerator.EmptyString;




            try
            {
                gen.PushStarts(html);
                gen.PrintProductionRules();
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

        private Scanner get_scanner()
        {
            if (scanner != null) return scanner;

            var sg = new ScannerGenerator();

            sg.PushRule("", @"[\r\n ]");  // Skip characters
            sg.PushRule("open", @"\<");
            sg.PushRule("empty-open", @"\<\/");
            sg.PushRule("close", @"\>");
            sg.PushRule("empty-close", @"\/\>");
            sg.PushRule("annoate-open", @"\<!\-\-");
            sg.PushRule("annoate-close", @"\-\-\>");
            sg.PushRule("annoate2-open", @"\<!");
            sg.PushRule("tag-name", @"[a-zA-Z0-9]+");
            sg.PushRule("attr-name", @"""([^\\""]|\\"")*""");

            sg.Generate();

            Console.Console.Instance.WriteLine(sg.PrintDiagram());

            return scanner = sg.CreateScannerInstance();
        }
    }
}
