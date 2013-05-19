using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CSharp;
using PostParse;

namespace ConsoleApplication1
{
    public class BuildFile
    {
        private SpokeMethod[] Methods;
        private Func<SpokeObject[], SpokeObject>[] InternalMethods;

        private string TOP;

        public BuildFile(Func<SpokeObject[], SpokeObject>[] internalMethods, Tuple<SpokeMethod[], SpokeConstruct> mets)
        {
            Methods = mets.Item1;
            Construct = mets.Item2;

            InternalMethods = internalMethods;


        }

        private class MethodItem
        {
            public SpokeMethod Method;
            public string Name;
            public List<PostParseExpression> Expressions;
            public int SpecVariables;
        }

        public Tuple<Command, TempFileCollection> buildIt(string name)
        {
            List<Tuple<SpokeMethod, Tuple<string, List<PostParseExpression>>, int>> itemz = new List<Tuple<SpokeMethod, Tuple<string, List<PostParseExpression>>, int>>();


            foreach (var spokeMethod in Methods.Where(a => a.Instructions != null && a.Instructions.Length > 0))
            {
                Tuple<string, List<PostParseExpression>> der = buildMethod(spokeMethod);

                var gotos = der.Item2.Where(a => a.ExpressionType == PPExpressionType.String && ((PostParseString)a).GotoString != null);
                var labels = der.Item2.Where(a => a.ExpressionType == PPExpressionType.String && ((PostParseString)a).LabelString != null).ToArray();

                foreach (var postParseExpression in labels) {
                    if (!gotos.Any(a=>((PostParseString)a).GotoString==((PostParseString)postParseExpression).LabelString)) {
                        for (int index = der.Item2.Count-1; index >= 0; index--)
                        {
                            var parseExpression = der.Item2[index];
                            if (parseExpression.Equals(postParseExpression)) {
                                der.Item2.RemoveAt(index);
                            }
                        }
                    }
                }




                for (int index = der.Item2.Count - 1; index >= 1; index--)
                {
                    var postParseExpression = der.Item2[index];

                  
                    
                    if (postParseExpression.ExpressionType == PPExpressionType.String && der.Item2[index - 1].ExpressionType == PPExpressionType.String)
                    {
                        if (((PostParseString)postParseExpression).LabelString != null && ((PostParseString)der.Item2[index - 1]).GotoString != null)
                        {
                            if (((PostParseString)postParseExpression).LabelString.Equals(((PostParseString)der.Item2[index - 1]).GotoString))
                            {

                                if (!der.Item2.Any(a => a.ExpressionType == PPExpressionType.String && ((PostParseString)a).GotoString == ((PostParseString)postParseExpression).LabelString))
                                {
                                    der.Item2.RemoveAt(index - 1);
                                    der.Item2.RemoveAt(index - 1);
                                    index -= 1;
                                }
                            }
                        }
                    }
                }


                File.WriteAllText("C:\\aabc.txt", der.Item2.Where(a => !a.MarkForRemoval).Aggregate("", (a, b) => a + "\t\t" + b.ToString() + "\r\n"));
                AssureSplitStacks d;
                fixThings(der.Item2, d = new AssureSplitStacks(der.Item2));
                File.WriteAllText("C:\\aabc3.txt", der.Item2.Where(a => !a.MarkForRemoval).Aggregate("", (a, b) => a + "\t\t" + b.ToString() + "\r\n"));
                doThings(der.Item2);
                if (spokeMethod.Class.Name == "Main" && spokeMethod.MethodName == ".ctor")
                    Construct.NumOfVars = spokeMethod.NumOfVars = AssureSplitStacks.getAllVariables(der.Item2).OrderBy(a => a.Index).Last().Index + 1;
                else spokeMethod.NumOfVars = AssureSplitStacks.getAllVariables(der.Item2).OrderBy(a => a.Index).Last().Index + 1;
                itemz.Add(new Tuple<SpokeMethod, Tuple<string, List<PostParseExpression>>, int>(spokeMethod, der, d.NumberOfSpecVariables));

            }


            foreach (var tuple in itemz)
            {
                foreach (var postParseMethod in AssureSplitStacks.getAllMethods(tuple.Item2.Item2).Where(a => !a.TotalMethodName.Contains("[")))
                {
                    var g = itemz.FirstOrDefault(a => a.Item1.Class.Name + a.Item1.CleanMethodName == postParseMethod.TotalMethodName);

                    if (g != null)
                    {
                        postParseMethod.NumOfVariables = g.Item1.NumOfVars;
                    }
                    else
                    {
                        throw new AbandonedMutexException();
                    }
                }
            }





            TOP = @"
        private SpokeMethod[] Methods;
        private Func<SpokeObject[], SpokeObject>[] InternalMethods;


        private SpokeObject FALSE = new SpokeObject(false);
        private SpokeObject TRUE = new SpokeObject(true) ;
        private SpokeObject[] ints;
        private SpokeObject NULL = new SpokeObject(ObjectType.Null);

        private SpokeObject intCache(int index)
        {
           // if (index > 0 && index < 100)
            {
           //     return ints[index];
            }
            return new SpokeObject(index);
        }



        public RunClass()
        {
        }
        public void loadUp(Func<SpokeObject[], SpokeObject>[] internalMethods, SpokeMethod[] mets){
            Methods = mets;
            InternalMethods = internalMethods;
            ints = new SpokeObject[100];
            for (int i = 0; i < 100; i++)
            {
                ints[i] = new SpokeObject(i);
            }
        }

        public SpokeObject Run()
        { 
            SpokeObject dm = new SpokeObject(new SpokeObject[" + Construct.NumOfVars + @"]);
            var gm = new SpokeObject[" + Construct.NumOfVars + @"];
            gm[0] = dm;
            return Mainctor(gm);
        }";










            StringBuilder file2 = new StringBuilder();
            StringBuilder file = new StringBuilder();
            file.AppendLine(@"using System;using System.Collections.Generic;using System.Linq;using System.Text;
            namespace ConsoleApplication1{public partial class RunClass:Command{");

            file.AppendLine(TOP);
            file2.Append(@"using System;using System.Collections.Generic;using System.Linq;using System.Text;
            namespace ConsoleApplication1{public partial class RunClass:Command{");
            file2.AppendLine(TOP);



            foreach (var tuple in itemz)
            {
                var spokeMethod = tuple.Item1;
                var der = tuple.Item2;
                file.AppendLine(string.Format("private SpokeObject {0}(SpokeObject[] variables) {{", spokeMethod.Class.Name + spokeMethod.CleanMethodName));
                file.AppendLine(string.Format("SpokeObject[] specVariables =new SpokeObject[" + tuple.Item3 + "]; SpokeObject[] sps;"));
                file.AppendLine(string.Format("SpokeObject bm2;"));
                file.AppendLine(string.Format("SpokeObject bm;"));
                file.AppendLine(string.Format("SpokeObject lastStack;"));

                file2.AppendLine(string.Format("private SpokeObject {0}(SpokeObject[] variables) {{", spokeMethod.Class.Name + spokeMethod.CleanMethodName));
                file2.AppendLine(string.Format("SpokeObject[] stack =new SpokeObject[100]; SpokeObject[] sps;"));
                file2.AppendLine(string.Format("SpokeObject[] specVariables =new SpokeObject[" + tuple.Item3 + "];"));
                file2.AppendLine(string.Format("SpokeObject bm2;"));
                file2.AppendLine(string.Format("SpokeObject bm;"));
                file2.AppendLine(string.Format("SpokeObject lastStack;"));




                file2.AppendLine(der.Item1);


                string dm = der.Item2.Where(a => !a.MarkForRemoval).Aggregate("", (a, b) => a + "\t\t" + b.ToString() + "\r\n");
                file.AppendLine(dm);



                file.AppendLine(string.Format("return null;}}"));
                file2.AppendLine(string.Format("return null;}}"));
            }

            file.AppendLine(string.Format("}}}}"));
            file2.AppendLine(string.Format("}}}}"));

            File.WriteAllText(@"c:\abc.cs", file.ToString());
            File.WriteAllText(@"c:\abc2.cs", file2.ToString());


            return compile(name,file .ToString());

        }

        private string n = "a";
        private SpokeConstruct Construct;

        private string GetNext()
        {
            if (n.Last() == 'z')
            {
                n = n + 'a';
            }
            return n = n.Substring(0, n.Length - 1) + (char)(n.Last() + 1);
        }



        private void fixThings(List<PostParseExpression> item2, AssureSplitStacks assurer, int index = -1)
        {
            for (index = index == -1 ? item2.Count - 1 : index; index >= 0; index--)
            {
                PostParseExpression postParseExpression = item2[index];
                switch (postParseExpression.ExpressionType)
                {
                    case PPExpressionType.Statement:
                        var st = ((PostParseStatement)postParseExpression);
                        foreach (var a in st.Items)
                            tryToNeeds(a, false, assurer);
                        break;
                    case PPExpressionType.Set:

                        if (((PostParseSet)postParseExpression).Left is PostParseVariable)
                        {
                            var pps = (PostParseVariable)((PostParseSet)postParseExpression).Left;

                            if (pps.Type == VariableType.Stack)
                            {
                                tryToNeeds(((PostParseSet)postParseExpression).Left, true, assurer);
                            }
                        }

                        tryToNeeds(((PostParseSet)postParseExpression).Right, false, assurer);

                        break;
                    case PPExpressionType.String:

                        if (((PostParseString)postParseExpression).GotoString != null)
                        {
                            return;
                        }
                        else if (((PostParseString)postParseExpression).LabelString != null)
                        {
                            var split = assurer.GetSplit();
                            var k = new Dictionary<int, Tuple<int, List<PostParseVariable>>>(split.Needs);
                            foreach (var i in findGoto(item2, ((PostParseString)postParseExpression).LabelString))
                            {
                                fixThings(item2, split, i - 1);
                                split.Needs = new Dictionary<int, Tuple<int, List<PostParseVariable>>>(k);
                            }
                            fixThings(item2, split, index - 1);
                            return;
                        }
                        else
                        {

                        }

                        break;
                    case PPExpressionType.Method:
                        var wst = ((PostParseMethod)postParseExpression);
                        foreach (var a in wst.Params)
                            tryToNeeds(a, false, assurer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void tryToNeeds(PostParseItem item, bool isLeft, AssureSplitStacks assurer)
        {

            switch (item.ItemType)
            {
                case PPItemType.Variable:
                    if (((PostParseVariable)item).Type == VariableType.Stack)
                    {
                        if (isLeft)
                            assurer.Resolve(((PostParseVariable)item).Index, ((PostParseVariable)item));
                        else
                            assurer.Need(((PostParseVariable)item).Index, ((PostParseVariable)item));

                    }
                    break;
                case PPItemType.String:
                    return;

                case PPItemType.Operation:

                    var op = ((PostParseOperation)item);
                    tryToNeeds(op.Left, false, assurer);
                    tryToNeeds(op.Right, false, assurer);

                    break;
                case PPItemType.Statement:
                    var st = ((PostParseStatement)item);
                    foreach (var postParseItem in st.Items)
                    {
                        tryToNeeds(postParseItem, false, assurer);
                    }
                    break;
                case PPItemType.Method:
                    var wst = ((PostParseMethod)item);
                    foreach (var a in wst.Params)
                        tryToNeeds(a, false, assurer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return;


            ;
        }

        private IEnumerable<int> findGoto(List<PostParseExpression> items, string labelString)
        {
            for (int index = items.Count - 1; index >= 0; index--)
            {
                var postParseExpression = items[index];
                if (postParseExpression is PostParseString && ((PostParseString)postParseExpression).GotoString == labelString)
                {
                    yield return index;
                }
            }
        }


        private void doThings(List<PostParseExpression> item2)
        {
            Dictionary<int, PostParseItem> stacks = new Dictionary<int, PostParseItem>();
            for (int index = 0; index < item2.Count; index++)
            {
                PostParseExpression postParseExpression = item2[index];
                switch (postParseExpression.ExpressionType)
                {
                    case PPExpressionType.Statement:
                        var st = ((PostParseStatement)postParseExpression);
                        item2[index] = new PostParseString(string.Format(st.Wrap, st.Items.Select(a => tryToFix(a, stacks)).ToArray()));
                        break;
                    case PPExpressionType.Set:
                        if (((PostParseSet)postParseExpression).Left is PostParseVariable)
                        {
                            var pps = (PostParseVariable)((PostParseSet)postParseExpression).Left;
                            switch (pps.Type)
                            {
                                case VariableType.Stack:

                                    if (!((PostParseSet)postParseExpression).OkayToRemove)
                                    {
                                        ((PostParseSet)postParseExpression).Right =
                                            tryToFix(((PostParseSet)postParseExpression).Right, stacks);
                                        ((PostParseSet)postParseExpression).MarkForRemoval = false;
                                        if (stacks.ContainsKey(pps.Index))
                                            stacks.Remove(pps.Index);
                                        string gc;
                                        stacks.Add(pps.Index, new PostParseString(gc = GetNext()));
                                        ((PostParseSet)postParseExpression).Left = new PostParseString("var " + gc);
                                    }
                                    else
                                    {

                                        var fm = tryToFix(((PostParseSet)postParseExpression).Right, stacks);

                                        if (stacks.ContainsKey(pps.Index))
                                            stacks.Remove(pps.Index);
                                        stacks.Add(pps.Index, fm);
                                        ((PostParseSet)postParseExpression).Right = fm;
                                        postParseExpression.MarkForRemoval = true;

                                        if (((PostParseSet)postParseExpression).RemoveLeft)
                                        {
                                            ((PostParseExpression)fm).endOfLine = true;
                                            item2[index] = (PostParseExpression)fm;
                                        }

                                    }

                                    break;
                                case VariableType.Special:
                                    ((PostParseSet)postParseExpression).Right = tryToFix(((PostParseSet)postParseExpression).Right, stacks);
                                    break;
                                case VariableType.Variable: ((PostParseSet)postParseExpression).Right = tryToFix(((PostParseSet)postParseExpression).Right, stacks);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                        }
                        break;
                    case PPExpressionType.String:
                        break;
                    case PPExpressionType.Method:
                        var wst = ((PostParseMethod)postParseExpression);
                        PostParseItem[] itm = new PostParseItem[wst.Params.Length];
                        for (int i = 0; i < wst.Params.Length; i++)
                        {
                            itm[i] = tryToFix(wst.Params[i], stacks);
                        }
                        wst.Params = itm;

                        //  item2[index] = new PostParseString(wst.ToString());

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private PostParseItem tryToFix(PostParseItem right, Dictionary<int, PostParseItem> stacks)
        {
            switch (right.ItemType)
            {
                case PPItemType.Variable:
                    if (((PostParseVariable)right).Type == VariableType.Stack)
                    {
                        return stacks[((PostParseVariable)right).Index];
                    }
                    return right;
                    break;
                case PPItemType.String:
                    return right;
                    break;
                case PPItemType.Operation:

                    var op = ((PostParseOperation)right);
                    return new PostParseString(string.Format(op.Wrap, tryToFix(op.Left, stacks), tryToFix(op.Right, stacks)));
                    break;
                case PPItemType.Statement:
                    var st = ((PostParseStatement)right);
                    return new PostParseString(string.Format(st.Wrap, st.Items.Select(a => tryToFix(a, stacks)).ToArray()));
                    break;
                case PPItemType.Method:
                    var wst = ((PostParseMethod)right);
                    PostParseItem[] itm = new PostParseItem[wst.Params.Length];
                    for (int index = 0; index < wst.Params.Length; index++)
                    {
                        itm[index] = tryToFix(wst.Params[index], stacks);
                    }
                    wst.Params = itm;
                    return wst;
                    // return new PostParseString(right.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void Cleanup(Tuple<Command,TempFileCollection> fs)
        {
            fs.Item2.Delete();
        }

        private Tuple<Command,TempFileCollection> compile(string name,string source)
        {
            CSharpCodeProvider csp = new CSharpCodeProvider();
            ICodeCompiler cc = csp.CreateCompiler();
            
            CompilerParameters cp = new CompilerParameters();
            cp.IncludeDebugInformation = true;
            cp.TempFiles.KeepFiles = true;
            cp.OutputAssembly = "C:\\fffs\\" +name+ ".dll";
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            cp.ReferencedAssemblies.Add("System.Xml.dll");
            cp.ReferencedAssemblies.Add("mscorlib.dll");
            var j = Directory.GetCurrentDirectory();
            cp.ReferencedAssemblies.Add(j+@"\ConsoleApplication1.exe");

            cp.WarningLevel = 0;

            cp.CompilerOptions = "/target:library /optimize";
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            //cp.IncludeDebugInformation = true;
            System.CodeDom.Compiler.TempFileCollection tfc = new TempFileCollection("C:\\fffs\\", false);
            
            CompilerResults cr = new CompilerResults(tfc);
            
            cr = cc.CompileAssemblyFromSource(cp, source);
            if (cr.Errors.Count > 0)
            {
                foreach (CompilerError ce in cr.Errors)
                {
                    Console.WriteLine(ce.ErrorNumber + ": " + ce.ErrorText);
                }
                Console.ReadLine();

                return null;
            }

            var fm= (Command)cr.CompiledAssembly.CreateInstance("ConsoleApplication1.RunClass");
            
            return new Tuple<Command, TempFileCollection>(fm,tfc);


        }


        private Tuple<string, List<PostParseExpression>> buildMethod(SpokeMethod fm)
        {
            StringBuilder sb = new StringBuilder();

#if stacktrace
            dfss.AppendLine( fm.Class.Name +" : : "+fm.MethodName+" Start");
#endif
            List<PostParseExpression> exps = new List<PostParseExpression>();

            for (int index = 0; index < fm.Instructions.Length; index++)
            {
                var ins = fm.Instructions[index];
#if stacktrace
                dfss.AppendLine(stackIndex + " ::  " + ins.ToString());
#endif
                int gj;
                PostParseVariable[] pps;
                switch (ins.Type)
                {
                    case SpokeInstructionType.CreateReference:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString(string.Format("new SpokeObject(new SpokeObject[{0}])", ins.Index)), false));

                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(new SpokeObject[{1}]);", ins.StackBefore_, ins.Index));
                        break;
                    case SpokeInstructionType.Comment:
                        exps.Add(new PostParseString(string.Format("/*{0}*/", ins.StringVal)));
                        sb.AppendLine(string.Format("/*{0}*/", ins.StringVal));
                        break;
                    case SpokeInstructionType.CreateArray:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString(string.Format("new SpokeObject(new List<SpokeObject>(20))")), false));

                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(new List<SpokeObject>(20) );", ins.StackBefore_));
                        break;
                    case SpokeInstructionType.CreateMethod:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString(string.Format("NULL")), false));
                        sb.AppendLine(string.Format("stack[{0}]=NULL;", ins.StackBefore_));
                        break;
                    case SpokeInstructionType.Label:
                        exps.Add(new PostParseString(ins.labelGuy + ":"));
                        sb.AppendLine(string.Format("{0}:", ins.labelGuy));
                        break;
                    case SpokeInstructionType.Goto:
                        exps.Add(new PostParseString("goto " + ins.gotoGuy + ";"));
                        sb.AppendLine(string.Format("goto {0};", ins.gotoGuy));
                        break;
                    case SpokeInstructionType.CallMethod:

                        pps = new PostParseVariable[ins.Index3];
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                            pps[i] = new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1 - i);

                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackAfter_ - 1), new PostParseMethod(Methods[ins.Index].Class.Name + Methods[ins.Index].CleanMethodName, pps.Reverse().ToArray(), ins.Index3), true));


                        sb.AppendLine(string.Format("sps = new SpokeObject[{0}];", Methods[ins.Index].NumOfVars));
                        gj = ins.StackBefore_ - 1;
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                        {
                            sb.AppendLine(string.Format("sps[{1}] = stack[{0}];", gj--, i));
                        }
                        sb.AppendLine(string.Format("stack[{0}] = {1}(sps);", ins.StackAfter_ - 1, Methods[ins.Index].Class.Name + Methods[ins.Index].CleanMethodName));


                        break;
                    case SpokeInstructionType.CallMethodFunc:

                        pps = new PostParseVariable[ins.Index3];
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                        {
                            pps[i] = new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1 - i);
                        }
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackAfter_ - 1), new PostParseMethod(string.Format("Methods[{0}].MethodFunc", ins.Index), pps.Reverse().ToArray(), ins.Index3), true));


                        sb.AppendLine(string.Format("sps = new SpokeObject[{0}];", ins.Index3));
                        gj = ins.StackBefore_ - 1;
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                        {
                            sb.AppendLine(string.Format("sps[{1}] = stack[{0}];", gj--, i));
                        }
                        sb.AppendLine(string.Format("stack[{0}] = Methods[{1}].MethodFunc(sps);", ins.StackAfter_ - 1, ins.Index));
                        break;
                    case SpokeInstructionType.CallInternal:

                        pps = new PostParseVariable[ins.Index3];
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                        {
                            pps[i] = new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1 - i);
                        }




                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackAfter_ - 1), new PostParseMethod(string.Format("InternalMethods[{0}]", ins.Index), pps.Reverse().ToArray(), ins.Index3), true));

                        sb.AppendLine(string.Format("sps = new SpokeObject[{0}];", ins.Index3));
                        gj = ins.StackBefore_ - 1;
                        for (int i = ins.Index3 - 1; i >= 0; i--)
                        {
                            sb.AppendLine(string.Format("sps[{1}] = stack[{0}];", gj--, i));
                        }

                        sb.AppendLine(string.Format("stack[{0}] = InternalMethods[{1}](sps);", ins.StackAfter_ - 1, ins.Index));

                        break;
                    case SpokeInstructionType.BreakpointInstruction:
                        Console.WriteLine("BreakPoint");
                        break;
                    case SpokeInstructionType.Return:
                        exps.Add(new PostParseStatement("return {0};", new PostParseVariable(VariableType.Stack, ins.StackBefore_)));
                        sb.AppendLine(string.Format("return stack[{0}];", ins.StackBefore_));
                        break;
                    case SpokeInstructionType.IfTrueContinueElse:
                        exps.Add(new PostParseStatement("if(!({0}).BoolVal) ", new PostParseVariable(VariableType.Stack, ins.StackBefore_)));
                        exps.Add(new PostParseString("goto " + ins.elseGuy + ";"));

                        sb.AppendLine(string.Format("if(!stack[{0}].BoolVal) goto {1};", ins.StackBefore_, ins.elseGuy));


                        break;
                    case SpokeInstructionType.Or:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                                                  new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                                                                         new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                                                                         "(({0}.BoolVal || {1}.BoolVal)?TRUE:FALSE)"), true));
                        sb.AppendLine(string.Format("stack[{0}] = (stack[{0}].BoolVal || stack[{1}].BoolVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackBefore_));
                        break;
                    case SpokeInstructionType.And:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                                                 new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                                                 "(({0}.BoolVal && {1}.BoolVal)?TRUE:FALSE)"), true));


                        sb.AppendLine(string.Format("stack[{0}] = (stack[{0}].BoolVal && stack[{1}].BoolVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackBefore_));
                        break;
                    case SpokeInstructionType.StoreLocalInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Variable, ins.Index), new PostParseStatement("intCache({0}.IntVal)", new PostParseVariable(VariableType.Stack, ins.StackBefore_)), true));

                        sb.AppendLine(string.Format("variables[{0}] = intCache(stack[{1}].IntVal);", ins.Index, ins.StackBefore_));
                        break;
                    case SpokeInstructionType.StoreLocalFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Variable, ins.Index),
                            new PostParseStatement("new SpokeObject({0}.FloatVal)", new PostParseVariable(VariableType.Stack, ins.StackBefore_)), true));

                        sb.AppendLine(string.Format("variables[{0}] = new SpokeObject(stack[{1}].FloatVal);", ins.Index, ins.StackBefore_));
                        break;
                    case SpokeInstructionType.StoreLocalBool:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Variable, ins.Index),
                            new PostParseStatement("new SpokeObject({0}.BoolVal)", new PostParseVariable(VariableType.Stack, ins.StackBefore_)), true));

                        sb.AppendLine(string.Format("variables[{0}] = stack[{1}].BoolVal ? TRUE : FALSE;", ins.Index, ins.StackBefore_));


                        //      variables[ins.Index] = lastStack.BoolVal ? TRUE : FALSE;
                        break;
                    case SpokeInstructionType.StoreLocalString:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Variable, ins.Index), new PostParseStatement("new SpokeObject({0}.StringVal)", new PostParseVariable(VariableType.Stack, ins.StackBefore_)), true));

                        sb.AppendLine(string.Format("variables[{0}] = new SpokeObject(stack[{1}].StringVal);", ins.Index, ins.StackBefore_));
                        break;

                    case SpokeInstructionType.StoreLocalMethod:
                    case SpokeInstructionType.StoreLocalObject:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Variable, ins.Index), new PostParseVariable(VariableType.Stack, ins.StackBefore_), true));

                        sb.AppendLine(string.Format("variables[{0}] = stack[{1}];", ins.Index, ins.StackBefore_));

                        break;
                    case SpokeInstructionType.StoreLocalRef:
                        exps.Add(
                            new PostParseStatement(
                                @"

//{1}={0};

lastStack = {0};
bm2 = {1};
bm2.Variables = lastStack.Variables;
bm2.ArrayItems = lastStack.ArrayItems;
bm2.StringVal = lastStack.StringVal;
bm2.IntVal = lastStack.IntVal;
bm2.BoolVal = lastStack.BoolVal;
bm2.FloatVal = lastStack.FloatVal;
",
                                new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseVariable(VariableType.Variable, ins.Index)));


                        sb.AppendLine(string.Format("lastStack = stack[{0}];", ins.StackBefore_));
                        sb.AppendLine(string.Format("bm = variables[{0}];", ins.Index));
                        sb.AppendLine(@"
bm.Variables = lastStack.Variables;
bm.ArrayItems = lastStack.ArrayItems;
bm.StringVal = lastStack.StringVal;
bm.IntVal = lastStack.IntVal;
bm.BoolVal = lastStack.BoolVal;
bm.FloatVal = lastStack.FloatVal;
");


                        break;

                    case SpokeInstructionType.StoreFieldBool:

                        exps.Add(new PostParseStatement("{0}.Variables[" + ins.Index + "]={1}.BoolVal?TRUE:FALSE;", new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1)));




                        sb.AppendLine(string.Format("stack[{0}].Variables[{2}]=stack[{1}].BoolVal?TRUE:FALSE;", ins.StackBefore_, ins.StackAfter_, ins.Index));
                        break;
                    case SpokeInstructionType.StoreFieldInt:
                        exps.Add(new PostParseStatement("{0}.Variables[" + ins.Index + "]=intCache({1}.IntVal);", new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1)));


                        sb.AppendLine(string.Format("stack[{0}].Variables[{2}]=intCache(stack[{1}].IntVal );", ins.StackBefore_, ins.StackAfter_, ins.Index));
                        break;
                    case SpokeInstructionType.StoreFieldFloat:
                        exps.Add(new PostParseStatement("{0}.Variables[" + ins.Index + "]=new SpokeObject({1}.FloatVal);", new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1)));


                        sb.AppendLine(string.Format("stack[{0}].Variables[{2}]=new SpokeObject( stack[{1}].FloatVal );", ins.StackBefore_, ins.StackAfter_, ins.Index));
                        break;
                    case SpokeInstructionType.StoreFieldString:
                        exps.Add(new PostParseStatement("{0}.Variables[" + ins.Index + "]=new SpokeObject({1}.StringVal);", new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1)));

                        sb.AppendLine(string.Format("stack[{0}].Variables[{2}]=new SpokeObject(stack[{1}].StringVal );", ins.StackBefore_, ins.StackAfter_, ins.Index));
                        break;
                    case SpokeInstructionType.StoreFieldMethod:
                    case SpokeInstructionType.StoreFieldObject:

                        exps.Add(new PostParseStatement("{0}.Variables[" + ins.Index + "]={1};", new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1)));



                        sb.AppendLine(string.Format("stack[{0}].Variables[{2}]=stack[{1}];", ins.StackBefore_, ins.StackAfter_, ins.Index));
                        break;
                    case SpokeInstructionType.StoreToReference:



                        exps.Add(new PostParseStatement("{0}.Variables[" + ins.Index + "] = {1};", new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackBefore_)));





                        sb.AppendLine(string.Format("stack[{1}].Variables[{2}] = stack[{0}];", ins.StackBefore_, ins.StackBefore_ - 1, ins.Index));
                        break;
                    case SpokeInstructionType.GetField:

                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseStatement("{0}.Variables[" + ins.Index + "]", new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1)), true));

                        sb.AppendLine(string.Format("stack[{0}] = stack[{0}].Variables[{1}];", ins.StackBefore_ - 1, ins.Index));
                        break;
                    case SpokeInstructionType.GetLocal:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseVariable(VariableType.Variable, ins.Index), true));

                        sb.AppendLine(string.Format("stack[{0}] = variables[{1}];", ins.StackBefore_, ins.Index));
                        break;
                    case SpokeInstructionType.PopStack:
                        var d = (PostParseSet)exps.Last(a => a.ExpressionType == PPExpressionType.Set);

                        if (d.Right.ItemType == PPItemType.Variable)
                        {
                            d.MarkForRemoval = true;
                        }
                        else d.RemoveLeft = true;

                        break;
                    case SpokeInstructionType.Not:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseStatement("{0}.BoolVal?FALSE:TRUE", new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1)), true));
                        sb.AppendLine(string.Format("stack[{0}] = stack[{0}].BoolVal?FALSE:TRUE;", ins.StackBefore_ - 1));
                        break;
                    case SpokeInstructionType.IntConstant:
                        if (ins.Index >= 0 && ins.Index < 100)
                        {
                            exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString(string.Format("ints[{0}]", ins.Index)), true));
                            sb.AppendLine(string.Format("stack[{0}] = ints[{1}];", ins.StackBefore_, ins.Index));
                        }
                        else
                        {
                            exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString(string.Format("new SpokeObject( {0} )", ins.Index)), true));
                            sb.AppendLine(string.Format("stack[{0}] = new SpokeObject( {1} );", ins.StackBefore_, ins.Index));
                        }
                        break;
                    case SpokeInstructionType.BoolConstant:
                        sb.AppendLine(string.Format("stack[{0}] = {1};", ins.StackBefore_, ins.BoolVal ? "TRUE" : "FALSE"));
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString(ins.BoolVal ? "TRUE" : "FALSE"), true));

                        break;
                    case SpokeInstructionType.FloatConstant:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString(string.Format("new SpokeObject( {0}f )", ins.FloatVal)), true));

                        sb.AppendLine(string.Format("stack[{0}] = new SpokeObject({1}f );", ins.StackBefore_, ins.FloatVal));
                        break;
                    case SpokeInstructionType.StringConstant:

                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString(string.Format("new SpokeObject( \"{0}\" )", ins.StringVal)), true));
                        sb.AppendLine(string.Format("stack[{0}] = new SpokeObject( \"{1}\" );", ins.StackBefore_, ins.StringVal));
                        break;

                    case SpokeInstructionType.Null:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_), new PostParseString("NULL"), true));
                        sb.AppendLine(string.Format("stack[{0}] = NULL;", ins.StackBefore_));
                        break;
                    case SpokeInstructionType.AddIntInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "intCache({0}.IntVal + {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=intCache(stack[{0}].IntVal + stack[{1}].IntVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.AddStringInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.StringVal + {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject( stack[{0}].StringVal + stack[{1}].IntVal );", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.AddIntString:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.IntVal + {1}.StringVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].IntVal + stack[{1}].StringVal );", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.AddFloatString:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal + {1}.StringVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject( stack[{0}].FloatString + stack[{1}].StringVal );", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.AddStringFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.StringVal + {1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].StringVal + stack[{1}].FloatVal );", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.AddStringString:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.StringVal + {1}.StringVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].StringVal + stack[{1}].StringVal );", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.SubtractIntInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "intCache({0}.IntVal - {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=intCache (stack[{0}].IntVal - stack[{1}].IntVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.MultiplyIntInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "intCache({0}.IntVal * {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=intCache (stack[{0}].IntVal * stack[{1}].IntVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.DivideIntInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "intCache({0}.IntVal / {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=intCache (stack[{0}].IntVal / stack[{1}].IntVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.GreaterIntInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackBefore_), "(({0}.IntVal > {1}.IntVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].IntVal > stack[{1}].IntVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.GreaterIntFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackBefore_), "(({0}.IntVal > {1}.FloatVal)?TRUE:FALSE)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].IntVal > stack[{1}].FloatVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.GreaterFloatInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackBefore_), "(({0}.FloatVal > {1}.IntVal)?TRUE:FALSE)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].FloatVal > stack[{1}].IntVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.GreaterFloatFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackBefore_), "(({0}.FloatVal > {1}.FloatVal)?TRUE:FALSE)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].FloatVal > stack[{1}].FloatVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.LessIntInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.IntVal < {1}.IntVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].IntVal < stack[{1}].IntVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.LessIntFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.IntVal < {1}.FloatVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].IntVal < stack[{1}].FloatVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.LessFloatInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.FloatVal < {1}.IntVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].FloatVal < stack[{1}].IntVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.LessFloatFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.FloatVal < {1}.FloatVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].FloatVal < stack[{1}].FloatVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.GreaterEqualIntInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.IntVal >= {1}.IntVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].IntVal >= stack[{1}].IntVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.GreaterEqualIntFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.IntVal >= {1}.FloatVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].IntVal >= stack[{1}].FloatVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.GreaterEqualFloatInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.FloatVal >= {1}.IntVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].FloatVal >= stack[{1}].IntVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.GreaterEqualFloatFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.FloatVal >= {1}.FloatVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].FloatVal >= stack[{1}].FloatVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.LessEqualIntInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.IntVal <= {1}.IntVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].IntVal <= stack[{1}].IntVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.LessEqualIntFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.IntVal <= {1}.FloatVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].IntVal <= stack[{1}].FloatVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.LessEqualFloatInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "(({0}.FloatVal <= {1}.IntVal)?TRUE:FALSE)"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].FloatVal <= stack[{1}].IntVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.LessEqualFloatFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                          new PostParseOperation(
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                              new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                              "({0}.FloatVal <= {1}.FloatVal)?TRUE:FALSE"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].FloatVal <= stack[{1}].FloatVal)?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.Equal:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                                                  new PostParseOperation(
                                                      new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1),
                                                      new PostParseVariable(VariableType.Stack, ins.StackBefore_),
                                                      "(({0}.Compare({1})?TRUE:FALSE))"), true));

                        sb.AppendLine(string.Format("stack[{0}]=(stack[{0}].Compare(stack[{1}]))?TRUE:FALSE;", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;

                    case SpokeInstructionType.AddToArray:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackAfter_ - 1),
                                                  new PostParseStatement("{0}.AddArray({1})",
                                                                         new PostParseVariable(VariableType.Stack, ins.StackAfter_ - 1),
                                                                         new PostParseVariable(VariableType.Stack, ins.StackAfter_)), true));

                        sb.AppendLine(string.Format("stack[{0}].AddArray(stack[{1}]);", ins.StackAfter_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.AddRangeToArray:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackAfter_ - 1), new PostParseStatement("{0}.AddArrayRange({1})",
                                                                                                                            new PostParseVariable(VariableType.Stack, ins.StackAfter_ - 1),
                                                                                                                            new PostParseVariable(VariableType.Stack, ins.StackAfter_)), true));

                        sb.AppendLine(string.Format("stack[{0}].AddArrayRange(stack[{1}]);", ins.StackAfter_ - 1, ins.StackAfter_));
                        break;

                    case SpokeInstructionType.ArrayElem:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseStatement("{0}.ArrayItems[{1}.IntVal]", new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackBefore_)), true));
                        sb.AppendLine(string.Format("stack[{0}]=stack[{0}].ArrayItems[stack[{1}].IntVal];", ins.StackBefore_ - 1, ins.StackBefore_));

                        break;
                    case SpokeInstructionType.StoreArrayElem:

                        exps.Add(new PostParseStatement("{0}.ArrayItems[{1}.IntVal] = {2};", new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 2), new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 3)));

                        sb.AppendLine(string.Format("stack[{1}].ArrayItems[stack[{0}].IntVal] = stack[{2}];", ins.StackBefore_ - 1, ins.StackBefore_ - 2, ins.StackBefore_ - 3));

                        break;


                    case SpokeInstructionType.AddIntFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.IntVal + {1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].IntVal + stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_));

                        break;
                    case SpokeInstructionType.AddFloatInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal + {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].IntVal + stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.AddFloatFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal + {1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].FloatVal + stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.SubtractIntFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.IntVal - {1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].IntVal + stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.SubtractFloatInt:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal - {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].FloatVal -stack[{1}].IntVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.SubtractFloatFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal - {1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].FloatVal - stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.MultiplyIntFloat:

                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.IntVal * {1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].IntVal * stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_)); break;
                    case SpokeInstructionType.MultiplyFloatInt:

                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal * {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].FloatVal * stack[{1}].IntVal);", ins.StackBefore_ - 1, ins.StackAfter_)); break;
                    case SpokeInstructionType.MultiplyFloatFloat:
                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal *{1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].FloatVal + stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_));
                        break;
                    case SpokeInstructionType.DivideIntFloat:

                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.IntVal / {1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].IntVal / stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_)); break;
                    case SpokeInstructionType.DivideFloatInt:

                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal / {1}.IntVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].FloatVal / stack[{1}].IntVal);", ins.StackBefore_ - 1, ins.StackAfter_)); break;
                    case SpokeInstructionType.DivideFloatFloat:

                        exps.Add(new PostParseSet(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseOperation(new PostParseVariable(VariableType.Stack, ins.StackBefore_ - 1), new PostParseVariable(VariableType.Stack, ins.StackAfter_), "new SpokeObject({0}.FloatVal / {1}.FloatVal)"), true));
                        sb.AppendLine(string.Format("stack[{0}]=new SpokeObject(stack[{0}].FloatVal / stack[{1}].FloatVal);", ins.StackBefore_ - 1, ins.StackAfter_)); break;


                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new Tuple<string, List<PostParseExpression>>(sb.ToString(), exps);


        }

    }

    public interface Command
    {
        SpokeObject Run();
        void loadUp(Func<SpokeObject[], SpokeObject>[] internalMethods, SpokeMethod[] mets);

    }
}