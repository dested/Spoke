using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    public class PreparseInstructions
    {
        private SpokeMethod[] mets;

        public PreparseInstructions(SpokeMethod[] item1)
        {

            mets = item1;
             
            preparseInstructions(mets);

        }

        private void preparseInstructions(SpokeMethod[] mets)
        {
            foreach (var spokeMethod in mets)
            {
                Dictionary<string, int> labels = new Dictionary<string, int>();
                if (spokeMethod.Instructions == null)
                {
                    continue;
                }
                for (int index = 0; index < spokeMethod.Instructions.Length; index++)
                {
                    var spokeInstruction = spokeMethod.Instructions[index];
                    if (spokeInstruction.Type == SpokeInstructionType.Label)
                    {
                        labels.Add(spokeInstruction.labelGuy, index);
                    }

                }
                for (int index = 0; index < spokeMethod.Instructions.Length; index++)
                {
                    var spokeInstruction = spokeMethod.Instructions[index];

                    if (spokeInstruction.Type == SpokeInstructionType.Goto && spokeInstruction.gotoGuy != null)
                    {
                        spokeInstruction.Index = labels[spokeInstruction.gotoGuy];
                    }
                    if (spokeInstruction.Type == SpokeInstructionType.IfTrueContinueElse)
                    {
                        spokeInstruction.Index = labels[spokeInstruction.elseGuy];
                    }
                }
            }

            StringBuilder sbw = new StringBuilder();

            foreach (var spokeMethod in mets)
            {
                sbw.AppendLine("Method: " + spokeMethod.Class.Name + ":" + spokeMethod.MethodName);
                if (spokeMethod.Instructions == null)
                {
                    continue;
                }
                //sbw.AppendLine("Variables: ");
                //foreach (var spokeType in spokeMethod.VariableRefs.allVariables) {
                //    sbw.AppendLine(spokeType.Item1 + "(" + spokeType.Item2 + "): " + spokeType.Item3);
                //}
                //sbw.AppendLine();
                for (int index = 0; index < spokeMethod.Instructions.Length; index++)
                {
                    var spokeInstruction = spokeMethod.Instructions[index];

                    sbw.AppendLine(index + "\t\t\t" +
                                   spokeInstruction.ToString());
                }
            }
            File.WriteAllText("C:\\spokeins.txt", sbw.ToString());



            foreach (var spokeMethod in mets)
            {
                if (spokeMethod.Instructions != null)
                {
                    doit(0, 0, spokeMethod.Instructions);
                }
            }

            var d = indexes.GroupBy(a => a.Type).Select(a => a.Key).ToArray();


        }


        private List<SpokeInstruction> indexes = new List<SpokeInstruction>();
        private List<int> indexe2s = new List<int>();


        private void doit(int curStack, int index, SpokeInstruction[] ins)
        {


            for (; index < ins.Length; index++)
            {

                var spokeInstruction = ins[index];

                indexes.Insert(0, spokeInstruction);
                indexe2s.Insert(0, index);

                int stackBefore = spokeInstruction.StackBefore();
                curStack += stackBefore;

                if (curStack < 0)
                {
                    throw new AbandonedMutexException("Gay");
                }
                if (spokeInstruction.Type == SpokeInstructionType.Return)
                {
                    spokeInstruction.StackBefore_ = curStack;
                    spokeInstruction.StackAfter_ = curStack + spokeInstruction.StackAfter();
            
                    return;
                }


                if (spokeInstruction.Type == SpokeInstructionType.Goto)
                {
                    if (spokeInstruction.StackBefore_ > -1)
                    {
                        if (spokeInstruction.StackBefore_ != curStack)
                        {
                            throw new AbandonedMutexException("Theory gay");
                        }
                        return;
                    }
                    else
                    {
                        spokeInstruction.StackBefore_ = curStack;
                        if (spokeInstruction.Index > -1)
                        {
                            index = spokeInstruction.Index;
                        }
                    }
                }
                if (spokeInstruction.Type == SpokeInstructionType.IfTrueContinueElse)
                {
                    if (spokeInstruction.StackBefore_ > -1)
                    {
                        if (spokeInstruction.StackBefore_ != curStack)
                        {
                            throw new AbandonedMutexException("Theory gay");
                        }
                    }
                    else
                    {
                        spokeInstruction.StackBefore_ = curStack;

                        doit(curStack, index + 1, ins);
                        doit(curStack, spokeInstruction.Index + 1, ins);
                    }
                    return;
                }



 




                if (spokeInstruction.StackBefore_ == -1)
                {
                    spokeInstruction.StackBefore_ = curStack;
                    spokeInstruction.StackAfter_ = curStack + spokeInstruction.StackAfter();
                }
                else
                {
                    if (spokeInstruction.StackBefore_ != curStack)
                    {
                        throw new AbandonedMutexException("Theory gay");
                    }

                }
                curStack += spokeInstruction.StackAfter();
            }
        }


    }
}