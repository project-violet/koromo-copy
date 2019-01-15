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
    }

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
        }

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

        public interface SRCALRunnable { }
        
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

        public SRCALBlock Parse(List<string> raw_script)
        {
            root_block = new SRCALBlock();
            return root_block;
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
    /// Simple Robust CAL 스크립트를 실행하는 클래스입니다.
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
