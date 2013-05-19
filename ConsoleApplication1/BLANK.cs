using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ConsoleApplication1
{
    public partial class RunClass : Command
    {

        private SpokeMethod[] Methods;
        private Func<SpokeObject[], SpokeObject>[] InternalMethods;


        private SpokeObject FALSE = new SpokeObject(false);
        private SpokeObject TRUE = new SpokeObject(true);
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
        public void loadUp(Func<SpokeObject[], SpokeObject>[] internalMethods, SpokeMethod[] mets)
        {
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
            SpokeObject dm = new SpokeObject(new SpokeObject[9]);
            var gm = new SpokeObject[9];
            gm[0] = dm;
            return Mainctor(gm);
        }
        private SpokeObject Mainctor(SpokeObject[] variables)
        {
            SpokeObject[] specVariables = new SpokeObject[0]; SpokeObject[] sps;
            SpokeObject bm2;
            SpokeObject bm;
            SpokeObject lastStack;
            var b = new SpokeObject(new List<SpokeObject>(20));
            variables[0].Variables[0] = b.AddArray(ints[0]);
            var c = new SpokeObject(new List<SpokeObject>(20));
            variables[0].Variables[1] = c.AddArray(ints[0]);
            var d = new SpokeObject(new List<SpokeObject>(20));
            variables[0].Variables[2] = d.AddArray(ints[0]);
            Methods[1].MethodFunc(new SpokeObject[] { variables[0].Variables[0] });
            Methods[1].MethodFunc(new SpokeObject[] { variables[0].Variables[1] });
            Methods[1].MethodFunc(new SpokeObject[] { variables[0].Variables[2] });
            variables[1] = intCache(ints[10].IntVal);
            InternalMethods[9](new SpokeObject[] { variables[0], new SpokeObject("Start") });
            variables[2] = intCache(ints[1].IntVal);
            variables[3] = intCache(variables[1].IntVal);
            var e = new SpokeObject(new List<SpokeObject>(20));
            variables[4] = e;
        _topOfWhile_160:
            if (!(((variables[2].IntVal <= variables[3].IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndLoop160;
            Methods[0].MethodFunc(new SpokeObject[] { variables[4], variables[2] });
            variables[2] = intCache(intCache(variables[2].IntVal + ints[1].IntVal).IntVal);
            goto _topOfWhile_160;
        EndLoop160:
            variables[5] = variables[4];
            variables[6] = variables[5];
            variables[7] = intCache(ints[0].IntVal);
        _topOfForeach_172:
            if (!(((variables[7].IntVal < Methods[2].MethodFunc(new SpokeObject[] { variables[6] }).IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndLoop172;
            variables[8] = variables[6].ArrayItems[variables[7].IntVal];
            Methods[0].MethodFunc(new SpokeObject[] { variables[0].Variables[0], variables[8] });
            variables[7] = intCache(intCache(variables[7].IntVal + ints[1].IntVal).IntVal);
            goto _topOfForeach_172;
        EndLoop172:
            Maindraw(new SpokeObject[] { variables[0], null, null, null, null, null, null, null, null, null, null, null, null, null });
            MaindoHanoi(new SpokeObject[] { variables[0], variables[1], new SpokeObject("1"), new SpokeObject("3"), new SpokeObject("2") });
            InternalMethods[9](new SpokeObject[] { variables[0], new SpokeObject("Done") });

            return null;
        }
        private SpokeObject MaindoHanoi(SpokeObject[] variables)
        {
            SpokeObject[] specVariables = new SpokeObject[0]; SpokeObject[] sps;
            SpokeObject bm2;
            SpokeObject bm;
            SpokeObject lastStack;
            if (!(((variables[1].IntVal > ints[0].IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndIf192;
            MaindoHanoi(new SpokeObject[] { variables[0], intCache(variables[1].IntVal - ints[1].IntVal), variables[2], variables[4], variables[3] });
            Mainmove(new SpokeObject[] { variables[0], variables[1], variables[2], variables[3], null, null, null });
            Maindraw(new SpokeObject[] { variables[0], null, null, null, null, null, null, null, null, null, null, null, null, null });
            MaindoHanoi(new SpokeObject[] { variables[0], intCache(variables[1].IntVal - ints[1].IntVal), variables[4], variables[3], variables[2] });
        EndIf192:

            return null;
        }
        private SpokeObject Mainmove(SpokeObject[] variables)
        {
            SpokeObject[] specVariables = new SpokeObject[0]; SpokeObject[] sps;
            SpokeObject bm2;
            SpokeObject bm;
            SpokeObject lastStack;
            InternalMethods[10](new SpokeObject[] { variables[0] });
            InternalMethods[9](new SpokeObject[] { variables[0], new SpokeObject("Move Disk"), variables[1], new SpokeObject("From"), variables[2], new SpokeObject("To"), variables[3] });
            if (!(((variables[2].Compare(new SpokeObject("1")) ? TRUE : FALSE))).BoolVal)
                goto EndIf235;
            Methods[3].MethodFunc(new SpokeObject[] { variables[0].Variables[0], variables[1] });
        EndIf235:
            if (!(((variables[2].Compare(new SpokeObject("2")) ? TRUE : FALSE))).BoolVal)
                goto EndIf244;
            Methods[3].MethodFunc(new SpokeObject[] { variables[0].Variables[1], variables[1] });
        EndIf244:
            if (!(((variables[2].Compare(new SpokeObject("3")) ? TRUE : FALSE))).BoolVal)
                goto EndIf253;
            Methods[3].MethodFunc(new SpokeObject[] { variables[0].Variables[2], variables[1] });
        EndIf253:
            if (!(((variables[3].Compare(new SpokeObject("1")) ? TRUE : FALSE))).BoolVal)
                goto EndIf262;
            Methods[6].MethodFunc(new SpokeObject[] { variables[0].Variables[0], variables[3], variables[1] });
        EndIf262:
            if (!(((variables[3].Compare(new SpokeObject("2")) ? TRUE : FALSE))).BoolVal)
                goto EndIf272;
            Methods[6].MethodFunc(new SpokeObject[] { variables[0].Variables[1], variables[3], variables[1] });
        EndIf272:
            if (!(((variables[3].Compare(new SpokeObject("3")) ? TRUE : FALSE))).BoolVal)
                goto EndIf282;
            Methods[6].MethodFunc(new SpokeObject[] { variables[0].Variables[2], variables[3], variables[1] });
        EndIf282:

            return null;
        }
        private SpokeObject Maindraw(SpokeObject[] variables)
        {
            SpokeObject[] specVariables = new SpokeObject[0]; SpokeObject[] sps;
            SpokeObject bm2;
            SpokeObject bm;
            SpokeObject lastStack;
            variables[1] = intCache(ints[2].IntVal);
            variables[2] = new SpokeObject(new SpokeObject(" ").StringVal);
            variables[3] = new SpokeObject(new SpokeObject("0").StringVal);
            variables[4] = intCache(ints[0].IntVal);
            InternalMethods[12](new SpokeObject[] { variables[0], variables[4], ints[1] });
            InternalMethods[0](new SpokeObject[] { variables[0], new SpokeObject("Peg1") });
            variables[5] = variables[0].Variables[0];
            variables[6] = intCache(ints[0].IntVal);
        _topOfForeach_310:
            if (!(((variables[6].IntVal < Methods[2].MethodFunc(new SpokeObject[] { variables[5] }).IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndLoop310;
            variables[7] = variables[5].ArrayItems[variables[6].IntVal];
            InternalMethods[12](new SpokeObject[] { variables[0], variables[4], variables[1] });
            if (!(((variables[7].IntVal < ints[10].IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndIf316;
        EndIf316:
            InternalMethods[0](new SpokeObject[] { variables[0], variables[7] });
            InternalMethods[0](new SpokeObject[] { variables[0], variables[2] });
            variables[1] = intCache(intCache(variables[1].IntVal + ints[1].IntVal).IntVal);
            variables[6] = intCache(intCache(variables[6].IntVal + ints[1].IntVal).IntVal);
            goto _topOfForeach_310;
        EndLoop310:
            variables[1] = intCache(ints[2].IntVal);
            variables[4] = intCache(intCache(variables[4].IntVal + ints[5].IntVal).IntVal);
            InternalMethods[12](new SpokeObject[] { variables[0], variables[4], ints[1] });
            InternalMethods[0](new SpokeObject[] { variables[0], new SpokeObject("Peg2") });
            variables[8] = variables[0].Variables[1];
            variables[9] = intCache(ints[0].IntVal);
        _topOfForeach_356:
            if (!(((variables[9].IntVal < Methods[2].MethodFunc(new SpokeObject[] { variables[8] }).IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndLoop356;
            variables[10] = variables[8].ArrayItems[variables[9].IntVal];
            InternalMethods[12](new SpokeObject[] { variables[0], variables[4], variables[1] });
            if (!(((variables[10].IntVal < ints[10].IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndIf362;
        EndIf362:
            InternalMethods[0](new SpokeObject[] { variables[0], variables[10] });
            InternalMethods[0](new SpokeObject[] { variables[0], variables[2] });
            variables[1] = intCache(intCache(variables[1].IntVal + ints[1].IntVal).IntVal);
            variables[9] = intCache(intCache(variables[9].IntVal + ints[1].IntVal).IntVal);
            goto _topOfForeach_356;
        EndLoop356:
            variables[1] = intCache(ints[2].IntVal);
            variables[4] = intCache(intCache(variables[4].IntVal + ints[5].IntVal).IntVal);
            InternalMethods[12](new SpokeObject[] { variables[0], variables[4], ints[1] });
            InternalMethods[0](new SpokeObject[] { variables[0], new SpokeObject("Peg3") });
            variables[11] = variables[0].Variables[2];
            variables[12] = intCache(ints[0].IntVal);
        _topOfForeach_402:
            if (!(((variables[12].IntVal < Methods[2].MethodFunc(new SpokeObject[] { variables[11] }).IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndLoop402;
            variables[13] = variables[11].ArrayItems[variables[12].IntVal];
            InternalMethods[12](new SpokeObject[] { variables[0], variables[4], variables[1] });
            if (!(((variables[13].IntVal < ints[10].IntVal) ? TRUE : FALSE)).BoolVal)
                goto EndIf408;
        EndIf408:
            InternalMethods[0](new SpokeObject[] { variables[0], variables[13] });
            InternalMethods[0](new SpokeObject[] { variables[0], variables[2] });
            variables[1] = intCache(intCache(variables[1].IntVal + ints[1].IntVal).IntVal);
            variables[12] = intCache(intCache(variables[12].IntVal + ints[1].IntVal).IntVal);
            goto _topOfForeach_402;
        EndLoop402:

            return null;
        }
    }
}
