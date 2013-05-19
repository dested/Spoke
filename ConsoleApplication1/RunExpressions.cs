//#define Stacktrace

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    public class RunExpressions
    {
        private SpokeMethod[] Methods ;
        private Func<SpokeObject[], SpokeObject>[] InternalMethods;


        private SpokeObject FALSE = new SpokeObject(ObjectType.Bool) { BoolVal = false };
        private SpokeObject TRUE = new SpokeObject(ObjectType.Bool) { BoolVal = true };
        private SpokeObject[] ints;
        private SpokeObject NULL = new SpokeObject() { Type = ObjectType.Null };

        private SpokeObject intCache(int index)
        {
            if (index >0 && index < 100)
            {
                return ints[index];
            }
            return new SpokeObject(ObjectType.Int) { IntVal = index };
        }



        public RunExpressions(Func<SpokeObject[], SpokeObject>[] internalMethods, SpokeMethod[] mets)
        {
            Methods = mets;
            InternalMethods = internalMethods;
            ints=new SpokeObject[100];
            for (int i = 0; i < 100; i++) {
                ints[i] = new SpokeObject(ObjectType.Int) {IntVal = i};

            }
        }

        public void Run(SpokeConstruct so)
        {
            var fm = Methods[so.MethodIndex];
            //            var fm = _cla.First(a => a.Name == so.ClassName).Methods.First(a => a.MethodName == ".ctor");
            SpokeObject dm = new SpokeObject() {  Type = ObjectType.Object };
            dm.Variables = new SpokeObject[so.NumOfVars];
            for (int index = 0; index < so.NumOfVars; index++){
                dm.SetVariable(index, new SpokeObject());
            }
            evaluate(fm, dm, new SpokeObject[1] { dm });
            // evaluate(fm, dm, new List<SpokeObject>() { dm });
        }

        public class SpokeMethodRun
        {
            public SpokeObject RunningClass;

        }
#if Stacktrace
        private StringBuilder dfss = new StringBuilder();

#endif

























        private bool sforSet;
        private SpokeObject evaluateLines(SpokeLine[] lines, SpokeMethodRun currentObject, SpokeObject[] variables)
        {
#if Stacktrace
            File.WriteAllText("C:\\mna.txt", dfss.ToString());
#endif

            foreach (var spokeLine in lines)
            {

#if Stacktrace
                dfss.AppendLine(spokeLine.ToString());
#endif


                switch (spokeLine.LType)
                {
                    case ISpokeLine.If:
                        var b = evalute(((SpokeIf)spokeLine).Condition, currentObject, variables);
                        if (b.BoolVal)
                        {
                            var df = evaluateLines(((SpokeIf)spokeLine).IfLines, currentObject, variables);
                            if (df != null)
                            {
                                return df;
                            }
                        }
                        else
                        {
                            if (((SpokeIf)spokeLine).ElseLines != null)
                            {
                                var df = evaluateLines(((SpokeIf)spokeLine).ElseLines, currentObject, variables);
                                if (df != null)
                                {
                                    return df;
                                }
                            }
                        }
                        break;
                    case ISpokeLine.Return:
                        if (((SpokeReturn)spokeLine).Return == null)
                        {
                            return NULL;
                        }
                        return evalute(((SpokeReturn)spokeLine).Return, currentObject, variables);
                        break;
                    //case ISpokeLine.Yield:
                    //    currentObject.ForYield.Add(evalute(((SpokeYield)spokeLine).Yield, currentObject, variables));
                    //    break;
                    //case ISpokeLine.YieldReturn:
                    //    SpokeObject d;
                    //    currentObject.ForYield.Add(d = evalute(((SpokeYieldReturn)spokeLine).YieldReturn, currentObject, variables));
                    //    return d;
                    //    break;
                    case ISpokeLine.MethodCall:


                        evalute((SpokeItem)spokeLine, currentObject, variables, true);
                        break;
                    case ISpokeLine.AnonMethod:
                        var arm = evalute((SpokeItem)spokeLine, currentObject, variables, true);
                        if (arm != null && !((SpokeAnonMethod)spokeLine).SpecAnon)
                        {
                            return arm;
                        }
                        break;
                    case ISpokeLine.Construct:
                        return evalute((SpokeItem)spokeLine, currentObject, variables);
                        break;
                    case ISpokeLine.Set:

                        var grf = ((SpokeEqual)spokeLine);

                        var right = evalute(grf.RightSide, currentObject, variables);


                        SpokeObject left;
                        switch (right.Type)
                        {
                            case ObjectType.Null:
                                break;
                            case ObjectType.Int:
                                left = evalute(grf.LeftSide, currentObject, variables);
                                left.Type = ObjectType.Int;
                                left.IntVal = right.IntVal;

                                break;
                            case ObjectType.String:
                                left = evalute(grf.LeftSide, currentObject, variables);
                                left.Type = ObjectType.String;
                                left.StringVal = right.StringVal;
                                

                                break;
                            case ObjectType.Float:
                                left = evalute(grf.LeftSide, currentObject, variables);
                                left.Type = ObjectType.Float;
                                left.FloatVal = right.FloatVal;

                                break;
                            case ObjectType.Bool:
                                left = evalute(grf.LeftSide, currentObject, variables);
                                left.Type = ObjectType.Bool;
                                left.BoolVal = right.BoolVal;

                                break;
                            case ObjectType.Array:
                                left = evalute(grf.LeftSide, currentObject, variables);
                                left.Type = ObjectType.Array;
                                left.ArrayItems = new List<SpokeObject>(right.ArrayItems);
                                break;
                            case ObjectType.Object:
                                left = evalute(grf.LeftSide, currentObject, variables);
                                left.Type = ObjectType.Object;
                                left.Variables = (SpokeObject[])right.Variables.Clone();

                                break;
                            case ObjectType.Method:
                                left = evalute(grf.LeftSide, currentObject, variables);
                                left.Type = ObjectType.Method;
                                left.AnonMethod = right.AnonMethod;
                                left.Variables = right.Variables;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }


                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return null;
        }



        private SpokeObject evaluate(SpokeMethod fm, SpokeObject parent, SpokeObject[] paras)
        {
#if Stacktrace
            dfss.AppendLine(fm.MethodName);
#endif

            SpokeObject[] variables = new SpokeObject[fm.NumOfVars];

            for (int i = 0; i < fm.Parameters.Length; i++)
            {
                variables[i] = paras[i];
            }

            var sm = new SpokeMethodRun();
            sm.RunningClass = parent;


            return evaluateLines(fm.Lines, sm, variables);

        }
        List<int> specialIndeces;
        


        private SpokeObject evalute(SpokeItem condition, SpokeMethodRun currentObject, SpokeObject[] variables, bool parentIsNull = false)
        {

#if Stacktrace
                dfss.AppendLine(condition.ToString());
#endif

            SpokeObject r;
            SpokeObject l;
            switch (condition.IType)
            {
                case ISpokeItem.Array:
                    var ar = new SpokeObject() { Type = ObjectType.Array};

                    ar.ArrayItems = new List<SpokeObject>(20);

                    foreach (var spokeItem in ((SpokeArray)condition).Parameters)
                    {
                        var grb = evalute(spokeItem, currentObject, variables);
                        if (grb.Type == ObjectType.Array)
                            ar.ArrayItems.AddRange(grb.ArrayItems);

                        else
                            ar.ArrayItems.Add(grb);
                    }

                    return ar;
                    break;
                case ISpokeItem.Float:
                    return new SpokeObject() { FloatVal = ((SpokeFloat)condition).Value, Type = ObjectType.Float };
                    break;
                case ISpokeItem.Int:
                    return intCache(((SpokeInt)condition).Value);
                    break;

                case ISpokeItem.String:
                    return new SpokeObject() { StringVal = ((SpokeString)condition).Value, Type = ObjectType.String };
                    break;
                case ISpokeItem.Bool:
                    return ((SpokeBool)condition).Value ? TRUE : FALSE;
                    break;
                case ISpokeItem.Variable:
                    SpokeObject g;
                    SpokeVariable mv = ((SpokeVariable)condition);



                    if (mv.Parent != null)
                    {
                        var ca = evalute(mv.Parent, currentObject, variables);

                        return ca.GetVariable(mv.VariableIndex, mv.ForSet);
                    }

                    if (mv.This)
                    {
                        g = currentObject.RunningClass.Variables[mv.VariableIndex];
                        if (g == null)
                        {
                            return currentObject.RunningClass.Variables[mv.VariableIndex] = new SpokeObject();
                        }
                        if (mv.ForSet)
                        {

                            return currentObject.RunningClass.Variables[mv.VariableIndex] = new SpokeObject();
                        }
                        return g;
                    }

                    if ((g = variables[mv.VariableIndex]) != null)
                    {
                        if (mv.ForSet)
                        {
                            return variables[mv.VariableIndex] = new SpokeObject();
                        }
                        return g;
                    }

                    return variables[((SpokeVariable)condition).VariableIndex] = new SpokeObject();

                    break;
                case ISpokeItem.ArrayIndex:
                    var pa = evalute(((SpokeArrayIndex)condition).Parent, currentObject, variables);

                    var ind = evalute(((SpokeArrayIndex)condition).Index, currentObject, variables);



                    if (ind.Type == ObjectType.String)
                    {
                        throw new AbandonedMutexException("hmmmm");
                        if (((SpokeArrayIndex)condition).ForSet)
                        {
                            var drb = new SpokeObject();
                            //                            pa.Variables[ind.StringVal] = drb;
                            return drb;
                        }


                        //                      return pa.Variables[ind.StringVal];
                    }


                    if (ind.Type == ObjectType.Array)
                    {
                        specialIndeces = new List<int>(ind.ArrayItems.Count);
                        foreach (var spokeObject in ind.ArrayItems)
                        {
                            specialIndeces.Add(spokeObject.IntVal);
                        }
                        return pa;

                    }
                    if (((SpokeArrayIndex)condition).ForSet)
                    {
                        var drb = new SpokeObject();
                        pa.ArrayItems[ind.IntVal] = drb;
                        return drb;
                    }

                    return pa.ArrayItems[ind.IntVal];
                    break;
                case ISpokeItem.Current:

                    return currentObject.RunningClass;
                    break;
                case ISpokeItem.Null:

                    return NULL;
                    break;
                case ISpokeItem.AnonMethod:

                    if (((SpokeAnonMethod)condition).HasYield || ((SpokeAnonMethod)condition).HasYieldReturn)
                    {
                        if (((SpokeAnonMethod)condition).ReturnYield != null)
                        {

                            var d = evalute(((SpokeAnonMethod)condition).ReturnYield, currentObject, variables);

                            d.ArrayItems = new List<SpokeObject>();
                            d.Type = ObjectType.Array;
                        }
                    }







                    if (((SpokeAnonMethod)condition).Parent != null)
                    {






#if Stacktrace
                        dfss.AppendLine(((SpokeAnonMethod)condition).Parent.ToString());
#endif
                        var rl = evalute(((SpokeAnonMethod)condition).Parent, currentObject, variables);
                        SpokeMethodRun df;

                        if (((SpokeAnonMethod)condition).RunOnVar != null)
                        {
                            df = new SpokeMethodRun() { RunningClass = evalute(((SpokeAnonMethod)condition).RunOnVar, currentObject, variables) };



                            if (df.RunningClass.AnonMethod != null)
                            {
                                var d = evalute(df.RunningClass.AnonMethod.ReturnYield, currentObject, variables);
                                d.ArrayItems = new List<SpokeObject>();
                                
                                d.Type = ObjectType.Array;
                            }


                        }
                        else
                        {
                            df = new SpokeMethodRun() { RunningClass = currentObject.RunningClass };

                        }







                        if (rl.Type == ObjectType.Array)
                        {
                            if (rl.ArrayItems.Count == 0)
                            {
                                if (((SpokeAnonMethod)condition).HasReturn)
                                {
                                    return NULL;
                                }
                                else
                                {

                                }
                            }



                            if (specialIndeces != null)
                            {
                                foreach (var item in specialIndeces)
                                {
                                    var spokeObject = rl.ArrayItems[item];




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

                                        if (rme != null)
                                            if (df.RunningClass.AnonMethod.HasReturn)
                                            {
                                                //for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                                //{
                                                //    variables.Remove(df.RunningClass.AnonMethod.Parameters[i].Name);
                                                //}

                                                return rme;
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
                                        {

                                            variables[((SpokeAnonMethod)condition).Parameters[0].Index] = spokeObject;

                                            if (((SpokeAnonMethod)condition).Parameters.Length == 2)
                                                variables[((SpokeAnonMethod)condition).Parameters[1].Index] = intCache(item);


                                        }
                                        var rme = evaluateLines(((SpokeAnonMethod)condition).Lines, df, variables);
                                        //yield return
                                        if (rme != null)
                                            if (((SpokeAnonMethod)condition).HasReturn)
                                            {
                                                return rme;
                                            }

                                    }

                                } specialIndeces = null;

                            }
                            else {

                                var vmr = rl.ArrayItems.ToArray();

                                for (int index = 0; index < vmr.Length; index++)
                                {
                                    var spokeObject = vmr[index];



                                    if (((SpokeAnonMethod)condition).RunOnVar != null)
                                    {
                                        for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                        {
                                            variables[df.RunningClass.AnonMethod.Parameters[i].Index] = spokeObject;
                                            if (df.RunningClass.AnonMethod.Parameters[i].ByRef)
                                            {
                                                spokeObject.ByRef = true;
                                            }
                                        }




                                        var rme = evaluateLines(df.RunningClass.AnonMethod.Lines, df, variables);


                                        if (rme != null)
                                            if (df.RunningClass.AnonMethod.HasReturn)
                                            {
                                                //for (int i = 0; i < df.RunningClass.AnonMethod.Parameters.Count(); i++)
                                                //{
                                                //    variables.Remove(df.RunningClass.AnonMethod.Parameters[i].Name);
                                                //}

                                                return rme;
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
                                        {
                                            variables[((SpokeAnonMethod)condition).Parameters[0].Index] = spokeObject;

                                            if (((SpokeAnonMethod)condition).Parameters.Length == 2)
                                                variables[((SpokeAnonMethod)condition).Parameters[1].Index] = intCache(index);
                                        }
                                        var rme = evaluateLines(((SpokeAnonMethod)condition).Lines, df, variables);
                                        //yield return
                                        if (rme != null && rme.Type != ObjectType.Null)
                                            if (((SpokeAnonMethod)condition).HasReturn)
                                            {
                                                return rme;
                                            }
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
                                if (def != null)
                                {
                                    if (((SpokeAnonMethod)condition).HasReturn)
                                    {
                                        return def;
                                    }

                                }

                                rl = evalute(((SpokeAnonMethod)condition).Parent, currentObject, variables);
                                goto g;
                            }

                        }

                        if (((SpokeAnonMethod)condition).ReturnYield != null)
                        {

                            return evalute(((SpokeAnonMethod)condition).ReturnYield, currentObject, variables);

                        }

                        if (df.RunningClass.AnonMethod != null)
                        {
                            var d = evalute(df.RunningClass.AnonMethod.ReturnYield, currentObject, variables);
                            return d;
                        }



                        if (((SpokeAnonMethod)condition).HasReturn)
                        {
                            return null;
                            return new SpokeObject() { Type = ObjectType.Null };//should have returned earlier
                        }
                    }
                    else
                    {
                        return new SpokeObject()
                                   {
                                       
                                       Type = ObjectType.Method,
                                       AnonMethod =
                                           new SpokeObjectMethod()
                                               {
                                                   ReturnYield = ((SpokeAnonMethod)condition).ReturnYield,
                                                   Lines = ((SpokeAnonMethod)condition).Lines,
                                                   Parameters = ((SpokeAnonMethod)condition).Parameters,
                                                   HasReturn = ((SpokeAnonMethod)condition).HasReturn,
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
                            variables[((SpokeAnonMethod)gf.Parent).Parameters[index - 1].Index] =
                                eh = evalute(spokeItem, currentObject, variables);

                            eh.ByRef = ((SpokeAnonMethod)gf.Parent).Parameters[index - 1].ByRef;
                        }
                        var fd = evaluateLines(((SpokeAnonMethod)gf.Parent).Lines, currentObject, variables);

                        if (!parentIsNull)
                        {
                            return fd;
                        }
                        else
                        {
                          //Console.Write("Hmm");    
                        }
                    }
                    else
                    {
                        var d = ((SpokeVariable)gf.Parent);
                        if (d.Parent == null)
                        {



                            SpokeMethod meth;


                            SpokeObject[] parms;
                            switch (d.VType)
                            {
                                case SpokeVType.V:
                                    if ((g = variables[d.VariableIndex]) != null && g.Type == ObjectType.Method)
                                    {
                                        for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                        {
                                            SpokeObject cg;
                                            variables[g.AnonMethod.Parameters[i].Index] = cg = evalute(gf.Parameters[i + 1], currentObject, variables);
                                            cg.ByRef = g.AnonMethod.Parameters[i].ByRef;


                                        }
                                        if (g.AnonMethod.ReturnYield != null)
                                        {
                                            var dr = evalute(g.AnonMethod.ReturnYield, currentObject, variables);

                                            dr.ArrayItems = new List<SpokeObject>();
                                            
                                            dr.Type = ObjectType.Array; 
                                        }

                                        var rme = evaluateLines(g.AnonMethod.Lines, currentObject, variables);

                                        if (g.AnonMethod.ReturnYield != null)
                                        {
                                            return evalute(g.AnonMethod.ReturnYield, currentObject, variables);

                                        }

                                        return rme;

                                    }
                                    break;
                                case SpokeVType.MethodName:
                                    meth = Methods[d.VariableIndex];

                                        parms = new SpokeObject[gf.Parameters.Length];
                                        for (int i = 0; i < parms.Length; i++)
                                        {
                                            parms[i] = evalute(gf.Parameters[i], currentObject, variables);
                                        }

                                        return evaluate(meth, currentObject.RunningClass, parms);
                                    


                                    break;
                                case SpokeVType.InternalMethodName:
                                    parms = new SpokeObject[gf.Parameters.Length];
                                    for (int i = 0; i < parms.Length; i++)
                                    {
                                        parms[i] = evalute(gf.Parameters[i], currentObject, variables);
                                    }


                                    return InternalMethods[d.VariableIndex](parms);

                                    break;
                                case SpokeVType.ThisV:
                                    if ((g = currentObject.RunningClass.Variables[d.VariableIndex]) != null && g.Type == ObjectType.Method)
                                    {
                                        for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                        {
                                            SpokeObject cg;
                                            variables[g.AnonMethod.Parameters[i].Index] = cg = evalute(gf.Parameters[i + 1], currentObject, variables);
                                            cg.ByRef = g.AnonMethod.Parameters[i].ByRef;
                                        }

                                        var rme = evaluateLines(g.AnonMethod.Lines, currentObject, variables);
                                        return rme;

                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }


                        }
                        else
                        {
                            var pp = evalute(d.Parent, currentObject, variables);




                            var meth =Methods[d.VariableIndex];
                            if (meth != null)
                            {
                                if (meth.MethodFunc != null)
                                {
                                    var gm = new SpokeObject[gf.Parameters.Length];

                                    gm[0] = pp;

                                    for (int index = 1; index < gf.Parameters.Length; index++)
                                    {
                                        gm[index] = evalute(gf.Parameters[index], currentObject, variables);
                                    }
                                    return meth.MethodFunc(gm);
                                }
                                else
                                {
                                    var parms = new SpokeObject[gf.Parameters.Length];
                                    parms[0] = pp;
                                    for (int i = 1; i < parms.Length; i++)
                                    {
                                        parms[i] = evalute(gf.Parameters[i], currentObject, variables);
                                    }


                                    return evaluate(meth, pp,
                                                    parms);
                                }

                            }



                            if (pp.TryGetVariable(d.VariableIndex, out g) && g.Type == ObjectType.Method)
                            {
                                for (int i = 0; i < g.AnonMethod.Parameters.Count(); i++)
                                {
                                    SpokeObject cg;
                                    variables[g.AnonMethod.Parameters[i].Index] = cg = evalute(gf.Parameters[i + 1], currentObject, variables);
                                    cg.ByRef = g.AnonMethod.Parameters[i].ByRef;
                                }

                                var rme = evaluateLines(g.AnonMethod.Lines, new SpokeMethodRun() { RunningClass = pp }, variables);
                                return rme;

                            }



                            else

                                throw new AbandonedMutexException("no method: " + d.VariableName);

                        }
                    }



                    break;
                case ISpokeItem.Construct:

                    var cons = new SpokeObject();
                    cons.Type = ObjectType.Object;
                    var rf = (SpokeConstruct)condition;

                    if (rf.MethodIndex >=0)
                    {
                        
                        var fd =Methods[rf.MethodIndex];


                        cons.Variables = new SpokeObject[rf.NumOfVars];



                        var parms = new SpokeObject[fd.NumOfVars];
                        parms[0] = cons;
                        for (int i = 1; i < rf.Parameters.Length + 1; i++)
                        {
                            parms[i] = evalute(rf.Parameters[i - 1], currentObject, variables);
                        }


                        evaluate(fd, cons, parms);
                    }

                    else
                    {
                        cons.Variables = new SpokeObject[rf.SetVars.Length];

                    }

                    for (int index = 0; index < rf.SetVars.Length; index++)
                    {
                        SVarItems spokeItem = rf.SetVars[index];
                        cons.Variables[spokeItem.Index] = evalute(spokeItem.Item, currentObject, variables);
                    }
                    return cons;

                    break;
                case ISpokeItem.Addition:

                    l = evalute(((SpokeAddition)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeAddition)condition).RightSide, currentObject, variables);


                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return intCache(l.IntVal + r.IntVal);

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

                    l = evalute(((SpokeSubtraction)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeSubtraction)condition).RightSide, currentObject, variables);




                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return intCache(l.IntVal- r.IntVal);

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

                    l = evalute(((SpokeMultiplication)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeMultiplication)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return intCache(l.IntVal * r.IntVal);

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
                    l = evalute(((SpokeDivision)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeDivision)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return intCache(l.IntVal /r.IntVal);

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
                    l = evalute(((SpokeGreaterThan)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeGreaterThan)condition).RightSide, currentObject, variables);
                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return l.IntVal > r.IntVal?TRUE:FALSE;

                                    break;
                                case ObjectType.Float: return l.IntVal > r.FloatVal ? TRUE : FALSE;

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                            case ObjectType.Int: return l.FloatVal > r.IntVal?TRUE:FALSE;

                                    break;
                            case ObjectType.Float: return l.FloatVal > r.FloatVal?TRUE:FALSE;

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

                    l = evalute(((SpokeLessThan)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeLessThan)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int:
                                    return l.IntVal < r.IntVal ? TRUE : FALSE;

                                    break;
                                case ObjectType.Float: return l.IntVal < r.FloatVal?TRUE:FALSE;

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                            case ObjectType.Int: return l.FloatVal < r.IntVal?TRUE:FALSE;

                                    break;
                            case ObjectType.Float: return l.FloatVal < r.FloatVal?TRUE:FALSE;

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

                    l = evalute(((SpokeAnd)condition).LeftSide, currentObject, variables);

                    if (l.BoolVal)
                    {
                        r = evalute(((SpokeAnd)condition).RightSide, currentObject, variables);
                        return r.BoolVal?TRUE:FALSE;
                    }
                    return FALSE;

                    break;
                case ISpokeItem.Or:

                    l = evalute(((SpokeOr)condition).LeftSide, currentObject, variables);
                    if (l.BoolVal)
                    {
                        return TRUE;
                    }
                    r = evalute(((SpokeOr)condition).RightSide, currentObject, variables);
                    return r.BoolVal?TRUE:FALSE;
                    break;
                case ISpokeItem.GreaterEqual:
                    l = evalute(((SpokeGreaterThanOrEqual)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeGreaterThanOrEqual)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return l.IntVal >= r.IntVal?TRUE:FALSE;

                                    break;
                                case ObjectType.Float: return l.IntVal >= r.FloatVal?TRUE:FALSE;

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                            case ObjectType.Int: return l.FloatVal >= r.IntVal?TRUE:FALSE;

                                    break;
                            case ObjectType.Float: return l.FloatVal >= r.FloatVal?TRUE:FALSE;

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
                    l = evalute(((SpokeLessThanOrEqual)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeLessThanOrEqual)condition).RightSide, currentObject, variables);

                    switch (l.Type)
                    {

                        case ObjectType.Int:
                            switch (r.Type)
                            {
                                case ObjectType.Int: return l.IntVal <= r.IntVal?TRUE:FALSE;

                                    break;
                                case ObjectType.Float: return l.IntVal <= r.FloatVal?TRUE:FALSE;

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        case ObjectType.Float: switch (r.Type)
                            {
                            case ObjectType.Int: return l.FloatVal <= r.IntVal?TRUE:FALSE;

                                    break;
                            case ObjectType.Float: return l.FloatVal <= r.FloatVal?TRUE:FALSE;

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
                    l = evalute(((SpokeEquality)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeEquality)condition).RightSide, currentObject, variables);

                    return SpokeObject.Compare(l, r)?TRUE:FALSE;
                    break;
                case ISpokeItem.NotEqual:
                    l = evalute(((SpokeNotEqual)condition).LeftSide, currentObject, variables);
                    r = evalute(((SpokeNotEqual)condition).RightSide, currentObject, variables);

                    return SpokeObject.Compare(l, r)?FALSE:TRUE;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}