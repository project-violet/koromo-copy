/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Script.SRCAL
{
    public enum SRCALType
    {
        None,
        Integer,
        Boolean,
        String,
        StringList,
        Internal,
        Function,
    }

    /// <summary>
    /// Parser for SRCAL - Crawler Descript Language
    /// </summary>
    public class SRCALParser
    {
        public class SRCALVariable
        {
            public SRCALType Type;
            public string Name;
            public int ContentInteger;
            public bool ContentBoolean;
            public string ContentString;
            public List<string> ContentStringList;
            public string ContentInternal;
            public SRCALFunction ContentFunction;
        }

        /*
        
        EBNF: SRCAL-CDL

            script   -> block

            line     -> ##.*?
                      | expr
                      | e
            
            expr     -> func
                      | var = variable
                      | runnable
                      
            block    -> [ block ]
                     -> line block
                     -> e
                     
            name     -> [_a-zA-Z]\w*
                      | $name            ; Inernal functions

            number   -> [0-9]+
            string   -> "([^\\"]|\\")*"
            const    -> number
                      | string
                     
            var      -> name
            
            variable -> var
                      | function
                      | variable [ number ]

            argument -> variable
                      | variable, argument
            function -> name ( )
                      | name ( argument )
            
            runnable -> loop (var = variable "to" variable) block
                      | foreach (var : variable)            block
                      | if (variable)                       block
                      | if (variable)                       block else block
        */

        public class SRCALFunction
        {
            public bool IsReturn;
            public bool IsInternal;
            public string Name;

            public SRCALVariable ContentReturn;
            public List<SRCALVariable> ContentArguments;
        }

        public class SRCALStatement
        {
            public bool IsFunctionCall;
            public bool IsVariable;

            public SRCALVariable ContentVariable;
        }

        public abstract class SRCALRunnable {
            public int LineNumber;
            public int LineColumn;
        }
        
        public class SRCALExpression : SRCALRunnable
        {
            public bool IsEqual;

            public SRCALStatement Stmt;
            public SRCALVariable Left;
            public SRCALExpression Right;
        }

        public class SRCALLoop : SRCALRunnable
        {
            public SRCALVariable Variable;
            public SRCALBlock InnerBlock;
        }

        public class SRCALForeach : SRCALRunnable
        {
            public SRCALVariable Source;
            public SRCALVariable Variable;
            public SRCALBlock InnerBlock;
        }

        public class SRCALIfElse : SRCALRunnable
        {
            public SRCALStatement Stmt;

            public bool IsElseExists;
            public SRCALBlock IfBlock;
            public SRCALBlock ElseBlock;
        }

        public class SRCALBlock
        {
            public List<SRCALRunnable> RunList;
            public void AddRunnable(SRCALRunnable runnable) => RunList.Add(runnable);
        }

        SRCALBlock root_block;
        Dictionary<string, SRCALVariable> attributes;

        public SRCALParser()
        {
            attributes = new Dictionary<string, SRCALVariable>
            {
                {"$ScriptName", null },
                {"$ScriptVersion", null },
                {"$ScriptAuthor", null },
                {"$ScriptFolderName", null },
                {"$ScriptRequestName", null },
                {"$URLSpecifier", null },
                {"$UsingDriver", null },
            };
        }

        List<string> raw_script;
        int line_number;
        int column;
        public SRCALBlock Parse(List<string> raw_script)
        {
            root_block = new SRCALBlock();
            line_number = 0;
            this.raw_script = raw_script;

            for (; line_number < raw_script.Count; line_number++)
            {
                var line_string = raw_script[line_number].Trim();
                if (string.IsNullOrEmpty(line_string) || line_string.StartsWith("##"))
                    continue;
                column = 0;
                parse_internal();
            }

            return root_block;
        }

        private void parse_internal()
        {

        }

        private SRCALExpression parse_expression()
        {
            var expr = new SRCALExpression();

            return expr;
        }
    }

    /// <summary>
    /// 스크립트의 현재 상태를 저장하고 명령 구문을 해석하고 실행합니다.
    /// </summary>
    public class SRCALEngine
    {
        SRCALParser.SRCALBlock root_block;

        public void ParseScript(List<string> raw_script)
        {
            root_block = new SRCALParser().Parse(raw_script);
        }
    }

    /// <summary>
    /// Simple Robust CAL - CDL 스크립트를 실행하는 클래스입니다.
    /// </summary>
    public class SRCALScript
    {
        SRCALEngine engine;
        List<string> raw_script;

        public SRCALScript(string script)
        {
            raw_script = script.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
                ).ToList();
            init_script();
        }

        public bool SpecifyURL(string url)
        {
            return false;
        }

        public void Run(string url, Action<List<string>, List<string>> request_download)
        {

        }

        private void init_script()
        {

        }
    }

}
