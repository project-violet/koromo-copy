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

        public LPFunction CreateFunction()
        {
            var func = new LPFunction(this);
            funcs.Add(func);
            return new LPFunction(this);
        }
    }
}
