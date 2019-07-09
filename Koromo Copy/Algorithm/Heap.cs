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
        where C : IComparer<T>, new()
    {
        List<T> heap;
        C comp;

        public Heap(int capacity = 256)
        {
            heap = new List<T>(capacity);
            comp = new C();
        }

        public void Push(T d)
        {
            heap.Add(d);
            leaf_to_root();
        }

        public void Pop()
        {
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);
            root_to_leaf();
        }

        public T Front => heap[0];

        private void root_to_leaf()
        {
            int x = 0;
            int l = heap.Count - 1;
            while ( x < l )
            {
                int c1 = x * 2 + 1;
                int c2 = c1 + 1;

                //
                //      x
                //     / \
                //    /   \
                //   c1   c2
                //

                int c = c1;
                if (c2 < l && comp.Compare(heap[c2], heap[c1]) > 0)
                    c = c2;

                if (c < l && comp.Compare(heap[c], heap[x]) > 0)
                {
                    swap(c, x);
                    x = c;
                }
                else
                {
                    break;
                }
            }
        }

        private void leaf_to_root()
        {
            int x = heap.Count - 1;
            while ( x > 0 )
            {
                int p = (x - 1) >> 1;
                // 큰 놈이 앞으로 가게끔
                if (comp.Compare(heap[x], heap[p]) > 0)
                {
                    swap(p, x);
                    x = p;
                }
                else
                    break;
            }
        }

        private void swap(int i, int j)
        {
            T t = heap[i];
            heap[i] = heap[j];
            heap[j] = t;
        }
    }

    public class DefaultHeapComparer<T> : Comparer<T> where T : IComparable
    {
        public override int Compare(T x, T y)
            => x.CompareTo(y);
    }

    public class MinHeapComparer<T> : Comparer<T> where T : IComparable
    {
        public override int Compare(T x, T y)
            => y.CompareTo(x);
    }

    public class Heap<T> : Heap<T, DefaultHeapComparer<T>> where T : IComparable { }
    public class MinHeap<T> : Heap<T, MinHeapComparer<T>> where T : IComparable { }
    public class MaxHeap<T> : Heap<T, DefaultHeapComparer<T>> where T : IComparable { }
}
