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
    public class LPTest
    {
        public static void test_operator()
        {
            /*
            int a = 0;
            for (int i = 0; i < 10; i++)
              a += i;
            */

            LPModule M = new LPModule();
            LPFunction F = M.CreateFunction("main");

            LPBasicBlock entry = F.CreateBasicBlock();
            LPBasicBlock check = F.CreateBasicBlock();
            LPBasicBlock exit = F.CreateBasicBlock();
            LPBasicBlock inc = F.CreateBasicBlock();
            LPBasicBlock body = F.CreateBasicBlock();

            // int a = 0;
            // int i = 0;
            LPAllocOperator var_a = LPAllocOperator.Create(new LPType { Type = LPType.TypeOption.t_integer });
            entry.Insert(var_a);
            LPStoreOperator store_a = LPStoreOperator.Create(LPConstant.Create(0), var_a);
            entry.Insert(store_a);
            LPAllocOperator var_i = LPAllocOperator.Create(new LPType { Type = LPType.TypeOption.t_integer });
            entry.Insert(var_i);
            LPStoreOperator store_i = LPStoreOperator.Create(LPConstant.Create(0), var_i);
            entry.Insert(store_i);
            entry.Insert(LPBranchOperator.Create(check));

            // i < 10
            LPConstant int_10 = LPConstant.Create(10);
            LPBinaryCompareOperator comp = LPBinaryCompareOperator.Create(LPCompareOperator.CompareOption.less, var_i, int_10);
            check.Insert(comp);
            check.Insert(LPBranchOperator.Create(comp, body, exit));

            // a += i
            LPBinaryOperator plus = LPBinaryOperator.Create(LPBinaryOperator.BinaryOption.plus, var_a, var_i);
            body.Insert(plus);
            LPStoreOperator store_result = LPStoreOperator.Create(var_a, var_i);
            body.Insert(store_result);
            body.Insert(LPBranchOperator.Create(inc));

            // i++
            LPUnaryOperator increase = LPUnaryOperator.Create(LPUnaryOperator.UnaryOption.inc, var_i);
            inc.Insert(increase);
            LPStoreOperator store2_result = LPStoreOperator.Create(increase, var_i);
            inc.Insert(store2_result);
            inc.Insert(LPBranchOperator.Create(check));

            // exit
        }
    }
}
