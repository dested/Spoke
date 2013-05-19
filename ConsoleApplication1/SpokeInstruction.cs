using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApplication1
{
    public class SpokeInstruction
    {
        public static List<SpokeInstruction> ins;
        public static void Beginner()
        {
            ins = new List<SpokeInstruction>(2000);
        }
        public static SpokeInstruction[] Ender()
        {
            var d = ins.ToArray();
            ins.Clear();
            return d;
        }
        public SpokeInstructionType Type;
        public int Index;
        public int Index2;
        public int Index3;
        public readonly bool BoolVal;
        public readonly float FloatVal;
        public string StringVal;
        public SpokeObjectMethod anonMethod;

        public string DEBUG;

        public override string ToString()
        {
            //            string m = ShouldOnlyBeStack.ToString();
            string m = "";

            if (Type == SpokeInstructionType.BoolConstant)
            {
                return Type + "  \t" + m + "  \t " + BoolVal;
            } if (Type == SpokeInstructionType.StringConstant)
            {
                return Type + "  \t" + m + "  \t \'" + StringVal + "'";
            }
            if (Type == SpokeInstructionType.IntConstant)
            {
                return Type + "  \t" + m + "  \t " + Index;
            }
            if (Type == SpokeInstructionType.FloatConstant)
            {
                return Type + "  \t" + m + "  \t " + FloatVal;
            }
            return Type.ToString() + "  \t" + m + "  \t " + Index + " " + Index2 + " " + Index3 + "  \t" + DEBUG + "  \t  \t" + (gotoGuy ?? labelGuy) + "  " + (elseGuy ?? "");
        }
        public SpokeInstruction(SpokeInstructionType it)
        {
            Type = it;
            ins.Add(this);

        }

        public SpokeInstruction(SpokeInstructionType getLocal, float i)
            : this(getLocal)
        {
            FloatVal = i;
        }
        public SpokeInstruction(SpokeInstructionType getLocal, int i, int i2, int i3)
            : this(getLocal)
        {
            Index = i;
            Index2 = i2;
            Index3 = i3;

        }
        public SpokeInstruction(SpokeInstructionType getLocal, int i)
            : this(getLocal)
        {
            Index = i;

        }

        public string gotoGuy;
        public string labelGuy;

        public SpokeInstruction(SpokeInstructionType getLocal, string i)
            : this(getLocal)
        {
            if (getLocal == SpokeInstructionType.Goto)
            {
                gotoGuy = i;
            }
            else
                if (getLocal == SpokeInstructionType.IfTrueContinueElse)
                {
                    elseGuy = i;
                }
                else
                    if (getLocal == SpokeInstructionType.Label)
                    {
                        labelGuy = i;
                    }
                    else

                        StringVal = i;


        }
        public string elseGuy;

        public SpokeInstruction(SpokeInstructionType getLocal, bool i)
            : this(getLocal)
        {
            BoolVal = i;
        }

        public SpokeInstruction(SpokeInstructionType createMethod, SpokeObjectMethod spokeObjectMethod)
            : this(createMethod)
        {
            anonMethod = spokeObjectMethod;
        }

        public int StackAfter_ = -1;
        public int StackBefore_ = -1;
        public int StackAfter()
        {
            switch (Type)
            {
                case SpokeInstructionType.EMPTY:
                    throw new AbandonedMutexException();

                case SpokeInstructionType.Comment:
                    return 0;
                case SpokeInstructionType.CreateReference:
                case SpokeInstructionType.CreateArray:
                case SpokeInstructionType.CreateMethod:
                    return 1;

                case SpokeInstructionType.Label:
                case SpokeInstructionType.Goto:
                    return 0;

                case SpokeInstructionType.CallMethod:
                    return 0 - Index3 + 1;
                case SpokeInstructionType.CallInternal:
                    return 0 - Index3 + 1;
                case SpokeInstructionType.CallMethodFunc:
                    return 0 - Index3 + 1;


                case SpokeInstructionType.BreakpointInstruction:
                    return 0;

                case SpokeInstructionType.Return:
                    return 0;

                case SpokeInstructionType.IfTrueContinueElse: 
                    return 0;


                case SpokeInstructionType.AddToArray:
                    return -1;

                case SpokeInstructionType.StoreLocalBool:
                case SpokeInstructionType.StoreLocalInt:
                case SpokeInstructionType.StoreLocalFloat:
                case SpokeInstructionType.StoreLocalMethod:
                case SpokeInstructionType.StoreLocalObject:
                case SpokeInstructionType.StoreLocalString:
                    return 0;

                case SpokeInstructionType.StoreLocalRef:
                    return 0;

                case SpokeInstructionType.StoreFieldBool:
                case SpokeInstructionType.StoreFieldInt:
                case SpokeInstructionType.StoreFieldFloat:
                case SpokeInstructionType.StoreFieldMethod:
                case SpokeInstructionType.StoreFieldObject:
                case SpokeInstructionType.StoreFieldString:
                    return -1;

                case SpokeInstructionType.StoreToReference:
                    return 0;

                case SpokeInstructionType.GetField:
                    return 0;

                case SpokeInstructionType.GetLocal:
                    return 1;

                case SpokeInstructionType.PopStack:
                    return -1;

                case SpokeInstructionType.Not:
                    return 0;

                case SpokeInstructionType.LengthOfArray:
                    return 0;

                case SpokeInstructionType.ArrayElem:
                    return 0;


                case SpokeInstructionType.IntConstant:
                case SpokeInstructionType.BoolConstant:
                case SpokeInstructionType.FloatConstant:
                case SpokeInstructionType.StringConstant:
                case SpokeInstructionType.Null:
                    return 1;
                case SpokeInstructionType.InsertToArray:
                    return -1;
                case SpokeInstructionType.RemoveToArray:
                    return -1;


                case SpokeInstructionType.AddStringInt:
                case SpokeInstructionType.AddIntString:
                case SpokeInstructionType.AddIntInt:
                case SpokeInstructionType.AddIntFloat:
                case SpokeInstructionType.AddFloatInt:
                case SpokeInstructionType.AddFloatFloat:
                case SpokeInstructionType.AddFloatString:
                case SpokeInstructionType.AddStringFloat:
                case SpokeInstructionType.AddStringString:
                case SpokeInstructionType.SubtractIntInt:
                case SpokeInstructionType.SubtractIntFloat:
                case SpokeInstructionType.SubtractFloatInt:
                case SpokeInstructionType.SubtractFloatFloat:
                case SpokeInstructionType.MultiplyIntInt:
                case SpokeInstructionType.MultiplyIntFloat:
                case SpokeInstructionType.MultiplyFloatInt:
                case SpokeInstructionType.MultiplyFloatFloat:
                case SpokeInstructionType.DivideIntInt:
                case SpokeInstructionType.DivideIntFloat:
                case SpokeInstructionType.DivideFloatInt:
                case SpokeInstructionType.DivideFloatFloat:
                case SpokeInstructionType.GreaterIntInt:
                case SpokeInstructionType.GreaterIntFloat:
                case SpokeInstructionType.GreaterFloatInt:
                case SpokeInstructionType.GreaterFloatFloat:
                case SpokeInstructionType.LessIntInt:
                case SpokeInstructionType.LessIntFloat:
                case SpokeInstructionType.LessFloatInt:
                case SpokeInstructionType.LessFloatFloat:
                case SpokeInstructionType.GreaterEqualIntInt:
                case SpokeInstructionType.GreaterEqualIntFloat:
                case SpokeInstructionType.GreaterEqualFloatInt:
                case SpokeInstructionType.GreaterEqualFloatFloat:
                case SpokeInstructionType.LessEqualIntInt:
                case SpokeInstructionType.LessEqualIntFloat:
                case SpokeInstructionType.LessEqualFloatInt:
                case SpokeInstructionType.LessEqualFloatFloat:
                case SpokeInstructionType.Equal:
                case SpokeInstructionType.Or:
                case SpokeInstructionType.And:
                    return 0;
                case SpokeInstructionType.AddRangeToArray:
                    return -1;

                case SpokeInstructionType.StoreArrayElem:
                    return -3;


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public int StackBefore()
        {
            switch (Type)
            {
                case SpokeInstructionType.EMPTY:
                    throw new AbandonedMutexException();
                case SpokeInstructionType.Comment:
                    return 0;


                case SpokeInstructionType.CreateReference:
                case SpokeInstructionType.CreateArray:
                case SpokeInstructionType.CreateMethod:
                    return 0;

                case SpokeInstructionType.Label:
                    return 0;

                case SpokeInstructionType.Goto:
                    return 0;

                case SpokeInstructionType.CallMethod:
                    return 0;
                case SpokeInstructionType.CallInternal:
                    return 0;
                case SpokeInstructionType.CallMethodFunc:
                    return 0;

                case SpokeInstructionType.BreakpointInstruction:
                    return 0;

                case SpokeInstructionType.Return:
                    return -1;

                case SpokeInstructionType.IfTrueContinueElse: 
                    return -1;
                case SpokeInstructionType.AddToArray:
                    return 0;

                case SpokeInstructionType.StoreLocalBool:
                case SpokeInstructionType.StoreLocalInt:
                case SpokeInstructionType.StoreLocalFloat:
                case SpokeInstructionType.StoreLocalMethod:
                case SpokeInstructionType.StoreLocalObject:
                case SpokeInstructionType.StoreLocalString:
                    return -1;

                case SpokeInstructionType.StoreLocalRef:
                    return -1;

                case SpokeInstructionType.StoreFieldBool:
                case SpokeInstructionType.StoreFieldInt:
                case SpokeInstructionType.StoreFieldFloat:
                case SpokeInstructionType.StoreFieldMethod:
                case SpokeInstructionType.StoreFieldObject:
                case SpokeInstructionType.StoreFieldString:
                    return -1;

                case SpokeInstructionType.StoreToReference:
                    return -1;

                case SpokeInstructionType.GetField:
                    return 0;

                case SpokeInstructionType.GetLocal:
                    return 0;

                case SpokeInstructionType.PopStack:
                    return 0;

                case SpokeInstructionType.Not:
                    return 0;

                case SpokeInstructionType.LengthOfArray:
                    return 0;

                case SpokeInstructionType.ArrayElem:
                    return -1;


                case SpokeInstructionType.IntConstant:
                case SpokeInstructionType.BoolConstant:
                case SpokeInstructionType.FloatConstant:
                case SpokeInstructionType.StringConstant:
                case SpokeInstructionType.Null:
                    return 0;

                case SpokeInstructionType.InsertToArray:
                    return 0;

                case SpokeInstructionType.AddStringInt:
                case SpokeInstructionType.AddIntString:
                case SpokeInstructionType.AddIntInt:
                case SpokeInstructionType.AddIntFloat:
                case SpokeInstructionType.AddFloatInt:
                case SpokeInstructionType.AddFloatFloat:
                case SpokeInstructionType.AddFloatString:
                case SpokeInstructionType.AddStringFloat:
                case SpokeInstructionType.AddStringString:
                case SpokeInstructionType.SubtractIntInt:
                case SpokeInstructionType.SubtractIntFloat:
                case SpokeInstructionType.SubtractFloatInt:
                case SpokeInstructionType.SubtractFloatFloat:
                case SpokeInstructionType.MultiplyIntInt:
                case SpokeInstructionType.MultiplyIntFloat:
                case SpokeInstructionType.MultiplyFloatInt:
                case SpokeInstructionType.MultiplyFloatFloat:
                case SpokeInstructionType.DivideIntInt:
                case SpokeInstructionType.DivideIntFloat:
                case SpokeInstructionType.DivideFloatInt:
                case SpokeInstructionType.DivideFloatFloat:
                case SpokeInstructionType.GreaterIntInt:
                case SpokeInstructionType.GreaterIntFloat:
                case SpokeInstructionType.GreaterFloatInt:
                case SpokeInstructionType.GreaterFloatFloat:
                case SpokeInstructionType.LessIntInt:
                case SpokeInstructionType.LessIntFloat:
                case SpokeInstructionType.LessFloatInt:
                case SpokeInstructionType.LessFloatFloat:
                case SpokeInstructionType.GreaterEqualIntInt:
                case SpokeInstructionType.GreaterEqualIntFloat:
                case SpokeInstructionType.GreaterEqualFloatInt:
                case SpokeInstructionType.GreaterEqualFloatFloat:
                case SpokeInstructionType.LessEqualIntInt:
                case SpokeInstructionType.LessEqualIntFloat:
                case SpokeInstructionType.LessEqualFloatInt:
                case SpokeInstructionType.LessEqualFloatFloat:
                case SpokeInstructionType.Equal:
                case SpokeInstructionType.Or:
                case SpokeInstructionType.And:
                    return -1;

                case SpokeInstructionType.AddRangeToArray:
                    return 0;

                case SpokeInstructionType.StoreArrayElem:
                    return 0;


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    public class SpokeMethod
    {
        public SpokeLine[] Lines;
        public SpokeClass Class;
        public SpokeType returnType;
        public string MethodName;
        public string CleanMethodName { get { return MethodName.Replace(".", ""); } }
        public string[] Parameters;
        public bool HasYield;
        public bool HasReturn;

        public Func<SpokeObject[], SpokeObject> MethodFunc;
        public bool HasYieldReturn;
        public int NumOfVars;
        public SpokeInstruction[] Instructions;
        public bool Evaluated;
        public SpokeVariableInfo VariableRefs;
    }
    public class SpokeClass
    {

        public List<SpokeMethod> Methods = new List<SpokeMethod>();
        public string Name;
        public string[] Variables;
    }

    public enum SpokeInstructionType
    {
        EMPTY,
        StoreLocalInt,
        StoreLocalFloat,
        StoreLocalObject,
        StoreLocalBool,
        StoreLocalString,
        StoreLocalMethod,
        StoreFieldInt,
        StoreFieldFloat,
        StoreFieldObject,
        StoreFieldBool,
        StoreFieldString,


        StoreFieldMethod,

        CreateReference,
        CreateArray,
        CreateMethod,

        Label,
        Goto,
        CallMethod,
        CallInternal,

        BreakpointInstruction,
        Return, 
        IfTrueContinueElse,
        Or,
        And,

        AddToArray,

        //StoreLocal,
        StoreLocalRef,
        // StoreField,
        StoreToReference,

        GetField,
        GetLocal,
        PopStack,
        Not,
        LengthOfArray,
        ArrayElem,
        AddStringInt,
        AddIntString,
        IntConstant,
        BoolConstant,
        FloatConstant,
        StringConstant,
        InsertToArray,
        RemoveToArray,

        Null,
        AddIntInt,
        AddIntFloat,
        AddFloatInt,
        AddFloatFloat,
        AddFloatString,
        AddStringFloat,
        AddStringString,
        SubtractIntInt,
        SubtractIntFloat,
        SubtractFloatInt,
        SubtractFloatFloat,
        MultiplyIntInt,
        MultiplyIntFloat,
        MultiplyFloatInt,
        MultiplyFloatFloat,
        DivideIntInt,
        DivideIntFloat,
        DivideFloatInt,
        DivideFloatFloat,
        GreaterIntInt,
        GreaterIntFloat,
        GreaterFloatInt,
        GreaterFloatFloat,
        LessIntInt,
        LessIntFloat,
        LessFloatInt,
        LessFloatFloat,
        GreaterEqualIntInt,
        GreaterEqualIntFloat,
        GreaterEqualFloatInt,
        GreaterEqualFloatFloat,
        LessEqualIntInt,
        LessEqualIntFloat,
        LessEqualFloatInt,
        LessEqualFloatFloat,
        Equal,
        AddRangeToArray,
        StoreArrayElem,
        CallMethodFunc,
        Comment
    }


}