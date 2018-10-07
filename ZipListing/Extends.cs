/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Windows.Forms;

namespace Koromo_Copy
{
    static class Extends
    {
        public static T Send<T>(this Control control, Func<T> func)
            => control.InvokeRequired ? (T)control.Invoke(func) : func();

        public static void Post(this Control control, Action action)
        {
            if (control.InvokeRequired) control.BeginInvoke(action);
            else action();
        }
    }
}
