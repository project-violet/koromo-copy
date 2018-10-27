/***

   Copyright (C) 2018. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;

namespace Koromo_Copy.Interface
{
    /// <summary>
    /// Lazy구현을 쉽게 해주는 클래스입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ILazy<T>
        where T : new()
    {
        private static readonly Lazy<T> instance = new Lazy<T>(() => new T());
        public static T Instance => instance.Value;
        public static bool IsValueCreated => instance.IsValueCreated;
    }
}
