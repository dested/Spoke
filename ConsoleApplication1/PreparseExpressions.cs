using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApplication1
{
    public class SpokeVariableInfo
    {
        public Dictionary<string, SpokeType> Variables;

        public SpokeVariableInfo(SpokeVariableInfo variables)
        {
            if (variables == null)
            {
                allVariables = new List<Tuple<string, int, SpokeType>>();
                Variables = new Dictionary<string, SpokeType>();
                return;
            }

            allVariables = variables.allVariables;
            index = variables.index;
            indeces = variables.indeces;
            Variables = new Dictionary<string, SpokeType>(variables.Variables);
        }
        public SpokeVariableInfo()
        {
            allVariables = new List<Tuple<string, int, SpokeType>>();
            Variables = new Dictionary<string, SpokeType>();
        }

        private Dictionary<string, int> indeces = new Dictionary<string, int>();
        public int index;


        public List<Tuple<string, int, SpokeType>> allVariables;

        public int Add(string s, SpokeType spokeType, SpokeVariable sv)
        {
            allVariables.Add(new Tuple<string, int, SpokeType>(s, index, spokeType));
            Variables.Add(s, spokeType);
            indeces.Add(s, index++);
            if (sv != null)
            {
                sv.VariableIndex = indeces[s];
            }

            return index - 1;
        }
        public void Remove(string s)
        {
            Variables.Remove(s);
            indeces.Remove(s);
        }
        public bool TryGetValue(string s, out SpokeType spokeType, SpokeVariable sv)
        {
            var def = Variables.TryGetValue(s, out spokeType);

            if (def && sv != null)
                sv.VariableIndex = indeces[s];

            return def;
        }

        public SpokeType Get(string variableName, SpokeVariable mv)
        {
            var def = Variables[variableName];
            if (def != null && mv != null)
                mv.VariableIndex = indeces[variableName];

            return def;
        }

        public SpokeType this[string s, SpokeVariable mv]
        {
            get { return Get(s, mv); }
        }

        public void IncreaseState()
        {
        }

        public void DecreaseState()
        {

        }

        public void Reset(string variableName, SpokeType spokeType, SpokeVariable sv)
        {
            if (Variables.ContainsKey(variableName))
            {
                Variables[variableName] = spokeType;
                if (sv != null)
                {
                    sv.VariableIndex = indeces[variableName];
                }
            }
        }
    }

    public class PreparseExpressions
    {
        private List<SpokeClass> _cla;
        private Dictionary<string, SpokeMethod> Methods = new Dictionary<string, SpokeMethod>();
        private Dictionary<string, SpokeType> InternalMethodsTypes;

        public PreparseExpressions(List<SpokeClass> cla, Dictionary<string, SpokeType> internalMethods, Dictionary<string, SpokeMethod> mets)
        {
            Methods = mets;
            _cla = cla;
            InternalMethodsTypes = internalMethods;
        }

        public SpokeConstruct Run(string main, string ctor)
        {

            var fm = Methods.First(a => a.Value.MethodName == ctor && a.Value.Class.Name == main);





            SpokeType st = new SpokeType(ObjectType.Object);

            st.ClassName = main;

            var drj = _cla.First(a => a.Name == main);

            st.Variables = new SpokeVariableInfo();


            foreach (var v in drj.Variables)
            {
                st.Variables.Add(v, new SpokeType(ObjectType.Unset), null);
            }


            evaluateMethod(fm.Value, st, new SpokeType[1] { st });
            return new SpokeConstruct() { ClassName = main, NumOfVars = st.Variables.index, MethodIndex = Methods.Select(a => a.Value).ToList().IndexOf(fm.Value) };

            // evaluateMethod(fm, dm, new List<SpokeObject>() { dm });
        }

        public class SpokeMethodRun
        {
            public SpokeObject RunningClass;
            public List<SpokeObject> ForYield;

        }

        private Dictionary<string, bool> ame;

        private List<Dictionary<string, bool>> cd = new List<Dictionary<string, bool>>();

        private Dictionary<string, bool> anonMethodsEntered
        {
            get
            {
                if (ame != null)
                {
                    if (cd.Any())
                    {
                        if (cd.Last().Count != ame.Count)
                        {
                            cd.Add(new Dictionary<string, bool>(ame));
                        }
                    }
                    else
                        cd.Add(new Dictionary<string, bool>(ame));
                }


                return ame;
            }
            set { ame = value; }
        }
        public class SpokeMethodParse
        {
            public SpokeType RunningClass;
            public SpokeType ForYieldArray;
            private SpokeType vr;
            public SpokeType ReturnType
            {
                get { return vr; }
                set
                {
                    vr = value;
                    if (Method != null)
                    {
                        Method.returnType = vr;

                    }
                }
            }
            public SpokeMethod Method;
        }


        private SpokeType evaluateMethod(SpokeMethod fm, SpokeType parent, SpokeType[] paras)
        {


            if (fm.Evaluated)
            {
                parent.Variables = fm.VariableRefs;
                return fm.returnType;
            }
            var db = anonMethodsEntered;
            anonMethodsEntered = new Dictionary<string, bool>();
            fm.Evaluated = true;
            SpokeVariableInfo variables = new SpokeVariableInfo();
            for (int i = 0; i < fm.Parameters.Length; i++)
            {
                variables.Add(fm.Parameters[i], paras[i], null);
            }

            var sm = new SpokeMethodParse();
            sm.RunningClass = parent;
            sm.Method = fm;
            if (fm.HasYield)
            {
                sm.ForYieldArray = new SpokeType(ObjectType.Unset) { };
                sm.ReturnType = new SpokeType(ObjectType.Array) { Type = ObjectType.Array, ArrayItemType = sm.ForYieldArray };
            }
            else
            {
                sm.ReturnType = new SpokeType(ObjectType.Unset);
            }

            fm.VariableRefs = parent.Variables;

            var toCotninue = SpokeInstruction.ins;


            SpokeInstruction.Beginner();
            var de = evaluateLines(ref fm.Lines, sm, variables);
            fm.Instructions = SpokeInstruction.Ender();
            SpokeInstruction.ins = toCotninue;

            fm.NumOfVars = variables.index;

            anonMethodsEntered = db;
            return de;
        }


        private SpokeType evaluateLines(ref SpokeLine[] lines, SpokeMethodParse currentObject, SpokeVariableInfo variables)
        {
            List<SpokeLine> ln = new List<SpokeLine>(lines);
            int jumpIndex = 0;

            for (int index = 0; index < lines.Length; index++)
            {
                var spokeLine = lines[index];


                SpokeType df;
                List<SpokeInstruction> ddsf;
                switch (spokeLine.LType)
                {
                    case ISpokeLine.If:
                        var b = evaluateItem(((SpokeIf)spokeLine).Condition, currentObject, variables);

                        if (b.Type != ObjectType.Bool)
                        {
                            throw new AbandonedMutexException("Expected bool");
                        }

                        new SpokeInstruction(SpokeInstructionType.IfTrueContinueElse, ((SpokeIf)spokeLine).ElseLines == null ? "EndIf" + ((SpokeIf)spokeLine).Guid : "ElseIf" + ((SpokeIf)spokeLine).Guid);

                        variables.IncreaseState();
                        df = evaluateLines(ref ((SpokeIf)spokeLine).IfLines, currentObject, variables);
                        variables.DecreaseState();
                        if (currentObject.ReturnType != null && !currentObject.ReturnType.CompareTo(df, true))
                        {
                            throw new AbandonedMutexException("for return:    Expected " + currentObject.ReturnType.Type +
                                                              " Got" + df.Type);
                        }

                        if (((SpokeIf)spokeLine).ElseLines != null)
                        {
                            new SpokeInstruction(SpokeInstructionType.Goto, "EndIf" + ((SpokeIf)spokeLine).Guid);
                            new SpokeInstruction(SpokeInstructionType.Label, "ElseIf" + ((SpokeIf)spokeLine).Guid);

                            df = evaluateLines(ref ((SpokeIf)spokeLine).ElseLines, currentObject,
                                               variables);
                            {
                                if (currentObject.ReturnType != null && !currentObject.ReturnType.CompareTo(df, true))
                                {
                                    throw new AbandonedMutexException("for return:    Expected " + currentObject.ReturnType.Type + " Got" + df.Type);
                                }
                            }
                        }
                        new SpokeInstruction(SpokeInstructionType.Label, "EndIf" + ((SpokeIf)spokeLine).Guid);

                        break;
                    case ISpokeLine.Return:
                        int va;
                        if (((SpokeReturn)spokeLine).Return == null)
                        {
                            currentObject.ReturnType = new SpokeType(ObjectType.Null);
                            new SpokeInstruction(SpokeInstructionType.Null);

                            if (!anonMethodsEntered.Any() || anonMethodsEntered.All(a => !a.Value))
                                new SpokeInstruction(SpokeInstructionType.Return);
                            else
                            {
                                new SpokeInstruction(SpokeInstructionType.StoreLocalObject, va = variables.Add("__tmpReturn" + ((SpokeReturn)spokeLine).Guid, currentObject.ReturnType, null));
                                new SpokeInstruction(SpokeInstructionType.GetLocal, va);
                                new SpokeInstruction(SpokeInstructionType.Goto, anonMethodsEntered.Last(a => a.Value).Key);
                            }
                        }
                        else
                        {
                            df = evaluateItem(((SpokeReturn)spokeLine).Return, currentObject, variables);

                            if (currentObject.ReturnType.CompareTo(df, true))
                            {
                                if (currentObject.ReturnType.Type == ObjectType.Unset)
                                {
                                    currentObject.ReturnType = df;
                                }
                            }
                            else
                            {
                                throw new AbandonedMutexException("for return:    Expected " +
                                                                  currentObject.ReturnType.Type + " Got" + df.Type);
                            }

                            if (!anonMethodsEntered.Any() || anonMethodsEntered.All(a => !a.Value))
                                new SpokeInstruction(SpokeInstructionType.Return);
                            else
                            {
                                setInstru(currentObject.ReturnType.Type, va = variables.Add("__tmpReturn" + ((SpokeReturn)spokeLine).Guid, currentObject.ReturnType, null));

                                new SpokeInstruction(SpokeInstructionType.GetLocal, va);

                                new SpokeInstruction(SpokeInstructionType.Goto, anonMethodsEntered.Last(a => a.Value).Key);
                            }

                        }




                        break;
                    case ISpokeLine.Yield:
                        ddsf = new List<SpokeInstruction>(SpokeInstruction.ins);
                        df = evaluateItem(((SpokeYield)spokeLine).Yield, currentObject, variables);
                        SpokeInstruction.ins = ddsf;
                        if (currentObject.ForYieldArray.CompareTo(df, true))
                        {
                            if (currentObject.ForYieldArray.Type == ObjectType.Unset)
                            {
                                currentObject.ForYieldArray = df;
                            }
                        }
                        else
                        {
                            throw new AbandonedMutexException("for yield:    Expected " +
                                                              currentObject.ForYieldArray.Type + " Got" + df.Type);
                        }

                        break;
                    case ISpokeLine.YieldReturn:
                        ddsf = new List<SpokeInstruction>(SpokeInstruction.ins);
                        df = evaluateItem(((SpokeYieldReturn)spokeLine).YieldReturn, currentObject, variables);
                        SpokeInstruction.ins = ddsf;
                        if (currentObject.ForYieldArray.CompareTo(df, true))
                        {
                            if (currentObject.ForYieldArray.Type == ObjectType.Unset)
                            {
                                currentObject.ForYieldArray = df;
                            }
                        }
                        else
                        {
                            throw new AbandonedMutexException("for yield:    Expected " +
                                                              currentObject.ForYieldArray.Type + " Got" + df.Type);
                        }

                        break;
                    case ISpokeLine.MethodCall:
                        var def = evaluateItem((SpokeItem)spokeLine, currentObject, variables, true);
                        if (def == null)
                        {
                            //        Console.WriteLine("A");
                        }
                        else
                            //if (def.Type == ObjectType.Void)
                            //{
                            //     new SpokeInstruction(SpokeInstructionType.PopStack);
                            //}
                            new SpokeInstruction(SpokeInstructionType.PopStack);


                        //no care about the typiola
                        break;
                    case ISpokeLine.AnonMethod:
                        var arm = evaluateItem((SpokeItem)spokeLine, currentObject, variables, true);



                        if (((SpokeAnonMethod)spokeLine).HasReturn)
                        {


                            //    ln[index] = new SpokeReturn() { Return = (SpokeAnonMethod)spokeLine };


                            //                            if (!anonMethodsEntered.Any())
                            //                                new SpokeInstruction(SpokeInstructionType.Return);
                            //                            else
                            //                            {
                            //                                new SpokeInstruction(SpokeInstructionType.Goto, anonMethodsEntered.Last().Key);
                            //                            }




                            if (arm == null)
                            {
                                throw new AbandonedMutexException("AIDS");
                            }
                            if (currentObject.ReturnType.CompareTo(arm, true))
                            {
                                if (currentObject.ReturnType.Type == ObjectType.Unset)
                                {
                                    currentObject.ReturnType = arm;
                                }
                            }
                            else
                            {
                                throw new AbandonedMutexException("for anonMethod:    Expected " +
                                                                  currentObject.ReturnType.Type + " Got" + arm.Type);
                            }
                        }
                        else if (((SpokeAnonMethod)spokeLine).HasYield || ((SpokeAnonMethod)spokeLine).HasYieldReturn)
                        {
                            //   ln[index] = new SpokeReturn() { Return = (SpokeAnonMethod)spokeLine };



                            //                          if (!anonMethodsEntered.Any())
                            //                              new SpokeInstruction(SpokeInstructionType.Return);
                            //                          else
                            //                          {
                            //                              new SpokeInstruction(SpokeInstructionType.Goto, anonMethodsEntered.Last().Key);
                            //                          }


                            ln.Insert(index + 1, new SpokeReturn() { Return = ((SpokeAnonMethod)spokeLine) });
                            ln.Remove(spokeLine);
                            jumpIndex++;



                        }
                        //     else
                        //        new SpokeInstruction(SpokeInstructionType.PopStack);


                        //if ((((SpokeAnonMethod)spokeLine).Lines[0] is SpokeEqual && ((SpokeEqual)((SpokeAnonMethod)spokeLine).Lines[0]).RightSide.IType==ISpokeItem.Array))
                        //{

                        //}

                        //ln.Insert(index + jumpIndex, ((SpokeAnonMethod)spokeLine).Lines[0]);

                        //jumpIndex++;

                        //var dfe = new List<SpokeLine>(((SpokeAnonMethod)spokeLine).Lines);
                        //dfe.RemoveAt(0);
                        //((SpokeAnonMethod)spokeLine).Lines = dfe.ToArray();

                        if (arm == null)
                        {
                            continue;
                        }
                        //var barm = new SpokeType(ObjectType.Array) { ArrayItemType = arm, ClassName = "Array" };

                        if (currentObject.ReturnType.CompareTo(arm, true))
                        {
                            if (currentObject.ReturnType.Type == ObjectType.Unset)
                            {
                                currentObject.ReturnType = arm;
                            }
                        }
                        else
                        {
                            throw new AbandonedMutexException("for anonMethod:    Expected " +
                                                              currentObject.ForYieldArray.Type + " Got" +
                                                              arm.Type);
                        }





                        break;
                    case ISpokeLine.Construct:
                        var d = evaluateItem((SpokeItem)spokeLine, currentObject, variables);



                        return d;
                        break;
                    case ISpokeLine.Set:

                        var grf = ((SpokeEqual)spokeLine);

                        var right = evaluateItem(grf.RightSide, currentObject, variables);

                        forSet = true;
                        SpokeType left;
                        switch (right.Type)
                        {
                            case ObjectType.Null:
                                throw new AbandonedMutexException("Set bad");
                                break;
                            case ObjectType.Int:
                                left = evaluateItem(grf.LeftSide, currentObject, variables);
                                if (!left.CompareTo(right, true))
                                {
                                    //throw new AbandonedMutexException("Set bad");
                                }

                                break;
                            case ObjectType.String:
                                left = evaluateItem(grf.LeftSide, currentObject, variables);
                                if (!left.CompareTo(right, true))
                                    throw new AbandonedMutexException("Set bad");

                                break;
                            case ObjectType.Float:
                                left = evaluateItem(grf.LeftSide, currentObject, variables);
                                if (!left.CompareTo(right, true))
                                {
                                    //throw new AbandonedMutexException("Set bad");
                                }

                                break;
                            case ObjectType.Bool:
                                left = evaluateItem(grf.LeftSide, currentObject, variables);
                                if (!left.CompareTo(right, true))
                                    throw new AbandonedMutexException("Set bad");
                                break;
                            case ObjectType.Array:
                                left = evaluateItem(grf.LeftSide, currentObject, variables);
                                if (!left.CompareTo(right, true))
                                    throw new AbandonedMutexException("Set bad");
                                break;
                            case ObjectType.Object:
                                left = evaluateItem(grf.LeftSide, currentObject, variables);
                                if (!left.CompareTo(right, true))
                                    throw new AbandonedMutexException("Set bad");
                                left.ClassName = right.ClassName;
                                if (right.Variables != null)
                                    left.Variables = new SpokeVariableInfo(right.Variables);

                                break;
                            case ObjectType.Method:
                                left = evaluateItem(grf.LeftSide, currentObject, variables);
                                if (!left.CompareTo(right, true))
                                    if (!(left.Type == ObjectType.Object && left.Variables.index == 0))
                                    {
                                        throw new AbandonedMutexException("Set bad");
                                    }
                                    else
                                    {
                                        left.ClassName = right.ClassName;
                                        left.AnonMethod = right.AnonMethod;
                                        left.Type = right.Type;
                                        left.ArrayItemType = right.ArrayItemType;
                                        if (right.Variables != null)
                                            left.Variables = new SpokeVariableInfo(right.Variables);
                                        left.MethodType = right.MethodType;

                                    }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                                break;
                        }

                        switch (SpokeInstruction.ins.Last().Type)
                        {
                            case SpokeInstructionType.GetLocal:

                                if (left.ByRef)
                                {
                                    SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalRef;
                                }
                                else
                                {
                                    switch (right.Type)
                                    {
                                        case ObjectType.Null:
                                            SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalObject;
                                            break;
                                        case ObjectType.Int:
                                            SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalInt;
                                            break;
                                        case ObjectType.Float:
                                            SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalFloat;
                                            break;
                                        case ObjectType.String:
                                            SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalString;
                                            break;
                                        case ObjectType.Bool:
                                            SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalBool;
                                            break;
                                        case ObjectType.Array:
                                            SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalObject;
                                            break;
                                        case ObjectType.Object:
                                            SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalObject;
                                            break;
                                        case ObjectType.Method:
                                            SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreLocalMethod;
                                            break;
                                        case ObjectType.Void:
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }


                                break;
                            case SpokeInstructionType.GetField:

                                switch (right.Type)
                                {
                                    case ObjectType.Null:
                                        SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreFieldObject;
                                        break;
                                    case ObjectType.Int:
                                        SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreFieldInt;
                                        break;
                                    case ObjectType.Float:
                                        SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreFieldFloat;
                                        break;
                                    case ObjectType.String:
                                        SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreFieldString;
                                        break;
                                    case ObjectType.Bool:
                                        SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreFieldBool;
                                        break;
                                    case ObjectType.Array:
                                        SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreFieldObject;
                                        break;
                                    case ObjectType.Object:
                                        SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreFieldObject;
                                        break;
                                    case ObjectType.Method:
                                        SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreFieldMethod;
                                        break;
                                    case ObjectType.Void:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }



                                break;
                            case SpokeInstructionType.ArrayElem:
                                SpokeInstruction.ins[SpokeInstruction.ins.Count - 1].Type = SpokeInstructionType.StoreArrayElem;
                                break;
                        }


                        if (left.Type == ObjectType.Unset)
                        {
                            left.ClassName = right.ClassName;
                            left.AnonMethod = right.AnonMethod;
                            left.Type = right.Type;
                            left.ArrayItemType = right.ArrayItemType;
                            if (right.Variables != null)
                                left.Variables = new SpokeVariableInfo(right.Variables);
                            left.MethodType = right.MethodType;
                        }

                        else
                        {
                            if (!left.CompareTo(right, false))
                            {
                                //  throw new AbandonedMutexException("hmm");
                            }
                        }
                        forSet = false;

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            lines = ln.ToArray();

            if (currentObject.ReturnType != null)
                return currentObject.ReturnType;

            return null;

        }

        private void setInstru(ObjectType type, int index)
        {
            switch (type)
            {
                case ObjectType.Null:
                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject, index);

                    break;
                case ObjectType.Int:
                    new SpokeInstruction(SpokeInstructionType.StoreLocalInt,
                                         index);

                    break;
                case ObjectType.Float:
                    new SpokeInstruction(SpokeInstructionType.StoreLocalFloat,
                                         index);

                    break;
                case ObjectType.String:
                    new SpokeInstruction(SpokeInstructionType.StoreLocalString,
                                         index);

                    break;
                case ObjectType.Bool:
                    new SpokeInstruction(SpokeInstructionType.StoreLocalBool,
                                         index);

                    break;
                case ObjectType.Array:
                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject,
                                         index);

                    break;
                case ObjectType.Object:
                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject,
                                         index);

                    break;
                case ObjectType.Method:
                    new SpokeInstruction(SpokeInstructionType.StoreLocalMethod,
                                         index);

                    break;
                case ObjectType.Void:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private bool forSet;

        private SpokeType evaluateItem(SpokeItem condition, SpokeMethodParse currentObject, SpokeVariableInfo variables, bool parentIsNull = false)
        {
            SpokeVariable fyv = null;
            SpokeType r;
            SpokeType l;
            switch (condition.IType)
            {

                case ISpokeItem.Current:

                    new SpokeInstruction(SpokeInstructionType.GetLocal, 0);
                    return currentObject.RunningClass;
                    break;
                case ISpokeItem.Null:
                    new SpokeInstruction(SpokeInstructionType.Null);
                    return new SpokeType(ObjectType.Null);
                    break;
                case ISpokeItem.String:
                    new SpokeInstruction(SpokeInstructionType.StringConstant, ((SpokeString)condition).Value);
                    return new SpokeType(ObjectType.String);
                    break;
                case ISpokeItem.Bool:
                    new SpokeInstruction(SpokeInstructionType.BoolConstant, ((SpokeBool)condition).Value);
                    return new SpokeType(ObjectType.Bool);
                    break;
                case ISpokeItem.Float:
                    new SpokeInstruction(SpokeInstructionType.FloatConstant, ((SpokeFloat)condition).Value);
                    return new SpokeType(ObjectType.Float);
                    break;
                case ISpokeItem.Int:

                    new SpokeInstruction(SpokeInstructionType.IntConstant, ((SpokeInt)condition).Value);
                    return new SpokeType(ObjectType.Int);
                    break;












                case ISpokeItem.Array:
                    var ar = new SpokeType(ObjectType.Array);
                    new SpokeInstruction(SpokeInstructionType.CreateArray);


                    foreach (var spokeItem in ((SpokeArray)condition).Parameters)
                    {
                        var grb = evaluateItem(spokeItem, currentObject, variables);
                        if (grb.Type == ObjectType.Array)
                        {
                            if (ar.ArrayItemType.CompareTo(grb.ArrayItemType, true))
                            {
                                if (ar.ArrayItemType.Type == ObjectType.Unset)
                                {
                                    ar.ArrayItemType = grb.ArrayItemType;
                                }
                            }
                            else throw new AbandonedMutexException("bad array");


                            new SpokeInstruction(SpokeInstructionType.AddRangeToArray);

                        }

                        else
                        {
                            if (ar.ArrayItemType.CompareTo(grb, true))
                            {
                                if (ar.ArrayItemType.Type == ObjectType.Unset)
                                {
                                    ar.ArrayItemType = grb;
                                }
                            }
                            else throw new AbandonedMutexException("bad array");

                            new SpokeInstruction(SpokeInstructionType.AddToArray);

                        }
                    }

                    return ar;
                    break;
                case ISpokeItem.Variable:
                    SpokeType g;
                    SpokeVariable mv = ((SpokeVariable)condition);



                    if (mv.Parent != null)
                    {
                        var fs = forSet;
                        forSet = false;

                        var ca = evaluateItem(mv.Parent, currentObject, variables);






                        if (ca.Variables.TryGetValue(mv.VariableName, out g, mv))
                        {
                            if (fs && !g.ByRef)
                            {
                                mv.ForSet = true;
                                ca.Variables.Reset(mv.VariableName, g = new SpokeType(ObjectType.Unset), mv);
                            }


                            new SpokeInstruction(SpokeInstructionType.GetField, mv.VariableIndex) { DEBUG = mv.VariableName };


                            return g;
                        }

                        mv.VariableIndex = ca.Variables.Add(mv.VariableName, g = new SpokeType(ObjectType.Unset), mv);

                        new SpokeInstruction(SpokeInstructionType.GetField, mv.VariableIndex) { DEBUG = mv.VariableName };
                        return g;


                    }

                    if (mv.VariableName == "this")
                    {
                        new SpokeInstruction(SpokeInstructionType.GetLocal, 0) { DEBUG = "this" };

                        return currentObject.RunningClass;
                    }

                    if (currentObject.RunningClass.Variables.TryGetValue(mv.VariableName, out g, mv))
                    {
                        mv.This = true;
                        new SpokeInstruction(SpokeInstructionType.GetLocal, 0) { DEBUG = "this" };

                        if (forSet && g.ByRef == false)
                        {
                            forSet = false;
                            var ddd = currentObject.RunningClass.Variables.Get(mv.VariableName, mv);

                            mv.ForSet = true;
                            currentObject.RunningClass.Variables.Reset(mv.VariableName, g = new SpokeType(ddd), mv);

                        }
                        new SpokeInstruction(SpokeInstructionType.GetField, mv.VariableIndex) { DEBUG = mv.VariableName };
                        return g;
                    }

                    if (variables.TryGetValue(mv.VariableName, out g, mv))
                    {
                        if (forSet && g.ByRef == false)
                        {
                            forSet = false;
                            mv.ForSet = true;
                            variables.Reset(mv.VariableName, g = new SpokeType(ObjectType.Unset), mv);
                        }

                        new SpokeInstruction(SpokeInstructionType.GetLocal, mv.VariableIndex) { DEBUG = mv.VariableName };

                        return g;
                    }

                    if (forSet)
                    {
                        forSet = false;
                        mv.ForSet = true;
                    }


                    ((SpokeVariable)condition).VariableIndex = variables.Add(((SpokeVariable)condition).VariableName,
                                                                              g = new SpokeType(ObjectType.Unset), mv);

                    new SpokeInstruction(SpokeInstructionType.GetLocal, mv.VariableIndex) { DEBUG = mv.VariableName };

                    return g;
                    break;
                case ISpokeItem.ArrayIndex:

                    var fs_ = forSet;
                    forSet = false;
                    var pa = evaluateItem(((SpokeArrayIndex)condition).Parent, currentObject, variables);

                    var ind = evaluateItem(((SpokeArrayIndex)condition).Index, currentObject, variables);

                    if (ind.ArrayItemType != null && ind.ArrayItemType.Type == ObjectType.Int)
                    {

                        /*  var orig = new array();
                          var d = new array();
                          var dexes = new array();
                        
                          int ind = 0;
                          while(true) {
                              if (ind < orig.length)
                              {
                                  d.add(dexes[ind]);
                              }
                              else break;
                          }

                          */


                        int dexes;
                        int orig;
                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, dexes = variables.Add("_dexes" + condition.Guid, new SpokeType(ObjectType.Array) { ClassName = "Array", ArrayItemType = new SpokeType(ObjectType.Int) }, null));
                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, orig = variables.Add("_orig" + condition.Guid, pa, null));

                        int arg;
                        new SpokeInstruction(SpokeInstructionType.CreateArray);
                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, arg = variables.Add("_rev" + condition.Guid, pa, null));


                        int loc;

                        new SpokeInstruction(SpokeInstructionType.IntConstant, 0);
                        new SpokeInstruction(SpokeInstructionType.StoreLocalInt, loc = variables.Add("__ind" + condition.Guid, new SpokeType(ObjectType.Int), null)) { DEBUG = "__ind" + condition.Guid };


                        new SpokeInstruction(SpokeInstructionType.Label, "_topOfForeachback_" + condition.Guid);


                        new SpokeInstruction(SpokeInstructionType.GetLocal, loc);
                        new SpokeInstruction(SpokeInstructionType.GetLocal, dexes);
                        new SpokeInstruction(SpokeInstructionType.CallMethodFunc, Methods.Select(a => a.Value).ToList().IndexOf(Methods.Select(a => a.Value).ToList().First(a => a.Class.Name == "Array" && a.MethodName == "length")), 0, 1) { DEBUG = "length" };
                        new SpokeInstruction(SpokeInstructionType.LessIntInt);
                        var ifTrdue = new SpokeInstruction(SpokeInstructionType.IfTrueContinueElse, "End2Loop" + condition.Guid);




                        new SpokeInstruction(SpokeInstructionType.GetLocal, arg);
                        new SpokeInstruction(SpokeInstructionType.GetLocal, orig);

                        new SpokeInstruction(SpokeInstructionType.GetLocal, dexes);
                        new SpokeInstruction(SpokeInstructionType.GetLocal, loc);
                        new SpokeInstruction(SpokeInstructionType.ArrayElem);

                        new SpokeInstruction(SpokeInstructionType.ArrayElem);
                        new SpokeInstruction(SpokeInstructionType.CallMethodFunc, Methods.Select(a => a.Value).ToList().IndexOf(Methods.Select(a => a.Value).ToList().First(a => a.Class.Name == "Array" && a.MethodName == "add")), 0, 2) { DEBUG = "add" };

                        new SpokeInstruction(SpokeInstructionType.PopStack);


                        new SpokeInstruction(SpokeInstructionType.GetLocal, loc) { DEBUG = "__ind" + condition.Guid };
                        new SpokeInstruction(SpokeInstructionType.IntConstant, 1);
                        new SpokeInstruction(SpokeInstructionType.AddIntInt);
                        new SpokeInstruction(SpokeInstructionType.StoreLocalInt, loc) { DEBUG = "__ind" + condition.Guid };

                        new SpokeInstruction(SpokeInstructionType.Goto, "_topOfForeachback_" + condition.Guid);


                        new SpokeInstruction(SpokeInstructionType.Label, "End2Loop" + condition.Guid);

                        new SpokeInstruction(SpokeInstructionType.GetLocal, arg);




                        //  throw new AbandonedMutexException("ADSd");
                        if (fs_ && pa.ByRef == false)
                        {
                            throw new AbandonedMutexException("wat");
                        }

                        return pa;

                    }
                    else if (ind.Type != ObjectType.Int)
                    {
                        throw new AbandonedMutexException();

                    }

                    //new SpokeInstruction(SpokeInstructionType.FloatConstant, ((SpokeFloat)condition).Value);
                    new SpokeInstruction(SpokeInstructionType.ArrayElem);

                    if (fs_ && pa.ByRef == false)
                    {
                        ((SpokeArrayIndex)condition).ForSet = true;
                        pa.ArrayItemType = new SpokeType(pa.ArrayItemType);

                        return pa.ArrayItemType;
                    }
                    if (pa.ByRef == true)
                    {
                        throw new AbandonedMutexException();
                    }
                    return pa.ArrayItemType;
                    break;
                case ISpokeItem.AnonMethod:


                    if (((SpokeAnonMethod)condition).Parent != null)
                    {
                        SpokeMethodParse df = null;





                        if (((SpokeAnonMethod)condition).HasYield || ((SpokeAnonMethod)condition).HasYieldReturn)
                        {
                            var drf = new List<SpokeLine>(((SpokeAnonMethod)condition).Lines);
                            //    drf.Insert(0, new SpokeEqual() { LeftSide = fyv = new SpokeVariable() { VariableName = "__" + condition.Guid }, RightSide = new SpokeArray() { Parameters = new SpokeItem[0] } });

                            fyv = new SpokeVariable() { VariableName = "__" + condition.Guid };
                            variables.Add("__" + condition.Guid, new SpokeType(ObjectType.Array) { ClassName = "Array" },
                                          fyv);
                            //  fyv.ForSet = true;
                            ((SpokeAnonMethod)condition).ReturnYield = fyv;

                            new SpokeInstruction(SpokeInstructionType.CreateArray);
                            new SpokeInstruction(SpokeInstructionType.StoreLocalObject, fyv.VariableIndex);



                            fuix(drf, fyv, false);


                            ((SpokeAnonMethod)condition).Lines = drf.ToArray();

                        }

                        new SpokeInstruction(SpokeInstructionType.Label, "_topOfWhile_" + condition.Guid);


                        if (((SpokeAnonMethod)condition).RunOnVar == null)
                        {
                            df = new SpokeMethodParse() { RunningClass = currentObject.RunningClass };
                            if (((SpokeAnonMethod)condition).HasYield || ((SpokeAnonMethod)condition).HasYieldReturn)
                            {
                                df.ForYieldArray = new SpokeType(ObjectType.Unset);
                            }
                        }
                        if (((SpokeAnonMethod)condition).HasReturn)
                        {
                            df.ReturnType = new SpokeType(ObjectType.Unset);
                        }

                        var rl = evaluateItem(((SpokeAnonMethod)condition).Parent, currentObject, variables);


                        if (rl.Type == ObjectType.Array)
                        {

                            if (((SpokeAnonMethod)condition).RunOnVar != null)
                            {

                                var dmei = SpokeInstruction.ins;



                                df = new SpokeMethodParse()
                                {
                                    RunningClass =
                                        evaluateItem(((SpokeAnonMethod)condition).RunOnVar,
                                                     currentObject, variables)
                                };
                                if (df.RunningClass.AnonMethod.HasYield || df.RunningClass.AnonMethod.HasYieldReturn)
                                {
                                    df.ForYieldArray = new SpokeType(ObjectType.Unset);
                                }
                                SpokeInstruction.ins = dmei;



                                if (((SpokeAnonMethod)condition).HasReturn)
                                {
                                    df.ReturnType = new SpokeType(ObjectType.Unset);
                                }



                                for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                {
                                    df.RunningClass.AnonMethod.Parameters[i].Index =
                                        variables.Add(df.RunningClass.AnonMethod.Parameters[i].Name, rl.ArrayItemType,
                                                      null);

                                    if (df.RunningClass.AnonMethod.Parameters[i].ByRef)
                                    {
                                        throw new Exception("idk");
                                        //   spokeObject.ByRef = true;
                                    }
                                }
                                anonMethodsEntered.Add("_anons1_" + condition.Guid, false);

                                foreach (var spokeLine in df.RunningClass.AnonMethod.Lines)
                                {
                                    resetGuids(spokeLine);
                                }


                                var rme = evaluateLines(ref df.RunningClass.AnonMethod.Lines, df, variables);

                                new SpokeInstruction(SpokeInstructionType.Label, anonMethodsEntered.Last().Key);
                                if (anonMethodsEntered.Last().Key != "_anons1_" + condition.Guid)
                                {
                                    throw new AbandonedMutexException("");

                                }
                                anonMethodsEntered.Remove(anonMethodsEntered.Last().Key);



                                if (rme != null)
                                    if (df.RunningClass.AnonMethod.HasReturn)
                                    {
                                        for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                        {
                                            variables.Remove(df.RunningClass.AnonMethod.Parameters[i].Name);
                                        }

                                        return rme;
                                    }


                                if (df.RunningClass.AnonMethod.HasYield || df.RunningClass.AnonMethod.HasYieldReturn)
                                {

                                }

                                for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                {
                                    var c = df.RunningClass.AnonMethod.Parameters[i].Name;
                                    variables[c, null].ByRef = false;
                                    variables.Remove(c);
                                }

                                return new SpokeType(ObjectType.Array) { ClassName = "Array", ArrayItemType = df.ForYieldArray };


                            }
                            else
                            {
                                int loc;
                                int arr;

                                new SpokeInstruction(SpokeInstructionType.StoreLocalObject, arr = variables.Add("__arr" + condition.Guid, rl, null)) { DEBUG = "__arr" + condition.Guid };
                                new SpokeInstruction(SpokeInstructionType.IntConstant, 0);
                                new SpokeInstruction(SpokeInstructionType.StoreLocalInt, loc = variables.Add("__ind" + condition.Guid, new SpokeType(ObjectType.Int), null)) { DEBUG = "__ind" + condition.Guid };


                                new SpokeInstruction(SpokeInstructionType.Label, "_topOfForeach_" + condition.Guid);


                                new SpokeInstruction(SpokeInstructionType.GetLocal, loc) { DEBUG = "__ind" + condition.Guid };
                                new SpokeInstruction(SpokeInstructionType.GetLocal, arr) { DEBUG = "__arr" + condition.Guid };
                                new SpokeInstruction(SpokeInstructionType.CallMethodFunc, Methods.Select(a => a.Value).ToList().IndexOf(Methods.Select(a => a.Value).ToList().First(a => a.Class.Name == "Array" && a.MethodName == "length")), 0, 1) { DEBUG = "length" };
                                new SpokeInstruction(SpokeInstructionType.LessIntInt);
                                var ifTrdue = new SpokeInstruction(SpokeInstructionType.IfTrueContinueElse, "End3Loop" + condition.Guid);




                                if (((SpokeAnonMethod)condition).Parameters != null)
                                {


                                    ((SpokeAnonMethod)condition).Parameters[0].Index = variables.Add(((SpokeAnonMethod)condition).Parameters[0].Name, rl.ArrayItemType, null);

                                    new SpokeInstruction(SpokeInstructionType.GetLocal, arr) { DEBUG = "__arr" + condition.Guid };
                                    new SpokeInstruction(SpokeInstructionType.GetLocal, loc) { DEBUG = "__ind" + condition.Guid };
                                    new SpokeInstruction(SpokeInstructionType.ArrayElem);
                                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject, ((SpokeAnonMethod)condition).Parameters[0].Index) { DEBUG = ((SpokeAnonMethod)condition).Parameters[0].Name };



                                    if (((SpokeAnonMethod)condition).Parameters.Length == 2)
                                    {


                                        ((SpokeAnonMethod)condition).Parameters[1].Index = variables.Add(((SpokeAnonMethod)condition).Parameters[1].Name, new SpokeType(ObjectType.Int), null);

                                        new SpokeInstruction(SpokeInstructionType.GetLocal, loc) { DEBUG = "__ind" + condition.Guid };
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalInt, ((SpokeAnonMethod)condition).Parameters[1].Index) { DEBUG = ((SpokeAnonMethod)condition).Parameters[1].Name };



                                    }

                                }

                                anonMethodsEntered.Add("_anons2_" + condition.Guid, false);

                                var rme = evaluateLines(ref ((SpokeAnonMethod)condition).@lines, df, variables);

                                new SpokeInstruction(SpokeInstructionType.Label, anonMethodsEntered.Last().Key);
                                if (anonMethodsEntered.Last().Key != "_anons2_" + condition.Guid)
                                {

                                }
                                anonMethodsEntered.Remove(anonMethodsEntered.Last().Key);


                                new SpokeInstruction(SpokeInstructionType.GetLocal, loc) { DEBUG = "__ind" + condition.Guid };
                                new SpokeInstruction(SpokeInstructionType.IntConstant, 1);
                                new SpokeInstruction(SpokeInstructionType.AddIntInt);
                                new SpokeInstruction(SpokeInstructionType.StoreLocalInt, loc) { DEBUG = "__ind" + condition.Guid };

                                new SpokeInstruction(SpokeInstructionType.Goto, "_topOfForeach_" + condition.Guid);


                                new SpokeInstruction(SpokeInstructionType.Label, "End3Loop" + condition.Guid);


                                //yield return
                                if (rme != null)
                                    if (((SpokeAnonMethod)condition).HasReturn)
                                    {
                                        if (((SpokeAnonMethod)condition).Parameters != null)
                                        {

                                            variables.Remove(((SpokeAnonMethod)condition).Parameters[0].Name);
                                            if (((SpokeAnonMethod)condition).Parameters.Length == 2)
                                                variables.Remove(((SpokeAnonMethod)condition).Parameters[1].Name);


                                        }
                                        if (df.ReturnType.CompareTo(rme, false))
                                        {
                                            if (rme.Type == ObjectType.Unset)
                                            {
                                                df.ReturnType = rme;
                                            }
                                        }
                                        else
                                        {
                                            df.ForYieldArray = rme;
                                        }

                                    }

                                if (((SpokeAnonMethod)condition).Parameters != null)
                                {

                                    variables.Remove(((SpokeAnonMethod)condition).Parameters[0].Name);
                                    if (((SpokeAnonMethod)condition).Parameters.Length == 2)
                                        variables.Remove(((SpokeAnonMethod)condition).Parameters[1].Name);


                                }


                                if (((SpokeAnonMethod)condition).HasYield ||
                                    ((SpokeAnonMethod)condition).HasYieldReturn)
                                {

                                    var drf = new List<SpokeLine>(((SpokeAnonMethod)condition).Lines);

                                    fuix(drf, fyv, true);

                                    ((SpokeAnonMethod)condition).Lines = drf.ToArray();

                                    new SpokeInstruction(SpokeInstructionType.GetLocal, fyv.VariableIndex) { DEBUG = fyv.VariableName };

                                    return new SpokeType(ObjectType.Array) { ClassName = "Array", ArrayItemType = df.ForYieldArray };

                                }
                                else if (((SpokeAnonMethod)condition).HasReturn)
                                {

                                    return df.ReturnType;
                                }

                            }
                        }
                        else if (rl.Type == ObjectType.Bool)
                        {



                            var ifTrdue = new SpokeInstruction(SpokeInstructionType.IfTrueContinueElse, "End1Loop" + condition.Guid) { DEBUG = "while loop" };

                            anonMethodsEntered.Add("_anons3_" + condition.Guid, false);
                            var def = evaluateLines(ref ((SpokeAnonMethod)condition).@lines, df, variables);

                            new SpokeInstruction(SpokeInstructionType.Label, anonMethodsEntered.Last().Key);
                            if (anonMethodsEntered.Last().Key != "_anons3_" + condition.Guid)
                            {

                            }

                            anonMethodsEntered.Remove(anonMethodsEntered.Last().Key);

                            new SpokeInstruction(SpokeInstructionType.Goto, "_topOfWhile_" + condition.Guid);




                            new SpokeInstruction(SpokeInstructionType.Label, "End1Loop" + condition.Guid);

                            if (def != null)
                            {
                                if (((SpokeAnonMethod)condition).HasReturn)
                                {

                                    if (df.ReturnType.CompareTo(def, false))
                                    {
                                        if (def.Type == ObjectType.Unset)
                                        {
                                            currentObject.ReturnType = def;
                                        }
                                    }
                                    else
                                    {
                                        df.ReturnType = def;
                                    }
                                    return df.ReturnType;


                                }
                            }


                            if (((SpokeAnonMethod)condition).HasYield || ((SpokeAnonMethod)condition).HasYieldReturn)
                            {

                                var drf = new List<SpokeLine>(((SpokeAnonMethod)condition).Lines);

                                fuix(drf, fyv, true);

                                ((SpokeAnonMethod)condition).Lines = drf.ToArray();

                                new SpokeInstruction(SpokeInstructionType.GetLocal, fyv.VariableIndex) { DEBUG = fyv.VariableName };
                                //new SpokeInstruction(SpokeInstructionType.Return);

                                return new SpokeType(ObjectType.Array) { ClassName = "Array", ArrayItemType = df.ForYieldArray };
                            }

                        }


                    }
                    else
                    {


                        if (((SpokeAnonMethod)condition).HasYield || ((SpokeAnonMethod)condition).HasYieldReturn)
                        {
                            var drf = new List<SpokeLine>(((SpokeAnonMethod)condition).Lines);
                            //drf.Insert(0, new SpokeEqual() { LeftSide = fyv = new SpokeVariable() { VariableName = "__" + condition.Guid }, RightSide = new SpokeArray() { Parameters = new SpokeItem[0] } });


                            fyv = new SpokeVariable() { VariableName = "__" + condition.Guid };
                            variables.Add("__" + condition.Guid, new SpokeType(ObjectType.Array) { ClassName = "Array" },
                                          fyv);
                            //  fyv.ForSet = true;
                            ((SpokeAnonMethod)condition).ReturnYield = fyv;


                            fuix(drf, fyv, false);
                            fuix(drf, fyv, true);


                            ((SpokeAnonMethod)condition).Lines = drf.ToArray();
                        }

                        SpokeObjectMethod ce;
                        new SpokeInstruction(SpokeInstructionType.CreateMethod, ce = new SpokeObjectMethod()
                                {
                                    Lines = ((SpokeAnonMethod)condition).Lines,
                                    Parameters = ((SpokeAnonMethod)condition).Parameters,
                                    HasReturn = ((SpokeAnonMethod)condition).HasReturn,
                                    HasYield = ((SpokeAnonMethod)condition).HasYield,
                                    HasYieldReturn = ((SpokeAnonMethod)condition).HasYieldReturn
                                });


                        return new SpokeType(ObjectType.Method)
                        {
                            Type = ObjectType.Method,
                            Variables = new SpokeVariableInfo(),
                            ClassName = currentObject.RunningClass.ClassName,
                            AnonMethod = ce
                        };
                    }


                    break;
                case ISpokeItem.MethodCall:

                    var gf = ((SpokeMethodCall)condition);




                    if (gf.Parent is SpokeAnonMethod)
                    {


                        gf.Parent = new SpokeAnonMethod()
                        {
                            HasReturn = ((SpokeAnonMethod)gf.Parent).HasReturn,
                            HasYield = ((SpokeAnonMethod)gf.Parent).HasYield,
                            HasYieldReturn = ((SpokeAnonMethod)gf.Parent).HasYieldReturn,
                            Lines = ((SpokeLine[])((SpokeAnonMethod)gf.Parent).Lines.Clone()),
                            Parameters = ((ParamEter[])((SpokeAnonMethod)gf.Parent).Parameters.Clone()),
                            Parent = ((SpokeAnonMethod)gf.Parent).Parent,
                            RunOnVar = ((SpokeAnonMethod)gf.Parent).RunOnVar
                        };

                        for (int index = 1; index < gf.Parameters.Length; index++)
                        {
                            var spokeItem = gf.Parameters[index];
                            SpokeType eh;

                            var dims = new List<SpokeInstruction>(SpokeInstruction.ins);


                            ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index =
                                variables.Add(((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name,
                                              eh = evaluateItem(spokeItem, currentObject, variables), null);



                            eh.ByRef = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].ByRef;

                            if (eh.ByRef)
                            {
                                new SpokeInstruction(SpokeInstructionType.StoreLocalObject, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };
                            }
                            else

                                switch (eh.Type)
                                {
                                    case ObjectType.Null:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };

                                        break;
                                    case ObjectType.Int:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalInt, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };

                                        break;
                                    case ObjectType.Float:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalFloat, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };

                                        break;
                                    case ObjectType.String:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalString, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };

                                        break;
                                    case ObjectType.Bool:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalBool, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };

                                        break;
                                    case ObjectType.Array:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };

                                        break;
                                    case ObjectType.Object:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };

                                        break;
                                    case ObjectType.Method:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalMethod, ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index) { DEBUG = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name };

                                        break;
                                    case ObjectType.Void:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                        }

                        var df = new SpokeMethodParse() { RunningClass = currentObject.RunningClass };
                        SpokeType oldRF = null;

                        if (((SpokeAnonMethod)gf.Parent).HasYieldReturn || ((SpokeAnonMethod)gf.Parent).HasYield)
                        {
                            df.ForYieldArray = new SpokeType(ObjectType.Unset);
                            fyv = new SpokeVariable() { VariableName = "__" + condition.Guid };
                            variables.Add("__" + condition.Guid, new SpokeType(ObjectType.Array) { ClassName = "Array" },
                                          fyv);
                            //  fyv.ForSet = true;
                            ((SpokeAnonMethod)condition).ReturnYield = fyv;

                            new SpokeInstruction(SpokeInstructionType.CreateArray) { DEBUG = "array for loop" };
                            new SpokeInstruction(SpokeInstructionType.StoreLocalObject, fyv.VariableIndex) { DEBUG = fyv.VariableName };


                        }
                        else if (((SpokeAnonMethod)gf.Parent).HasReturn)
                        {
                            oldRF = df.ReturnType;
                            df.ReturnType = new SpokeType(ObjectType.Unset);
                        }

                        anonMethodsEntered.Add("_anonMethod_" + gf.Parent.Guid, true);

                        variables.IncreaseState();
                        var fd = evaluateLines(ref ((SpokeAnonMethod)gf.Parent).@lines, df, variables);
                        variables.DecreaseState();

                        new SpokeInstruction(SpokeInstructionType.Label, anonMethodsEntered.Last().Key);

                        if (anonMethodsEntered.Last().Key != "_anonMethod_" + gf.Parent.Guid)
                        {

                        }
                        anonMethodsEntered.Remove(anonMethodsEntered.Last().Key);



                        if (((SpokeAnonMethod)gf.Parent).SpecAnon && parentIsNull)
                        {
                            df.ReturnType = oldRF;
                        }

                        if (((SpokeAnonMethod)gf.Parent).HasYieldReturn || ((SpokeAnonMethod)gf.Parent).HasYield)
                        {

                            new SpokeInstruction(SpokeInstructionType.GetLocal, fyv.VariableIndex) { DEBUG = fyv.VariableName };

                            fd = new SpokeType(ObjectType.Array) { ArrayItemType = fd, ClassName = "Array" };
                        }
                        else
                            if (((SpokeAnonMethod)gf.Parent).HasReturn)
                            {
                                var fe = "_ret_" + ((SpokeAnonMethod)gf.Parent).Guid;
                                int de = variables.Add(fe, df.ReturnType, null);
                                switch (df.ReturnType.Type)
                                {
                                    case ObjectType.Null:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, de) { DEBUG = fe };

                                        break;
                                    case ObjectType.Int:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalInt, de) { DEBUG = fe };

                                        break;
                                    case ObjectType.Float:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalFloat, de) { DEBUG = fe };

                                        break;
                                    case ObjectType.String:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalString, de) { DEBUG = fe };

                                        break;
                                    case ObjectType.Bool:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalBool, de) { DEBUG = fe };

                                        break;
                                    case ObjectType.Array:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, de) { DEBUG = fe };

                                        break;
                                    case ObjectType.Object:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, de) { DEBUG = fe };

                                        break;
                                    case ObjectType.Method:
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalMethod, de) { DEBUG = fe };

                                        break;
                                    case ObjectType.Void:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }


                                new SpokeInstruction(SpokeInstructionType.GetLocal, de) { DEBUG = fe };

                            }


                        for (int index = 1; index < gf.Parameters.Length; index++)
                        {

                            var m = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Name;
                            variables[m, null].ByRef = false;
                            variables.Remove(m);
                        }
                        return fd;
                    }
                    else
                    {
                        var d = ((SpokeVariable)gf.Parent);
                        if (d.Parent == null)
                        {

                            SpokeMethod meth;
                            var parms = new SpokeType[gf.Parameters.Length];


                            if (Methods.TryGetValue(currentObject.RunningClass.ClassName + d.VariableName, out meth))
                            {
                                d.VariableIndex = Methods.Select(a => a.Value).ToList().IndexOf(meth);


                                for (int i = 0; i < parms.Length; i++)
                                {
                                    parms[i] = evaluateItem(gf.Parameters[i], currentObject, variables);
                                }
                                var ii = new SpokeInstruction(SpokeInstructionType.CallMethod, d.VariableIndex) { DEBUG = d.VariableName };
                                d.VType = SpokeVType.MethodName;
                                var demf = evaluateMethod(meth, currentObject.RunningClass, parms);
                                ii.Index2 = meth.NumOfVars;
                                ii.Index3 = meth.Parameters.Length;
                                return demf;
                            }
                            else
                            {

                                if (currentObject.RunningClass.Variables.TryGetValue(d.VariableName, out g, d) &&
                                    g.Type == ObjectType.Method)
                                {

                                    for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                    {
                                        var dims = new List<SpokeInstruction>(SpokeInstruction.ins);


                                        SpokeType eh;
                                        g.AnonMethod.Parameters[i].Index = variables.Add(
                                            g.AnonMethod.Parameters[i].Name,
                                            eh = evaluateItem(gf.Parameters[i + 1], currentObject, variables), null);
                                        eh.ByRef = g.AnonMethod.Parameters[i].ByRef;

                                        if (eh.ByRef)
                                        {
                                            new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };
                                        }
                                        else

                                            switch (eh.Type)
                                            {
                                                case ObjectType.Null:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Int:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalInt, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Float:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalFloat, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.String:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalString, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Bool:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalBool, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Array:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Object:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Method:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalMethod, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Void:
                                                    break;
                                                default:
                                                    throw new ArgumentOutOfRangeException();
                                            }


                                    }
                                    var df = new SpokeMethodParse() { RunningClass = currentObject.RunningClass };


                                    if (g.AnonMethod.HasYieldReturn || g.AnonMethod.HasYield)
                                    {
                                        df.ForYieldArray = new SpokeType(ObjectType.Unset);
                                    }
                                    else if (g.AnonMethod.HasReturn)
                                    {
                                        df.ReturnType = new SpokeType(ObjectType.Unset);
                                    }

                                    var ii = new SpokeInstruction(SpokeInstructionType.CallMethod, d.VariableIndex) { DEBUG = d.VariableName };
                                    throw new AbandonedMutexException("ASD");
                                    var rme = evaluateLines(ref g.AnonMethod.Lines, df, variables);

                                    //gay if its an actual method not anon
                                    ii.Index3 = g.AnonMethod.Parameters.Length;

                                    for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                    {
                                        var m = g.AnonMethod.Parameters[i].Name;
                                        variables[m, null].ByRef = false;
                                        variables.Remove(m);

                                    }

                                    d.VType = SpokeVType.ThisV;

                                    return rme;

                                }



                                if (variables.TryGetValue(d.VariableName, out g, d) && g.Type == ObjectType.Method)
                                {
                                    d.VType = SpokeVType.V;

                                    for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                    {
                                        SpokeType eh;
                                        var dims = new List<SpokeInstruction>(SpokeInstruction.ins);


                                        g.AnonMethod.Parameters[i].Index = variables.Add(
                                            g.AnonMethod.Parameters[i].Name,
                                            eh = evaluateItem(gf.Parameters[i + 1], currentObject, variables), null);
                                        eh.ByRef = g.AnonMethod.Parameters[i].ByRef;

                                        if (eh.ByRef)
                                        {
                                            new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };
                                        }
                                        else

                                            switch (eh.Type)
                                            {
                                                case ObjectType.Null:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Int:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalInt, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Float:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalFloat, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.String:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalString, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Bool:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalBool, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Array:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Object:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Method:
                                                    new SpokeInstruction(SpokeInstructionType.StoreLocalMethod, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                    break;
                                                case ObjectType.Void:
                                                    break;
                                                default:
                                                    throw new ArgumentOutOfRangeException();
                                            }


                                    }




                                    var df = new SpokeMethodParse() { RunningClass = currentObject.RunningClass };


                                    if (g.AnonMethod.HasYieldReturn || g.AnonMethod.HasYield)
                                    {
                                        df.ForYieldArray = new SpokeType(ObjectType.Unset);
                                    }
                                    else if (g.AnonMethod.HasReturn)
                                    {
                                        df.ReturnType = new SpokeType(ObjectType.Unset);
                                    }


                                    //  new SpokeInstruction(SpokeInstructionType.CallMethod, d.VariableIndex, 0, g.AnonMethod.Parameters.Length);
                                    //   throw new AbandonedMutexException("ASD");

                                    anonMethodsEntered.Add("__anonVar__" + condition.Guid, true);

                                    foreach (var spokeLine in g.AnonMethod.Lines)
                                    {
                                        resetGuids(spokeLine);
                                    }
                                    var rme = evaluateLines(ref g.AnonMethod.Lines, df, variables);

                                    new SpokeInstruction(SpokeInstructionType.Label, anonMethodsEntered.Last().Key);
                                    if (anonMethodsEntered.Last().Key != "__anonVar__" + condition.Guid)
                                    {

                                    }
                                    anonMethodsEntered.Remove(anonMethodsEntered.Last().Key);


                                    for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                    {
                                        var m = g.AnonMethod.Parameters[i].Name;
                                        variables[m, null].ByRef = false;
                                        variables.Remove(m);


                                    }

                                    if (g.AnonMethod.HasYieldReturn || g.AnonMethod.HasYield)
                                    {
                                        //                                      new SpokeInstruction(SpokeInstructionType.Return);
                                    }
                                    else if (g.AnonMethod.HasReturn)
                                    {
                                        //                                        new SpokeInstruction(SpokeInstructionType.Return);
                                    }

                                    return rme;

                                }

                                d.VType = SpokeVType.InternalMethodName;


                                d.VariableIndex =
                                    InternalMethodsTypes.Select(a => a.Value).ToList().IndexOf(
                                        InternalMethodsTypes[d.VariableName]);




                                for (int i = 0; i < parms.Length; i++)
                                {
                                    parms[i] = evaluateItem(gf.Parameters[i], currentObject, variables);
                                }

                                new SpokeInstruction(SpokeInstructionType.CallInternal, d.VariableIndex, 0, parms.Length) { DEBUG = d.VariableName };

                                //                                return new SpokeType(ObjectType.Void);
                                return InternalMethodsTypes[d.VariableName];
                            }
                        }
                        else
                        {
                            var pp = evaluateItem(d.Parent, currentObject, variables);
                            if (pp == null)
                            {
                                Console.WriteLine("A");
                            }
                            var fm = Methods.First(a => a.Value.MethodName == d.VariableName && a.Value.Class.Name == pp.ClassName).Value;

                            if (Methods.Any(a => a.Value.MethodName == d.VariableName && a.Value.Class.Name == pp.ClassName))
                            {
                                d.VariableIndex = Methods.Select(a => a.Value).ToList().IndexOf(fm);
                                if (fm.MethodFunc != null)
                                {
                                    var gm = new SpokeType[gf.Parameters.Length];

                                    gm[0] = pp;

                                    for (int index = 1; index < gf.Parameters.Length; index++)
                                    {
                                        gm[index] = evaluateItem(gf.Parameters[index], currentObject, variables);
                                    }

                                    if (fm.Class.Name == "Array" && fm.MethodName == "add")
                                    {
                                        pp.ArrayItemType = gm[1];
                                    }

                                    new SpokeInstruction(SpokeInstructionType.CallMethodFunc, d.VariableIndex, 0, gf.Parameters.Length) { DEBUG = d.VariableName };

                                    //hmmm


                                    if (fm.returnType.Type == ObjectType.Null)
                                    {
                                        return pp.ArrayItemType;
                                    }
                                    else return fm.returnType;
                                    return fm.returnType;

                                    //                                    return new SpokeType(ObjectType.Void);
                                }
                                else
                                {
                                    var parms = new SpokeType[gf.Parameters.Length];
                                    parms[0] = pp;
                                    //0 is evaluated up top
                                    for (int i = 1; i < parms.Length; i++)
                                    {
                                        parms[i] = evaluateItem(gf.Parameters[i], currentObject, variables);
                                    }

                                    var deds = evaluateMethod(fm, pp, parms);
                                    new SpokeInstruction(SpokeInstructionType.CallMethod, d.VariableIndex, fm.NumOfVars, parms.Length) { DEBUG = d.VariableName };
                                    return deds;
                                }

                            }



                            if (pp.Variables.TryGetValue(d.VariableName, out g, d) && g.Type == ObjectType.Method)
                            {


                                for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                {
                                    var dims = new List<SpokeInstruction>(SpokeInstruction.ins);
                                    SpokeType eh;
                                    g.AnonMethod.Parameters[i].Index = variables.Add(g.AnonMethod.Parameters[i].Name, eh = evaluateItem(gf.Parameters[i + 1], currentObject, variables), null);
                                    eh.ByRef = g.AnonMethod.Parameters[i].ByRef;
                                    if (eh.ByRef)
                                    {
                                        new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };
                                    }
                                    else

                                        switch (eh.Type)
                                        {
                                            case ObjectType.Null:
                                                new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                break;
                                            case ObjectType.Int:
                                                new SpokeInstruction(SpokeInstructionType.StoreLocalInt, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                break;
                                            case ObjectType.Float:
                                                new SpokeInstruction(SpokeInstructionType.StoreLocalFloat, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                break;
                                            case ObjectType.String:
                                                new SpokeInstruction(SpokeInstructionType.StoreLocalString, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                break;
                                            case ObjectType.Bool:
                                                new SpokeInstruction(SpokeInstructionType.StoreLocalBool, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                break;
                                            case ObjectType.Array:
                                                new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                break;
                                            case ObjectType.Object:
                                                new SpokeInstruction(SpokeInstructionType.StoreLocalObject, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                break;
                                            case ObjectType.Method:
                                                new SpokeInstruction(SpokeInstructionType.StoreLocalMethod, g.AnonMethod.Parameters[i].Index) { DEBUG = g.AnonMethod.Parameters[i].Name };

                                                break;
                                            case ObjectType.Void:
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException();
                                        }



                                }
                                var df = new SpokeMethodParse() { RunningClass = pp };


                                if (g.AnonMethod.HasYieldReturn || g.AnonMethod.HasYield)
                                {
                                    df.ForYieldArray = new SpokeType(ObjectType.Unset);
                                }
                                else if (g.AnonMethod.HasReturn)
                                {
                                    df.ReturnType = new SpokeType(ObjectType.Unset);
                                }

                                new SpokeInstruction(SpokeInstructionType.CallMethod, d.VariableIndex) { DEBUG = d.VariableName };
                                throw new AbandonedMutexException("ads");
                                anonMethodsEntered.Add("__anonVar__" + condition.Guid, true);
                                var rme = evaluateLines(ref g.AnonMethod.Lines, df, variables);


                                new SpokeInstruction(SpokeInstructionType.Label, anonMethodsEntered.Last().Key);
                                anonMethodsEntered.Remove(anonMethodsEntered.Last().Key);


                                for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                {
                                    var m = g.AnonMethod.Parameters[i].Name;
                                    variables[m, null].ByRef = false;
                                    variables.Remove(m);

                                }
                                return rme;

                            }



                            else

                                throw new AbandonedMutexException("no method: " + d.VariableName);

                        }
                    }



                    break;
                case ISpokeItem.Construct:
                    var cons = new SpokeType(ObjectType.Object);
                    var rf = (SpokeConstruct)condition;
                    cons.Variables = new SpokeVariableInfo();

                    if (rf.ClassName != null)
                    {
                        cons.ClassName = rf.ClassName;
                        var drj = _cla.First(a => a.Name == rf.ClassName);


                        var fm =
                            Methods.First(
                                a =>
                                a.Value.MethodName == ".ctor" && a.Value.Class.Name == rf.ClassName &&
                                a.Value.Parameters.Length == rf.Parameters.Length + 1).Value;

                        if (fm.VariableRefs != null)
                            cons.Variables = fm.VariableRefs;

                        rf.MethodIndex = Methods.Select(a => a.Value).ToList().IndexOf(fm);

                        new SpokeInstruction(SpokeInstructionType.CreateReference, drj.Variables.Length) { DEBUG = rf.ClassName };

                        if (fm.VariableRefs == null)
                        {
                            foreach (var v in drj.Variables)
                            {
                                cons.Variables.Add(v, new SpokeType(ObjectType.Unset), null);
                            }
                        }
                        var parms = new SpokeType[rf.Parameters.Length + 1];
                        parms[0] = cons;

                        for (int i = 1; i < parms.Length; i++)
                        {
                            parms[i] = evaluateItem(rf.Parameters[i - 1], currentObject, variables);
                        }

                        new SpokeInstruction(SpokeInstructionType.CallMethod, rf.MethodIndex, fm.NumOfVars, parms.Length) { DEBUG = rf.ClassName + ".ctor" };

                        foreach (var spokeItem in rf.SetVars)
                        {

                            spokeItem.Index = cons.Variables.Add(spokeItem.Name,
                                                                 evaluateItem(spokeItem.Item, currentObject, variables),
                                                                 null);

                            new SpokeInstruction(SpokeInstructionType.StoreToReference, spokeItem.Index);
                        }
                        if (fm.Instructions == null)
                        {
                            evaluateMethod(fm, cons, parms);

                            var def = new List<SpokeInstruction>(fm.Instructions);
                            def.Add(new SpokeInstruction(SpokeInstructionType.GetLocal, 0));
                            def.Add(new SpokeInstruction(SpokeInstructionType.Return));
                            fm.Instructions = def.ToArray();

                            SpokeInstruction.ins.RemoveAt(SpokeInstruction.ins.Count - 1);
                            SpokeInstruction.ins.RemoveAt(SpokeInstruction.ins.Count - 1);

                        }
                        else
                        {
                        }

                        rf.NumOfVars = cons.Variables.index;
                    }
                    else
                    {

                        new SpokeInstruction(SpokeInstructionType.CreateReference, rf.SetVars.Length) { DEBUG = "{}" };

                        foreach (var spokeItem in rf.SetVars)
                        {
                            spokeItem.Index = cons.Variables.Add(spokeItem.Name,
                                                                 evaluateItem(spokeItem.Item, currentObject, variables),
                                                                 null);

                            new SpokeInstruction(SpokeInstructionType.StoreToReference, spokeItem.Index);

                        }
                    }

                    return cons;

                    break;
                case ISpokeItem.Addition:

                    l = evaluateItem(((SpokeAddition)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeAddition)condition).RightSide, currentObject, variables);


                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.AddIntInt);
                                    return new SpokeType(l.Type);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.AddIntFloat);

                                    return new SpokeType(r.Type);

                                    break;
                                case ObjectType.String:
                                    new SpokeInstruction(SpokeInstructionType.AddIntString);

                                    return new SpokeType(r.Type);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.AddFloatInt);

                                    return new SpokeType(l.Type);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.AddFloatFloat);

                                    return new SpokeType(l.Type);

                                    break;
                                case ObjectType.String:
                                    new SpokeInstruction(SpokeInstructionType.AddFloatString);

                                    return new SpokeType(r.Type);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case ObjectType.String:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.AddStringInt);
                                    return new SpokeType(l.Type);
                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.AddStringFloat);
                                    return new SpokeType(l.Type);
                                    break;
                                case ObjectType.String:
                                    new SpokeInstruction(SpokeInstructionType.AddStringString);
                                    return new SpokeType(l.Type);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }





                    break;
                case ISpokeItem.Subtraction:

                    l = evaluateItem(((SpokeSubtraction)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeSubtraction)condition).RightSide, currentObject, variables);




                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.SubtractIntInt);
                                    return new SpokeType(l.Type);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.SubtractIntFloat);
                                    return new SpokeType(r.Type);

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.SubtractFloatInt);

                                    return new SpokeType(l.Type);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.SubtractFloatFloat);
                                    return new SpokeType(l.Type);

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }


                    break;
                case ISpokeItem.Multiplication:

                    l = evaluateItem(((SpokeMultiplication)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeMultiplication)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.MultiplyIntInt);

                                    return new SpokeType(l.Type);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.MultiplyIntFloat);
                                    return new SpokeType(r.Type);

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.MultiplyFloatInt);
                                    return new SpokeType(l.Type);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.MultiplyFloatFloat);
                                    return new SpokeType(l.Type);

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case ISpokeItem.Division:
                    l = evaluateItem(((SpokeDivision)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeDivision)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.DivideIntInt);

                                    return new SpokeType(l.Type);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.DivideIntFloat);
                                    return new SpokeType(l.Type);


                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.DivideFloatInt);
                                    return new SpokeType(l.Type);


                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.DivideFloatFloat);
                                    return new SpokeType(l.Type);


                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ISpokeItem.Greater:
                    l = evaluateItem(((SpokeGreaterThan)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeGreaterThan)condition).RightSide, currentObject, variables);
                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.GreaterIntInt);
                                    return new SpokeType(ObjectType.Bool);


                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.GreaterIntFloat);
                                    return new SpokeType(ObjectType.Bool);

                                    break;

                                default:

                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.GreaterFloatInt);

                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.GreaterFloatFloat);
                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }



                    break;
                case ISpokeItem.Less:

                    l = evaluateItem(((SpokeLessThan)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeLessThan)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.LessIntInt);

                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.LessIntFloat);
                                    return new SpokeType(ObjectType.Bool);

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.LessFloatInt);
                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.LessFloatFloat);
                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case ISpokeItem.And:

                    l = evaluateItem(((SpokeAnd)condition).LeftSide, currentObject, variables);
                    if (l.Type != ObjectType.Bool)
                    {
                        throw new AbandonedMutexException("Expected bool");
                    }

                    new SpokeInstruction(SpokeInstructionType.IfTrueContinueElse, "FakeShort" + condition.Guid);
                    r = evaluateItem(((SpokeAnd)condition).RightSide, currentObject, variables);
                    if (r.Type != ObjectType.Bool)
                    {
                        throw new AbandonedMutexException("Expected bool");
                    }

                    new SpokeInstruction(SpokeInstructionType.Goto, "EndShort" + condition.Guid);
                    new SpokeInstruction(SpokeInstructionType.Label, "FakeShort" + condition.Guid);
                    new SpokeInstruction(SpokeInstructionType.BoolConstant, false);

                    new SpokeInstruction(SpokeInstructionType.Label, "EndShort" + condition.Guid);

                    return new SpokeType(ObjectType.Bool);

                    break;
                case ISpokeItem.Or:
                    new SpokeInstruction(SpokeInstructionType.Comment, "OR BEGIN");


                    l = evaluateItem(((SpokeOr)condition).LeftSide, currentObject, variables);

                    if (l.Type != ObjectType.Bool)
                    {
                        throw new AbandonedMutexException("Expected bool");
                    }

                    new SpokeInstruction(SpokeInstructionType.Not);
                    new SpokeInstruction(SpokeInstructionType.IfTrueContinueElse, "FakeShort" + condition.Guid);



                    r = evaluateItem(((SpokeOr)condition).RightSide, currentObject, variables);
                    if (r.Type != ObjectType.Bool)
                    {
                        throw new AbandonedMutexException("Expected bool");
                    }

                    new SpokeInstruction(SpokeInstructionType.Goto, "EndShort" + condition.Guid);
                    new SpokeInstruction(SpokeInstructionType.Label, "FakeShort" + condition.Guid);
                    new SpokeInstruction(SpokeInstructionType.BoolConstant, true);

                    new SpokeInstruction(SpokeInstructionType.Label, "EndShort" + condition.Guid);



                    return new SpokeType(ObjectType.Bool);
                    break;
                case ISpokeItem.GreaterEqual:
                    l = evaluateItem(((SpokeGreaterThanOrEqual)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeGreaterThanOrEqual)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.GreaterEqualIntInt);
                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.GreaterEqualIntFloat);
                                    return new SpokeType(ObjectType.Bool);

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.GreaterEqualFloatInt);
                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.GreaterEqualFloatFloat);
                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ISpokeItem.LessEqual:
                    l = evaluateItem(((SpokeLessThanOrEqual)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeLessThanOrEqual)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.LessEqualIntInt);
                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.LessEqualIntFloat);
                                    return new SpokeType(ObjectType.Bool);

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    new SpokeInstruction(SpokeInstructionType.LessEqualFloatInt);

                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                case ObjectType.Float:
                                    new SpokeInstruction(SpokeInstructionType.LessEqualFloatFloat);
                                    return new SpokeType(ObjectType.Bool);

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ISpokeItem.Equality:
                    l = evaluateItem(((SpokeEquality)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeEquality)condition).RightSide, currentObject, variables);
                    if (!l.CompareTo(r, false))
                    {
                        throw new AbandonedMutexException(l.ToString() + " not equal to " + r.ToString());
                    }
                    new SpokeInstruction(SpokeInstructionType.Equal);


                    return new SpokeType(ObjectType.Bool);
                    break;
                case ISpokeItem.NotEqual:
                    l = evaluateItem(((SpokeNotEqual)condition).LeftSide, currentObject, variables);
                    r = evaluateItem(((SpokeNotEqual)condition).RightSide, currentObject, variables);
                    if (!l.CompareTo(r, false))
                    {
                        throw new AbandonedMutexException(l.ToString() + " not equal to " + r.ToString());
                    }

                    new SpokeInstruction(SpokeInstructionType.Equal);
                    new SpokeInstruction(SpokeInstructionType.Not);



                    return new SpokeType(ObjectType.Bool);

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return null;
        }

        private void resetGuids(SpokeLine l)
        {
            if (l is s)
            {
                ((s)l).resetGUID();
            }
            if (l is SpokeLines)
            {
                foreach (var spokeLine in ((SpokeLines)l).Lines)
                {
                    resetGuids(spokeLine);
                }
            }


        }


        private void fuix(List<SpokeLine> lines, SpokeVariable vr, bool remove)
        {
            for (int index = lines.Count - 1; index >= 0; index--)
            {
                var e = lines[index];


                if (e is SpokeYield)
                {
                    if (remove)
                    {
                        //      lines.Add(new SpokeReturn() { Return = vr });
                        lines.Remove(e);
                    }
                    else
                        lines.Insert(index, new SpokeMethodCall() { Parameters = new SpokeItem[] { new SpokeCurrent(), ((SpokeYield)e).Yield }, Parent = new SpokeVariable() { VariableName = "add", Parent = new SpokeVariable() { Parent = vr.Parent, VariableIndex = vr.VariableIndex, VariableName = vr.VariableName } } });
                }
                else if (e is SpokeYieldReturn)
                {
                    if (remove)
                    {
                        // lines.Add(new SpokeReturn() { Return = vr });
                        lines.Remove(e);
                    }
                    else
                        lines.Insert(index, new SpokeMethodCall() { Parameters = new SpokeItem[] { new SpokeCurrent(), ((SpokeYieldReturn)e).YieldReturn }, Parent = new SpokeVariable() { VariableName = "add", Parent = new SpokeVariable() { Parent = vr.Parent, VariableIndex = vr.VariableIndex, VariableName = vr.VariableName } } });
                }

                if (e is SpokeLines && (!(e is SpokeAnonMethod)))
                {

                    if (e is SpokeIf)
                    {
                        var df = new List<SpokeLine>(((SpokeIf)e).IfLines);
                        fuix(df, vr, remove);
                        ((SpokeIf)e).IfLines = df.ToArray();

                        if (((SpokeIf)e).ElseLines != null)
                        {
                            df = new List<SpokeLine>(((SpokeIf)e).ElseLines);
                            fuix(df, vr, remove);
                            ((SpokeIf)e).ElseLines = df.ToArray();
                        }


                    }
                    else
                    {
                        var df = new List<SpokeLine>(((SpokeLines)e).Lines);
                        fuix(df, vr, remove);
                        ((SpokeLines)e).Lines = df.ToArray();
                    }
                }
            }
        }
    }
    public class SpokeMethodType
    {
        public SpokeType[] Params;
        public SpokeType Return;

        public SpokeMethodType(SpokeMethodType methodType)
        {
            Params = (SpokeType[])methodType.Params.Clone();
            Return = new SpokeType(methodType.Return);

        }
        public SpokeMethodType(SpokeType[] p, SpokeType r)
        {
            Params = p;
            Return = r;

        }

        public bool CompareTo(SpokeMethodType grb, bool allowLeftUnset)
        {
            if (grb == null)
            {
                return false;
            }
            if (grb.Params.Length != Params.Length)
                return false;
            if (!grb.Return.CompareTo(Return, allowLeftUnset))
                return false;

            for (int index = 0; index < Params.Length; index++)
            {
                var spokeType = Params[index];
                if (!spokeType.CompareTo(grb.Params[index], allowLeftUnset))
                {
                    return false;
                }
            }

            return true;
        }
    }
    public class SpokeType
    {
        public ObjectType Type;

        public SpokeType ArrayItemType;
        public SpokeMethodType MethodType;
        public SpokeVariableInfo Variables;
        public string ClassName;
        public bool ByRef;

        public SpokeObjectMethod AnonMethod;
        public SpokeType CanOnlyBe;

        public SpokeType(SpokeType t)
        {
            Type = t.Type;
            ClassName = t.ClassName;
            ArrayItemType = t.ArrayItemType;
            ByRef = t.ByRef;
            if (t.Variables != null)
                Variables = new SpokeVariableInfo(t.Variables);
            if (t.MethodType != null)
                MethodType = new SpokeMethodType(t.MethodType);
        }
        public SpokeType(ObjectType t)
        {
            Type = t;
            if (t == ObjectType.Array)
            {
                ClassName = "Array";
                ArrayItemType = new SpokeType(ObjectType.Unset);
            }
        }
        public override string ToString()
        {
            return Type.ToString();
        }
        public bool CompareTo(SpokeType grb, bool allowLeftUnset, bool arrayRules = false)
        {
            if (Type == ObjectType.Unset)
            {
                return allowLeftUnset;
            }


            if (arrayRules && grb.Type == ObjectType.Unset)
            {

                grb.CanOnlyBe = this;

                return true;
            }

            if ((Type == ObjectType.Null && grb.Type == ObjectType.Object) || Type == ObjectType.Object && grb.Type == ObjectType.Null)
            {
                return true;
            }

            if (Type == grb.Type)
            {
                switch (Type)
                {
                    case ObjectType.Null:
                        return true;
                        break;
                    case ObjectType.Int:
                    case ObjectType.Float:
                    case ObjectType.String:
                    case ObjectType.Bool:
                        return true;
                        break;
                    case ObjectType.Array:
                        return ArrayItemType.CompareTo(grb.ArrayItemType, true, true);//may be true
                        break;
                    case ObjectType.Object:
                        foreach (var spokeType in Variables.Variables)
                        {
                            SpokeType rb;
                            if (grb.Variables.TryGetValue(spokeType.Key, out rb, null))
                            {
                                if (spokeType.Value.CompareTo(rb, true))//may be true
                                {
                                    continue;
                                }
                            }
                            return false;
                        }
                        return true;
                        break;
                    case ObjectType.Method:
                        return true;
                        return MethodType.CompareTo(grb.MethodType, false);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
            return false;
        }
        public void a()
        {

        }
    }
}


