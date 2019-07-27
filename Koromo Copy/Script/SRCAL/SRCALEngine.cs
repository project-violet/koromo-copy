/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using HtmlAgilityPack;
using Koromo_Copy.Net.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

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
        /*
        
        EBNF: SRCAL-CDL

            script   -> block

            comment  -> ##.*?
            line     -> comment
                      | expr
                      | expr comment
                      | e
            
            expr     -> func
                      | var = index
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
            
            index    -> variable
                      | variable [ variable ]
            variable -> var
                      | function
                      | const

            argument -> index
                      | index, argument
            function -> name ( )
                      | name ( argument )
            
            runnable -> loop (var = index "to" index) block
                      | foreach (var : index)         block
                      | if (index)                    block
                      | if (index)                    block else block
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
            public CDLIndex ContentIndex;
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
                StringList,
                JSon,
            }
            public CDLVarType Type;
            public string Name;
            public bool ContentBoolean;
            public int ContentInteger;
            public string ContentString;
            public List<string> ContentStringList;
            public JToken ContentJSon;
        }
        public class CDLIndex : CDLDebugInfo {
            public CDLVariable ContentVariable;
            public bool UseIndex;
            public CDLVariable ContentIndex;
        }

        public class CDLVariable : CDLDebugInfo {
            public enum CDLVariableType
            {
                Var,
                Function,
                Const,
            }
            public CDLVariableType Type;
            public CDLVar ContentVar;
            public CDLFunction ContentFunction;
            public CDLConst ContentConst;
        }
        public class CDLArgument : CDLDebugInfo {
            public List<CDLIndex> ContentArguments;
        }
        public class CDLFunction : CDLDebugInfo {
            public bool IsReturnVoid;
            public CDLIndex ContentReturn;
            public string ContentFunctionName;
            public List<CDLIndex> ContentArguments;
        }

        public class CDLRunnable : CDLDebugInfo { }
        public class CDLLoop : CDLRunnable {
            public CDLVar ContentIterator;
            public CDLIndex ContentStarts;
            public CDLIndex ContentEnds;
            public CDLBlock ContentInnerBlock;
        }
        public class CDLForEach : CDLRunnable {
            public CDLVar ContentIterator;
            public CDLIndex ContentSource;
            public CDLBlock ContentBlock;
        }
        public class CDLIf : CDLRunnable {
            public CDLIndex ContentStatement;
            public CDLBlock ContentBlock;
        }
        public class CDLIfElse : CDLRunnable {
            public CDLIndex ContentStatement;
            public CDLBlock ContentIfBlock;
            public CDLBlock ContentElseBlock;
        }

        public Dictionary<string, string> attributes;
        
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
        int nt_line;
        int nt_column;
        bool hard_check = true;
        public List<Tuple<CDLDebugInfo, string>> errors; 

        public CDLScript Parse(List<string> raw_script)
        {
            this.raw_script = raw_script;
            errors = new List<Tuple<CDLDebugInfo, string>>();
            line = 0;
            column = 0;
            var script = parse_script();
            set_attributes(script);
            return script;
        }

        private void set_attributes(CDLScript script)
        {
            foreach (var line in script.start_block.ContentLines)
            {
                if (line.ContentExpr.Type == CDLExpr.CDLExprType.Equal)
                {
                    if (attributes.ContainsKey(line.ContentExpr.ContentVar.Name))
                    {
                        if (line.ContentExpr.Type == CDLExpr.CDLExprType.Equal)
                        {
                            if (line.ContentExpr.ContentIndex.ContentVariable.Type == CDLVariable.CDLVariableType.Const)
                            {
                                if (line.ContentExpr.ContentIndex.ContentVariable.ContentConst.Type == CDLConst.CDLConstType.String)
                                    attributes[line.ContentExpr.ContentVar.Name] = line.ContentExpr.ContentIndex.ContentVariable.ContentConst.ContentString;
                                else
                                    attributes[line.ContentExpr.ContentVar.Name] = Convert.ToString(line.ContentExpr.ContentIndex.ContentVariable.ContentConst.ContentInteger);
                            }
                        }
                    }
                }
            }
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
        bool nullstr = false;
        private string next_token()
        {
            nullstr = false;
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

            if (raw_script[line].Trim().StartsWith("##") || raw_script[line].Substring(column).StartsWith("##"))
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
                nt_line = line;
                nt_column = column;
                if (char.IsDigit(str[column]) || str[column] == '-')
                {
                    latest_token_type = token_type.Integer;
                    builder.Append(str[column++]);
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
                            ('0' <= str[column] && str[column] <= '9') ||
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
                            if (builder.ToString() == "")
                                nullstr = true;
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
                    push_error(line, column, "constant string closure not found!");
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
            var l = line;
            var c = column;
            var ff = look_up_token();

            var block = new CDLBlock { Line = l, Column = c };
            if (ff == "[")
            {
                var ll = new List<CDLLine>();
                test_terminal("[", l, c);
                while (look_up_token() != "]")
                    ll.Add(parse_line());
                test_terminal("]", l, c);
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
            var nt = next_token();

            var l = nt_line;
            var c = nt_column;

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
                    ContentVar = parse_var(l, c, nt),
                    ContentIndex = parse_index()
                };
            }
            
            push_error(l, c, "cannot parse expr!");
            return new CDLExpr();
        }

        private CDLFunction parse_function(string name)
        {
            var l = nt_line;
            var c = nt_column;

            test_terminal("(", l, c);
            var args = new List<CDLIndex>();

            var lookup = look_up_token();
            while ((lookup != "" || nullstr) && lookup != ")")
            {
                args.Add(parse_index());
                if (look_up_token() == ",")
                    next_token();
                lookup = look_up_token();
            }

            if (lookup == "" && !nullstr)
            {
                push_error(l, c, "function arguments parse error!");
                return new CDLFunction();
            }

            var close = next_token();
            if (close != ")")
            {
                push_error(l, c, "closure not found in function!");
            }
            return new CDLFunction { Line = l, Column = c, ContentFunctionName = name, ContentArguments = args };
        }

        private CDLVar parse_var(int l, int c, string name)
        {
            return new CDLVar { Line = l, Column = c, Name = name };
        }

        private CDLIndex parse_index()
        {
            var l = line;
            var c = column;
            var v = parse_variable();

            var nt = look_up_token();
            if (nt == "[")
            {
                test_terminal("[", l, c);
                var index = parse_variable();
                test_terminal("]", l, c);
                return new CDLIndex { Line = l, Column = c, ContentVariable = v, UseIndex = true, ContentIndex = index };
            }
            return new CDLIndex { Line = l, Column = c, ContentVariable = v, UseIndex = false };
        }

        private CDLVariable parse_variable()
        {
            var nt = next_token();

            var l = nt_line;
            var c = nt_column;

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
            
            return new CDLVariable { Line = l, Column = c, Type = CDLVariable.CDLVariableType.Var, ContentVar = parse_var(line, column, nt) };
        }

        private CDLRunnable parse_runnable(string specific)
        {
            var l = line;
            var c = column;
            if (specific == "loop")
            {
                test_terminal("(", l, c);
                var iter = parse_var(l,c,next_token());
                test_terminal("=", l, c);
                var start = parse_index();
                test_terminal("to", l, c);
                var ends = parse_index();
                test_terminal(")", l, c);
                var block = parse_block();
                return new CDLLoop { Line = l, Column = c, ContentIterator = iter, ContentStarts = start, ContentEnds = ends, ContentInnerBlock = block };
            }
            else if (specific == "foreach")
            {
                test_terminal("(", l, c);
                var iter = parse_var(line,column,next_token());
                test_terminal(":", l, c);
                var src = parse_index();
                test_terminal(")", l, c);
                var block = parse_block();
                return new CDLForEach { Line = l, Column = c, ContentIterator = iter, ContentSource = src, ContentBlock = block };
            }
            else if (specific == "if")
            {
                test_terminal("(", l, c);
                var stmt = parse_index();
                test_terminal(")", l, c);
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

        private void push_error(int l, int c, string msg)
        {
            errors.Add(new Tuple<CDLDebugInfo, string>(new CDLDebugInfo { Line = l, Column = c }, msg));
            if (hard_check)
                throw new Exception(msg);
        }

        private void test_terminal(string term, int l, int c)
        {
            if (next_token() != term)
                push_error(l, c, $"missmatch terminal symbol {term}!");
        }

        #endregion
    }

    /// <summary>
    /// 스크립트의 현재 상태를 저장하고 명령 구문을 해석하고 실행합니다.
    /// </summary>
    public class SRCALEngine
    {
        SRCALParser.CDLScript script;
        SRCALAttribute attribute;
        SeleniumWrapper driver;
        Action<SRCALAttribute, List<Tuple<string, string>>> request_download;

        public class SRCALAttribute
        {
            public string ScriptName;
            public string ScriptVersion;
            public string ScriptAuthor;
            public string ScriptFolderName;
            public string ScriptRequestName;
            public string URLSpecifier;
            public bool UsingDriver;
        }
        
        public SRCALEngine()
        {

        }

        public SRCALAttribute Attribute => attribute;

        public bool ParseScript(List<string> raw_script)
        {
            var parser = new SRCALParser();
            try
            {
                script = parser.Parse(raw_script);

                if (parser.errors.Count == 0)
                {
                    attribute = new SRCALAttribute();
                    attribute.ScriptName = parser.attributes["$ScriptName"];
                    attribute.ScriptVersion = parser.attributes["$ScriptVersion"];
                    attribute.ScriptAuthor = parser.attributes["$ScriptAuthor"];
                    attribute.ScriptFolderName = parser.attributes["$ScriptFolderName"];
                    attribute.ScriptRequestName = parser.attributes["$ScriptRequestName"];
                    attribute.URLSpecifier = parser.attributes["$URLSpecifier"];
                    attribute.UsingDriver = Convert.ToInt32(parser.attributes["$UsingDriver"]) == 0 ? false : true;

                    return true;
                }
            }
            catch (Exception e)
            {
                Monitor.Instance.Push($"[SRCAL Engine] Script parsing error. {e.Message}\r\n{e.StackTrace}");
            }

            if (parser.errors.Count > 0)
            {
                Monitor.Instance.Push($"[SRCAL Engine] Occurred some errors when parsing script ...");
                for (int i = 0; i < parser.errors.Count; i++)
                {
                    Monitor.Instance.Push($"[{parser.errors[i].Item1.Line}, {parser.errors[i].Item1.Column}] {parser.errors[i].Item2}");
                }
            }

            return false;
        }

        public bool RunScript(string request_url, Action<SRCALAttribute, List<Tuple<string, string>>> request_download_callback)
        {
            request_download = request_download_callback;
            variables = new List<Tuple<int, SRCALParser.CDLVar>>();
            error_message = new List<Tuple<int, int, string>>();
            info_message = new List<Tuple<int, int, string>>();
            error = false;
            variable_update(new SRCALParser.CDLVar { Name = "$RequestURL", Type = SRCALParser.CDLVar.CDLVarType.String, ContentString = request_url });
            variable_update(new SRCALParser.CDLVar { Name = "$Infinity", Type = SRCALParser.CDLVar.CDLVarType.Integer, ContentInteger = int.MaxValue });
            variable_update(new SRCALParser.CDLVar { Name = "$LatestImagesCount", Type = SRCALParser.CDLVar.CDLVarType.Integer, ContentInteger = 0 });
            enter_block();
            error_message = new List<Tuple<int, int, string>>();
            
            Thread thread = new Thread(new ParameterizedThreadStart(run_script));
            thread.Start(script);
            thread.Join();

            if (error)
            {
                error_message.ForEach(x => Monitor.Instance.Push($"[SRCAL Engine] error occurred [{x.Item1}, {x.Item2}] {x.Item3}"));
            }

            return error;
        }

        bool error = false;
        List<Tuple<int,int,string>> error_message;
        List<Tuple<int,int,string>> info_message;
        
        #region Variable Management

        List<Tuple<int, SRCALParser.CDLVar>> variables;

        int block_depth = 0;
        private void enter_block()
        {
            block_depth++;
        }

        private void exit_block()
        {
            block_depth--;
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Item1 > block_depth)
                    variables.RemoveAt(i--);
            }
        }
        
        private bool variable_exists(string name) => variables.Any(x => x.Item2.Name == name);

        private SRCALParser.CDLVar variable_get(string name)
        {
            foreach (var v in variables)
                if (v.Item2.Name == name)
                    return v.Item2;
            return null;
        }

        private void variable_update(SRCALParser.CDLVar var)
        {
            if (var.Name == "$RequestHtml")
            {
                current_html = var.ContentString;
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(current_html);
                root_node = document.DocumentNode;
            }
            for (int i = 0; i < variables.Count; i++)
                if (variables[i].Item2.Name == var.Name)
                {
                    variables[i] = new Tuple<int, SRCALParser.CDLVar>(variables[i].Item1, var);
                    return;
                }
            variables.Add(new Tuple<int, SRCALParser.CDLVar>(block_depth, var));
        }

        #endregion

        #region Script Runner

        string current_html;
        List<Tuple<string, string>> image_list;
        HtmlNode root_node;

        private void run_script(object script)
        {
            try
            {
                var url = variable_get("$RequestURL").ContentString;
                info_message.Add(Tuple.Create(-1, -1, $"download request html {url}"));

                if (!attribute.UsingDriver)
                {
                    current_html = Net.NetCommon.DownloadString(url);
                }
                else
                {
                    driver = new SeleniumWrapper();
                    driver.Navigate(url);
                    current_html = driver.GetHtml();
                }
                variable_update(new SRCALParser.CDLVar { Name = "$RequestHtml", Type = SRCALParser.CDLVar.CDLVarType.String, ContentString = current_html });
                image_list = new List<Tuple<string, string>>();
                run_block(((SRCALParser.CDLScript)script).start_block);

                if (attribute.UsingDriver)
                {
                    driver.Close();
                }
            }
            catch (Exception e)
            {
                error_message.Add(Tuple.Create(-1, -1, e.Message));
                error = true;
            }
        }

        private void run_block(SRCALParser.CDLBlock block)
        {
            foreach (var line in block.ContentLines)
            {
                try
                {
                    if (line.ContentExpr == null)
                        throw new Exception("content expr is null.");
                    run_expr(line.ContentExpr);
                }
                catch (Exception e)
                {
                    error_message.Add(Tuple.Create(line.Line, line.Column, e.Message));
                    error = true;
                    break;
                }
            }
        }

        private void run_expr(SRCALParser.CDLExpr expr)
        {
            switch (expr.Type)
            {
                case SRCALParser.CDLExpr.CDLExprType.Function:
                    run_function(expr.ContentFunction);
                    break;

                case SRCALParser.CDLExpr.CDLExprType.Runnable:
                    run_runnable(expr.ContentRunnable);
                    break;

                case SRCALParser.CDLExpr.CDLExprType.Equal:
                    variable_update(run_index(expr.ContentVar, expr.ContentIndex));
                    break;
            }
        }

        bool exit_loop = false;
        private SRCALParser.CDLVar run_function(SRCALParser.CDLFunction func)
        {
            //
            //  Internfal Functions
            //
            if (func.ContentFunctionName == "$LoadPage")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$LoadPage' function must have one argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = variable_get("$RequestURL");
                var v1 = run_index(v,func.ContentArguments[0]);
                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }
                var referer = v.ContentString;
                v.ContentString = v1.ContentString;
                variable_update(v);
                info_message.Add(Tuple.Create(func.Line, func.Column, $"download request html {v.ContentString}"));
                var client = Net.NetCommon.GetDefaultClient();
                client.Headers.Add(System.Net.HttpRequestHeader.Referer, referer);
                current_html = client.DownloadString(v.ContentString);
                variable_update(new SRCALParser.CDLVar { Name = "$RequestHtml", Type = SRCALParser.CDLVar.CDLVarType.String, ContentString = current_html });
            }
            else if (func.ContentFunctionName == "$AppendImage")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'$LoadPage' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String || v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                info_message.Add(Tuple.Create(func.Line, func.Column, $"append image {v1.ContentString} { v2.ContentString}"));
                image_list.Add(Tuple.Create(v1.ContentString, v2.ContentString));
                var latest_images_count = variable_get("$LatestImagesCount").ContentInteger;
                variable_update(new SRCALParser.CDLVar { Name = "$LatestImagesCount", Type = SRCALParser.CDLVar.CDLVarType.Integer, ContentInteger = latest_images_count + 1 });
            }
            else if (func.ContentFunctionName == "$RequestDownload")
            {
                request_download.Invoke(attribute, image_list);
                image_list = new List<Tuple<string, string>>();
            }
            else if (func.ContentFunctionName == "$ExitLoop")
            {
                exit_loop = true;
            }
            else if (func.ContentFunctionName == "$ClearImagesCount")
            {
                variable_update(new SRCALParser.CDLVar { Name = "$LatestImagesCount", Type = SRCALParser.CDLVar.CDLVarType.Integer, ContentInteger = 0 });
            }
            else if (func.ContentFunctionName == "$ConsolePrint")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$ConsolePrint' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                Console.Console.Instance.Write(v1.ContentString);
            }
            else if (func.ContentFunctionName == "$ConsolePrintln")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$ConsolePrint' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                Console.Console.Instance.WriteLine(v1.ContentString);
            }
            else if (func.ContentFunctionName == "$MonitorPrint")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$ConsolePrint' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                Monitor.Instance.Push(v1.ContentString);
            }
            //
            //  Driver Internal Functions
            //
            else if (func.ContentFunctionName == "$DriverNew")
            {
                driver.Close();
                driver = new SeleniumWrapper();
            }
            else if (func.ContentFunctionName == "$DriverLoadPage")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$LoadPage' function must have one argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = variable_get("$RequestURL");
                var v1 = run_index(v, func.ContentArguments[0]);
                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }
                v.ContentString = v1.ContentString;
                variable_update(v);
                info_message.Add(Tuple.Create(func.Line, func.Column, $"download request html {v.ContentString}"));
                driver.Navigate(v.ContentString);
                driver.WaitComplete();
                current_html = driver.GetHtml();
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(current_html);
                root_node = document.DocumentNode;
            }
            else if (func.ContentFunctionName == "$DriverClickByXPath")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$DriverClickByXPath' function must have one argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                driver.ClickXPath(v1.ContentString);
            }
            else if (func.ContentFunctionName == "$DriverClickByName")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$DriverClickByName' function must have one argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                driver.ClickName(v1.ContentString);
            }
            else if (func.ContentFunctionName == "$DriverSendKey")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'$DriverSendKey' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String || v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                driver.SendKeyId(v1.ContentString, v2.ContentString);
            }
            else if (func.ContentFunctionName == "$DriverGetScrollHeight")
            {
                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.Integer,
                    ContentInteger = Convert.ToInt32(driver.GetHeight())
                };
            }
            else if (func.ContentFunctionName == "$DriverScrollTo")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$DriverScrollTo' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = "argument type must be integer type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                driver.Scroll(v1.ContentInteger);
            }
            else if (func.ContentFunctionName == "$DriverScrollBottom")
            {
                driver.ScrollDown();
            }
            //
            //  Message Internal Function
            //
            else if (func.ContentFunctionName == "$MessageFadeOn")
            {
                // $MessageFadeOn(false, "error")
                // $MessageFadeOn(true, "start download")

                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'$MessageFadeOn' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (!(v1.Type == SRCALParser.CDLVar.CDLVarType.Integer || v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean))
                {
                    var msg = "argument type must be integer or boolean type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                if (v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v2.Line, v2.Column, msg));
                    throw new Exception(msg);
                }

                bool progress = false;
                if (v1.Type == SRCALParser.CDLVar.CDLVarType.Integer)
                    progress = v1.ContentInteger == 0 ? false : true;
                else if (v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean)
                    progress = v1.ContentBoolean;
                Global.MessageFadeOn(progress, v2.ContentString);
            }
            else if (func.ContentFunctionName == "$MessageText")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'$MessageText' function must have one argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                Global.MessageText(v1.ContentString);
            }
            else if (func.ContentFunctionName == "$MessageFadeOff")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'$MessageFadeOff' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (!(v1.Type == SRCALParser.CDLVar.CDLVarType.Integer || v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean))
                {
                    var msg = "argument type must be integer or boolean type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                if (v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v2.Line, v2.Column, msg));
                    throw new Exception(msg);
                }

                bool progress = false;
                if (v1.Type == SRCALParser.CDLVar.CDLVarType.Integer)
                    progress = v1.ContentInteger == 0 ? false : true;
                else if (v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean)
                    progress = v1.ContentBoolean;
                Global.MessageFadeOff(progress, v2.ContentString);
            }
            else if (func.ContentFunctionName == "$MessageFadeInFadeOut")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'$MessageFadeInFadeOut' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (!(v1.Type == SRCALParser.CDLVar.CDLVarType.Integer || v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean))
                {
                    var msg = "argument type must be integer or boolean type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                if (v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v2.Line, v2.Column, msg));
                    throw new Exception(msg);
                }

                bool progress = false;
                if (v1.Type == SRCALParser.CDLVar.CDLVarType.Integer)
                    progress = v1.ContentInteger == 0 ? false : true;
                else if (v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean)
                    progress = v1.ContentBoolean;
                Global.MessageFadeInFadeOut(progress, v2.ContentString);
            }
            //
            //  Common functions
            //
            else if (func.ContentFunctionName == "concat")
            {
                var builder = new StringBuilder();

                foreach (var arg in func.ContentArguments)
                {
                    var v = new SRCALParser.CDLVar();
                    var v1 = run_index(v, arg);

                    if (v1.Type == SRCALParser.CDLVar.CDLVarType.Integer)
                        builder.Append(Convert.ToString(v1.ContentInteger));
                    else if (v1.Type == SRCALParser.CDLVar.CDLVarType.String)
                        builder.Append(v1.ContentString);
                    else
                    {
                        var msg = $"boolean or string-list type cannot be 'concat' arguments";
                        error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                        throw new Exception(msg);
                    }
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = builder.ToString()
                };
            }
            else if (func.ContentFunctionName == "cal")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'cal' function must have one argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(v1.Line, v1.Column, msg));
                    throw new Exception(msg);
                }

                List<string> sl;
                try
                {
                    sl = Html.HtmlCAL.Calculate(v1.ContentString, root_node);
                }
                catch (Exception e)
                {
                    //error_message.Add(Tuple.Create(func.Line, func.Column, e.Message));
                    sl = new List<string>() { "" };
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.StringList,
                    ContentStringList = sl
                };
            }
            else if (func.ContentFunctionName == "equal")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'equal' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != v2.Type)
                {
                    var msg = "each type is different, so cannot compare two elements.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                if (v1.Type == SRCALParser.CDLVar.CDLVarType.StringList)
                {
                    var msg = "cannot test equal string-list type";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                if ((v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean && v1.ContentBoolean == v2.ContentBoolean) ||
                    (v1.Type == SRCALParser.CDLVar.CDLVarType.Integer && v1.ContentInteger == v2.ContentInteger) ||
                    (v1.Type == SRCALParser.CDLVar.CDLVarType.String  && v1.ContentString  == v2.ContentString ))
                {
                    return new SRCALParser.CDLVar
                    {
                        Line = func.Line,
                        Column = func.Column,
                        Name = "$rvalue",
                        Type = SRCALParser.CDLVar.CDLVarType.Boolean,
                        ContentBoolean = true
                    };
                }
                else
                {
                    return new SRCALParser.CDLVar
                    {
                        Line = func.Line,
                        Column = func.Column,
                        Name = "$rvalue",
                        Type = SRCALParser.CDLVar.CDLVarType.Boolean,
                        ContentBoolean = false
                    };
                }
            }
            else if (func.ContentFunctionName == "split")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'equal' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String || v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "arguments type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.StringList,
                    ContentStringList = v1.ContentString.Split(new string[] { v2.ContentString }, StringSplitOptions.None).ToList()
                };
            }
            else if (func.ContentFunctionName == "count")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'count' function must have 1 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.StringList)
                {
                    var msg = "arguments type must be string-list type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.Integer,
                    ContentInteger = v1.ContentStringList.Count
                };
            }
            else if (func.ContentFunctionName == "add")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'mul' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.Integer || v2.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = "arguments type must be integer type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.Integer,
                    ContentInteger = v1.ContentInteger + v2.ContentInteger
                };
            }
            else if (func.ContentFunctionName == "mul")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'multiple' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.Integer || v2.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = "arguments type must be integer type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.Integer,
                    ContentInteger = v1.ContentInteger * v2.ContentInteger
                };
            }
            else if (func.ContentFunctionName == "div")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'div' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.Integer || v2.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = "arguments type must be integer type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.Integer,
                    ContentInteger = v1.ContentInteger / v2.ContentInteger
                };
            }
            else if (func.ContentFunctionName == "mod")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'mod' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.Integer || v2.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = "arguments type must be integer type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.Integer,
                    ContentInteger = v1.ContentInteger % v2.ContentInteger
                };
            }
            else if (func.ContentFunctionName == "url_parameter")
            {
                if (func.ContentArguments.Count != 3 && func.ContentArguments.Count != 2)
                {
                    var msg = "'url_parameter' function must have 2 or 3 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String || v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "1,2 arguments type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                if (func.ContentArguments.Count == 2)
                {
                    var url = new Uri(v1.ContentString);
                    var query = HttpUtility.ParseQueryString(url.Query);

                    return new SRCALParser.CDLVar
                    {
                        Line = func.Line,
                        Column = func.Column,
                        Name = "$rvalue",
                        Type = SRCALParser.CDLVar.CDLVarType.String,
                        ContentString = query.Get(v2.ContentString)
                    };
                }
                else if (func.ContentArguments.Count == 3)
                {
                    var v3 = run_index(v, func.ContentArguments[2]);

                    if (!(v3.Type == SRCALParser.CDLVar.CDLVarType.String || v3.Type == SRCALParser.CDLVar.CDLVarType.Integer))
                    {
                        var msg = "3rd argument type must be int or string type.";
                        error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                        throw new Exception(msg);
                    }

                    var param = "";

                    if (v3.Type == SRCALParser.CDLVar.CDLVarType.Integer)
                        param = v3.ContentInteger.ToString();
                    else
                        param = v3.ContentString;

                    var url = new Uri(v1.ContentString);
                    var query = HttpUtility.ParseQueryString(url.Query);
                    query.Set(v2.ContentString, param);

                    return new SRCALParser.CDLVar
                    {
                        Line = func.Line,
                        Column = func.Column,
                        Name = "$rvalue",
                        Type = SRCALParser.CDLVar.CDLVarType.String,
                        ContentString = url.AbsoluteUri.Split('?').FirstOrDefault() + '?' + query.ToString()
                    };
                }
            }
            else if (func.ContentFunctionName == "url_parameter_tidy")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'url_parameter_tidy' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String || v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "arguments type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }
                
                var url = new Uri(v1.ContentString);
                var query = HttpUtility.ParseQueryString(url.Query);
                query.Remove(v2.ContentString);

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = url.AbsoluteUri.Split('?').FirstOrDefault() + '?' + query.ToString()
                };
            }
            else if (func.ContentFunctionName == "int")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'int' function must have 1 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (!(v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean ||
                      v1.Type == SRCALParser.CDLVar.CDLVarType.Integer ||
                      v1.Type == SRCALParser.CDLVar.CDLVarType.String ))
                {
                    var msg = "arguments type must be boolean, integer or string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                int value = 0;
                if (v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean)
                {
                    value = v1.ContentBoolean ? 1 : 0;
                }
                else if (v1.Type == SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    value = v1.ContentInteger;
                }
                else if (v1.Type == SRCALParser.CDLVar.CDLVarType.String)
                {
                    if (!int.TryParse(v1.ContentString, out value))
                    {
                        var msg = $"cannot convert '{v1.ContentString}' to integer type.";
                        error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                        throw new Exception(msg);
                    }
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.Integer,
                    ContentInteger = value
                };
            }
            else if (func.ContentFunctionName == "string")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'string' function must have 1 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (!(v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean ||
                      v1.Type == SRCALParser.CDLVar.CDLVarType.Integer ||
                      v1.Type == SRCALParser.CDLVar.CDLVarType.String))
                {
                    var msg = "arguments type must be boolean, integer or string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                string value = "";
                if (v1.Type == SRCALParser.CDLVar.CDLVarType.Boolean)
                {
                    value = v1.ContentBoolean ? "true" : "false";
                }
                else if (v1.Type == SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    value = v1.ContentInteger.ToString();
                }
                else if (v1.Type == SRCALParser.CDLVar.CDLVarType.String)
                {
                    value = v1.ContentString;
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = value
                };
            }
            else if (func.ContentFunctionName == "regex_exists")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'regex_exists' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String || v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "arguments type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var regex = new Regex(v1.ContentString);
                var match = regex.Match(v2.ContentString);
                if (match != null && match.Groups.Count > 0)
                {
                    return new SRCALParser.CDLVar
                    {
                        Line = func.Line,
                        Column = func.Column,
                        Name = "$rvalue",
                        Type = SRCALParser.CDLVar.CDLVarType.Boolean,
                        ContentBoolean = true
                    };
                }
                else
                {
                    return new SRCALParser.CDLVar
                    {
                        Line = func.Line,
                        Column = func.Column,
                        Name = "$rvalue",
                        Type = SRCALParser.CDLVar.CDLVarType.Boolean,
                        ContentBoolean = false
                    };
                }
            }
            else if (func.ContentFunctionName == "regex_match")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'regex_match' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String || v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "arguments type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var regex = new Regex(v1.ContentString);

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = regex.Match(v2.ContentString).Groups[0].Value
                };
            }
            else if (func.ContentFunctionName == "regex_matches")
            {
                if (func.ContentArguments.Count != 2 && func.ContentArguments.Count != 3)
                {
                    var msg = "'regex_matches' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);
                var v3 = 0;

                if (func.ContentArguments.Count == 3)
                {
                    var vt = run_index(v, func.ContentArguments[2]);
                    if (vt.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                    {
                        var msg = "3rd argument type must be integer type.";
                        error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                        throw new Exception(msg);
                    }
                    v3 = vt.ContentInteger;
                }

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String || v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "arguments type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var regex = new Regex(v1.ContentString);
                var match = regex.Match(v2.ContentString);
                List<string> mat = new List<string>();

                while (match.Success)
                {
                    mat.Add(match.Groups[v3].Value);
                    match = match.NextMatch();
                }
                
                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.StringList,
                    ContentStringList = mat
                };
            }
            else if (func.ContentFunctionName == "type")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'type' function must have 1 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = v1.Type.ToString()
                };
            }
            else if (func.ContentFunctionName == "to_json")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'to_json' function must have 1 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                var json = JObject.Parse(v1.ContentString);

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.JSon,
                    ContentJSon = json
                };
            }
            else if (func.ContentFunctionName == "get_json")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'get_json' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.JSon)
                {
                    var msg = "first argument type must be json type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                if (v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "second argument type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.JSon,
                    ContentJSon = v1.ContentJSon[v2.ContentString]
                };
            }
            else if (func.ContentFunctionName == "get_json_string")
            {
                if (func.ContentArguments.Count != 2)
                {
                    var msg = "'get_json' function must have 2 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);
                var v2 = run_index(v, func.ContentArguments[1]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.JSon)
                {
                    var msg = "first argument type must be json type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                if (v2.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "second argument type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = v1.ContentJSon[v2.ContentString].ToString()
                };
            }
            else if (func.ContentFunctionName == "base64decode")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'base64decode' function must have 1 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = Encoding.UTF8.GetString(Convert.FromBase64String(v1.ContentString))
                };
            }
            else if (func.ContentFunctionName == "htmldecode")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'htmldecode' function must have 1 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = HttpUtility.HtmlDecode(v1.ContentString)
                };
            }
            else if (func.ContentFunctionName == "urldecode")
            {
                if (func.ContentArguments.Count != 1)
                {
                    var msg = "'urldecode' function must have 1 argument.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var v1 = run_index(v, func.ContentArguments[0]);

                if (v1.Type != SRCALParser.CDLVar.CDLVarType.String)
                {
                    var msg = "argument type must be string type.";
                    error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                    throw new Exception(msg);
                }

                return new SRCALParser.CDLVar
                {
                    Line = func.Line,
                    Column = func.Column,
                    Name = "$rvalue",
                    Type = SRCALParser.CDLVar.CDLVarType.String,
                    ContentString = HttpUtility.UrlDecode(v1.ContentString)
                };
            }
            else
            {
                var msg = $"'{func.ContentFunctionName}' function not found.";
                error_message.Add(Tuple.Create(func.Line, func.Column, msg));
                throw new Exception(msg);
            }
            return null;
        }

        private void run_runnable(SRCALParser.CDLRunnable runnable)
        {
            if (runnable is SRCALParser.CDLLoop _loop)
            {
                var iter = _loop.ContentIterator;
                if (variable_exists(iter.Name))
                {
                    var msg = $"'{iter.Name}' variable already declared.";
                    error_message.Add(Tuple.Create(iter.Line, iter.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var start = run_index(v, _loop.ContentStarts);

                if (start.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = $"'loop starts' must be integer type.";
                    error_message.Add(Tuple.Create(start.Line, start.Column, msg));
                    throw new Exception(msg);
                }

                iter.Type = SRCALParser.CDLVar.CDLVarType.Integer;
                iter.ContentInteger = start.ContentInteger;

                enter_block();
                while (true)
                {
                    variable_update(iter);
                    var ends = run_index(v, _loop.ContentEnds);

                    if (ends.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                    {
                        var msg = $"'loop ends' must be integer type.";
                        error_message.Add(Tuple.Create(iter.Line, iter.Column, msg));
                        throw new Exception(msg);
                    }

                    if (iter.ContentInteger-1 == ends.ContentInteger) break;

                    enter_block();
                    run_block(_loop.ContentInnerBlock);
                    exit_block();

                    if (exit_loop)
                    {
                        exit_loop = false;
                        break;
                    }
                    iter.ContentInteger++;
                }
                exit_block();
            }
            else if (runnable is SRCALParser.CDLForEach _foreach)
            {
                var iter = _foreach.ContentIterator;
                if (variable_exists(iter.Name))
                {
                    var msg = $"'{iter.Name}' variable already declared.";
                    error_message.Add(Tuple.Create(iter.Line, iter.Column, msg));
                    throw new Exception(msg);
                }

                var v = new SRCALParser.CDLVar();
                var contents = run_index(v, _foreach.ContentSource);

                if (contents.Type != SRCALParser.CDLVar.CDLVarType.StringList && contents.Type != SRCALParser.CDLVar.CDLVarType.JSon)
                {
                    var msg = $"'foreach source' must be string-list or json type.";
                    error_message.Add(Tuple.Create(contents.Line, contents.Column, msg));
                    throw new Exception(msg);
                }


                enter_block();
                if (contents.Type == SRCALParser.CDLVar.CDLVarType.StringList)
                {
                    iter.Type = SRCALParser.CDLVar.CDLVarType.String;
                    foreach (var str in contents.ContentStringList)
                    {
                        iter.ContentString = str;
                        variable_update(iter);

                        enter_block();
                        run_block(_foreach.ContentBlock);
                        exit_block();

                        if (exit_loop)
                        {
                            exit_loop = false;
                            break;
                        }
                    }
                }
                else if (contents.Type == SRCALParser.CDLVar.CDLVarType.JSon)
                {
                    iter.Type = SRCALParser.CDLVar.CDLVarType.JSon;
                    foreach (var json in contents.ContentJSon.Children())
                    {
                        iter.ContentJSon = (JObject)json;
                        variable_update(iter);

                        enter_block();
                        run_block(_foreach.ContentBlock);
                        exit_block();

                        if (exit_loop)
                        {
                            exit_loop = false;
                            break;
                        }
                    }
                }
                exit_block();
            }
            else if (runnable is SRCALParser.CDLIf _if)
            {
                var v = new SRCALParser.CDLVar();
                var stmt = run_index(v, _if.ContentStatement);
                if (stmt.Type != SRCALParser.CDLVar.CDLVarType.Boolean && stmt.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = $"'if statement' must be boolean or integer type.";
                    error_message.Add(Tuple.Create(_if.ContentStatement.Line, _if.ContentStatement.Column, msg));
                    throw new Exception(msg);
                }

                if ((stmt.Type == SRCALParser.CDLVar.CDLVarType.Boolean && stmt.ContentBoolean == true) ||
                    (stmt.Type == SRCALParser.CDLVar.CDLVarType.Integer && stmt.ContentInteger != 0))
                {
                    enter_block();
                    run_block(_if.ContentBlock);
                    exit_block();
                }

            }
            else if (runnable is SRCALParser.CDLIfElse _ifelse)
            {
                var v = new SRCALParser.CDLVar();
                var stmt = run_index(v, _ifelse.ContentStatement);
                if (stmt.Type != SRCALParser.CDLVar.CDLVarType.Boolean && stmt.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = $"'if-else statement' must be boolean or integer type.";
                    error_message.Add(Tuple.Create(_ifelse.ContentStatement.Line, _ifelse.ContentStatement.Column, msg));
                    throw new Exception(msg);
                }

                enter_block();
                if ((stmt.Type == SRCALParser.CDLVar.CDLVarType.Boolean && stmt.ContentBoolean == true) ||
                    (stmt.Type == SRCALParser.CDLVar.CDLVarType.Integer && stmt.ContentInteger != 0))
                {
                    run_block(_ifelse.ContentIfBlock);
                }
                else
                {
                    run_block(_ifelse.ContentElseBlock);
                }
                exit_block();
            }
        }

        private SRCALParser.CDLVar run_index(SRCALParser.CDLVar var, SRCALParser.CDLIndex index)
        {
            var _var = run_variable(index.ContentVariable);

            if (_var == null)
            {
                var msg = "variable is null.";
                error_message.Add(Tuple.Create(index.ContentVariable.Line, index.ContentVariable.Column, msg));
                throw new Exception(msg);
            }

            if (index.UseIndex)
            {
                if (_var.Type != SRCALParser.CDLVar.CDLVarType.StringList)
                {
                    var msg = "var is not string-list type.";
                    error_message.Add(Tuple.Create(_var.Line, _var.Column, msg));
                    throw new Exception(msg);
                }

                var result1 = new SRCALParser.CDLVar();
                result1.Line = _var.Line;
                result1.Column = _var.Column;
                result1.Name = var.Name;
                result1.Type = SRCALParser.CDLVar.CDLVarType.String;

                var _index = run_variable(index.ContentIndex);

                if (_index.Type != SRCALParser.CDLVar.CDLVarType.Integer)
                {
                    var msg = "index is not integer type.";
                    error_message.Add(Tuple.Create(_var.Line, _var.Column, msg));
                    throw new Exception(msg);
                }

                if (_var.Type == SRCALParser.CDLVar.CDLVarType.StringList)
                {
                    if (_index.ContentInteger == -1)
                        result1.ContentString = _var.ContentStringList.Last();
                    else
                        result1.ContentString = _var.ContentStringList[_index.ContentInteger];
                    return result1;
                }
            }

            var result = new SRCALParser.CDLVar();
            result.Line = _var.Line;
            result.Column = _var.Column;
            result.Type = _var.Type;
            result.Name = var.Name;
            result.ContentBoolean = _var.ContentBoolean;
            result.ContentInteger = _var.ContentInteger;
            result.ContentString = _var.ContentString;
            result.ContentStringList = _var.ContentStringList;
            result.ContentJSon = _var.ContentJSon;
            return result;
        }

        private SRCALParser.CDLVar run_variable(SRCALParser.CDLVariable variable)
        {
            if (variable.Type == SRCALParser.CDLVariable.CDLVariableType.Function &&
                variable.ContentFunction.IsReturnVoid == true)
            {
                var msg = "this function must be a function that returns a value.";
                error_message.Add(Tuple.Create(variable.ContentFunction.Line, variable.ContentFunction.Column, msg));
                throw new Exception(msg);
            }

            switch (variable.Type)
            {
                case SRCALParser.CDLVariable.CDLVariableType.Var:
                    if (variable_exists(variable.ContentVar.Name))
                        return variable_get(variable.ContentVar.Name);
                    else
                    {
                        var msg = $"'{variable.ContentVar.Name}' variable not declared in this area.";
                        error_message.Add(Tuple.Create(variable.ContentVar.Line, variable.ContentVar.Column, msg));
                        throw new Exception(msg);
                    }

                case SRCALParser.CDLVariable.CDLVariableType.Function:
                    return run_function(variable.ContentFunction);

                case SRCALParser.CDLVariable.CDLVariableType.Const:
                    if (variable.ContentConst.Type == SRCALParser.CDLConst.CDLConstType.Boolean)
                        return new SRCALParser.CDLVar { Line = variable.ContentConst.Line, Column = variable.ContentConst.Column, Type = SRCALParser.CDLVar.CDLVarType.Boolean, ContentBoolean = variable.ContentConst.ContentBoolean };
                    else if (variable.ContentConst.Type == SRCALParser.CDLConst.CDLConstType.Integer)
                        return new SRCALParser.CDLVar { Line = variable.ContentConst.Line, Column = variable.ContentConst.Column, Type = SRCALParser.CDLVar.CDLVarType.Integer, ContentInteger = variable.ContentConst.ContentInteger };
                    else if (variable.ContentConst.Type == SRCALParser.CDLConst.CDLConstType.String)
                        return new SRCALParser.CDLVar { Line = variable.ContentConst.Line, Column = variable.ContentConst.Column, Type = SRCALParser.CDLVar.CDLVarType.String, ContentString = variable.ContentConst.ContentString };
                    else
                    {
                        var msg = "unidentified type.";
                        error_message.Add(Tuple.Create(variable.ContentConst.Line, variable.ContentConst.Column, msg));
                        throw new Exception(msg);
                    }

                default:
                    {
                        var msg = "unidentified type.";
                        error_message.Add(Tuple.Create(variable.Line, variable.Column, msg));
                        throw new Exception(msg);
                    }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Simple Robust CAL - CDL 스크립트를 실행하는 클래스입니다.
    /// </summary>
    public class SRCALScript
    {
        SRCALEngine engine = new SRCALEngine();
        List<string> raw_script;

        public List<string> RawScript { get { return raw_script; } }
        public SRCALEngine.SRCALAttribute Attribute() => engine.Attribute;
        
        public bool SpecifyURL(string url) => url.Contains(Attribute().URLSpecifier);
        
        public bool AllocScript(string script)
        {
            raw_script = script.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
                ).ToList();
            return engine.ParseScript(raw_script);
        }

        public bool Run(string url, Action<SRCALEngine.SRCALAttribute, List<Tuple<string, string>>> request_download)
        {
            return engine.RunScript(url, request_download);
        }
    }
}
