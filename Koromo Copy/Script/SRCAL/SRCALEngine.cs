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
        }

        /*
        
        EBNF: SRCAL-CDL

            script   -> block

            comment  -> ##.*?
            line     -> comment
                      | expr
                      | expr comment
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

        public abstract class CDLDebugInfo {
            public int Line;
            public int Column;
        }
        
        public class CDLScript {
            public CDLBlock start_block;
        }
        public class CDLBlock : CDLDebugInfo { }
        public class CDLLine : CDLDebugInfo { }
        public class CDLExptr : CDLDebugInfo { }
        public class CDLConst : CDLDebugInfo { }
        public class CDLVariable : CDLDebugInfo { }
        public class CDLArgument : CDLDebugInfo { }
        public class CDLFunction : CDLDebugInfo { }

        public abstract class CDLRunnable { }
        public class CDLLoop : CDLRunnable { }
        public class CDLForEach : CDLRunnable { }
        public class CDLIf : CDLRunnable { }
        public class CDLIfElse : CDLRunnable { }

        Dictionary<string, string> attributes;

        public SRCALParser()
        {
            attributes = new Dictionary<string, string>
            {
                {"$ScriptName", "" },
                {"$ScriptVersion", "" },
                {"$ScriptAuthor", "" },
                {"$ScriptFolderName", "" },
                {"$ScriptRequestName", "" },
                {"$URLSpecifier", "" },
                {"$UsingDriver", "" },
            };
        }

        List<string> raw_script;
        int line;
        int column;
        public void Parse(List<string> raw_script)
        { 
        }
        
    }

    /// <summary>
    /// 스크립트의 현재 상태를 저장하고 명령 구문을 해석하고 실행합니다.
    /// </summary>
    public class SRCALEngine
    {
        //SRCALParser.SRCALBlock root_block;

        public void ParseScript(List<string> raw_script)
        {
            //root_block = new SRCALParser().Parse(raw_script);
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
