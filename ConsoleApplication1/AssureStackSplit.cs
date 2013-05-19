using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostParse;

namespace ConsoleApplication1
{
    public class AssureSplitStacks
    {
        private readonly List<PostParseExpression> myWholeList;
        private List<PostParseVariable>[] Items = new List<PostParseVariable>[1024];
        public Dictionary<int, Tuple<int, List<PostParseVariable>>> Needs = new Dictionary<int, Tuple<int, List<PostParseVariable>>>();

        public AssureSplitStacks(List<PostParseExpression> wholeList)
        {
            myWholeList = wholeList;
            for (int i = 0; i < 1024; i++)
            {
                Items[i] = new List<PostParseVariable>();
            }
        }

        private static int height = 0;

        public int NumberOfSpecVariables
        {
            get
            {
                height++;
                if (height > 2000)
                {
                    return 0;
                }
                if (possibleParent == null)
                {
                    height = 0;
                    return numOfSpec;
                }
                if (possibleParent == this)
                {
                    height = 0;
                    return numOfSpec;
                }
                return possibleParent.NumberOfSpecVariables;
            }
            set
            {

                if (possibleParent == null)
                {
                    numOfSpec = value;
                }
                else if (possibleParent == this)
                {
                    numOfSpec = value;
                }
                else possibleParent.NumberOfSpecVariables = value;
            }
        }

        private AssureSplitStacks possibleParent;

        private int numOfSpec;

        public void Resolve(int index, PostParseVariable left)
        {
            Tuple<int, List<PostParseVariable>> item;
            if (Needs.TryGetValue(index, out item))
            {
                Needs.Remove(index);

                left.Type = VariableType.Variable;
                left.Index = item.Item1;

                foreach (PostParseVariable postParseItem in item.Item2)
                {
                    postParseItem.Type = VariableType.Variable;
                    postParseItem.Index = item.Item1;
                }
                foreach (var postParseVariable in Items[index])
                {
                    postParseVariable.Type = VariableType.Variable;
                    postParseVariable.Index = item.Item1;

                }
            }

            else if (!Items[left.Index].Any())
            {
                left.Destroy = true;
            }

            Items[left.Index] = new List<PostParseVariable>() { };

        }
        public void Need(int index, PostParseVariable right)
        {
            Items[right.Index].Add(right);
        }

        public AssureSplitStacks GetSplit()
        {
            var d = new AssureSplitStacks(myWholeList);
            d.possibleParent = topParent(this);
            d.Needs = new Dictionary<int, Tuple<int, List<PostParseVariable>>>();

            for (int index = 0; index < Items.Length; index++)
            {
                var postParseItem = Items[index];
                if (postParseItem.Any())
                {

                    d.Needs.Add(index, new Tuple<int, List<PostParseVariable>>(getHighestVariableIndex()+1+index, postParseItem));
                }
            }
            return d;
        }

        private AssureSplitStacks topParent(AssureSplitStacks assureSplitStacks)
        {
            if (assureSplitStacks.possibleParent == null)
            {
                return assureSplitStacks;
            }
            return topParent(assureSplitStacks.possibleParent);
        }

        public int getHighestVariableIndex()
        {
            var d = getAllVariables(myWholeList).Where(a => a.Type == VariableType.Variable).OrderBy(a => a.Index).Last().Index;
            return d;
        }

        public static IEnumerable<PostParseVariable> getAllVariables(List<PostParseExpression> wholeList) {
            for (int index = 0; index < wholeList.Count; index++) {
                var postParseExpression = wholeList[index];
                switch (postParseExpression.ExpressionType) {
                    case PPExpressionType.Statement:
                        foreach (var parseExpression in ((PostParseStatement) postParseExpression).Items) {
                            foreach (var expression in getItem(parseExpression)) {
                                yield return expression;
                            }
                        }
                        break;
                    case PPExpressionType.Set:

                        foreach (var postParseItem in getItem(((PostParseSet) postParseExpression).Left))
                            yield return postParseItem;
                        foreach (var postParseItem in getItem(((PostParseSet) postParseExpression).Right))
                            yield return postParseItem;
                        break;
                    case PPExpressionType.String:

                        break;
                    case PPExpressionType.Method:
                        foreach (var p in ((PostParseMethod) postParseExpression).Params) {
                            foreach (var postParseMethod in getItem(p)) {
                                yield return postParseMethod;
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static IEnumerable<PostParseVariable> getItem(PostParseItem parseExpression)
        {
            switch (parseExpression.ItemType)
            {
                case PPItemType.Variable:
                    yield return (PostParseVariable)parseExpression;
                    break;
                case PPItemType.String:
                    break;
                case PPItemType.Operation:
                    foreach (var d in getItem(((PostParseOperation)parseExpression).Left)) yield return d;
                    foreach (var d in getItem(((PostParseOperation)parseExpression).Right)) yield return d;
                    break;
                case PPItemType.Statement:
                    foreach (var d in ((PostParseStatement)parseExpression).Items) foreach (var postParseItem in getItem(d)) yield return postParseItem;

                    break;
                case PPItemType.Method:
                    foreach (var p in ((PostParseMethod)parseExpression).Params)
                    {
                        foreach (var postParseMethod in getItem(p))
                        {
                            yield return postParseMethod;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }







        public static IEnumerable<PostParseMethod> getAllMethods(List<PostParseExpression> wholeList)
        {

            foreach (var postParseExpression in wholeList)
            {
                switch (postParseExpression.ExpressionType)
                {
                    case PPExpressionType.Statement:
                        foreach (var parseExpression in ((PostParseStatement)postParseExpression).Items)
                        {
                            foreach (var expression in getMethodItem(parseExpression))
                            {
                                yield return expression;
                            }
                        }
                        break;
                    case PPExpressionType.Set:

                        foreach (var postParseItem in getMethodItem(((PostParseSet)postParseExpression).Left))
                            yield return postParseItem;
                        foreach (var postParseItem in getMethodItem(((PostParseSet)postParseExpression).Right))
                            yield return postParseItem;
                        break;
                    case PPExpressionType.String:

                        break;
                    case PPExpressionType.Method:
                        foreach (var p in ((PostParseMethod)postParseExpression).Params)
                        {
                            foreach (var postParseMethod in getMethodItem(p))
                            {
                                yield return postParseMethod;
                            }
                        }

                        yield return ((PostParseMethod)postParseExpression);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static IEnumerable<PostParseMethod> getMethodItem(PostParseItem parseExpression)
        {
            switch (parseExpression.ItemType)
            {
                case PPItemType.Variable:

                    break;
                case PPItemType.String:
                    break;
                case PPItemType.Operation:
                    foreach (var d in getMethodItem(((PostParseOperation)parseExpression).Left)) yield return d;
                    foreach (var d in getMethodItem(((PostParseOperation)parseExpression).Right)) yield return d;
                    break;
                case PPItemType.Statement:
                    foreach (var d in ((PostParseStatement)parseExpression).Items) foreach (var postParseItem in getMethodItem(d)) yield return postParseItem;
                    break;
                case PPItemType.Method:
                    foreach (var p in ((PostParseMethod)parseExpression).Params)
                    {
                        foreach (var postParseMethod in getMethodItem(p))
                        {
                            yield return postParseMethod;
                        }
                    }


                    yield return ((PostParseMethod)parseExpression);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }




    }

}
