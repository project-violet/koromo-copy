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
    /// <summary>
    /// Resource Manager for LP Code
    /// </summary>
    public class LPModule
    {
        List<LPFunction> funcs;

        public LPModule()
        {
            funcs = new List<LPFunction>();
        }

        public LPFunction CreateFunction(string name, LPType return_type, List<LPArgument> args)
        {
            if (funcs.Any(x => x.Name == name))
                return funcs.Where(x => x.Name == name).ElementAt(0);
            var func = new LPFunction(this, name, return_type, args);
            funcs.Add(func);
            return func;
        }

        public LPFunction CreateFunction(string name)
        {
            if (funcs.Any(x => x.Name == name))
                return funcs.Where(x => x.Name == name).ElementAt(0);
            var func = new LPFunction(this, name, new LPType { Type = LPType.TypeOption.t_void }, new List<LPArgument>());
            funcs.Add(func);
            return func;
        }
    }
}
