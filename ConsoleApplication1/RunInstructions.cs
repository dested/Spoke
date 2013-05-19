//#define stacktrace
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    public class RunInstructions
    {
        private SpokeMethod[] Methods;
        private Func<SpokeObject[], SpokeObject>[] InternalMethods;


        private SpokeObject[] ints;
        private SpokeObject NULL = new SpokeObject() { Type = ObjectType.Null };


        private SpokeObject TRUE = new SpokeObject(ObjectType.Bool) { BoolVal = true };
        private SpokeObject FALSE = new SpokeObject(ObjectType.Bool) { BoolVal = false };


        public RunInstructions(Func<SpokeObject[], SpokeObject>[] internalMethods, SpokeMethod[] mets)
        {
            Methods = mets;
            InternalMethods = internalMethods;
            ints = new SpokeObject[100];
            for (int i = 0; i < 100; i++)
            {
                ints[i] = new SpokeObject(ObjectType.Int) { IntVal = i };
            }



        }



        public void Run(SpokeConstruct so)
        {
            var fm = Methods[so.MethodIndex];
            SpokeObject dm = new SpokeObject() { Type = ObjectType.Object };
            dm.Variables = new SpokeObject[so.NumOfVars];
            for (int index = 0; index < so.NumOfVars; index++)
            {
                dm.SetVariable(index, new SpokeObject());
            }
#if stacktrace
            try {
#endif
            evaluateMethod(fm, new SpokeObject[1] { dm });
#if stacktrace
            }
            catch (Exception er) {
                dfss.AppendLine(er.ToString());
                File.WriteAllText("C:\\ded.txt", dfss.ToString());
                throw er;
            }
#endif




            // evaluate(fm, dm, new List<SpokeObject>() { dm });
        }
        private SpokeObject intCache(int index)
        {
            if (index > 0 && index < 100)
            {
                return ints[index];
            }
            return new SpokeObject(ObjectType.Int) { IntVal = index };
        }

#if stacktrace
        private StringBuilder dfss = new StringBuilder();
#endif
        private SpokeObject evaluateMethod(SpokeMethod fm, SpokeObject[] paras)
        {

            SpokeObject[] variables = new SpokeObject[fm.NumOfVars];

            for (int i = 0; i < fm.Parameters.Length; i++)
            {
                variables[i] = paras[i];
            }

#if stacktrace
            dfss.AppendLine( fm.Class.Name +" : : "+fm.MethodName+" Start");
#endif

            SpokeObject lastStack;
            int stackIndex = 0;
            SpokeObject[] stack = new SpokeObject[3000];


            for (int index = 0; index < fm.Instructions.Length; index++)
            {
                var ins = fm.Instructions[index];
#if stacktrace
                dfss.AppendLine(stackIndex + " ::  " + ins.ToString());
#endif 

                SpokeObject[] sps;
                
                SpokeObject bm;
                switch (ins.Type)
                {
                    case SpokeInstructionType.CreateReference:
                        stack[stackIndex++] = new SpokeObject(new SpokeObject[ins.Index]);
                        break;
                    case SpokeInstructionType.CreateArray:
                        stack[stackIndex++] = new SpokeObject(new List<SpokeObject>(20));
                        break;
                    case SpokeInstructionType.CreateMethod:
                        stack[stackIndex++] = NULL;//new SpokeObject(ObjectType.Method) { AnonMethod = ins.anonMethod };
                        break;
                    case SpokeInstructionType.Label:
                        //   throw new NotImplementedException("");
                        break;
                    case SpokeInstructionType.Goto:
                       
                        index = ins.Index;

                        break;
                    case SpokeInstructionType.Comment:
                        

                        break;
                    case SpokeInstructionType.CallMethod:
                        sps = new SpokeObject[ins.Index3];
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                        {
                            sps[i] = stack[--stackIndex];
                        }
                        stack[stackIndex++] = evaluateMethod(Methods[ins.Index], sps);
                        break;
                    case SpokeInstructionType.CallMethodFunc:
                        sps = new SpokeObject[ins.Index3];
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                        {
                            sps[i] = stack[--stackIndex];
                        }

                        stack[stackIndex++] = Methods[ins.Index].MethodFunc(sps);
                        break;
                    case SpokeInstructionType.CallInternal:

                        sps = new SpokeObject[ins.Index3];
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                        {
                            sps[i] = stack[--stackIndex];
                        }
                        stack[stackIndex++] = InternalMethods[ins.Index](sps);
                        break;
                    case SpokeInstructionType.BreakpointInstruction:
                        Console.WriteLine("BreakPoint");
                        break;
                    case SpokeInstructionType.Return:
#if stacktrace
                        dfss.AppendLine(fm.Class.Name + " : : " + fm.MethodName + " End");
#endif
                        return stack[--stackIndex];
                        break;
                    case SpokeInstructionType.IfTrueContinueElse:

                        if (stack[--stackIndex].BoolVal)
                            continue;

                        index = ins.Index;

                        break;
                    case SpokeInstructionType.Or:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].BoolVal || stack[stackIndex - 1].BoolVal) ? TRUE : FALSE;
                        stackIndex--;
                        break;
                    case SpokeInstructionType.And:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].BoolVal && stack[stackIndex - 1].BoolVal) ? TRUE : FALSE;
                        stackIndex--;
                        break;
                    case SpokeInstructionType.StoreLocalInt:
                        lastStack = stack[--stackIndex];
                        bm = variables[ins.Index];
                        variables[ins.Index] = new SpokeObject(ObjectType.Int) { IntVal = lastStack.IntVal };
                        break;
                    case SpokeInstructionType.StoreLocalFloat:
                        lastStack = stack[--stackIndex];
                        bm = variables[ins.Index];
                        variables[ins.Index] = new SpokeObject(ObjectType.Float) { FloatVal = lastStack.FloatVal };
                        break;
                    case SpokeInstructionType.StoreLocalBool:
                        lastStack = stack[--stackIndex];
                        bm = variables[ins.Index];
                        variables[ins.Index] = lastStack.BoolVal ? TRUE : FALSE;
                        break;
                    case SpokeInstructionType.StoreLocalString:
                        lastStack = stack[--stackIndex];
                        bm = variables[ins.Index];
                        variables[ins.Index] = new SpokeObject(ObjectType.String) { StringVal = lastStack.StringVal };
                        break;

                    case SpokeInstructionType.StoreLocalMethod:
                    case SpokeInstructionType.StoreLocalObject:
                        lastStack = stack[--stackIndex];
                        bm = variables[ins.Index];
                        variables[ins.Index] = lastStack;
                        break;
                    case SpokeInstructionType.StoreLocalRef:
                        lastStack = stack[--stackIndex];
                        bm = variables[ins.Index];
                        bm.Variables = lastStack.Variables;
                        bm.ArrayItems = lastStack.ArrayItems;
                        bm.StringVal = lastStack.StringVal;
                        bm.IntVal = lastStack.IntVal;
                        bm.BoolVal = lastStack.BoolVal;
                        bm.FloatVal = lastStack.FloatVal;
                        break;



                    case SpokeInstructionType.StoreFieldBool:
                        lastStack = stack[--stackIndex];
                        lastStack.Variables[ins.Index] = stack[--stackIndex].BoolVal ? TRUE : FALSE;

                        break;
                    case SpokeInstructionType.StoreFieldInt:
                        lastStack = stack[--stackIndex];
                        lastStack.Variables[ins.Index] = new SpokeObject(ObjectType.Int) { IntVal = stack[--stackIndex].IntVal };

                        break;
                    case SpokeInstructionType.StoreFieldFloat:
                        lastStack = stack[--stackIndex];
                        lastStack.Variables[ins.Index] = new SpokeObject(ObjectType.Float) { FloatVal = stack[--stackIndex].FloatVal };

                        break;
                    case SpokeInstructionType.StoreFieldString:
                        lastStack = stack[--stackIndex];
                        lastStack.Variables[ins.Index] = new SpokeObject(ObjectType.String) { StringVal = stack[--stackIndex].StringVal };
                        break;

                    case SpokeInstructionType.StoreFieldMethod:
                    case SpokeInstructionType.StoreFieldObject:
                        lastStack = stack[--stackIndex];
                        lastStack.Variables[ins.Index] = stack[--stackIndex];

                        break;


                    case SpokeInstructionType.StoreToReference:

                        lastStack = stack[--stackIndex];
                        stack[stackIndex - 1].Variables[ins.Index] = lastStack;
                        break;
                    case SpokeInstructionType.GetField:

                        stack[stackIndex - 1] = stack[stackIndex - 1].Variables[ins.Index];

                        break;
                    case SpokeInstructionType.GetLocal:

                        stack[stackIndex++] = variables[ins.Index];
                        break;
                    case SpokeInstructionType.PopStack:
                        stackIndex--;
                        break;
                    case SpokeInstructionType.Not:
                        stack[stackIndex-1] = stack[stackIndex-1].BoolVal ? FALSE : TRUE;
                        break;
                    case SpokeInstructionType.AddStringInt:
                        stack[stackIndex - 2] = new SpokeObject(ObjectType.String) { StringVal = stack[stackIndex - 2].StringVal + stack[stackIndex - 1].IntVal };
                        stackIndex--;

                        break;
                    case SpokeInstructionType.AddIntString:

                        stack[stackIndex - 2] = new SpokeObject(ObjectType.String) { StringVal = stack[stackIndex - 2].IntVal + stack[stackIndex - 1].StringVal };
                        stackIndex--;
                        break;
                    case SpokeInstructionType.IntConstant:
                        stack[stackIndex++] = intCache(ins.Index);
                        break;
                    case SpokeInstructionType.BoolConstant:

                        stack[stackIndex++] = ins.BoolVal ? TRUE : FALSE;
                        break;
                    case SpokeInstructionType.FloatConstant:
                        stack[stackIndex++] = new SpokeObject(ObjectType.Float) { FloatVal = ins.FloatVal };
                        break;
                    case SpokeInstructionType.StringConstant:

                        stack[stackIndex++] = new SpokeObject(ObjectType.String) { StringVal = ins.StringVal };
                        break;

                    case SpokeInstructionType.Null:
                        stack[stackIndex++] = NULL;
                        break;
                    case SpokeInstructionType.AddIntInt:
                        stack[stackIndex - 2] = intCache(stack[stackIndex - 2].IntVal + stack[stackIndex - 1].IntVal);
                        stackIndex--;
                        break;
                    case SpokeInstructionType.AddIntFloat:
                        break;
                    case SpokeInstructionType.AddFloatInt:
                        break;
                    case SpokeInstructionType.AddFloatFloat:
                        break;
                    case SpokeInstructionType.AddFloatString:
                        stack[stackIndex - 2] = new SpokeObject(ObjectType.String) { StringVal = stack[stackIndex - 2].FloatVal + stack[stackIndex - 1].StringVal };
                        stackIndex--;
                        break;
                    case SpokeInstructionType.AddStringFloat:
                        stack[stackIndex - 2] = new SpokeObject(ObjectType.String) { StringVal = stack[stackIndex - 2].StringVal + stack[stackIndex - 1].FloatVal };
                        stackIndex--;
                        break;
                    case SpokeInstructionType.AddStringString:
                        stack[stackIndex - 2] = new SpokeObject(ObjectType.String) { StringVal = stack[stackIndex - 2].StringVal + stack[stackIndex - 1].StringVal };
                        stackIndex--;
                        break;
                    case SpokeInstructionType.SubtractIntInt:
                        stack[stackIndex - 2] = intCache(stack[stackIndex - 2].IntVal - stack[stackIndex - 1].IntVal);
                        stackIndex--;

                        break;
                    case SpokeInstructionType.SubtractIntFloat:
                        break;
                    case SpokeInstructionType.SubtractFloatInt:
                        break;
                    case SpokeInstructionType.SubtractFloatFloat:
                        break;
                    case SpokeInstructionType.MultiplyIntInt:

                        stack[stackIndex - 2] = intCache(stack[stackIndex - 2].IntVal * stack[stackIndex - 1].IntVal);
                        stackIndex--;
                        break;
                    case SpokeInstructionType.MultiplyIntFloat:
                        break;
                    case SpokeInstructionType.MultiplyFloatInt:
                        break;
                    case SpokeInstructionType.MultiplyFloatFloat:
                        break;
                    case SpokeInstructionType.DivideIntInt:

                        stack[stackIndex - 2] = intCache(stack[stackIndex - 2].IntVal / stack[stackIndex - 1].IntVal);
                        stackIndex--;
                        break;
                    case SpokeInstructionType.DivideIntFloat:
                        break;
                    case SpokeInstructionType.DivideFloatInt:
                        break;
                    case SpokeInstructionType.DivideFloatFloat:
                        break;

                    case SpokeInstructionType.GreaterIntInt:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].IntVal > stack[stackIndex - 1].IntVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.GreaterIntFloat:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].IntVal > stack[stackIndex - 1].FloatVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.GreaterFloatInt:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].FloatVal > stack[stackIndex - 1].IntVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.GreaterFloatFloat:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].FloatVal > stack[stackIndex - 1].FloatVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.LessIntInt:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].IntVal < stack[stackIndex - 1].IntVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.LessIntFloat:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].IntVal < stack[stackIndex - 1].FloatVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.LessFloatInt:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].FloatVal < stack[stackIndex - 1].IntVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.LessFloatFloat:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].FloatVal < stack[stackIndex - 1].FloatVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.GreaterEqualIntInt:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].IntVal >= stack[stackIndex - 1].IntVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.GreaterEqualIntFloat:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].IntVal >= stack[stackIndex - 1].FloatVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.GreaterEqualFloatInt:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].FloatVal >= stack[stackIndex - 1].IntVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.GreaterEqualFloatFloat:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].FloatVal >= stack[stackIndex - 1].FloatVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.LessEqualIntInt:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].IntVal <= stack[stackIndex - 1].IntVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.LessEqualIntFloat:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].IntVal <= stack[stackIndex - 1].FloatVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.LessEqualFloatInt:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].FloatVal <= stack[stackIndex - 1].IntVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.LessEqualFloatFloat:
                        stack[stackIndex - 2] = (stack[stackIndex - 2].FloatVal <= stack[stackIndex - 1].FloatVal) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.Equal:
                        stack[stackIndex - 2] = SpokeObject.Compare(stack[stackIndex - 2], stack[stackIndex - 1]) ? TRUE : FALSE;
                        stackIndex = stackIndex - 1;
                        break;
                    case SpokeInstructionType.InsertToArray:
                        break;
                    case SpokeInstructionType.RemoveToArray:
                        break;
                    case SpokeInstructionType.AddToArray:
                        lastStack = stack[--stackIndex];
                        stack[stackIndex - 1].AddArray(lastStack);
                        break;
                    case SpokeInstructionType.AddRangeToArray:
                        lastStack = stack[--stackIndex];
                        stack[stackIndex - 1].AddRangeArray(lastStack);
                        break;
                    case SpokeInstructionType.LengthOfArray:
                        break;

                    case SpokeInstructionType.ArrayElem:

                        lastStack = stack[--stackIndex];
                        stack[stackIndex - 1] = stack[stackIndex - 1].ArrayItems[lastStack.IntVal];

                        break;
                    case SpokeInstructionType.StoreArrayElem:

                        var indexs = stack[--stackIndex];
                        var ars = stack[--stackIndex];

                        ars.ArrayItems[indexs.IntVal] = stack[--stackIndex];


                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

#if stacktrace
            dfss.AppendLine(fm.Class.Name + " : : " + fm.MethodName + " End");
#endif

            return null;


        }
    }
}
