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
    public class LPArgument
        : LPDefine
    {
        public LPType Type { get; set; }
    }

    public class LPFunction
    {
        LPType return_type;
        List<LPBasicBlock> blocks;
        List<LPArgument> arguments;
        string name;
        LPModule module;

        public List<LPBasicBlock> Childs { get { return blocks; } }
        public bool IsExtern { get; set; }
        public string Name { get; set; }
        public LPModule Module { get { return module; } }

        public LPFunction(LPModule module)
        {
            this.module = module;
        }
    }
}
