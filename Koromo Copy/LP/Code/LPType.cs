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
    public class LPType
    {
        public enum TypeOption
        {
            t_notdetermine,
            t_void,
            t_i8,
            t_i16,
            t_i32,
            t_i64,
            t_bool,
            t_float,
            t_double,
            t_ldouble,
            t_string,
        }

        public TypeOption Type { get; set; }
    }
}
