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

    public class LPUnaryOperator
        : LPOperator
    {
        public enum UnaryOption
        {
            inc,   // ++
            dec,   // --
            index, // [~]
            not,   // ~
        }

        public UnaryOption Option { get; set; }
        public static LPUnaryOperator Create(UnaryOption option, LPUser operand1)
        {
            var lpuo = new LPUnaryOperator
            {
                Option = option
            };
            lpuo.operand.Add(operand1);
            return lpuo;
        }
    }

    public class LPBinaryOperator
        : LPOperator
    {
        public enum BinaryOption
        {
            plus,
            minus,
            multiple,
            divide,
            modular,
            and,
            or,
            xor,
        }

        public BinaryOption Option { get; set; }
        public LPUser Operand { get { return operand[0]; } set { operand[0] = value; } }
        public static LPBinaryOperator Create(BinaryOption option, LPUser operand1, LPUser operand2)
        {
            var lpbo = new LPBinaryOperator
            {
                Option = option
            };
            lpbo.operand.Add(operand1);
            lpbo.operand.Add(operand2);
            return lpbo;
        }
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

        public static LPUnaryCompareOperator Create(CompareOption option, LPUser operand1)
        {
            var lpuco = new LPUnaryCompareOperator
            {
                Option = option
            };
            lpuco.operand.Add(operand1);
            return lpuco;
        }
    }

    public class LPBinaryCompareOperator
        : LPCompareOperator
    {
        public static LPBinaryCompareOperator Create(CompareOption option, LPUser operand1, LPUser operand2)
        {
            var lpbco = new LPBinaryCompareOperator
            {
                Option = option
            };
            lpbco.operand.Add(operand1);
            lpbco.operand.Add(operand2);
            return lpbco;
        }
    }

    public class LPBranchOperator
        : LPOperator
    {
        public LPCompareOperator Comparator { get; set; }

        public LPBasicBlock TrueBlock { get; set; }
        public LPBasicBlock FalseBlock { get; set; }

        public bool IsJump { get; set; }

        public static LPBranchOperator Create(LPCompareOperator comp, LPBasicBlock true_block, LPBasicBlock false_block)
            => new LPBranchOperator { Comparator = comp, TrueBlock = true_block, FalseBlock = false_block };
        public static LPBranchOperator Create(LPBasicBlock jump_block)
            => new LPBranchOperator { TrueBlock = jump_block, IsJump = true };
    }

    public class LPCallOperator
        : LPOperator
    {
        public LPFunction Caller { get; set; }
        public List<LPUser> Arguments { get; set; }

        public static LPCallOperator Create(LPFunction function, List<LPUser> args)
            => new LPCallOperator { Caller = function, Arguments = args };
    }

    public class LPAllocOperator
        : LPOperator
    {
        public LPType Type { get; set; }

        public static LPAllocOperator Create(LPType type)
            => new LPAllocOperator { Type = type };
    }

    public class LPStoreOperator
        : LPOperator
    {
        public LPUser Value { get; set; }
        public LPUser Pointer { get; set; }

        public static LPStoreOperator Create(LPUser value, LPUser pointer)
            => new LPStoreOperator { Value = value, Pointer = pointer };
    }

    public class LPLoadOperator
        : LPOperator
    {
        public LPUser Value { get; set; }

        public static LPStoreOperator Create(LPUser value)
            => new LPStoreOperator { Value = value };
    }
}
