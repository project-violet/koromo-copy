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

        public LPFunction(LPModule module, string name, LPType return_type, List<LPArgument> args)
        {
            blocks = new List<LPBasicBlock>();
            arguments = args;
            this.name = name;
            this.return_type = return_type;
            this.module = module;
        }

        public List<LPBasicBlock> Childs { get { return blocks; } }
        public bool IsExtern { get; set; }
        public string Name { get; set; }
        public LPModule Module { get { return module; } }
        public LPBasicBlock Entry { get { return blocks[0]; } }
        public LPType ReturnType { get { return return_type; } }
        public List<LPArgument> Arguments { get { return arguments; } }

        public LPBasicBlock CreateBasicBlock()
        {
            var block = new LPBasicBlock();
            blocks.Add(block);
            return block;
        }
    }
}
