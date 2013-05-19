using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    public class PrintExpressions
    {
        private List<SpokeClass> _cla;
        private readonly Dictionary<string, SpokeMethod> myCla2;
        private static  bool myShowIndex;

        public PrintExpressions(List<SpokeClass> cla, bool showIndex)
        {
            _cla = cla;
            myShowIndex = showIndex;
        }


        public PrintExpressions(SpokeMethod[] cla2, bool showIndex)
        {
            myCla2 = cla2.ToDictionary(a => a.Class.Name + a.MethodName);
            myShowIndex = showIndex;
        }


        public string Run()
        {
            StringBuilder sb = new StringBuilder();

            if (myCla2!=null) {
                foreach (var spokeMethod in myCla2) {
                    sb.AppendLine("\r\n\r\nClass: " + spokeMethod.Value.Class.Name);
                        sb.Append("\r\n  \t" + spokeMethod.Value.MethodName + "(");
                        var c = spokeMethod.Value.Parameters.ToArray();
                        for (int index = 0; index < c.Length; index++)
                        {
                            var spokeType = c[index];
                            sb.Append(spokeType);

                            if (index < c.Length - 1)
                            {
                                sb.Append(",");
                            }
                        }
                        sb.AppendLine(")");
                        if (spokeMethod.Value.Lines != null)
                            sb.AppendLine(evaluateLines(spokeMethod.Value.Lines, 2));
                     
                }


                return sb.ToString();
            }
            
            
            foreach (var spokeClass in _cla)
            {
                sb.AppendLine("\r\n\r\nClass: " + spokeClass.Name);
                         sb.AppendLine("  Methods: ");
                foreach (var spokeMethod in spokeClass.Methods)
                {

                    sb.Append("\r\n  \t" + spokeMethod.MethodName + "(");
                    var c = spokeMethod.Parameters.ToArray();
                    for (int index = 0; index < c.Length; index++)
                    {
                        var spokeType = c[index];
                        sb.Append(spokeType);

                        if (index < c.Length - 1)
                        {
                            sb.Append(",");
                        }
                    }
                    sb.AppendLine(")");
                    if (spokeMethod.Lines != null)
                      sb.AppendLine(  evaluateLines(spokeMethod.Lines, 2));

                }
            }


            return sb.ToString();



            // evaluate(fm, dm, new List<SpokeObject>() { dm });
        }

        public class SpokeMethodRun
        {
            public SpokeObject RunningClass;
            public List<SpokeObject> ForYield;

        }

        public static string evaluateLines(SpokeLine[] lines, int tabIndex)
        {
            StringBuilder sb = new StringBuilder();


            foreach (var spokeLine in lines)
            {
                sb.Append(getLine(spokeLine, tabIndex));
            }
            return sb.ToString();
        }

        public static string getLine(SpokeLine spokeLine,int tabIndex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            for (int i = 0; i < tabIndex; i++)
            {
                sb.Append("  \t");

            }

            switch (spokeLine.LType)
            {
                case ISpokeLine.If:
                    sb.Append("If ");
        sb.Append(            evalute(((SpokeIf)spokeLine).Condition, tabIndex));
                    sb.AppendLine(evaluateLines(((SpokeIf)spokeLine).IfLines.ToArray(), tabIndex + 1));
                    if (((SpokeIf)spokeLine).ElseLines != null)
                    {
                        for (int i = 0; i < tabIndex; i++)
                        {
                            sb.Append("  \t");
                        }
                        sb.Append("Else  ");
                        sb.AppendLine(evaluateLines(((SpokeIf)spokeLine).ElseLines.ToArray(), tabIndex + 1));
                    }

                    break;
                case ISpokeLine.Return:
                    sb.Append("Return ");
                    sb.Append(evalute(((SpokeReturn)spokeLine).Return, tabIndex));
                    break;
                case ISpokeLine.Yield: sb.Append("Yield ");
                    sb.Append(evalute(((SpokeYield)spokeLine).Yield, tabIndex));
                    break;
                case ISpokeLine.YieldReturn:
                    sb.Append("Yield Return ");
                    sb.Append(evalute(((SpokeYieldReturn)spokeLine).YieldReturn, tabIndex));
                    break;
                case ISpokeLine.MethodCall:


                    sb.Append(evalute((SpokeItem)spokeLine, tabIndex));
                    break;
                case ISpokeLine.AnonMethod:
                    sb.Append(evalute((SpokeItem)spokeLine, tabIndex));
                    break;
                case ISpokeLine.Construct:
                    sb.Append(evalute((SpokeItem)spokeLine, tabIndex));
                    break;
                case ISpokeLine.Set:

                    var grf = ((SpokeEqual)spokeLine);
                    sb.Append("Set ");
                    sb.Append(evalute(grf.LeftSide, tabIndex));
                    sb.Append("=");

                    sb.Append(evalute(grf.RightSide, tabIndex));


                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return sb.ToString();
        }


        public static string evalute(SpokeItem condition, int tabIndex)
        {
         
            StringBuilder sb = new StringBuilder();
            if (condition==null)
            {
                return sb.ToString();
            }
            switch (condition.IType)
            {
                case ISpokeItem.Array:
                    sb.Append("[");
                    for (int index = 0; index < ((SpokeArray)condition).Parameters.Length; index++)
                    {
                        var spokeItem = ((SpokeArray)condition).Parameters[index];
                        sb.Append(evalute(spokeItem, tabIndex));

                        if (index < ((SpokeArray)condition).Parameters.Length - 1)
                        {
                            sb.Append(",");
                        }
                    }
                    sb.Append("]");
                    break;
                case ISpokeItem.Float:
                    sb.Append(((SpokeFloat)condition).Value);

                    break;
                case ISpokeItem.Int:
                    sb.Append(((SpokeInt)condition).Value);
                    break;

                case ISpokeItem.Variable:
                    if (((SpokeVariable)condition).Parent != null)
                    {
                        sb.Append(evalute(((SpokeVariable) condition).Parent, tabIndex));
                        if (myShowIndex)
                        {
                            sb.Append("." + ((SpokeVariable)condition).VariableIndex);
                        }
                        else                        sb.Append("." + ((SpokeVariable)condition).VariableName);
                        return sb.ToString();
                    }
                         if (myShowIndex) {
                             sb.Append(((SpokeVariable)condition).VariableIndex);
                         }
                         else
                    sb.Append(((SpokeVariable)condition).VariableName);
                    break;
                case ISpokeItem.ArrayIndex:
                    sb.Append(evalute(((SpokeArrayIndex) condition).Parent, tabIndex));

                    sb.Append("[");

                    sb.Append(evalute(((SpokeArrayIndex) condition).Index, tabIndex));

                    sb.Append("]");
                    break;
                case ISpokeItem.Current:
                    sb.Append("this");

                    break;
                case ISpokeItem.Null:
                    sb.Append("null");
                    break;
                case ISpokeItem.AnonMethod:

                    if (((SpokeAnonMethod)condition).Parent != null)
                    {

                        sb.Append("(");
                        sb.Append("(");
                        sb.Append(evalute(((SpokeAnonMethod) condition).Parent, tabIndex));

                        sb.Append(")=>");

                        if (((SpokeAnonMethod)condition).RunOnVar != null)
                        {
                            evalute(((SpokeAnonMethod)condition).RunOnVar, tabIndex);
                            //                            sb.Append(".");

                        }


                        if (((SpokeAnonMethod)condition).Parameters != null)
                        {
                            sb.Append("|(");

                            for (int index = 0; index < ((SpokeAnonMethod)condition).Parameters.Length; index++)
                            {
                                var p = ((SpokeAnonMethod)condition).Parameters[index];
                                if (p.ByRef)
                                {
                                    sb.Append("ref ");
                                }
                                sb.Append(p.Name);
                                if (index < ((SpokeAnonMethod)condition).Parameters.Length - 1)
                                {
                                    sb.Append(",");
                                }
                            }
                            sb.Append(")");

                        }



                        sb.Append(evaluateLines(((SpokeAnonMethod)condition).Lines, tabIndex + 1));

                        sb.AppendLine();
                        for (int i = 0; i < tabIndex; i++)
                        {
                            sb.Append("  \t");

                        }

                        sb.Append(")");


                    }
                    else
                    {
                        sb.Append("(");


                        if (((SpokeAnonMethod)condition).Parameters != null)
                        {
                            sb.Append("|(");

                            for (int index = 0; index < ((SpokeAnonMethod)condition).Parameters.Length; index++)
                            {
                                var p = ((SpokeAnonMethod)condition).Parameters[index];
                                if (p.ByRef)
                                {
                                    sb.Append("ref ");
                                }
                                sb.Append(p.Name);
                                if (index < ((SpokeAnonMethod)condition).Parameters.Length - 1)
                                {
                                    sb.Append(",");
                                }

                            }
                            sb.Append(")=>");

                        }



                        sb.Append(evaluateLines(((SpokeAnonMethod)condition).Lines, tabIndex + 1));
                        sb.AppendLine();
                        for (int i = 0; i < tabIndex; i++)
                        {
                            sb.Append("  \t");

                        }

                        sb.Append(")");




                    }


                    break;
                case ISpokeItem.MethodCall:


                    var gf = ((SpokeMethodCall)condition);


                    if (gf.Parent is SpokeAnonMethod)
                    {
                        sb.Append("(");

                        sb.Append("|(");
                        var ds = ((SpokeAnonMethod)gf.Parent);
                        for (int index = 0; index < ds.Parameters.Length; index++)
                        {
                            ParamEter p = ds.Parameters[index];

                            if (p.ByRef)
                            {
                                sb.Append("ref ");
                            }

                            sb.Append(p.Name + " = ");

                            sb.Append(evalute(gf.Parameters[index + 1], tabIndex));




                            if (index < ds.Parameters.Length - 1)
                            {
                                sb.Append(",");
                            }

                        }
                        sb.Append(")");




                        sb.Append(evaluateLines(((SpokeAnonMethod)gf.Parent).Lines, tabIndex + 1));

                        sb.AppendLine();
                        for (int i = 0; i < tabIndex; i++)
                        {
                            sb.Append("  \t");

                        }

                        sb.Append(")");
                    }
                    else
                    {
                        var d = ((SpokeVariable)gf.Parent);
                        if (d.Parent == null)
                        {
                            sb.Append(d.VariableName + "(");
                            for (int index = 0; index < gf.Parameters.Length; index++)
                            {
                                var spokeItem = gf.Parameters[index];
                                sb.Append(evalute(spokeItem, tabIndex));

                                if (index < gf.Parameters.Length - 1)
                                {
                                    sb.Append(",");
                                }

                            }
                            sb.Append(")");
                        }
                        else
                        {
                            sb.Append(evalute(d.Parent, tabIndex));

                            sb.Append("." + d.VariableName + "(");
                            for (int index = 0; index < gf.Parameters.Length; index++)
                            {
                                var spokeItem = gf.Parameters[index];
                                sb.Append(evalute(spokeItem, tabIndex));

                                if (index < gf.Parameters.Length - 1)
                                {
                                    sb.Append(",");
                                }
                            }
                            sb.Append(")");

                        }
                    }



                    break;
                case ISpokeItem.String:
                    sb.Append("\"" + ((SpokeString)condition).Value + "\"");
                    break;
                case ISpokeItem.Bool:
                    sb.Append(((SpokeBool)condition).Value);
                    break;
                case ISpokeItem.Construct:

                    var rf = (SpokeConstruct)condition;

                    sb.Append("Create ");

                    if (rf.ClassName != null)
                    {

                        sb.Append(rf.ClassName);



                    }
                    sb.Append("(");
                    for (int index = 0; index < rf.Parameters.Length; index++)
                    {
                        var spokeItem = rf.Parameters[index];
                        sb.Append(evalute(spokeItem, tabIndex));

                        if (index < rf.Parameters.Length - 1)
                        {
                            sb.Append(",");
                        }
                    }
                    sb.Append(")");

                    sb.Append("{");
                    for (int index = 0; index < rf.SetVars.Length; index++)
                    {
                        var spokeItem = rf.SetVars[index];
                        sb.Append(spokeItem.Name
                            + ":");
                        sb.Append(evalute(spokeItem.Item, tabIndex));

                        if (index < rf.SetVars.Length - 1)
                        {
                            sb.Append(",");
                        }
                    }
                    sb.Append("}");



                    break;
                case ISpokeItem.Addition:

                    sb.Append("(");

                    sb.Append(evalute(((SpokeAddition) condition).LeftSide, tabIndex));

                    sb.Append("+");
                    sb.Append(evalute(((SpokeAddition) condition).RightSide, tabIndex));

                    sb.Append(")");

                    break;
                case ISpokeItem.Subtraction:
                    sb.Append("(");

                    sb.Append(evalute(((SpokeSubtraction) condition).LeftSide, tabIndex));

                    sb.Append("-");

                    sb.Append(evalute(((SpokeSubtraction) condition).RightSide, tabIndex));
                    sb.Append(")");


                    break;
                case ISpokeItem.Multiplication:

                    sb.Append("(");
                    sb.Append(evalute(((SpokeMultiplication) condition).LeftSide, tabIndex));

                    sb.Append("*");
                    sb.Append(evalute(((SpokeMultiplication) condition).RightSide, tabIndex));

                    sb.Append(")");


                    break;
                case ISpokeItem.Division: sb.Append("(");

                    sb.Append(evalute(((SpokeDivision) condition).LeftSide, tabIndex));

                    sb.Append("/");
                    sb.Append(evalute(((SpokeDivision) condition).RightSide, tabIndex));

                    sb.Append(")");

                    break;
                case ISpokeItem.Greater:
                    sb.Append(evalute(((SpokeGreaterThan) condition).LeftSide, tabIndex));

                    sb.Append(">");
                    sb.Append(evalute(((SpokeGreaterThan) condition).RightSide, tabIndex));


                    break;
                case ISpokeItem.Less:

                    sb.Append(evalute(((SpokeLessThan) condition).LeftSide, tabIndex));

                    sb.Append(">");
                    sb.Append(evalute(((SpokeLessThan) condition).RightSide, tabIndex));



                    break;
                case ISpokeItem.And:

                    sb.Append(evalute(((SpokeAnd) condition).LeftSide, tabIndex));

                    sb.Append("&&");

                    sb.Append(evalute(((SpokeAnd) condition).RightSide, tabIndex));


                    break;
                case ISpokeItem.Or:

                    sb.Append(evalute(((SpokeOr) condition).LeftSide, tabIndex));

                    sb.Append("||");
                    sb.Append(evalute(((SpokeOr) condition).RightSide, tabIndex));

                    break;
                case ISpokeItem.GreaterEqual:
                    sb.Append(evalute(((SpokeGreaterThanOrEqual) condition).LeftSide, tabIndex));

                    sb.Append(">=");
                    sb.Append(evalute(((SpokeGreaterThanOrEqual) condition).RightSide, tabIndex));


                    break;
                case ISpokeItem.LessEqual:
                    sb.Append(evalute(((SpokeLessThanOrEqual) condition).LeftSide, tabIndex));

                    sb.Append("<=");
                    sb.Append(evalute(((SpokeLessThanOrEqual) condition).RightSide, tabIndex));


                    break;
                case ISpokeItem.Equality:
                    sb.Append(evalute(((SpokeEquality) condition).LeftSide, tabIndex));

                    sb.Append("==");
                    sb.Append(evalute(((SpokeEquality) condition).RightSide, tabIndex));


                    break;
                case ISpokeItem.NotEqual:
                    sb.Append(evalute(((SpokeNotEqual) condition).LeftSide, tabIndex));

                    sb.Append("!=");
                    sb.Append(evalute(((SpokeNotEqual) condition).RightSide, tabIndex));


                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return sb.ToString();
        }
    }
}