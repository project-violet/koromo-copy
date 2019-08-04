/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.LP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.JS
{
    public class JSScannerGenerator
    {
        static Scanner scanner;
        public static Scanner Scanner { get { if (scanner == null) create_scanner(); return scanner; } }

        /// <summary>
        /// Create JavaScript Scanner
        /// </summary>
        private static void create_scanner()
        {
            var gen = new ScannerGenerator();

            gen.PushRule("", @"[\r\n ]");
            //gen.PushRule("", @"");
            gen.PushRule("Class", "class");
            gen.PushRule("Break", "break");
            gen.PushRule("Do", "do");
            gen.PushRule("Instanceof", "instanceof");
            gen.PushRule("Typeof", "typeof");
            gen.PushRule("Case", "case");
            gen.PushRule("Else", "else");
            gen.PushRule("New", "new");
            gen.PushRule("Var", "var");
            gen.PushRule("Catch", "catch");
            gen.PushRule("Finally", "finally");
            gen.PushRule("Return", "return");
            gen.PushRule("Void", "void");
            gen.PushRule("Continue", "continue");
            gen.PushRule("For", "for");
            gen.PushRule("Switch", "switch");
            gen.PushRule("While", "while");
            gen.PushRule("Debugger", "debugger");
            gen.PushRule("Function", "function");
            gen.PushRule("This", "this");
            gen.PushRule("With", "with");
            gen.PushRule("Default", "default");
            gen.PushRule("If", "if");
            gen.PushRule("Throw", "throw");
            gen.PushRule("Delete", "delete");
            gen.PushRule("In", "in");
            gen.PushRule("Try", "try");
            gen.PushRule("Enum", "enum");
            gen.PushRule("Extends", "extends");
            gen.PushRule("Super", "super");
            gen.PushRule("Const", "const");
            gen.PushRule("Export", "export");
            gen.PushRule("Import", "import");
            gen.PushRule("Implements", "implements");
            gen.PushRule("Let", "let");
            gen.PushRule("Private", "private");
            gen.PushRule("Public", "public");
            gen.PushRule("Interface", "interface");
            gen.PushRule("Package", "package");
            gen.PushRule("Protected", "protected");
            gen.PushRule("Static", "static");
            gen.PushRule("Yield", "yield");
            //gen.PushRule("SemiColon", @"SemiColon");

            gen.PushRule("{", @"\{");
            gen.PushRule("}", @"\}");
            gen.PushRule(",", @"\,");
            gen.PushRule("(", @"\(");
            gen.PushRule(")", @"\)");
            gen.PushRule(";", @"\;");
            gen.PushRule("*", @"\*");
            gen.PushRule("[", @"\[");
            gen.PushRule("]", @"\]");
            gen.PushRule(".", @"\.");
            gen.PushRule("++", @"\+\+");
            gen.PushRule("--", @"\-\-");
            gen.PushRule("+", @"\+");
            gen.PushRule("-", @"\-");
            gen.PushRule("~", @"\~");
            gen.PushRule("!", @"\!");
            gen.PushRule("/", @"\/");
            gen.PushRule("%", @"\%");
            gen.PushRule("<<", @"\<\<");
            gen.PushRule(">>", @"\>\>");
            gen.PushRule(">>>", @"\>\>\>");
            gen.PushRule("<", @"\<");
            gen.PushRule(">", @"\>");
            gen.PushRule("<=", @"\<\=");
            gen.PushRule(">=", @"\>\=");
            gen.PushRule("==", @"\=\=");
            gen.PushRule("===", @"\=\=\=");
            gen.PushRule("!==", @"\!\=\=");
            gen.PushRule("&", @"\&");
            gen.PushRule("^", @"\^");
            gen.PushRule("|", @"\|");
            gen.PushRule("&&", @"\&\&");
            gen.PushRule("||", @"\|\|");
            gen.PushRule("?", @"\?");
            gen.PushRule("=>", @"\=\>");
            gen.PushRule("*=", @"\*\=");
            gen.PushRule("/=", @"\/\=");
            gen.PushRule("%=", @"\%\=");
            gen.PushRule("+=", @"\+\=");
            gen.PushRule("-=", @"\-\=");
            gen.PushRule("<<=", @"\<\<\=");
            gen.PushRule(">>=", @"\>\>\=");
            gen.PushRule("&=", @"\&\=");
            gen.PushRule("^=", @"\^\=");
            gen.PushRule("|=", @"\|\=");

            gen.PushRule("StringLiteral", @"""([^""\\]|\\.)*""");
            gen.PushRule("DecimalLiteral", @"[0-9]+(\.[0-9]+)?([Ee][\+\-]?[0-9]+)?");
            gen.PushRule("BinaryIntegerLiteral", @"[0-9]+b");
            gen.PushRule("HexIntegerLiteral", @"0x[0-9]+");
            gen.PushRule("OctalIntegerLiteral", @"0[oO][0-9]+");
            gen.PushRule("BooleanLiteral", @"true|false");
            gen.PushRule("Identifier", @"[_$a-zA-Z][_$a-zA-Z0-9]*");

            gen.Generate();
            
            Console.Console.Instance.WriteLine(gen.PrintDiagram());

            scanner = gen.CreateScannerInstance();
        }
    }
}
