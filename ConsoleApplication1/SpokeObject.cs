using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    public enum ObjectType
    {
        Unset, Null, Int, Float, String, Bool, Array, Object, Method,
        Void
    }
    public class SpokeObjectMethod
    {
        public bool HasYield;
        public bool HasYieldReturn;
        public bool HasReturn;
        public SpokeLine[] Lines;
        public ParamEter[] Parameters;
        public SpokeInstruction[] Instructions;
        public SpokeVariable ReturnYield;
    }
    public class SpokeObject
    {
        public int IntVal;
        public string StringVal;
        public bool BoolVal;
        public float FloatVal;
        public ObjectType Type;
        public SpokeObject[] Variables;
        public List<SpokeObject> ArrayItems;
        public string ClassName;
        public SpokeObjectMethod AnonMethod;
        public bool ByRef;

        public SpokeObject()
        {

        }
        public SpokeObject(int inde)
        {
            IntVal = inde;
            Type = ObjectType.Int;
        }
        public SpokeObject(SpokeObject[] inde)
        {
            Array.Resize(ref inde, 20);

            Variables = inde;
            Type = ObjectType.Object;
        }
        public SpokeObject(List<SpokeObject> inde)
        {
            ArrayItems = inde; Type = ObjectType.Array;

        }
        public SpokeObject(float inde)
        {
            FloatVal = inde; Type = ObjectType.Float;


        }
        public SpokeObject(string inde)
        {
            StringVal = inde; Type = ObjectType.String;

        }
        public SpokeObject(bool inde) {
            BoolVal = inde; Type = ObjectType.Bool;

        }


        public SpokeObject(ObjectType type)
        {

            Type = type;
        }

        public void SetVariable(int name, SpokeObject obj)
        {
            Variables[name] = obj;

        }
        public SpokeObject GetVariable(int name, bool forSet)
        {
            SpokeObject g = Variables[name];

            if (g == null || forSet)
            {
                return Variables[name] = new SpokeObject();
            }
            return g;
        }
        public bool TryGetVariable(int name, out SpokeObject obj)
        {
            return (obj = Variables[name]) != null;
        }


        public bool Compare(SpokeObject obj) {
            return Compare(this, obj);
        }


        internal static bool Compare(SpokeObject left, SpokeObject right)
        {
            if (left == null)
            {
                if (right == null)
                {
                    return true;
                }
                if (right.Type == ObjectType.Null)
                {
                    return true;
                }
            }
            else
            {
                if (left.Type == ObjectType.Null && right == null)
                {
                    return true;
                }
            }
            if (left.Type != right.Type)
            {
                return false;
            }
            switch (left.Type)
            {
                case ObjectType.Null:
                    return true;
                    break;
                case ObjectType.Int:
                    return left.IntVal == right.IntVal;
                    break;
                case ObjectType.Float:
                    return left.FloatVal == right.FloatVal;
                    break;
                case ObjectType.String:
                    return left.StringVal == right.StringVal;
                    break;
                case ObjectType.Bool:
                    return left.BoolVal == right.BoolVal;
                    break;

                case ObjectType.Object:
                    //                    throw new AbandonedMutexException("not yet cowbow");

                    for (int i = 0; i < left.Variables.Length; i++)
                    {
                        if (!Compare(right.Variables[i], left.Variables[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                    break;
                case ObjectType.Array:

                case ObjectType.Method:
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public override string ToString()
        {
            StringBuilder sb;
            switch (Type)
            {
                case ObjectType.Unset:
                    return "fail";
                    break;
                case ObjectType.Null:
                    return "NULL";
                    break;
                case ObjectType.Int:
                    return IntVal.ToString();
                    break;
                case ObjectType.Float:
                    return FloatVal.ToString();
                    break;
                case ObjectType.String:
                    return StringVal;
                    break;
                case ObjectType.Bool:
                    return BoolVal.ToString();
                    break;

                case ObjectType.Object:
                    sb = new StringBuilder();
                    sb.Append(" " + this.ClassName ?? "");
                    for (int index = 0; index < Variables.Length; index++)
                    {
                        var spokeObject = Variables[index];
                        sb.Append(",  " + index + ": " + Variables[index]);
                    }
                    return sb.ToString();

                case ObjectType.Array:

                    sb = new StringBuilder();

                    sb.Append("[");

                    foreach (var spokeObject in ArrayItems)
                    {
                        sb.Append("  " + spokeObject.ToString());
                    }
                    sb.Append("]");

                    return sb.ToString();
                case ObjectType.Method:
                default:
                    //throw new ArgumentOutOfRangeException();
                    break;
            }

            return base.ToString();
        }

        public void AddRangeArray(SpokeObject lastStack)
        {
            ArrayItems.AddRange(lastStack.ArrayItems);
        }
        public SpokeObject AddArray(SpokeObject lastStack)
        {

            ArrayItems.Add(lastStack);
            return this;
        }

        public SpokeObject AddArrayRange(SpokeObject lastStack)
        {
            ArrayItems.AddRange(lastStack.ArrayItems);
            return this;
        }
    }
}