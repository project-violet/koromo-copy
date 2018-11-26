/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Koromo_Copy_UX3.Domain
{
    static class GlobalImpl
    {
        public static void InitGlobal()
        {
            Global.UXInvoke = (Action x) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        x();
                    }));
            };
        }
    }
}
