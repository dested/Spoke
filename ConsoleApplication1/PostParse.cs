using System;
using System.Linq;
using System.Text;

namespace PostParse
{

    public enum PPItemType
    {
        Variable, String, Operation,
        Statement,
        Method
    }
    public enum PPExpressionType
    {
        Statement, Set,
        String,
        Method
    }
    public interface PostParseItem
    {
        PPItemType ItemType { get; }
    }
    public interface PostParseExpression
    {
        bool MarkForRemoval { get; set; }
        PPExpressionType ExpressionType { get; }
        bool endOfLine { get; set; }
    }
    public class PostParseStatement : PostParseExpression, PostParseItem
    {
        public PostParseItem[] Items;
        public string Wrap;

        public PostParseStatement(string wrap, params PostParseItem[] items)
        {
            Items = items;
            Wrap = wrap;
        }

        public PPItemType ItemType
        {
            get { return PPItemType.Statement; }
        }

        public bool MarkForRemoval { get; set; }

        public PPExpressionType ExpressionType
        {
            get { return PPExpressionType.Statement; }
        }
        public override string ToString()
        {
            return string.Format(Wrap, Items.Select(a => a).ToArray())+(endOfLine?";":"");
        }

        public bool endOfLine { get; set; }

    }

    public class PostParseSet : PostParseExpression
    {
        public PostParseItem Left;

        public PostParseSet(PostParseItem left, PostParseItem right, bool okayToRemove)
        {
            Left = left;
            Right = right;
            OkayToRemove = okayToRemove;
        }

        public PostParseItem Right;
        public bool OkayToRemove;
        public bool RemoveLeft;
        public bool MarkForRemoval { get; set; }

        public PPExpressionType ExpressionType
        {
            get { return PPExpressionType.Set; }
        }
        public override string ToString()
        {
            if (Left is PostParseVariable && ((PostParseVariable)Left).Destroy)
            {
                return string.Format("{0};", Right);
            }
            return string.Format("{0} = {1};", Left, Right);
        }

        public bool endOfLine { get; set; }


    }

    public enum VariableType
    {
        Stack, Special, Variable
    }
    public class PostParseVariable : PostParseItem
    {
        public PostParseVariable(VariableType t, int index)
        {
            Type = t;
            Index = index;
        }

        public VariableType Type;
        public int Index;
        public bool Destroy
        {
            get
            {
                return Type != VariableType.Special && dest;
            }
            set { dest = value; }
        }

        private bool dest;

        public PPItemType ItemType
        {
            get { return PPItemType.Variable; }

        }

        public override string ToString()
        {
            switch (Type)
            {
                case VariableType.Stack:
                    return string.Format("stack[{0}]", Index);
                    break;
                case VariableType.Special:
                    return string.Format("specVariables[{0}]", Index);
                    break;
                case VariableType.Variable:
                    return string.Format("variables[{0}]", Index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public string getChar(int index)
        {
            string cc = "v";

            while (index >= 0)
            {
                cc += ((char)('a' + ((char)(index % 26))));
                index -= 26;
            }
            return cc + "_";
        }

    }
    public class PostParseMethod : PostParseItem, PostParseExpression
    {
        public string TotalMethodName;
        public int NumOfVariables;
        public PostParseItem[] Params;

        public PPItemType ItemType
        {
            get { return PPItemType.Method; }
        }

        public bool MarkForRemoval { get; set; }

        public PostParseMethod(string cleanMethodName, PostParseItem[] @params, int numofv)
        {
            TotalMethodName = cleanMethodName;
            Params = @params;
            NumOfVariables = numofv;
        }

        public PPExpressionType ExpressionType
        {
            get { return PPExpressionType.Method; }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(TotalMethodName);
            sb.Append("(new SpokeObject[]{");
            foreach (var postParseItem in Params)
            {
                sb.Append(postParseItem + ",");
            }
            for (int i = Params.Count(); i < NumOfVariables; i++)
            {
                sb.Append("null,");
            }


            return sb.ToString().TrimEnd(',') + "})" + (endOfLine ? ";" : "");
        }

        public bool endOfLine { get; set; }

    }

    public class PostParseString : PostParseItem, PostParseExpression
    {
        public string Value;
        public string GotoString
        {
            get
            {
                if (Value.StartsWith("goto"))
                {
                    return Value.Replace("goto ", "").TrimEnd(';');
                }
                return null;
            }
        }
        public string LabelString
        {
            get
            {
                if (Value.EndsWith(":"))
                {
                    return Value.TrimEnd(':');
                }
                return null;
            }

        }

        public PostParseString(string value)
        {
            Value = value;
        }

        public bool MarkForRemoval { get; set; }

        public PPItemType ItemType
        {
            get { return PPItemType.String; }
        }

        public PPExpressionType ExpressionType
        {
            get { return PPExpressionType.String; }
        }
        public override string ToString()
        {
            return Value + (endOfLine ? ";" : "");
        }

        public bool endOfLine { get; set; }


    }
    public class PostParseOperation : PostParseItem
    {
        public PostParseItem Left;
        public PostParseItem Right;
        public string Wrap;

        public PostParseOperation(PostParseItem left, PostParseItem right, string wrap)
        {
            Left = left;
            Right = right;
            Wrap = wrap;
        }

        public PPItemType ItemType
        {
            get { return PPItemType.Operation; }
        }
        public override string ToString()
        {
            return string.Format(Wrap, Left, Right);

        }

    }
}