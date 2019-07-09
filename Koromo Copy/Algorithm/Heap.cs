/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Algorithm
{
    public class Heap<T, C>
        where T : IComparable
        where C : IComparer
    {
        List<T> heap;

        public Heap(int capacity = 256)
        {
            heap = new List<T>(capacity);
        }

        public void Push(T d)
        {

        }

        public void Pop()
        {

        }

        public T Front()
        {
            throw new NotImplementedException();
        }
    }

    public class Heap<T> 
        : Heap<T, Comparer<T>> 
        where T : IComparable
    {
    }
}
