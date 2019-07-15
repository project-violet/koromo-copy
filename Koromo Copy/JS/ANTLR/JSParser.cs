/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.JS.ANTLR
{
    public class JSParser
    {
        public JSParser()
        {
            try
            {
                string input = "";
                StringBuilder text = new StringBuilder();
                Console.Console.Instance.WriteLine("Input the chat.");
                
                AntlrInputStream inputStream = new AntlrInputStream(text.ToString());
                JavaScriptLexer speakLexer = new JavaScriptLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(speakLexer);
                JavaScriptParser speakParser = new JavaScriptParser(commonTokenStream);

                //SpeakParser.ChatContext chatContext = speakParser.chat();
                //JavaScriptParserVisitor visitor = new JavaScriptParserVisitor();
                //visitor.Visit(chatContext);

                //foreach (var line in visitor.Lines)
                //{
                //    Console.Console.Instance.WriteLine("{0} has said {1}", line.Person, line.Text);
                //}

                //JavaScriptParser parser = new JavaScriptParser();
                //parser.
            }
            catch (Exception ex)
            {
                Console.Console.Instance.WriteLine("Error: " + ex);
            }
        }
    }
}
