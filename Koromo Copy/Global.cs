/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy
{
    public class Global
    {
        public static Action<Action> UXInvoke;
        public static Func<Action,Task> UXWaitInvoke;

        public static Action<string> ShowArtistView;
    }
}
