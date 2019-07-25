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
    /// BNF to Parser Descriptions
    /// </summary>
    public class BNFParser
    {
        public struct BNFModel
        {
            public List<string> NonTerminals;
            public List<string> Teminals;

            public List<Tuple<string, List<string>>> ProductionRule;

            public string StartSymbol;
        }

        public BNFParser() { }

        public void Parse(string target)
        {
            raw_string = target;
            line = column = pos = 0;
        }

        private enum token_type
        {
            None,
            Literal,
            Symbol,
            Operator
        }

        /// <summary>
        /// A::= A ( '+' B | '-' C ) +
        ///     | C (D)? E
        ///     ;
        /// </summary>

        token_type tok;
        int pos;
        int line;
        int column;
        string raw_string;

        private string lex()
        {
            return "";
        }

        private string lookup()
        {
            token_type ttok = tok;
            int tpos = pos;
            int tline = line;
            int tcolumn = column;
            var str = lex();
            pos = tpos;
            line = tline;
            column = tcolumn;
            return str;
        }
    }
}
