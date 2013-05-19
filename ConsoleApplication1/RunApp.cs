//#define runExpressions
//#define runInstructions
 #define runClass
//#define runClass2

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConsoleApplication1
{
    public class RunApp
    {

        public RunApp(string fileName)
        {


            var b = new BuildLanguage();
            var classes = b.Run(File.ReadAllText(fileName));


            var r = new BuildExpressions();
            List<SpokeClass> cla = r.Run(classes.Item1, classes.Item2);

            var br = new PrintExpressions(cla, false);
            string f;
            File.WriteAllText("C:\\spokes.txt", f = br.Run());
            //    Console.WriteLine(f);


        }

        public RunApp(string fileName, Dictionary<string, Func<SpokeObject[], SpokeObject>> rv, Dictionary<string, SpokeType> intenMethodTypes)
        {
            Stopwatch sw = new Stopwatch();
            // try { 

            Console.WriteLine(fileName);
            string name = fileName.Split('.')[0];

            Console.WriteLine("Building language...");
            sw.Start();
            var b = new BuildLanguage();
            Tuple<List<Class>, List<TokenMacroPiece>> classes = b.Run(File.ReadAllText(fileName));
            sw.Stop();
            Console.WriteLine("Building language done in " + sw.ElapsedMilliseconds + " milliseconds");

            sw.Reset();

            Console.WriteLine("Builing expressions...");
            sw.Restart();
            var r = new BuildExpressions();
            List<SpokeClass> cla = r.Run(classes.Item1, classes.Item2);
            sw.Stop();
            Console.WriteLine("Building expressions done in " + sw.ElapsedMilliseconds + " milliseconds");

            sw.Reset();
             



            Console.WriteLine("Preparsing expressions...");
            sw.Restart();
            Tuple<SpokeMethod[], SpokeConstruct> dfe = preparse(intenMethodTypes, cla);
            sw.Stop();
            Console.WriteLine("Preparsing expressions done in " + sw.ElapsedMilliseconds + " milliseconds");
            sw.Reset();


            Console.WriteLine("Printing expressions...");
            sw.Restart();
            var br = new PrintExpressions(cla, false);

            File.WriteAllText("C:\\spokesi.txt", br.Run());
            sw.Stop();
            Console.WriteLine("Printing expressions done in " + sw.ElapsedMilliseconds + " milliseconds");
            sw.Reset();

            Console.WriteLine("Preparsing Instructions...");
            sw.Restart();
            new PreparseInstructions(dfe.Item1);

            File.WriteAllText("C:\\spokesi.txt", br.Run());
            sw.Stop();
            Console.WriteLine("Preparsing Instructions done in " + sw.ElapsedMilliseconds + " milliseconds");
            sw.Reset();

            Func<SpokeObject[], SpokeObject>[] ce = rv.Select(a => a.Value).ToArray();

#if runExpressions
            
            Console.WriteLine("RUNNING Expressions" + fileName.Split('.')[0] + "...");
            sw.Restart();
            
            
            runExpression(ce,dfe);
            sw.Stop();


            Console.WriteLine("Done in " + sw.ElapsedMilliseconds + " milliseconds");
            Console.WriteLine("");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("");
#endif
#if runExpressions && runInstructions
            Console.ReadLine();
#endif
#if runInstructions
            Console.WriteLine("RUNNING Instructions" + fileName.Split('.')[0] + "...");
            sw.Restart();
            

            runInstructions(ce, dfe);
            sw.Stop();


            Console.WriteLine("Done in " + sw.ElapsedMilliseconds + " milliseconds");
            Console.WriteLine("");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("");
#endif
#if runClass && runInstructions
            Console.ReadLine();
#endif

#if runClass || runClass2
            var com = buildClass(ce, dfe,name);
#endif
#if runClass 
            Console.WriteLine("Running Class " + fileName.Split('.')[0] + "...");
            sw.Restart();
            runClass(com.Item1, ce, dfe.Item1);
            sw.Stop();
            BuildFile.Cleanup(com);
            Console.WriteLine("Done in " + sw.ElapsedMilliseconds + " milliseconds");
            Console.WriteLine("");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("");

#endif
#if runClass && runClass2
            Console.ReadLine();
#endif

#if runClass2
            Console.WriteLine("Running Class2 " + fileName.Split('.')[0] + "...");
            sw.Restart();

            runClass2(ce, dfe.Item1, dfe.Item2);
            sw.Stop();
            Console.WriteLine("Done in " + sw.ElapsedMilliseconds + " milliseconds");
            Console.WriteLine("");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("");

#endif

            //  } catch(Exception ecx) {
            //    Console.WriteLine(ecx.ToString());


            //  Console.WriteLine("Failed in " + sw.ElapsedMilliseconds + " milliseconds");
            Console.WriteLine("");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("");

            // }
#if !dontwrite
            Console.ReadLine();
#endif
        }

        private Tuple<Command, TempFileCollection> buildClass(Func<SpokeObject[], SpokeObject>[] rv, Tuple<SpokeMethod[], SpokeConstruct> dms,string name)
        {
            BuildFile bf = new BuildFile(rv, dms);
            return bf.buildIt(name);
        }

        private void runClass(Command d, Func<SpokeObject[], SpokeObject>[] rv, SpokeMethod[] methods)
        {
            d.loadUp(rv, methods);
            d.Run();
        }
#if runClass2
        private void runClass2(Func<SpokeObject[], SpokeObject>[] rv, SpokeMethod[] dms, SpokeConstruct k)
        {
            RunClass2 d = new RunClass2();
            d.loadUp(rv, dms);
            d.Run();

        }

#endif




 




        private void runInstructions(Func<SpokeObject[], SpokeObject>[] rv, Tuple<SpokeMethod[], SpokeConstruct> dms)
        {


            RunInstructions ri = new RunInstructions(rv, dms.Item1);
            ri.Run(dms.Item2);


        }
        private void runExpression(Func<SpokeObject[], SpokeObject>[] rv, Tuple<SpokeMethod[], SpokeConstruct> dms)
        {


            var m = new RunExpressions(rv, dms.Item1);

            m.Run(dms.Item2);

        }







        public Tuple<SpokeMethod[], SpokeConstruct> preparse(Dictionary<string, SpokeType> rv, List<SpokeClass> cla)
        {
            Dictionary<string, SpokeMethod> dms =
                cla.SelectMany(a => a.Methods).ToDictionary(a => a.Class.Name + a.MethodName);
            var m = new PreparseExpressions(cla, rv, dms);

            var d = m.Run("Main", ".ctor");
            return new Tuple<SpokeMethod[], SpokeConstruct>(dms.Select(a => a.Value).ToArray(), d);
        }


    }












}
