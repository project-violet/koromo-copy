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
    public class LPConstant
        : LPUser
    {
        public LPType Type { get; set; }
        public object Content { get; set; }

        public static LPConstant Create(int content)
            => new LPConstant { Type = new LPType { Type = LPType.TypeOption.t_integer }, Content = content };
        public static LPConstant Create(bool content)
            => new LPConstant { Type = new LPType { Type = LPType.TypeOption.t_bool }, Content = content };
        public static LPConstant Create(string content)
            => new LPConstant { Type = new LPType { Type = LPType.TypeOption.t_string }, Content = content };
        public static LPConstant Create(float content)
            => new LPConstant { Type = new LPType { Type = LPType.TypeOption.t_float }, Content = content };
        public static LPConstant Create(double content)
            => new LPConstant { Type = new LPType { Type = LPType.TypeOption.t_double }, Content = content };
        public static LPConstant Create(LPType type, object content = null)
            => new LPConstant { Type = type, Content = content };
    }
}
