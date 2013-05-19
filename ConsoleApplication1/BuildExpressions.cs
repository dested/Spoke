#define DEBUG
#define DEBUGd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{

    public static class StaticMethods
    {
        public static T[] Trim<T>(this T[] items, params Func<T,bool>[] trims )
        {
            int starting = 0;
            int ending = 0;

            while (true)
            {
                bool trimmed = false;
                foreach (var func in trims)
                {
                    if (func(items[starting]))
                    {
                        trimmed = true;
                        break;
                    }
                }

                if (!trimmed)
                {
                    break;
                }
                starting++;
            }


            while (true)
            {
                bool trimmed = false;
                foreach (var func in trims)
                {
                    if (func(items[(items.Length - 1)-ending]))
                    {
                        trimmed = true;
                        break;
                    }
                }

                if (!trimmed)
                {
                    break;
                }
                ending++;
            }
            var ij = new T[items.Length - (starting) - (ending )];
            Array.Copy(items, starting, ij, 0, ij.Length);
            return ij;
        }
        public static void Assert(bool b, string fail)
        {
            if (!b)
            {
                throw new AbandonedMutexException(fail);
            }
        }
    }
    public class BuildExpressions
    {




#if DEBUGs
        private static StringBuilder sb = new StringBuilder();
#else

#endif

        private List<TokenMacroPiece> allMacros_;
        public List<SpokeClass> Run(List<Class> someClasses, List<TokenMacroPiece> tp)
        {
            allMacros_ = tp;
            List<SpokeClass> classes = new List<SpokeClass>();


            SpokeClass cl = new SpokeClass();
            classes.Add(cl);

            cl.Name = "Array";
            cl.Methods = new List<SpokeMethod>()
                             {
                                 new SpokeMethod()
                                     {
                                         MethodFunc = (a) =>
                                                          {
                                                              a[0].ArrayItems.Add(a[1]);
                                                              return null;
                                                          },
                                         MethodName = "add",
                                         Class = cl,
                                         returnType = new SpokeType(ObjectType.Void),
                                         Parameters = new string[]{"this","v"}
                                     },new SpokeMethod()
                                     {
                                         MethodFunc = (a) =>
                                                          {
                                                              a[0].ArrayItems.Clear();  
                                                              return null;
                                                          },
                                         MethodName = "clear",
                                         returnType = new SpokeType(ObjectType.Void),
                                         Class = cl,
                                         Parameters = new string[]{"this"}
                                     }, new SpokeMethod()
                                     {
                                         MethodFunc = (a) => new SpokeObject(a[0].ArrayItems.Count),
                                         MethodName = "length",
                                         Class = cl,
                                         returnType = new SpokeType(ObjectType.Int),
                                         Parameters = new string[]{"this"}
                                     },                                 new SpokeMethod()
                                     {
                                         MethodFunc = (a) =>
                                                          {
                                                              a[0].ArrayItems.RemoveAll(b => SpokeObject.Compare(b,a[1]));
                                                              return null;
                                                          },
                                         MethodName = "remove",
                                         Class = cl,
                                         returnType = new SpokeType(ObjectType.Void),
                                         Parameters = new string[]{"this","v"}
                                     },new SpokeMethod()
                                     {
                                         MethodFunc = (a) =>
                                                          {
                                                              return a[0].ArrayItems.Last();
                                                          },
                                         MethodName = "last",
                                         Class = cl,
                                         returnType = new SpokeType(ObjectType.Null),
                                         Parameters = new string[]{"this"}
                                     },new SpokeMethod()
                                     {
                                         MethodFunc = (a) =>
                                                          {
                                                              return a[0].ArrayItems.First();
                                                          },
                                         MethodName = "first",
                                         returnType = new SpokeType(ObjectType.Null),
                                         Class = cl,
                                         Parameters = new string[]{"this"}
                                     },                                 new SpokeMethod()
                                     {
                                         MethodFunc = (a) =>
                                                          {
                                                              a[0].ArrayItems.Insert(a[1].IntVal, a[2]);
                                                              return null;
                                                          },
                                         MethodName = "insert",
                                         Class = cl,
                                         returnType = new SpokeType(ObjectType.Void),
                                         Parameters = new string[]{"this","v","v2"}
                                     }
                             };


            foreach (var @class in someClasses)
            {
                classes.Add(cl = new SpokeClass());
                cl.Variables = @class.VariableNames.ToArray();
                cl.Name = @class.Name;
                foreach (var method in @class.Methods)
                {
                    SpokeMethod me;
                    cl.Methods.Add(me = new SpokeMethod());
                    me.MethodName = method.Name;

                    me.Parameters = new string[method.paramNames.Count + 1];
                    me.Parameters[0] = "this";
                    for (int index = 0; index < method.paramNames.Count; index++)
                    {
                        me.Parameters[index + 1] = method.paramNames[index];
                    }


                    me.Class = cl;

                    TokenEnumerator enumerator = new TokenEnumerator(method.Lines.ToArray());
#if DEBUGs
                    sb.AppendLine("Evaluating " + cl.Name + ": " + me.MethodName);
#endif
                      
                    me.Lines = getLines(enumerator, 2, new evalInformation()).ToArray();



                    me.HasYieldReturn = linesHave(me.Lines, ISpokeLine.YieldReturn);
                    me.HasYield = linesHave(me.Lines, ISpokeLine.Yield);
                    me.HasReturn = linesHave(me.Lines, ISpokeLine.Return);


                }
            }


            return classes;

        }
        private bool linesHave(SpokeLine[] lines, ISpokeLine r)
        {
            for (int index = 0; index < lines.Length; index++)
            {
                var e = lines[index];
                if (e.LType == r)
                {
                    return true;
                }
                if (e is SpokeLines && (!(e is SpokeAnonMethod) || (r == ISpokeLine.Return)))
                {

                    if (e is SpokeAnonMethod)
                    {
                        if (linesHave(((SpokeLines)e).Lines, ISpokeLine.Return) || linesHave(((SpokeLines)e).Lines, ISpokeLine.Yield) || linesHave(((SpokeLines)e).Lines, ISpokeLine.YieldReturn))
                        {
                            return true;
                        }
                    }
                    else
                        if (linesHave(((SpokeLines)e).Lines, r))
                        {
                            return true;
                        }
                }

            }
            return false;
        }


#if DEBUGs
        public static void save()
        {
            File.WriteAllText("C:\\spokers.spoke", sb.ToString());
        }
#endif
        public List<SpokeLine> getLines(TokenEnumerator enumerator, int tabIndex, evalInformation inf)
        {
            int lineIndex = 0;
            List<SpokeLine> lines = new List<SpokeLine>();


        top:
#if DEBUGs
            sb.AppendLine("Line " + lineIndex);
            sb.AppendLine("");
            lineIndex++;
#endif

            if (inf.CheckMacs == 0)
            {
                Console.Write(lines.LastOrDefault() == null ? "Parsing..." : PrintExpressions.getLine(lines.Last(), 0));
            }
            IToken token = enumerator.Current;

            if (token == null || token.Type == Token.EndOfCodez)
            {
                return lines;
            }

            if (token.Type == Token.NewLine)
            {
                enumerator.MoveNext();
                goto top;
            }

            if (token.Type == Token.Tab && enumerator.PeakNext().Type == Token.NewLine)
            {
                enumerator.MoveNext();
                enumerator.MoveNext();
                goto top;
            }

            if (((TokenTab)token).TabIndex < tabIndex)
            {
                enumerator.PutBack();
                return lines;
            }

            StaticMethods.Assert(((TokenTab)token).TabIndex == tabIndex, "Bad Tab");
            enumerator.MoveNext();

            if (enumerator.Current.Type == Token.NewLine)
            {
                enumerator.MoveNext();
                goto top;
            }


            CurrentItem = null;



            var s = eval(enumerator, tabIndex, new evalInformation(inf) { EatTab = false, ResetCurrentVal = true });

            if (s is SpokeLine)
            {
                lines.Add((SpokeLine)s);
            }
            else
            {
                throw new AbandonedMutexException("wat");
            }


            goto top;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public class evalInformation
        {
            public bool BreakBeforeEvaler;
            public bool ResetCurrentVal;
            public bool BreakBeforeEqual;
            public bool EatTab;
            public int CheckMacs;
            public bool DontEvalEquals;

            public evalInformation()
            {
                CheckMacs = 0;
            }
            public evalInformation(evalInformation inf)
            {
                //this.BreakBeforeEvaler = inf.BreakBeforeEvaler;
                //this.BreakBeforeEqual = inf.BreakBeforeEqual;
                this.EatTab = true;
                this.CheckMacs = inf.CheckMacs;

                //               this.ResetCurrentVal = inf.ResetCurrentVal;
            }
        }

        public Spoke eval(TokenEnumerator enumerator, int tabIndex, evalInformation inf)
        {
            if (inf.ResetCurrentVal)
            {
                CurrentItem = null;
            }
#if DEBUGs
            sb.AppendLine("Starting Eval " + enumerator.Current.Type);
#endif


            if (inf.CheckMacs < 2)
            {
                var df = CurrentItem;
                CurrentItem = null;
                var rm = checkRunMacro(enumerator, tabIndex, inf);
                if (rm != null)
                {
                    CurrentItem = rm;
                }
                else CurrentItem = df;
            }




            



            switch (enumerator.Current.Type)
            {
                case Token.Word:

                    if (((TokenWord)enumerator.Current).Word.ToLower() == "null")
                    {
                        CurrentItem = new SpokeNull();

                    }
                    else CurrentItem = new SpokeVariable() { Parent = CurrentItem, VariableName = ((TokenWord)enumerator.Current).Word }; 
                    enumerator.MoveNext(); 
                    break;
                case Token.Int:
                    CurrentItem = new SpokeInt() { Value = ((TokenInt)enumerator.Current)._value };
                    enumerator.MoveNext();
                    break;
                case Token.Float:
                    CurrentItem = new SpokeFloat() { Value = ((TokenFloat)enumerator.Current)._value };
                    enumerator.MoveNext();
                    break;
                case Token.String:
                    CurrentItem = new SpokeString() { Value = ((TokenString)enumerator.Current)._value };
                    enumerator.MoveNext();
                    break;
                case Token.False:
                    CurrentItem = new SpokeBool() { Value = false };
                    enumerator.MoveNext();
                    break;
                case Token.True:
                    CurrentItem = new SpokeBool() { Value = true };
                    enumerator.MoveNext();
                    break;
                case Token.OpenParen:
                    enumerator.MoveNext();

                    CurrentItem = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf));


                    if (enumerator.Current.Type == Token.Tab)
                    {
                        enumerator.MoveNext();

                    }


                    if (enumerator.Current.Type != Token.CloseParen)
                    {
                        throw new AbandonedMutexException();

                    }
                    enumerator.MoveNext();

                    break;
                case Token.If:
                    enumerator.MoveNext();

                    var i_f = new SpokeIf() { Condition = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf)) };
                    enumerator.MoveNext();
                    i_f.IfLines = getLines(enumerator, tabIndex + 1, new evalInformation(inf)).ToArray();
                    StaticMethods.Assert(enumerator.Current.Type == Token.NewLine || enumerator.Current.Type == Token.EndOfCodez, enumerator.Current.Type + " Isnt Newline");
                    enumerator.MoveNext();

                    if (enumerator.Current.Type == Token.Tab && enumerator.PeakNext().Type == Token.Else)
                    {
                        enumerator.MoveNext();
                        enumerator.MoveNext();
                        i_f.ElseLines = getLines(enumerator, tabIndex + 1, new evalInformation(inf)).ToArray();
                        StaticMethods.Assert(enumerator.Current.Type == Token.NewLine || enumerator.Current.Type == Token.EndOfCodez, enumerator.Current.Type + " Isnt Newline");
                        enumerator.MoveNext();

                    }

                    if (enumerator.Current.Type == Token.Tab && inf.EatTab)
                        enumerator.MoveNext();
                    return i_f;

                    break;
                case Token.Bar:

                    var an = new SpokeAnonMethod() { Parent = CurrentItem };
                    enumerator.MoveNext();
                    StaticMethods.Assert(enumerator.Current.Type == Token.OpenParen, enumerator.Current.Type + " Isnt OpenParen");
                    List<ParamEter> parameters_ = new List<ParamEter>();
                    enumerator.MoveNext();
                    if (enumerator.Current.Type != Token.CloseParen)
                    {
                    pback2:
                        bool byRe_f = false;

                        if (((TokenWord)enumerator.Current).Word.ToLower() == "ref")
                        {
                            byRe_f = true;
                            enumerator.MoveNext();

                        }
                        parameters_.Add(new ParamEter() { ByRef = byRe_f, Name = ((TokenWord)enumerator.Current).Word });

                        enumerator.MoveNext();
                        switch (enumerator.Current.Type)
                        {
                            case Token.CloseParen:
                                enumerator.MoveNext();
                                break;
                            case Token.Comma:
                                enumerator.MoveNext();
                                goto pback2;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }


                    an.Parameters = parameters_.ToArray();


                    StaticMethods.Assert(enumerator.Current.Type == Token.AnonMethodStart, enumerator.Current.Type + " Isnt anonmethodstart");
                    enumerator.MoveNext();

                    StaticMethods.Assert(enumerator.Current.Type == Token.NewLine, enumerator.Current.Type + " Isnt Newline");
                    enumerator.MoveNext();
                    an.Lines = getLines(enumerator, tabIndex + 1, new evalInformation(inf)).ToArray();
                    StaticMethods.Assert(enumerator.Current.Type == Token.NewLine || enumerator.Current.Type == Token.EndOfCodez, enumerator.Current.Type + " Isnt Newline");
                    enumerator.MoveNext();
                    an.HasYield = linesHave(an.Lines, ISpokeLine.Yield);
                    an.HasReturn = linesHave(an.Lines, ISpokeLine.Return);
                    an.HasYieldReturn = linesHave(an.Lines, ISpokeLine.YieldReturn);


                    StaticMethods.Assert(enumerator.Current.Type == Token.Tab && ((TokenTab)enumerator.Current).TabIndex == tabIndex, "Bad tabindex");
                    if (enumerator.Current.Type == Token.Tab && inf.EatTab)
                        enumerator.MoveNext();
                    if (enumerator.PeakNext().Type != Token.CloseParen)
                    {
                        return an;
                    }
                    else
                    {
                        enumerator.MoveNext();
                        CurrentItem = an;
                    }


                    break;

                case Token.OpenSquare:
                    CurrentItem = dyanmicArray(enumerator, tabIndex, inf);
                    break;

                case Token.OpenCurly:
                    CurrentItem = new SpokeConstruct();
                    CurrentItem = dynamicObject(enumerator, tabIndex, inf);
                    break;
                case Token.Create:

                    return createObject(enumerator, tabIndex, inf);
                    break;
                case Token.Return:
                    enumerator.MoveNext();

                    var r = new SpokeReturn() { Return = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf)) };
                    enumerator.MoveNext();
                    return r;
                case Token.Yield:
                    enumerator.MoveNext();
                    if (enumerator.Current.Type == Token.Return)
                    {
                        enumerator.MoveNext();
                        var y = new SpokeYieldReturn() { YieldReturn = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf)) };
                        enumerator.MoveNext();
                        return y;
                    }
                    else
                    {
                        var y = new SpokeYield() { Yield = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf)) };
                        enumerator.MoveNext();
                        return y;
                    }
            }


#if DEBUGs
            sb.AppendLine("Checking Eval 1" + enumerator.Current.Type);
#endif

            switch (enumerator.Current.Type)
            {
                case Token.OpenSquare:
                    var ar = new SpokeArrayIndex();
                    ar.Parent = CurrentItem;

                    enumerator.MoveNext();

                    ar.Index = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = false, ResetCurrentVal = true });

                    StaticMethods.Assert(enumerator.Current.Type == Token.CloseSquare, enumerator.Current.Type + " Isnt closesquare");
                    enumerator.MoveNext();

                    if (enumerator.Current.Type == Token.OpenSquare)
                    {

                    }

                    CurrentItem = ar;

                    break;
                case Token.OpenParen:
                    var meth = new SpokeMethodCall() { Parent = CurrentItem };
                    enumerator.MoveNext();
                    List<SpokeItem> param_ = new List<SpokeItem>();
                    param_.Add(new SpokeCurrent());
                    CurrentItem = null;
                    if (enumerator.Current.Type != Token.CloseParen)
                    {
                    g:
                        param_.Add((SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }));
                        if (enumerator.Current.Type == Token.Comma)
                        {
                            enumerator.MoveNext();
                            goto g;
                        }

                    }
                    enumerator.MoveNext();//closeparen

                    meth.Parameters = param_.ToArray();

                    CurrentItem = meth;
                    //loop params
                    break;
                case Token.Period:
                    var t = CurrentItem;
                    enumerator.MoveNext();
                    SpokeParent g;
                    Spoke c;
                    CurrentItem = g = (SpokeParent)(c = eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEvaler = true }));
                    //g.Parent = t;

                    //enumerator.MoveNext();
                    break;

            }
            switch (enumerator.Current.Type)
            {
                case Token.Period:
                    var t = CurrentItem;
                    enumerator.MoveNext();
                    SpokeParent g;
                    Spoke c;
                    CurrentItem = g = (SpokeParent)(c = eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEvaler = true }));
                    //g.Parent = t;

                    //enumerator.MoveNext();
                    break;

            }

#if DEBUGs
            sb.AppendLine("Checking Eval 2" + enumerator.Current.Type);
#endif

        forMethods:
            switch (enumerator.Current.Type)
            {
                case Token.OpenParen:
                    var meth = new SpokeMethodCall() { Parent = CurrentItem };
                    enumerator.MoveNext();
                    List<SpokeItem> param_ = new List<SpokeItem>();
                    param_.Add(new SpokeCurrent());
                g:
                    param_.Add((SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }));
                    if (enumerator.Current.Type == Token.Comma)
                    {
                        enumerator.MoveNext();
                        goto g;
                    }

                    enumerator.MoveNext();//closeparen

                    meth.Parameters = param_.ToArray();

                    CurrentItem = meth;


                    goto forMethods;
            }

#if DEBUGs
            sb.AppendLine("Checking Eval 3" + enumerator.Current.Type);
#endif
            if (inf.BreakBeforeEvaler)
            {
                return CurrentItem;
            }


            if (!inf.DontEvalEquals)
            {
                if (enumerator.Current.Type == Token.Equal)
                {

                    var equ = new SpokeEqual() { LeftSide = CurrentItem };
                    enumerator.MoveNext();

                    equ.RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { EatTab = false, ResetCurrentVal = true });

                    if (enumerator.Current.Type == Token.NewLine)
                    {
                        //   enumerator.MoveNext();  //newline        
                    }


                    return equ;


                }

            }

            switch (enumerator.Current.Type)
            {
                case Token.AnonMethodStart:
                    //checkparams
                    //getlines
                    var an = new SpokeAnonMethod() { Parent = CurrentItem };
                    enumerator.MoveNext();
                    if (enumerator.Current.Type == Token.Bar)
                    {
                        enumerator.MoveNext();
                        StaticMethods.Assert(enumerator.Current.Type == Token.OpenParen, enumerator.Current.Type + " Isnt openparen");

                        List<ParamEter> parameters_ = new List<ParamEter>();
                        enumerator.MoveNext();
                    pback2:

                        bool byRe_f = false;

                        if (((TokenWord)enumerator.Current).Word.ToLower() == "ref")
                        {
                            byRe_f = true;
                            enumerator.MoveNext();

                        }
                        parameters_.Add(new ParamEter() { ByRef = byRe_f, Name = ((TokenWord)enumerator.Current).Word });
                        enumerator.MoveNext();
                        switch (enumerator.Current.Type)
                        {
                            case Token.CloseParen:
                                enumerator.MoveNext();
                                break;
                            case Token.Comma:
                                enumerator.MoveNext();
                                goto pback2;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        an.Parameters = parameters_.ToArray();
                    }
                    else
                    {
                        an.RunOnVar = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true, BreakBeforeEqual = false, BreakBeforeEvaler = true });
                    }

                    StaticMethods.Assert(enumerator.Current.Type == Token.NewLine, enumerator.Current.Type + " Isnt Newline");
                    enumerator.MoveNext();
                    CurrentItem = null;
                    an.Lines = getLines(enumerator, tabIndex + 1, new evalInformation(inf)).ToArray();
                    an.HasYield = linesHave(an.Lines, ISpokeLine.Yield);
                    an.HasReturn = linesHave(an.Lines, ISpokeLine.Return);
                    an.HasYieldReturn = linesHave(an.Lines, ISpokeLine.YieldReturn);
                    StaticMethods.Assert(enumerator.Current.Type == Token.NewLine || enumerator.Current.Type == Token.EndOfCodez, enumerator.Current.Type + " Isnt Newline");
                    enumerator.MoveNext();

                    if (enumerator.Current.Type == Token.Tab && inf.EatTab)
                        enumerator.MoveNext();

                    CurrentItem = an;
                    break;
            }
            //    5*6-7+8/9+10
            switch (enumerator.Current.Type)
            {
                case Token.Plus:

                    enumerator.MoveNext();

                    CurrentItem = new SpokeAddition() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                    break;
                case Token.Minus:
                    enumerator.MoveNext();
                    CurrentItem = new SpokeSubtraction() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                    break;
                case Token.Divide:
                    enumerator.MoveNext();
                    CurrentItem = new SpokeDivision() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                    break;
                case Token.Mulitply:
                    enumerator.MoveNext();
                    CurrentItem = new SpokeMultiplication() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                    break;
            }

            if (inf.BreakBeforeEqual)
            {
                return CurrentItem;
            }



            switch (enumerator.Current.Type)
            {
                case Token.DoubleOr:
                    enumerator.MoveNext();
                    CurrentItem = new SpokeOr() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }) };

                    break;
                case Token.DoubleAnd:
                    enumerator.MoveNext();
                    CurrentItem = new SpokeAnd() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }) };

                    break;

            }

#if DEBUGs
            sb.AppendLine("Checking Eval 4" + enumerator.Current.Type);
#endif
            SpokeItem e;
            switch (enumerator.Current.Type)
            {
                case Token.DoubleEqual:
                    enumerator.MoveNext();
                    e = new SpokeEquality() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                    CurrentItem = e;

                    break;
                case Token.NotEqual:
                    enumerator.MoveNext();
                    e = new SpokeNotEqual() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                    CurrentItem = e;

                    break;
                case Token.Less:
                    enumerator.MoveNext();
                    if (enumerator.Current.Type == Token.Equal)
                    {
                        enumerator.MoveNext();
                        e = new SpokeLessThanOrEqual() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                        CurrentItem = e;
                        break;
                    }
                    e = new SpokeLessThan() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                    CurrentItem = e;
                    break;
                case Token.Greater:
                    enumerator.MoveNext();
                    if (enumerator.Current.Type == Token.Equal)
                    {
                        enumerator.MoveNext();
                        e = new SpokeGreaterThanOrEqual() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                        CurrentItem = e;
                        break;
                    }
                    e = new SpokeGreaterThan() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { BreakBeforeEqual = true, ResetCurrentVal = true }) };

                    CurrentItem = e;
                    break;

            }
            switch (enumerator.Current.Type)
            {
                case Token.DoubleOr:
                    enumerator.MoveNext();
                    CurrentItem = new SpokeOr() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }) };

                    break;
                case Token.DoubleAnd:
                    enumerator.MoveNext();
                    CurrentItem = new SpokeAnd() { LeftSide = CurrentItem, RightSide = (SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }) };

                    break;

            }


            return CurrentItem;



        }

        private SpokeMethodCall checkRunMacro(TokenEnumerator enumerator, int tabIndex, evalInformation inf)
        {
            //tm.


            for (int index = 0; ; index++)
            {
            gombo:
                if (index >= allMacros_.Count)
                    break;
                var tokenMacroPiece = allMacros_[index];
                List<SpokeItem> parameters = new List<SpokeItem>();
                var tm = enumerator.Clone();
                var outs = tm.OutstandingLine.ToArray();
                bool bad = false;
                for (int i = 0; i < tokenMacroPiece.Macro.Length; i++)
                {
                    var mp = tokenMacroPiece.Macro[i];

                    if ((mp.Type == Token.Ampersand || (mp.Type == Token.Word && ((TokenWord)mp).Word.StartsWith("$"))))
                    {

                        for (int j = i + 1; j < tokenMacroPiece.Macro.Length; j++)
                        {
                            var r = tokenMacroPiece.Macro[j];




                            if (!outs.Any(a => (mp.Type == Token.Ampersand || (mp.Type == Token.Word && ((TokenWord)mp).Word.StartsWith("$"))) || (a.Type == r.Type && (a.Type == Token.Word && r.Type == Token.Word ? ((TokenWord)a).Word == ((TokenWord)r).Word : true))))
                            {
                                bad = true;
                                break;
                            }
                        }
                        if (bad)
                        {
                            break;
                        }


                        var frf = tokenMacroPiece.Macro.Length == i + 1 ? new TokenNewLine() : tokenMacroPiece.Macro[i + 1];

                        int selectedLine = 0;

                        IToken tp = null;
                        int selectedToken = tm.tokenIndex;
                        if (frf.Type != Token.NewLine)
                        {
                            for (int j = 0; j < tm.CurrentLines.Count; j++)
                            {

                                for (int ic = selectedToken; ic < tm.CurrentLines[j].Tokens.Count; ic++)
                                {
                                    var a = tm.CurrentLines[j].Tokens[ic];

                                    if (tm.CurrentLines[j].Tokens[ic].Type == frf.Type &&
                                        (a.Type == frf.Type &&
                                         (a.Type == Token.Word && frf.Type == Token.Word
                                              ? ((TokenWord)a).Word == ((TokenWord)frf).Word
                                              : true)))
                                    {
                                        tp = tm.CurrentLines[j].Tokens[ic];
                                        break;
                                    }
                                }

                                if (tp != null)
                                {
                                    selectedLine = j;
                                    break;
                                }
                                else
                                {
                                    selectedToken = 0;
                                }
                            }
                            if(tp==null)
                            {
                                index++;
                                goto gombo;
                            }
                            var bf = new TokenAmpersand();
                            tm.CurrentLines[selectedLine].Tokens.Insert(tm.CurrentLines[selectedLine].Tokens.IndexOf(tp), bf);
                            try
                            {
                                CurrentItem = null;
                                var d = eval(tm, tabIndex, new evalInformation(inf) { CheckMacs = inf.CheckMacs + 1, DontEvalEquals = true, BreakBeforeEqual = true });
                                parameters.Add((SpokeItem)d);
                                if (d == null || (!(tm.Current.Type == Token.Ampersand || tokenMacroPiece.Macro.Length == i + 1)))
                                {
                                    index++;
                                    goto gombo;
                                }
                            }
                            catch (Exception e)
                            {
                                index++;
                                goto gombo;
                            }
                            tm.CurrentLine.Tokens.Remove(bf);


                        }
                        else
                        {
                            try
                            {
                                CurrentItem = null;
                                var d = eval(tm, tabIndex, new evalInformation(inf) { CheckMacs = inf.CheckMacs + 1, DontEvalEquals = true, BreakBeforeEqual = true });
                                parameters.Add((SpokeItem)d);
                                if (d == null)
                                {
                                    index++;
                                    goto gombo;
                                }
                            }
                            catch (Exception e)
                            {
                                index++;
                                goto gombo;
                            }

                        }
                    }
                    else
                    {
                        if (mp.Type == tm.Current.Type && (mp.Type == Token.Word && tm.Current.Type == Token.Word ? ((TokenWord)mp).Word == ((TokenWord)tm.Current).Word : true))
                        {
                            tm.MoveNext();
                        }
                        else
                        {
                            bad = true;
                            break;
                        }
                    }
                }
                if (!bad)
                {
                    SpokeMethodCall ambe = new SpokeMethodCall();
                    SpokeAnonMethod me = new SpokeAnonMethod();
                    me.SpecAnon = true;
                    me.Parameters = tokenMacroPiece.Parameters;
                    me.Lines = getLines(new TokenEnumerator(tokenMacroPiece.Lines.ToArray()), ((TokenTab)tokenMacroPiece.Lines[0].Tokens[0]).TabIndex, inf).ToArray();

                    me.HasYieldReturn = linesHave(me.Lines, ISpokeLine.YieldReturn);
                    me.HasYield = linesHave(me.Lines, ISpokeLine.Yield);
                    me.HasReturn = linesHave(me.Lines, ISpokeLine.Return);

                    ambe.Parent = me;
                    parameters.Insert(0, new SpokeCurrent());

                    ambe.Parameters = parameters.ToArray();

                    enumerator.Set(tm);
                    return ambe;
                }
            }
            return null;
        }


        private SpokeConstruct createObject(TokenEnumerator enumerator, int tabIndex, evalInformation inf)
        {
            enumerator.MoveNext();

            var sp = new SpokeConstruct();

            StaticMethods.Assert(enumerator.Current.Type == Token.Word, enumerator.Current.Type + " Isnt word");
            sp.ClassName = ((TokenWord)enumerator.Current).Word;
            enumerator.MoveNext();

            enumerator.MoveNext();//openeparam

            List<SpokeItem> param_ = new List<SpokeItem>();
            if (enumerator.Current.Type != Token.CloseParen)
            {
                CurrentItem = null;
            g:
                param_.Add((SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }));
                if (enumerator.Current.Type == Token.Comma)
                {
                    enumerator.MoveNext();
                    goto g;
                }


            }

            sp.Parameters = param_.ToArray();
            enumerator.MoveNext();//closeparam

            CurrentItem = sp;

            if (enumerator.Current.Type == Token.OpenCurly)
            {
                CurrentItem = dynamicObject(enumerator, tabIndex, inf);
            } 
            return (SpokeConstruct)CurrentItem; 
        }

        private SpokeItem dynamicObject(TokenEnumerator enumerator, int tabIndex, evalInformation inf)
        {

            enumerator.MoveNext(); //openesquigly
            SpokeConstruct cons = (SpokeConstruct)CurrentItem;

            List<SpokeEqual> param_ = new List<SpokeEqual>();
            if (enumerator.Current.Type != Token.CloseCurly)
            {

                CurrentItem = null;
            g:
                param_.Add((SpokeEqual)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }));
                if (enumerator.Current.Type == Token.Comma)
                {
                    enumerator.MoveNext();
                    goto g;
                }

            }
            enumerator.MoveNext();//closesquigly

            cons.SetVars = param_.Select(a => new SVarItems(((SpokeVariable)a.LeftSide).VariableName, 0, a.RightSide)).ToArray();
            return cons;
        }

        private SpokeArray dyanmicArray(TokenEnumerator enumerator, int tabIndex, evalInformation inf)
        {
            var ars = new SpokeArray();

            List<SpokeItem> parameters = new List<SpokeItem>();
            enumerator.MoveNext();
            if (enumerator.Current.Type == Token.CloseSquare)
            {
                enumerator.MoveNext();
                ars.Parameters = parameters.ToArray();
                return (ars);
            }
        pback:
            parameters.Add((SpokeItem)eval(enumerator, tabIndex, new evalInformation(inf) { ResetCurrentVal = true }));
            switch (enumerator.Current.Type)
            {
                case Token.CloseSquare:
                    enumerator.MoveNext();
                    break;
                case Token.Comma:
                    enumerator.MoveNext();
                    goto pback;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ars.Parameters = parameters.ToArray();
            return (ars);
        }

        private SpokeItem CurrentItem;












        /*private List<SpokeLine> getSpokeLines(TokenEnumerator enumerator, int tabIndex)
        {


            List<SpokeLine> lines = new List<SpokeLine>();

            bool newLine = true;


        top:

            IToken token = enumerator.Current;


            if (newLine)
            {
                if (((TokenTab)token).TabIndex < tabIndex)
                {
                    enumerator.PutBack();
                    return lines;
                }

                StaticMethods.Assert(((TokenTab)token).TabIndex == tabIndex);
                enumerator.MoveNext();
                token = enumerator.Current;
            }
            switch (token.Type)
            {
                case Token.OpenSquare:
                    SpokeItem fdf = GetSpokeItem(enumerator);
                    lines.Add((SpokeLine)fdf);

                    break;
                case Token.OpenParen:
                    SpokeItem df = GetSpokeItem(enumerator);
                    StaticMethods.Assert(enumerator.Current.Type == Token.CloseParen);
                    enumerator.MoveNext();
                    StaticMethods.Assert(enumerator.Current.Type == Token.AnonMethodStart);
                    enumerator.MoveNext();


                    var anonm = new SpokeAnonMethod();

                    anonm.Parent = df;
                    if (enumerator.PeakNext().Type == Token.OpenParen)
                    {

                        enumerator.MoveNext();


                        List<string> parameters_ = new List<string>();
                        enumerator.MoveNext();
                    pback2:
                        parameters_.Add(((TokenWord)enumerator.Current).Word);
                        enumerator.MoveNext();
                        switch (enumerator.Current.Type)
                        {
                            case Token.CloseParen:
                                enumerator.MoveNext();
                                break;
                            case Token.Comma:
                                enumerator.MoveNext();
                                goto pback2;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        anonm.Parameters = parameters_.ToArray();
                        enumerator.MoveNext();

                    }
                    StaticMethods.Assert(enumerator.Current.Type == Token.NewLine);
                    enumerator.MoveNext();

                    anonm.Lines = getSpokeLines(enumerator, ((TokenTab)enumerator.getFirstInLine()).TabIndex).ToArray();



                    lines.Add(anonm);

                    break;
                case Token.CloseParen:
                    break;
                case Token.Create:
                    break;
                case Token.Return:

                    SpokeReturn rt = new SpokeReturn();
                    enumerator.MoveNext();
                    rt.Return = GetSpokeItem(enumerator);
                    lines.Add(rt);

                    break;
                case Token.Word:
                    lines.Add((SpokeLine)GetSpokeItem(enumerator));

                    break;
                case Token.If:
                    var ifLine = new SpokeIf();
                    enumerator.MoveNext();
                    ifLine.Condition = GetSpokeItem(enumerator);
                    enumerator.MoveNext();
                    ifLine.IfLines = getSpokeLines(enumerator, tabIndex + 1);

                    lines.Add(ifLine);
                    break;
                case Token.NewLine:
                    newLine = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (!enumerator.MoveNext())
            {
                return lines;
            }
            goto top;
        }

        private SpokeItem GetSpokeItem(TokenEnumerator enumerator)
        {
            SpokeExpression expression = new SpokeExpression();


        top:
            IToken item = enumerator.Current;

            switch (item.Type)
            {
                case Token.AnonMethodStart:

                    var gf = EvaluatePieces(enumerator, expression.Items.Last());
                    expression.Items.Remove(expression.Items.Last());
                    expression.Items.Add(gf);

                    break;
                case Token.OpenSquare:


                    var ars = new SpokeArray();

                    List<SpokeItem> parameters = new List<SpokeItem>();
                    enumerator.MoveNext();
                pback:
                    parameters.Add(GetSpokeItem(enumerator));
                    switch (enumerator.Current.Type)
                    {
                        case Token.CloseSquare:
                            enumerator.MoveNext();
                            break;
                        case Token.Comma:
                            enumerator.MoveNext();
                            goto pback;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    ars.Parameters = parameters.ToArray();
                    expression.Items.Add(ars);

                    break;
                case Token.OpenCurly:
                    break;
                case Token.OpenParen:

                    if (enumerator.PeakNext(2).Type == Token.Comma || enumerator.PeakNext(2).Type == Token.CloseParen)
                    {
                        SpokeItem df = EvaluatePieces(enumerator, expression.Items.FirstOrDefault());
                        expression.Items.Add(df);
                        enumerator.MoveNext();


                    }
                    else
                    {
                        enumerator.MoveNext();

                        expression.Items.Add(GetSpokeItem(enumerator));
                        enumerator.PutBack();
                        if (enumerator.Current.Type != Token.CloseParen && enumerator.Current.Type != Token.NewLine)
                        {
                            enumerator.MoveNext();
                        }
                        //StaticMethods.Assert(enumerator.Current.Type==Token.CloseParen);
                    }
                    break;
                case Token.String:
                    expression.Items.Add(new SpokeString() { Value = ((TokenString)enumerator.Current)._value });
                    enumerator.MoveNext();
                    break;
                case Token.Int:
                    var ina_ = new SpokeInt() { Value = ((TokenInt)enumerator.Current)._value };
                    enumerator.MoveNext();
                    if (enumerator.Current.Type == Token.Period && (enumerator.MoveNext() && enumerator.Current.Type == Token.Period))
                    {
                        enumerator.MoveNext();
                        var ina_2 = new SpokeInt() { Value = ((TokenInt)enumerator.Current)._value };
                        var spa = new SpokeArray();
                        List<SpokeItem> nums = new List<SpokeItem>();
                        for (int i = ina_.Value; i <= ina_2.Value; i++)
                        {
                            nums.Add(new SpokeInt() { Value = i });
                        }
                        spa.Parameters = nums.ToArray();
                        expression.Items.Add(spa);
                        enumerator.MoveNext();

                    }
                    else
                    {
                        var plm = EvaluatePieces(enumerator, ina_);
                        if (plm == ina_)
                        {
                            expression.Items.Add(ina_);
                        }
                        else
                        {
                            expression.Items.Add(plm);
                        }
                    }

                    break;
                case Token.Float:
                    expression.Items.Add(new SpokeFloat() { Value = ((TokenFloat)enumerator.Current)._value });
                    enumerator.MoveNext();
                    break;
                case Token.Create:
                    break;
                case Token.Word:
                    expression.Items.Add(EvaluatePieces(enumerator, expression.Items.FirstOrDefault()));
                    break;
                case Token.CloseParen:

                    //enumerator.PutBack();


                    goto end;
                    break;

                case Token.NewLine:
                case Token.CloseSquare:
                case Token.Tab:
                case Token.Comma:

                    goto end;

                default:
                    throw new ArgumentOutOfRangeException(item.Type.ToString());
            }

            goto top;
        end:

            return expression.Items.Count == 1 ? expression.Items.First() : expression;
        }

        private SpokeItem EvaluatePieces(TokenEnumerator enumerator, SpokeItem currentItem)
        {
            SpokeItem Top;

            if (enumerator.Current.Type == Token.Word)
            {
                currentItem = new SpokeVariable() { Parent = null, VariableName = ((TokenWord)enumerator.Current).Word };
                enumerator.MoveNext();

                Top = currentItem;

            }
            else
            {
                Top = currentItem;

            }

            SpokeAnonMethod anonm;

        top:

            var p = enumerator.Current.Type;
            switch (p)
            {
                case Token.OpenSquare:
                    break;
                case Token.OpenCurly:
                    break;
                case Token.OpenParen:

                    if (currentItem == null)
                    {

                        int topTab = ((TokenTab)enumerator.getFirstInLine()).TabIndex;
                        anonm = new SpokeAnonMethod()
                        {
                            Parent = currentItem,
                        };

                        enumerator.MoveNext();


                        List<string> parameters_ = new List<string>();

                    pback2:
                        parameters_.Add(((TokenWord)enumerator.Current).Word);
                        enumerator.MoveNext();
                        switch (enumerator.Current.Type)
                        {
                            case Token.CloseParen:
                                enumerator.MoveNext();
                                break;
                            case Token.Comma:
                                enumerator.MoveNext();
                                goto pback2;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        anonm.Parameters = parameters_.ToArray();




                        StaticMethods.Assert(enumerator.Current.Type == Token.AnonMethodStart);
                        enumerator.MoveNext();
                        StaticMethods.Assert(enumerator.Current.Type == Token.NewLine);
                        enumerator.MoveNext();

                        anonm.Lines = getSpokeLines(enumerator, ((TokenTab)enumerator.getFirstInLine()).TabIndex).ToArray();

                        StaticMethods.Assert(enumerator.Current.Type == Token.NewLine);
                        enumerator.MoveNext();

                        StaticMethods.Assert(enumerator.Current.Type == Token.Tab && ((TokenTab)enumerator.Current).TabIndex == topTab);
                        enumerator.MoveNext();
                        StaticMethods.Assert(enumerator.Current.Type == Token.CloseParen);
                        enumerator.MoveNext();


                        currentItem = anonm;
                        Top = currentItem;
                        goto top;

                        break;

                    }

                    currentItem = new SpokeMethodCall() { Parent = currentItem };
                    List<SpokeItem> parameters = new List<SpokeItem>();

                    enumerator.MoveNext();
                pback:
                    parameters.Add(GetSpokeItem(enumerator));
                    //enumerator.MoveNext();
                    switch (enumerator.Current.Type)
                    {
                        case Token.CloseParen:
                            enumerator.MoveNext();
                            break;
                        case Token.Comma:
                            enumerator.MoveNext();
                            goto pback;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    ((SpokeMethodCall)currentItem).Parameters = parameters.ToArray();
                    //return currentItem;
                    Top = currentItem;
                    goto top;
                    break;

                case Token.AnonMethodStart:
                    anonm = new SpokeAnonMethod()
                    {
                        Parent = currentItem,
                    };

                    if (enumerator.PeakNext().Type == Token.OpenParen)
                    {

                        enumerator.MoveNext();


                        List<string> parameters_ = new List<string>();
                        enumerator.MoveNext();
                    pback2:
                        parameters_.Add(((TokenWord)enumerator.Current).Word);
                        enumerator.MoveNext();
                        switch (enumerator.Current.Type)
                        {
                            case Token.CloseParen:
                                enumerator.MoveNext();
                                break;
                            case Token.Comma:
                                enumerator.MoveNext();
                                goto pback2;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        anonm.Parameters = parameters_.ToArray();
                        enumerator.MoveNext();

                    }
                    anonm.Lines = getSpokeLines(enumerator, ((TokenTab)enumerator.getFirstInLine()).TabIndex).ToArray();
                    currentItem = anonm;
                    Top = currentItem;
                    goto top;

                    break;
                case Token.Comma:

                    break;
                case Token.Period:
                    enumerator.MoveNext();

                    currentItem = new SpokeVariable() { Parent = currentItem, VariableName = ((TokenWord)enumerator.Current).Word };

                    enumerator.MoveNext();
                    goto top;
                    break;
                case Token.Plus:

                    SpokeAddition add = new SpokeAddition();
                    add.LeftSide = currentItem;
                    enumerator.MoveNext();
                    add.RightSide = GetSpokeItem(enumerator);
                    return add;

                    break;
                    break;
                case Token.Minus: SpokeSubtraction sub = new SpokeSubtraction();
                    sub.LeftSide = currentItem;
                    enumerator.MoveNext();
                    sub.RightSide = GetSpokeItem(enumerator);
                    return sub;

                    break;
                    break;
                case Token.Divide:
                    SpokeDivision div = new SpokeDivision();
                    div.LeftSide = currentItem;
                    enumerator.MoveNext();
                    div.RightSide = GetSpokeItem(enumerator);
                    return div;

                    break;
                case Token.Mulitply: SpokeMultiplication mul = new SpokeMultiplication();
                    mul.LeftSide = currentItem;
                    enumerator.MoveNext();
                    mul.RightSide = GetSpokeItem(enumerator);
                    return mul;

                    break;
                case Token.Greater:
                    SpokeGreaterThan gtr = new SpokeGreaterThan();
                    gtr.LeftSide = currentItem;
                    enumerator.MoveNext();
                    gtr.RightSide = GetSpokeItem(enumerator);
                    return gtr;
                    break;
                case Token.Less:
                    SpokeLessThan ltr = new SpokeLessThan();
                    ltr.LeftSide = currentItem;
                    enumerator.MoveNext();
                    ltr.RightSide = GetSpokeItem(enumerator);
                    return ltr;
                    break;
                case Token.Equal:
                    SpokeEqual equal = new SpokeEqual();
                    equal.LeftSide = currentItem;
                    enumerator.MoveNext();

                    equal.RightSide = GetSpokeItem(enumerator);
                    return equal;
                    break;

                case Token.CloseParen:
                case Token.CloseSquare:

                    //                    enumerator.PutBack();
                    return Top;
                    break;

                case Token.NewLine:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(p.ToString());
            }


            return Top;
        }*/
    }




}