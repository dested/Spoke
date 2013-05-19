using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    public interface SpokeLine : Spoke
    {
        ISpokeLine LType { get; }
    }
    public interface Spoke
    {

    }
    public interface SpokeLines
    {
        SpokeLine[] Lines { get; set; }
        string Guid { get; }
    }
    public interface SpokeItem : Spoke
    {
        ISpokeItem IType { get; }
        string Guid { get; }
    }
    public interface SpokeParent : SpokeItem
    {
        SpokeItem Parent { get; set; }
    }

    public enum ISpokeLine
    {
        If, Return, MethodCall, AnonMethod, Construct,
        Set,
        Yield,
        YieldReturn
    }
    public enum ISpokeItem
    {
        Array, Float, Int, Variable, AnonMethod, MethodCall, String, Construct, Addition, Subtraction, Multiplication, Division, Greater, Less, GreaterEqual, LessEqual, Equal, Expression,
        ArrayIndex,
        Current,
        Equality,
        Or,
        And,
        NotEqual,
        Null,
        Bool
    }
    public class SpokeIf : s,SpokeLine, SpokeLines
    {
        public SpokeItem Condition;
        public SpokeLine[] IfLines;
        public SpokeLine[] ElseLines;

        public ISpokeLine LType { get { return ISpokeLine.If; } }

        public SpokeLine[] Lines
        {
            get
            {
                SpokeLine[] lm = new SpokeLine[IfLines.Length + (ElseLines == null ? 0 : ElseLines.Length)];
                int o = 0;
                foreach (var spokeLine in IfLines)
                {
                    lm[o++] = spokeLine;
                }
                if (ElseLines != null)
                    foreach (var spokeLine in ElseLines)
                    {
                        lm[o++] = spokeLine;
                    }
                return lm;
            }
            set
            {

            }
        }


        public override string ToString()
        {
            return "if";
        }

    }

    public class SpokeReturn : s, SpokeLine
    {
        public SpokeItem Return;
        public ISpokeLine LType { get { return ISpokeLine.Return; } }
        public override string ToString()
        {
            return "Return ";
        }
    }
    public class SpokeYield : s, SpokeLine
    {
        public SpokeItem Yield;
        public ISpokeLine LType { get { return ISpokeLine.Yield; } }
    }
    public class s
    {
        public static int Index;
        private string guid = (Index++).ToString();
        public string Guid
        {
            get { return guid; }
        }
        public void resetGUID() {
            guid = (Index++).ToString();
        }
    }
    public class SpokeNull : s, SpokeItem
    {
        public ISpokeItem IType
        {
            get { return ISpokeItem.Null; }
        }
        public override string ToString()
        {
            return "Null";
        }
    }

    public class SpokeYieldReturn : SpokeLine
    {
        public SpokeItem YieldReturn;
        public ISpokeLine LType { get { return ISpokeLine.YieldReturn; } }
    }
    public class SpokeMethodCall : s, SpokeParent, SpokeLine, SpokeItem
    {
        public SpokeItem Parent { get; set; }
        public SpokeItem[] Parameters;


        public ISpokeLine LType
        {
            get { return ISpokeLine.MethodCall; }
        }

        public ISpokeItem IType
        {
            get { return ISpokeItem.MethodCall; }
        }

        public override string ToString()
        {
            return Parent.ToString();
        }
    }
    public class SpokeArrayIndex : s, SpokeItem, SpokeParent
    {
        public bool ForSet;
        public SpokeItem Parent { get; set; }
        public SpokeItem Index { get; set; }

        public ISpokeItem IType
        {
            get { return ISpokeItem.ArrayIndex; }
        }

        public override string ToString() {
            return Parent + "[" + Index + "]";
        }

    }
    public class SpokeArray : s, SpokeItem
    {
        public SpokeItem[] Parameters;

        public ISpokeItem IType
        {
            get { return ISpokeItem.Array; }
        }
        public override string ToString() {
            return "[" + Parameters.Aggregate("", (a, b) => a + b + ", ") + "]";
        }
    }
    public class SpokeFloat : s, SpokeItem
    {
        public float Value;

        public ISpokeItem IType
        {
            get { return ISpokeItem.Float; }
        }
        public override string ToString()
        {
            return Value.ToString();
        }

    }
    public class SpokeCurrent : s, SpokeItem
    {
        public ISpokeItem IType
        {
            get { return ISpokeItem.Current; }
        }
        public override string ToString()
        {
            return "this";
        }

    }

    public class SpokeInt : s, SpokeItem
    {
        public int Value;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Int; }
        }
        public override string ToString()
        {
            return Value.ToString();
        }

    }

    public enum SpokeVType { V,MethodName, ThisV,
        InternalMethodName
    }
    public class SpokeVariable : s, SpokeParent, SpokeItem
    {
        public SpokeItem Parent { get; set; }
        public string VariableName;
        public int VariableIndex;
        public bool This;
        public SpokeVType VType;
        public bool ForSet;

        public ISpokeItem IType
        {
            get { return ISpokeItem.Variable; }
        }
        public override string ToString()
        {
            if (Parent==null) {
                return VariableName;
            }
            return Parent + "." + VariableName;
        }

    }
    public class SpokeAnonMethod : s, SpokeParent, SpokeItem, SpokeLine, SpokeLines
    {
        public SpokeItem Parent { get; set; }
        public SpokeLine[] Lines { get { return lines; } set { lines = value; } }
        public SpokeItem RunOnVar;
        public SpokeLine[] @lines;
        public ParamEter[] Parameters;
        public bool SpecAnon;
        public SpokeVariable ReturnYield;
        public bool HasYield { get; set; }
        public bool HasYieldReturn { get; set; }
        public bool HasReturn { get; set; }

        public ISpokeItem IType
        {
            get { return ISpokeItem.AnonMethod; }
        }

        public ISpokeLine LType
        {
            get { return ISpokeLine.AnonMethod; }
        }


        public override string ToString()
        {
            return base.ToString();
        }


    }
    public class SpokeBool : s, SpokeItem
    {
        public bool Value;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Bool; }
        }
        public override string ToString()
        {
            return Value.ToString();
        }


    }
    public class SpokeString : s, SpokeItem
    {
        public string Value;
        public ISpokeItem IType
        {
            get { return ISpokeItem.String; }
        }
        public override string ToString()
        {
            return Value.ToString();
        }

    }
    public class SVarItems
    {
        public SVarItems(string name, int index, SpokeItem item)
        {
            Name = name;
            Index = index;
            Item = item;
        }

        public string Name;
        public int Index;
        public SpokeItem Item;

    }
    public class SpokeConstruct : s, SpokeItem, SpokeLine
    {
        public string ClassName;
        public SpokeItem[] Parameters = new SpokeItem[0];
        public SVarItems[] SetVars = new SVarItems[0];
        public int NumOfVars
            ;

        public int MethodIndex=-1;

        public ISpokeItem IType
        {
            get { return ISpokeItem.Construct; }
        }

        public ISpokeLine LType
        {
            get { return ISpokeLine.Construct; }
        }

        public override string ToString()
        {
            return "Construct " + ClassName;
        }
    }
    public class SpokeAddition : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Addition; }
        }

        public override string ToString()
        {
            return "+";
        }
    }
    public class SpokeDivision : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Division; }
        }
        public override string ToString()
        {
            return "/";
        }

    }
    public class SpokeMultiplication : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Multiplication; }
        }
        public override string ToString()
        {
            return "*";
        }

    }
    public class SpokeOr : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Or; }
        }
        public override string ToString()
        {
            return "||";
        }

    }
    public class SpokeAnd : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.And; }
        }
        public override string ToString()
        {
            return "&&";
        }

    }

    public class SpokeGreaterThan : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Greater; }
        }
        public override string ToString()
        {
            return ">";
        }

    }

    public class SpokeLessThan : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Less; }
        }
        public override string ToString()
        {
            return "<";
        }


    }
    public class SpokeGreaterThanOrEqual : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.GreaterEqual; }
        }
        public override string ToString()
        {
            return ">=";
        }

    }
    public class SpokeLessThanOrEqual : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.LessEqual; }
        }
        public override string ToString()
        {
            return LeftSide + "<=" + RightSide;
        }

    }
    public class SpokeEqual : SpokeLine
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Equal; }
        }
        public ISpokeLine LType
        {
            get { return ISpokeLine.Set; }
        }
        public override string ToString()
        {
            return LeftSide+ "=="+RightSide;
        }


    }
    public class SpokeNotEqual : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.NotEqual; }
        }

        public override string ToString()
        {
            return "!=";
        }

    }

    public class SpokeEquality : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Equality; }
        }
        public override string ToString()
        {
            return " = ";
        }


    }

    public class SpokeSubtraction : s, SpokeItem
    {
        public SpokeItem LeftSide;
        public SpokeItem RightSide;
        public ISpokeItem IType
        {
            get { return ISpokeItem.Subtraction; }
        }
        public override string ToString()
        {
            return "-";
        }

    }




}