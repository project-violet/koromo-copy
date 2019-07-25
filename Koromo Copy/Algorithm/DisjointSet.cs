/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.

   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Algorithm
{
    public class DisjointSet
    {
        int[] array;

        public DisjointSet(int N)
        {
            array = Enumerable.Range(0, N).ToArray();
        }

        public int Find(int x)
        {
            if (array[x] == x) return x;
            return array[x] = Find(x);
        }
        
        public void Union(int a, int b)
        {
            int aa = Find(a);
            int bb = Find(b);

            if (aa == bb) return;

            if (aa > bb)
            {
                array[aa] = bb;
            }
            else
            {
                array[bb] = aa;
            }
        }
    }
}
