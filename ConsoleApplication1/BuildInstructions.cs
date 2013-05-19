/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApplication1
{
    public class BuildInstructions
    {
        private List<SpokeClass> _cla;
        private Dictionary<string, SpokeMethod> Methods = new Dictionary<string, SpokeMethod>();
        private Dictionary<string, SpokeType> InternalMethods;

        public BuildInstructions(List<SpokeClass> cla, Dictionary<string, SpokeType> internalMethods, Dictionary<string, SpokeMethod> mets)
        {
            Methods = mets;
            _cla = cla;
            InternalMethods = internalMethods;
        }

        public void Run(string main, string ctor)
        {
            var fm = _cla.First(a => a.Name == main).Methods.First(a => a.MethodName == ctor);
            SpokeObject dm = new SpokeObject() { ClassName = main, Type = ObjectType.Object };



            var drj = _cla.First(a => a.Name == main);
            var fd = drj.Methods.First(a => a.MethodName == ".ctor" && a.Parameters.Length == 1);
            dm.Variables = new SpokeObject[fm.NumOfVars];


            for (int index = 0; index < drj.Variables.Length; index++)
            {

                dm.SetVariable(index, new SpokeObject());
            }

            evaluateLines(null, fd);




            // evaluate(fm, dm, new List<SpokeObject>() { dm });
        }

        public class SpokeMethodRun
        {
            public SpokeObject RunningClass;
            public List<SpokeObject> ForYield;

        }

        private bool forSet;
        private void evaluateLines(IEnumerable<SpokeLine> lines, SpokeMethod sm)
        {
            var lines_ = lines != null ? sm.Lines : lines.ToArray();


            foreach (var spokeLine in lines_)
            {
                switch (spokeLine.LType)
                {
                    case ISpokeLine.If:
                        var b = evalute(((SpokeIf)spokeLine).Condition, sm);
                        if (b.BoolVal)
                        {
                            evaluateLines(((SpokeIf)spokeLine).IfLines, sm);

                        }
                        else
                        {
                            if (((SpokeIf)spokeLine).ElseLines != null)
                            {
                                evaluateLines(((SpokeIf)spokeLine).ElseLines, sm);

                            }
                        }
                        break;
                    case ISpokeLine.Return:
                        evalute(((SpokeReturn)spokeLine).Return, sm);
                        new SpokeInstruction(SpokeInstructionType.Return);

                        break;
                    case ISpokeLine.Yield:
                        evalute(((SpokeYield)spokeLine).Yield, sm);
                        new SpokeInstruction(SpokeInstructionType.Yield);

                        break;
                    case ISpokeLine.YieldReturn:
                        evalute(((SpokeYieldReturn)spokeLine).YieldReturn, sm);
                        new SpokeInstruction(SpokeInstructionType.YieldReturn);

                        break;
                    case ISpokeLine.MethodCall:
                        evalute((SpokeItem)spokeLine, sm);
                        break;
                    case ISpokeLine.AnonMethod:
                        var arm = evalute((SpokeItem)spokeLine, sm);

                        break;
                    case ISpokeLine.Construct:
                        evalute((SpokeItem)spokeLine, sm);
                        break;
                    case ISpokeLine.Set:

                        var grf = ((SpokeEqual)spokeLine);

                        var right = evalute(grf.RightSide, sm);
                        forSet = true;

                        SpokeObject left;
                        switch (right.Type)
                        {
                            case ObjectType.Null:
                                break;
                            case ObjectType.Int:
                                left = evalute(grf.LeftSide, sm);
                                left.Type = ObjectType.Int;
                                left.IntVal = right.IntVal;

                                break;
                            case ObjectType.String:
                                left = evalute(grf.LeftSide, sm);
                                left.Type = ObjectType.String;
                                left.StringVal = right.StringVal;
                                left.ClassName = right.ClassName;

                                break;
                            case ObjectType.Float:
                                left = evalute(grf.LeftSide, sm);
                                left.Type = ObjectType.Float;
                                left.FloatVal = right.FloatVal;

                                break;
                            case ObjectType.Bool:
                                left = evalute(grf.LeftSide, sm);
                                left.Type = ObjectType.Bool;
                                left.BoolVal = right.BoolVal;

                                break;
                            case ObjectType.Array:
                                left = evalute(grf.LeftSide, sm);
                                left.Type = ObjectType.Array;
                                left.ClassName = "Array";
                                left.ArrayItems = (right.ArrayItems);
                                break;
                            case ObjectType.Object:
                                left = evalute(grf.LeftSide, sm);
                                left.Type = ObjectType.Object;
                                left.ClassName = right.ClassName;
                                left.Variables = (right.Variables);

                                break;
                            case ObjectType.Method:
                                left = evalute(grf.LeftSide, sm);
                                left.ClassName = right.ClassName;
                                left.Type = ObjectType.Method;
                                left.AnonMethod = right.AnonMethod;
                                left.Variables = right.Variables;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }


                        forSet = false;

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }



        private void evalute(SpokeItem condition, SpokeMethod sm)
        {
            SpokeObject r;
            SpokeObject l;
            switch (condition.IType)
            {
                case ISpokeItem.Array:
                    var ar = new SpokeObject() { Type = ObjectType.Array, ClassName = "Array" };

                    ar.ArrayItems = new List<SpokeObject>(20);

                    foreach (var spokeItem in ((SpokeArray)condition).Parameters)
                    {
                        var grb = evalute(spokeItem, sm);
                        if (grb.Type == ObjectType.Array)
                            ar.ArrayItems.AddRange(grb.ArrayItems);

                        else
                            ar.ArrayItems.Add(grb);
                    }

                    return ar;
                    break;
                case ISpokeItem.Float:
                    new SpokeInstruction(SpokeInstructionType.FloatConstant);
                    break;
                case ISpokeItem.Int:
                    new SpokeInstruction(SpokeInstructionType.IntConstant);
                    break;

                case ISpokeItem.Variable:
                    SpokeObject g;
                    SpokeVariable mv = ((SpokeVariable)condition);



                    if (mv.Parent != null)
                    {
                        var fs = forSet;
                        forSet = false;
                        evalute(mv.Parent, sm);

                        new SpokeInstruction(SpokeInstructionType.GetField, mv.VariableIndex);
                        return;
                    }

                    if (mv.This)
                    {
                        g = currentObject.RunningClass.Variables[mv.VariableIndex];
                        if (g == null)
                        {
                            currentObject.RunningClass.Variables[mv.VariableIndex] = g = new SpokeObject();
                        }
                        if (forSet && g.ByRef == false)
                        {
                            forSet = false;

                            currentObject.RunningClass.Variables[mv.VariableIndex] = g = new SpokeObject();
                        }
                        return g;
                    }

                    if (mv.VariableIndex == 0)
                    {
                        new SpokeInstruction(SpokeInstructionType.GetLocal, 0);
                        return;
                    }

                    if ((g = variables[mv.VariableIndex]) != null)
                    {
                        if (forSet && g.ByRef == false)
                        {
                            forSet = false;
                            return variables[mv.VariableIndex] = new SpokeObject();
                        }
                        return g;
                    }

                    return variables[((SpokeVariable)condition).VariableIndex] = new SpokeObject();

                    break;
                case ISpokeItem.ArrayIndex:
                    var fs_ = forSet;
                    forSet = false;
                    evalute(((SpokeArrayIndex)condition).Parent, sm);

                    evalute(((SpokeArrayIndex)condition).Index, sm);



                    if (ind.Type == ObjectType.String)
                    {
                        throw new AbandonedMutexException("hmmmm");
                        if (fs_ && pa.ByRef == false)
                        {
                            var drb = new SpokeObject();
                            //                            pa.Variables[ind.StringVal] = drb;
                            return drb;
                        }


                        //                      return pa.Variables[ind.StringVal];
                    }


                    if (ind.Type == ObjectType.Array)
                    {

                        SpokeObject arb = new SpokeObject();
                        arb.Type = ObjectType.Array;
                        arb.ClassName = "Array";
                        arb.ArrayItems = new List<SpokeObject>();
                        foreach (var spokeObject in ind.ArrayItems)
                        {
                            arb.ArrayItems.Add(pa.ArrayItems[spokeObject.IntVal]);
                        }


                        return arb;
                    }
                    if (fs_ && pa.ByRef == false)
                    {
                        var drb = new SpokeObject();
                        pa.ArrayItems[ind.IntVal] = drb;
                        return drb;
                    }

                    return pa.ArrayItems[ind.IntVal];
                    break;
                case ISpokeItem.Current:
                    new SpokeInstruction(SpokeInstructionType.GetLocal, 0);


                    break;
                case ISpokeItem.Null:
                    return new SpokeObject() { Type = ObjectType.Null };
                    break;
                case ISpokeItem.AnonMethod:


                    if (((SpokeAnonMethod)condition).Parent != null)
                    {
                        var rl = evalute(((SpokeAnonMethod)condition).Parent, sm);
                        SpokeMethodRun df;

                        if (((SpokeAnonMethod)condition).RunOnVar != null)
                        {
                            df = new SpokeMethodRun() { RunningClass = evalute(((SpokeAnonMethod)condition).RunOnVar, sm) };
                            if (df.RunningClass.AnonMethod.HasYield || df.RunningClass.AnonMethod.HasYieldReturn)
                            {
                                df.ForYield = new List<SpokeObject>();
                            }
                        }
                        else
                        {
                            df = new SpokeMethodRun() { RunningClass = currentObject.RunningClass };
                            if (((SpokeAnonMethod)condition).HasYield || ((SpokeAnonMethod)condition).HasYieldReturn)
                            {
                                df.ForYield = new List<SpokeObject>();
                            }
                        }






                        if (rl.Type == ObjectType.Array)
                        {
                            if (rl.ArrayItems.Count == 0)
                            {
                                if (((SpokeAnonMethod)condition).HasReturn)
                                {
                                    return new SpokeObject() { Type = ObjectType.Null };
                                }
                                else
                                {

                                }
                            }


                            foreach (var spokeObject in rl.ArrayItems)
                            {


                                if (((SpokeAnonMethod)condition).RunOnVar != null)
                                {

                                    for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                    {
                                        variables[i] = spokeObject;
                                        if (df.RunningClass.AnonMethod.Parameters[i].ByRef)
                                        {
                                            spokeObject.ByRef = true;
                                        }
                                    }

                                    var rme = evaluateLines(df.RunningClass.AnonMethod.Lines, df, variables);

                                    if (rme != null && rme.Type != ObjectType.Null)
                                        if (df.RunningClass.AnonMethod.HasReturn)
                                        {
                                            //for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                            //{
                                            //    variables.Remove(df.RunningClass.AnonMethod.Parameters[i].Name);
                                            //}

                                            return rme;
                                        }
                                        else if (df.RunningClass.AnonMethod.HasYield || df.RunningClass.AnonMethod.HasYieldReturn)
                                        {
                                            df.ForYield.Add(rme);
                                        }

                                    //for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                    //{
                                    //    var c = df.RunningClass.AnonMethod.Parameters[i].Name;
                                    //    variables[c].ByRef = false;
                                    //    variables.Remove(c);
                                    //}



                                }
                                else
                                {
                                    if (((SpokeAnonMethod)condition).Parameters != null)
                                        variables[((SpokeAnonMethod)condition).Parameters[0].Index] = spokeObject;
                                    var rme = evaluateLines(((SpokeAnonMethod)condition).Lines, df, variables);
                                    //yield return
                                    if (rme != null && rme.Type != ObjectType.Null)
                                        if (((SpokeAnonMethod)condition).HasReturn)
                                        {
                                            return rme;
                                        }
                                        else if (((SpokeAnonMethod)condition).HasYield || ((SpokeAnonMethod)condition).HasYieldReturn)
                                        {
                                            df.ForYield.Add(rme);
                                        }
                                }


                            }
                        }
                        else if (rl.Type == ObjectType.Bool)
                        {
                        g:
                            if (rl.BoolVal)
                            {
                                var def = evaluateLines(((SpokeAnonMethod)condition).Lines, df, variables);
                                if (def != null && def.Type != ObjectType.Null)
                                {
                                    if (((SpokeAnonMethod)condition).HasReturn)
                                    {
                                        return def;
                                    }
                                    else if (((SpokeAnonMethod)condition).HasYield || ((SpokeAnonMethod)condition).HasYieldReturn)
                                    {
                                        df.ForYield.Add(def);
                                    }

                                }

                                rl = evalute(((SpokeAnonMethod)condition).Parent, sm);
                                goto g;
                            }

                        }

                        if (df.ForYield != null)
                            return new SpokeObject() { Type = ObjectType.Array, ArrayItems = df.ForYield, ClassName = "Array" };
                        if (((SpokeAnonMethod)condition).HasReturn)
                        {
                            return new SpokeObject() { Type = ObjectType.Null };//should have returned earlier
                        }
                    }
                    else
                    {
                        return new SpokeObject()
                                   {
                                       ClassName = currentObject.RunningClass.ClassName,
                                       Type = ObjectType.Method,
                                       Variables = null,
                                       AnonMethod =
                                           new SpokeObjectMethod()
                                               {
                                                   Lines = ((SpokeAnonMethod)condition).Lines,
                                                   Parameters = ((SpokeAnonMethod)condition).Parameters,
                                                   HasReturn = ((SpokeAnonMethod)condition).HasReturn,
                                                   HasYield = ((SpokeAnonMethod)condition).HasYield,
                                                   HasYieldReturn = ((SpokeAnonMethod)condition).HasYieldReturn
                                               }
                                   };
                    }


                    break;
                case ISpokeItem.MethodCall:


                    var gf = ((SpokeMethodCall)condition);


                    if (gf.Parent is SpokeAnonMethod)
                    {
                        for (int index = 1; index < gf.Parameters.Length; index++)
                        {
                            var spokeItem = gf.Parameters[index];
                            SpokeObject eh;
                            variables[((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index] = eh = evalute(spokeItem, sm);
                            eh.ByRef = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].ByRef;
                        }
                        evaluateLines(((SpokeAnonMethod)gf.Parent).Lines, sm);

                    }
                    else
                    {
                        var d = ((SpokeVariable)gf.Parent);
                        if (d.Parent == null)
                        {
                            SpokeMethod meth;
                            if (Methods.TryGetValue(sm.Class.Name + d.VariableName, out meth))
                            {
                                for (int i = 0; i < gf.Parameters.Length; i++)
                                    evalute(gf.Parameters[i], sm);


                                SpokeInstruction.Beginner();
                                evaluateLines(null, meth);
                                SpokeInstruction[] c = SpokeInstruction.Ender();
                                meth.Instructions = c;
                            }
                            else
                            {

                                if ((g = currentObject.RunningClass.Variables[d.VariableIndex]) != null && g.Type == ObjectType.Method)
                                {
                                    for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                    {
                                        SpokeObject cg;
                                        variables[g.AnonMethod.Parameters[i].Index] = cg = evalute(gf.Parameters[i + 1], sm);
                                        cg.ByRef = g.AnonMethod.Parameters[i].ByRef;
                                    }

                                    evaluateLines(g.AnonMethod.Lines, sm);


                                }

                                if ((g = variables[d.VariableIndex]) != null && g.Type == ObjectType.Method)
                                {
                                    for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                    {
                                        SpokeObject cg;
                                        variables[g.AnonMethod.Parameters[i].Index] = cg = evalute(gf.Parameters[i + 1], sm);
                                        cg.ByRef = g.AnonMethod.Parameters[i].ByRef;


                                    }

                                    evaluateLines(g.AnonMethod.Lines, sm);


                                }





                                var parms = new SpokeObject[gf.Parameters.Length];
                                for (int i = 0; i < parms.Length; i++)
                                {
                                    parms[i] = evalute(gf.Parameters[i], sm);
                                }


                                return InternalMethods[d.VariableName](parms);
                            }
                        }
                        else
                        {
                            var pp = evalute(d.Parent, sm);

                            var meth =
                                _cla.First(a => a.Name == pp.ClassName).Methods.FirstOrDefault(
                                    a => a.MethodName == d.VariableName);
                            if (meth != null)
                            {
                                if (meth.MethodFunc != null)
                                {
                                    var gm = new SpokeObject[gf.Parameters.Length];

                                    gm[0] = pp;

                                    for (int index = 1; index < gf.Parameters.Length; index++)
                                    {
                                        gm[index] = evalute(gf.Parameters[index], sm);
                                    }
                                    return meth.MethodFunc(gm);
                                }
                                else
                                {
                                    var parms = new SpokeObject[gf.Parameters.Length];
                                    for (int i = 0; i < parms.Length; i++)
                                    {
                                        parms[i] = evalute(gf.Parameters[i], sm);
                                    }


                                    evaluateLines(null, meth);
                                }

                            }



                            if (pp.TryGetVariable(d.VariableIndex, out g) && g.Type == ObjectType.Method)
                            {
                                for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                {
                                    SpokeObject cg;
                                    variables[g.AnonMethod.Parameters[i].Index] = cg = evalute(gf.Parameters[i + 1], sm);
                                    cg.ByRef = g.AnonMethod.Parameters[i].ByRef;
                                }

                                evaluateLines(g.AnonMethod.Lines, sm);


                            }



                            else

                                throw new AbandonedMutexException("no method: " + d.VariableName);

                        }
                    }



                    break;
                case ISpokeItem.String:
                    new SpokeInstruction(SpokeInstructionType.StringConstant);
                    return new SpokeObject() { StringVal = ((SpokeString)condition).Value, Type = ObjectType.String };
                    break;
                case ISpokeItem.Bool:
                    return new SpokeObject() { BoolVal = ((SpokeBool)condition).Value, Type = ObjectType.Bool };
                    break;
                case ISpokeItem.Construct:

                    var cons = new SpokeObject();
                    cons.Type = ObjectType.Object;
                    var rf = (SpokeConstruct)condition;

                    if (rf.ClassName != null)
                    {
                        cons.ClassName = rf.ClassName;
                        var drj = _cla.First(a => a.Name == rf.ClassName);
                        var fd = drj.Methods.First(a => a.MethodName == ".ctor" && a.Parameters.Length == rf.Parameters.Length + 1);

                        cons.Variables = new SpokeObject[rf.NumOfVars];



                        var parms = new SpokeObject[fd.NumOfVars];
                        parms[0] = cons;
                        for (int i = 1; i < parms.Length; i++)
                        {
                            parms[i] = evalute(rf.Parameters[i - 1], sm);
                        }


                        evaluateLines(fd, cons, parms);
                    }

                    else
                    {
                        cons.Variables = new SpokeObject[rf.SetVars.Length];

                    }

                    for (int index = 0; index < rf.SetVars.Length; index++)
                    {
                        SVarItems spokeItem = rf.SetVars[index];
                        cons.Variables[spokeItem.Index] = evalute(spokeItem.Item, sm);
                    }
                    return cons;

                    break;
                case ISpokeItem.Addition:

                    l = evalute(((SpokeAddition)condition).LeftSide, sm);
                    r = evalute(((SpokeAddition)condition).RightSide, sm);


                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { IntVal = l.IntVal + r.IntVal, Type = l.Type };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { FloatVal = l.IntVal + r.FloatVal, Type = r.Type };

                                    break;
                                case ObjectType.String: return new SpokeObject() { StringVal = l.IntVal + r.StringVal, Type = r.Type };
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { FloatVal = l.FloatVal + r.IntVal, Type = l.Type };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { FloatVal = l.FloatVal + r.FloatVal, Type = l.Type };

                                    break;
                                case ObjectType.String: return new SpokeObject() { StringVal = l.FloatVal + r.StringVal, Type = r.Type };
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case ObjectType.String: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { StringVal = l.StringVal + r.IntVal, Type = l.Type };
                                    break;
                                case ObjectType.Float: return new SpokeObject() { StringVal = l.StringVal + r.FloatVal, Type = l.Type };
                                    break;
                                case ObjectType.String: return new SpokeObject() { StringVal = l.StringVal + r.StringVal, Type = l.Type };
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

                    l = evalute(((SpokeSubtraction)condition).LeftSide, sm);
                    r = evalute(((SpokeSubtraction)condition).RightSide, sm);




                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { IntVal = l.IntVal - r.IntVal, Type = l.Type };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { FloatVal = l.IntVal - r.FloatVal, Type = r.Type };

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { FloatVal = l.FloatVal - r.IntVal, Type = l.Type };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { FloatVal = l.FloatVal - r.FloatVal, Type = l.Type };

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

                    l = evalute(((SpokeMultiplication)condition).LeftSide, sm);
                    r = evalute(((SpokeMultiplication)condition).RightSide, sm);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { IntVal = l.IntVal * r.IntVal, Type = l.Type };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { FloatVal = l.IntVal * r.FloatVal, Type = r.Type };

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { FloatVal = l.FloatVal * r.IntVal, Type = l.Type };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { FloatVal = l.FloatVal * r.FloatVal, Type = l.Type };

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
                    l = evalute(((SpokeDivision)condition).LeftSide, sm);
                    r = evalute(((SpokeDivision)condition).RightSide, sm);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { IntVal = l.IntVal / r.IntVal, Type = l.Type };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { FloatVal = l.IntVal / r.FloatVal, Type = r.Type };

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { FloatVal = l.FloatVal / r.IntVal, Type = l.Type };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { FloatVal = l.FloatVal / r.FloatVal, Type = l.Type };

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
                    l = evalute(((SpokeGreaterThan)condition).LeftSide, sm);
                    r = evalute(((SpokeGreaterThan)condition).RightSide, sm);
                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { BoolVal = l.IntVal > r.IntVal, Type = ObjectType.Bool };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { BoolVal = l.IntVal > r.FloatVal, Type = ObjectType.Bool };

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { BoolVal = l.FloatVal > r.IntVal, Type = ObjectType.Bool };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { BoolVal = l.FloatVal > r.FloatVal, Type = ObjectType.Bool };

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }



                    return new SpokeObject() { BoolVal = l.IntVal > r.IntVal, Type = ObjectType.Bool };
                    break;
                case ISpokeItem.Less:

                    l = evalute(((SpokeLessThan)condition).LeftSide, sm);
                    r = evalute(((SpokeLessThan)condition).RightSide, sm);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { BoolVal = l.IntVal < r.IntVal, Type = ObjectType.Bool };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { BoolVal = l.IntVal < r.FloatVal, Type = ObjectType.Bool };

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { BoolVal = l.FloatVal < r.IntVal, Type = ObjectType.Bool };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { BoolVal = l.FloatVal < r.FloatVal, Type = ObjectType.Bool };

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

                    l = evalute(((SpokeAnd)condition).LeftSide, sm);

                    if (l.BoolVal)
                    {
                        r = evalute(((SpokeAnd)condition).RightSide, sm);
                        return new SpokeObject() { BoolVal = r.BoolVal, Type = ObjectType.Bool };
                    }
                    return new SpokeObject() { BoolVal = false, Type = ObjectType.Bool };

                    break;
                case ISpokeItem.Or:

                    l = evalute(((SpokeOr)condition).LeftSide, sm);
                    if (l.BoolVal)
                    {
                        return new SpokeObject() { BoolVal = true, Type = ObjectType.Bool };
                    }
                    r = evalute(((SpokeOr)condition).RightSide, sm);
                    return new SpokeObject() { BoolVal = r.BoolVal, Type = ObjectType.Bool };
                    break;
                case ISpokeItem.GreaterEqual:
                    l = evalute(((SpokeGreaterThanOrEqual)condition).LeftSide, sm);
                    r = evalute(((SpokeGreaterThanOrEqual)condition).RightSide, sm);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { BoolVal = l.IntVal >= r.IntVal, Type = ObjectType.Bool };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { BoolVal = l.IntVal >= r.FloatVal, Type = ObjectType.Bool };

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { BoolVal = l.FloatVal >= r.IntVal, Type = ObjectType.Bool };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { BoolVal = l.FloatVal >= r.FloatVal, Type = ObjectType.Bool };

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
                    l = evalute(((SpokeLessThanOrEqual)condition).LeftSide, sm);
                    r = evalute(((SpokeLessThanOrEqual)condition).RightSide, sm);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { BoolVal = l.IntVal <= r.IntVal, Type = ObjectType.Bool };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { BoolVal = l.IntVal <= r.FloatVal, Type = ObjectType.Bool };

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                                case ObjectType.Int: return new SpokeObject() { BoolVal = l.FloatVal <= r.IntVal, Type = ObjectType.Bool };

                                    break;
                                case ObjectType.Float: return new SpokeObject() { BoolVal = l.FloatVal <= r.FloatVal, Type = ObjectType.Bool };

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
                    l = evalute(((SpokeEquality)condition).LeftSide, sm);
                    r = evalute(((SpokeEquality)condition).RightSide, sm);

                    return new SpokeObject() { BoolVal = SpokeObject.Compare(l, r), Type = ObjectType.Bool };
                    break;
                case ISpokeItem.NotEqual:
                    l = evalute(((SpokeNotEqual)condition).LeftSide, sm);
                    r = evalute(((SpokeNotEqual)condition).RightSide, sm);

                    return new SpokeObject() { BoolVal = !SpokeObject.Compare(l, r), Type = ObjectType.Bool };

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }

}*/