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
                      | const

            argument -> variable
                      | variable, argument
            function -> name ( )
                      | name ( argument )
            
            runnable -> loop (var = variable "to" variable) block
                      | foreach (var : variable)            block
                      | if (variable)                       block
                      | if (variable)                       block else block
        */

        public class CDLDebugInfo {
            public int Line;
            public int Column;
        }
        
        public class CDLScript {
            public CDLBlock start_block;
        }
        public class CDLBlock : CDLDebugInfo {
            public List<CDLLine> ContentLines;
        }
        public class CDLLine : CDLDebugInfo {
            public CDLExpr ContentExpr;
        }
        public class CDLExpr : CDLDebugInfo {
            public enum CDLExprType
            {
                Function,
                Runnable,
                Equal
            }
            public CDLExprType Type;
            public CDLFunction ContentFunction;
            public CDLRunnable ContentRunnable;
            public CDLVar ContentVar;
            public CDLVariable ContentVariable;
        }
        public class CDLConst : CDLDebugInfo {
            public enum CDLConstType
            {
                Boolean,
                Integer,
                String,
            }
            public CDLConstType Type;
            public string Name;
            public bool ContentBoolean;
            public int ContentInteger;
            public string ContentString;
        }
        public class CDLVar : CDLDebugInfo {
            public enum CDLVarType
            {
                Boolean,
                Integer,
                String,
                StringList
            }
            public CDLVarType Type;
            public string Name;
            public bool ContentBoolean;
            public int ContentInteger;
            public string ContentString;
            public List<string> ContentStringList;
        }
        public class CDLVariable : CDLDebugInfo {
            public enum CDLVariableType
            {
                Var,
                Function,
                VariableIndex,
                Const,
            }
            public CDLVariableType Type;
            public CDLVar ContentVar;
            public CDLFunction ContentFunction;
            public CDLVariable ContentVariableIndex;
            public int Index;
            public CDLConst ContentConst;
        }
        public class CDLArgument : CDLDebugInfo {
            public List<CDLVariable> ContentArguments;
        }
        public class CDLFunction : CDLDebugInfo {
            public bool IsReturnVoid;
            public CDLVariable ContentReturn;
            public string ContentFunctionName;
            public List<CDLVariable> ContentArguments;
        }

        public class CDLRunnable : CDLDebugInfo { }
        public class CDLLoop : CDLRunnable {
            public CDLVar ContentIterator;
            public CDLVariable ContentStarts;
            public CDLVariable ContentEnds;
            public CDLBlock ContentInnerBlock;
        }
        public class CDLForEach : CDLRunnable {
            public CDLVar ContentIterator;
            public CDLVariable ContentSource;
            public CDLBlock ContentBlock;
        }
        public class CDLIf : CDLRunnable {
            public CDLVariable ContentStatement;
            public CDLBlock ContentBlock;
        }
        public class CDLIfElse : CDLRunnable {
            public CDLVariable ContentStatement;
            public CDLBlock ContentIfBlock;
            public CDLBlock ContentElseBlock;
        }

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
        List<Tuple<CDLDebugInfo, string>> errors; 

        public CDLScript Parse(List<string> raw_script)
        {
            this.raw_script = raw_script;
            errors = new List<Tuple<CDLDebugInfo, string>>();
            line = 0;
            column = 0;
            return parse_script();
        }

        #region Parse Tool

        private enum token_type
        {
            None,
            Integer,
            String,
            Name,
            Internal,
            Special
        }

        token_type latest_token_type;

        private string next_token()
        {
        STARTS:

            if (line == raw_script.Count) {
                latest_token_type = token_type.None;
                return "";
            }
            
            if (raw_script[line].Length == column || raw_script[line].Substring(column).Trim() == "")
            {
                line++;
                column = 0;
                goto STARTS;
            }

            if (raw_script[line].Substring(column).StartsWith("##"))
            {
                line++;
                column = 0;
                goto STARTS;
            }

            var builder = new StringBuilder();
            var str = raw_script[line];

            for (; column < str.Length; column++)
            {
                if (str[column] == ' ') continue;
                if (char.IsDigit(str[column]))
                {
                    latest_token_type = token_type.Integer;
                    while (column < str.Length && char.IsDigit(str[column]))
                        builder.Append(str[column++]);
                    return builder.ToString();
                }
                else if (('a' <= str[column] && str[column] <= 'z') ||
                         ('A' <= str[column] && str[column] <= 'Z') ||
                         str[column] == '_' || str[column] == '$')
                {
                    latest_token_type = token_type.Name;
                    if (str[column] == '$') latest_token_type = token_type.Internal;
                    builder.Append(str[column++]);
                    while (column < str.Length)
                    {
                        if (('a' <= str[column] && str[column] <= 'z') ||
                            ('A' <= str[column] && str[column] <= 'Z') ||
                            str[column] == '_' || str[column] == '$')
                            builder.Append(str[column++]);
                        else
                            break;
                    }
                    return builder.ToString();
                }
                else if (str[column] == '"')
                {
                    latest_token_type = token_type.String;
                    column++;
                    while (column < str.Length)
                    {
                        if (str[column] == '"') {
                            column++;
                            return builder.ToString();
                        }
                        if (str[column] == '\\')
                        {
                            if (column < str.Length)
                                column++;
                            else
                                break;
                        }
                        builder.Append(str[column++]);
                    }
                    errors.Add(new Tuple<CDLDebugInfo, string>(new CDLDebugInfo
                    {
                        Line = line, Column = column
                    }, "constant string closure not found!"));
                }
                else if ("([,])=:".Contains(str[column]))
                {
                    latest_token_type = token_type.Special;
                    return str[column++].ToString();
                }
            }

            latest_token_type = token_type.None;
            return "";
        }

        private string look_up_token()
        {
            var l = line;
            var c = column;
            var t = latest_token_type;
            var str = next_token();
            line = l;
            column = c;
            latest_token_type = t;
            return str;
        }
        
        #endregion

        #region Parse Recursion

        private CDLScript parse_script()
        {
            var script = new CDLScript();
            var ll = new List<CDLLine>();
            while (line != raw_script.Count && look_up_token() != "")
                ll.Add(parse_line());
            script.start_block = new CDLBlock { ContentLines = ll };
            return script;
        }
        
        private CDLBlock parse_block()
        {
            var ff = look_up_token();

            var block = new CDLBlock { Line = line, Column = column };
            if (ff == "[")
            {
                var ll = new List<CDLLine>();
                next_token(); // [
                while (look_up_token() != "]")
                    ll.Add(parse_line());
                next_token(); // ]
                block.ContentLines = ll;
            }
            else if (ff != "")
            {
                block.ContentLines = new List<CDLLine>() { parse_line() };
            }

            return block;
        }

        private CDLLine parse_line()
        {
            var l = line;
            var c = column;
            return new CDLLine { Line = l, Column = c, ContentExpr = parse_expr() };
        }

        private CDLExpr parse_expr()
        {
            var l = line;
            var c = column;

            var nt = next_token();
            var ntt = latest_token_type;
            var nnt = look_up_token();

            if (nt == "loop" || nt == "foreach" || nt == "if")
            {
                return new CDLExpr { Line = l, Column = c, Type = CDLExpr.CDLExprType.Runnable,
                    ContentRunnable = parse_runnable(nt)
                };
            }
            else if (nnt == "(")
            {
                return new CDLExpr { Line = l, Column = c, Type = CDLExpr.CDLExprType.Function,
                    ContentFunction = parse_function(nt)
                };
            }
            else if (nnt == "=")
            {
                next_token();
                return new CDLExpr { Line = l, Column = c, Type = CDLExpr.CDLExprType.Equal,
                    ContentVar = parse_var(nt),
                    ContentVariable = parse_variable()
                };
            }

            errors.Add(Tuple.Create(new CDLDebugInfo
            {
                Line = l,
                Column = c
            }, "cannot parse expr!"));
            return new CDLExpr();
        }

        private CDLFunction parse_function(string name)
        {
            var l = line;
            var c = column;

            next_token(); // (
            var args = new List<CDLVariable>();

            var lookup = look_up_token();
            while (lookup != "" && lookup != ")")
            {
                args.Add(parse_variable());
                if (look_up_token() == ",")
                {
                    next_token();
                    continue;
                }
                lookup = look_up_token();
            }

            if (lookup == "")
            {
                errors.Add(Tuple.Create(new CDLDebugInfo
                {
                    Line = l,
                    Column = c
                }, "function arguments parse error!"));
                return new CDLFunction();
            }

            var close = next_token();
            if (close != ")")
            {
                errors.Add(Tuple.Create(new CDLDebugInfo
                {
                    Line = l,
                    Column = c
                }, "closure not found in function!"));
            }
            return new CDLFunction { Line = l, Column = c, ContentFunctionName = name, ContentArguments = args };
        }

        private CDLVar parse_var(string name)
        {
            return new CDLVar { Line = line, Column = column, Name = name };
        }

        private CDLVariable parse_variable()
        {
            var l = line;
            var c = column;
            var nt = next_token();
            var tt = latest_token_type;

            if (tt == token_type.Integer)
                return new CDLVariable { Line = l, Column = c, Type = CDLVariable.CDLVariableType.Const, ContentConst = 
                    new CDLConst { Line = l, Column = c, Type = CDLConst.CDLConstType.Integer, ContentInteger = Convert.ToInt32(nt) } };
            if (tt == token_type.String)
                return new CDLVariable { Line = l, Column = c, Type = CDLVariable.CDLVariableType.Const, ContentConst = 
                    new CDLConst { Line = l, Column = c, Type = CDLConst.CDLConstType.String, ContentString = nt } };

            if (nt == "true")
                return new CDLVariable { Line = l, Column = c, Type = CDLVariable.CDLVariableType.Const, ContentConst = 
                    new CDLConst { Line = l, Column = c, Type = CDLConst.CDLConstType.Boolean, ContentBoolean = true } };
            if (nt == "false")
                return new CDLVariable { Line = l, Column = c, Type = CDLVariable.CDLVariableType.Const, ContentConst = 
                    new CDLConst { Line = l, Column = c, Type = CDLConst.CDLConstType.Boolean, ContentBoolean = false } };

            var nnt = look_up_token();
            if (nnt == "(")
                return new CDLVariable { Line = l, Column = c, Type = CDLVariable.CDLVariableType.Function, ContentFunction = parse_function(nt) };
            //if (nnt == "[")
            //    return new CDLVariable { Line = l, Column = c, Type = CDLVariable.CDLVariableType.VariableIndex, ContentVariableIndex = p }
            
            return new CDLVariable { Line = l, Column = c, Type = CDLVariable.CDLVariableType.Var, ContentVar = parse_var(nt) };
        }

        private CDLRunnable parse_runnable(string specific)
        {
            var l = line;
            var c = column;
            if (specific == "loop")
            {
                next_token(); // (
                var iter = parse_var(next_token());
                next_token(); // =
                var start = parse_variable();
                next_token(); // to
                var ends = parse_variable();
                next_token(); // )
                var block = parse_block();
                return new CDLLoop { Line = l, Column = c, ContentIterator = iter, ContentStarts = start, ContentEnds = ends, ContentInnerBlock = block };
            }
            else if (specific == "foreach")
            {
                next_token(); // (
                var iter = parse_var(next_token());
                next_token(); // :
                var src = parse_variable();
                next_token(); // )
                var block = parse_block();
                return new CDLForEach { Line = l, Column = c, ContentIterator = iter, ContentSource = src, ContentBlock = block };
            }
            else if (specific == "if")
            {
                var stmt = parse_variable();
                var block = parse_block();

                if (look_up_token() == "else")
                {
                    next_token(); // else
                    var block2 = parse_block();
                    return new CDLIfElse { Line = l, Column = c, ContentStatement = stmt, ContentIfBlock = block, ContentElseBlock = block2 };
                }
                return new CDLIf { Line = l, Column = c, ContentStatement = stmt, ContentBlock = block };
            }
            return new CDLRunnable();
        }

        #endregion
    }

    /// <summary>
    /// 스크립트의 현재 상태를 저장하고 명령 구문을 해석하고 실행합니다.
    /// </summary>
    public class SRCALEngine
    {
        SRCALParser.CDLScript script;

        public void ParseScript(List<string> raw_script)
        {
            try
            {
                script = new SRCALParser().Parse(raw_script);
            }
            catch (Exception e)
            {
                Monitor.Instance.Push($"[SRCAL Engine] Script parse error. {e.Message}");
            }
        }

        public void RunScript(string request_url)
        {

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
