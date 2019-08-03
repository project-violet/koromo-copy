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
    public abstract class LPOperator
        : LPUser
    {
        protected List<LPUser> operand;
        protected int opcode;
        
        public LPBasicBlock Parent { get; set; }
        public LPFunction Function { get; set; }
        public LPModule Module { get; set; }
        public LPDebugInfo DebugInfo { get; set; }

        public void RemoveFromParent()
        {

        }

        public void InsertBefore(LPOperator op)
        {

        }

        public void InsertAfter(LPOperator op)
        {

        }

        public void MoveBefore(LPOperator op)
        {

        }

        public void MoveAfter(LPOperator op)
        {

        }

        public LPUser GetOperand(int index) => operand[index];

        public override string ToString()
        {
            return "";
        }
    }

    public abstract class LPUnaryOperator
        : LPOperator
    {
    }

    public abstract class LPBinaryOperator
        : LPOperator
    {
    }

    public abstract class LPCompareOperator
        : LPOperator
    {
        public enum CompareOption
        {
            not,  // !
            zr,   // == 0
            nz,   // != 0

            eq,   // ==
            neq,  // !=
            less, // <
            leq,  // <=
            gret, // >
            geq,  // >=
        }

        public CompareOption Option { get; set; }
    }

    public class LPUnaryCompareOperator
        : LPCompareOperator
    {
        public LPUser Operand { get { return operand[0]; } set { operand[0] = value; } }
    }

    public class LPBinaryCompareOperator
        : LPCompareOperator
    {
    }

    public class LPBranchOperator
        : LPOperator
    {
        public LPCompareOperator Comparator { get; set; }

        public LPBasicBlock TrueBlock { get; set; }
        public LPBasicBlock FalseBlock { get; set; }
    }

    public class LPCallOperator
        : LPOperator
    {
        public LPFunction Caller { get; set; }
    }
}
