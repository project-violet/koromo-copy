/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.LP.Code
{
    public class LPBasicBlock
        : LPDefine
    {
        List<LPOperator> insts;

        public LPBasicBlock()
        {
            insts = new List<LPOperator>();
        }

        public List<LPOperator> Childs { get { return insts; } }

        public void Insert(LPOperator op)
        {
            insts.Add(op);
        }
    }
}
