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
    public interface LPDefine
    {
    }

    public abstract class LPUser
        : LPDefine
    {
        public LPDebugInfo DebugInfo { get; set; }
    }
}
