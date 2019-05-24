/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace Koromo_Copy
{
    public static class Extends
    {
        public static T Send<T>(this Control control, Func<T> func)
            => control.InvokeRequired ? (T)control.Invoke(func) : func();

        public static void Post(this Control control, Action action)
        {
            if (control.InvokeRequired) control.BeginInvoke(action);
            else action();
        }

        public static int ToInt32(this string str) => Convert.ToInt32(str);
    }

    ///<summary>
    /// https://blog.slaks.net/2010/12/simplifying-value-comparison-semantics.html
    /// Contains all of the properties of a class that 
    /// are used to provide value semantics.
    ///</summary>
    ///<remarks>
    /// You can create a static readonly ValueComparer for your class,
    /// then call into it from Equals, GetHashCode, and CompareTo.
    ///</remarks>
    class ValueComparer<T> : IComparer<T>, IEqualityComparer<T>
    {
        public ValueComparer(params Func<T, object>[] props)
        {
            Properties = new ReadOnlyCollection<Func<T, object>>(props);
        }

        public ReadOnlyCollection<Func<T, object>> Properties
        { get; private set; }

        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            //Object.Equals handles strings and nulls correctly
            return Properties.All(f => Equals(f(x), f(y)));
        }

        //https://stackoverflow.com/questions/263400/263416#263416
        public int GetHashCode(T obj)
        {
            if (obj == null) return -42;
            unchecked
            {
                int hash = 17;
                foreach (var prop in Properties)
                {
                    object value = prop(obj);
                    if (value == null)
                        hash = hash * 23 - 1;
                    else
                        hash = hash * 23 + value.GetHashCode();
                }
                return hash;
            }
        }

        public int Compare(T x, T y)
        {
            foreach (var prop in Properties)
            {
                //The properties can be any type including null.
                var comp = Comparer.DefaultInvariant
                    .Compare(prop(x), prop(y));
                if (comp != 0)
                    return comp;
            }
            return 0;
        }
    }
}
