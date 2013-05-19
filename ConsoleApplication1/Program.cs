//#define dontwrite
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;


namespace ConsoleApplication1
{

    using System;
    using System.Threading;
    using System.IO;

    public class MandelBrot
    {
        private static int n = 200;
        private static byte[][] data;
        private static int lineCount = -1;
        private static double[] xa;

        public MandelBrot()
        { 
            Console.Out.WriteLine("P4\n{0} {0}", n);

            int lineLen = (n - 1) / 8 + 1;
            data = new byte[n][];
            for (int i = 0; i < n; i++) data[i] = new byte[lineLen];

            xa = new double[n];
            for (int x = 0; x < n; x++) xa[x] = x * 2.0 / n - 1.5;

            var threads = new Thread[Environment.ProcessorCount];
            for (int i = 0; i < threads.Length; i++)
                (threads[i] = new Thread(Calculate)).Start();

            foreach (var t in threads) t.Join();

            var s = Console.OpenStandardOutput();
            for (int y = 0; y < n; y++)
                s.Write(data[y], 0, lineLen);
        }

        private void Calculate()
        {
            int y;
            while ((y = Interlocked.Increment(ref lineCount)) < n)
            {
                var line = data[y];
                int xbyte = 0, bits = 1;
                double ci = y * 2.0 / n - 1.0;

                for (int x = 0; x < n; x++)
                {
                    double cr = xa[x];
                    if (bits > 0xff) { line[xbyte++] = (byte)bits; bits = 1; }
                    double zr = cr, zi = ci, tr = cr * cr, ti = ci * ci;
                    int i = 49;
                    do
                    {
                        zi = zr * zi + zr * zi + ci; zr = tr - ti + cr;
                        tr = zr * zr; ti = zi * zi;
                    }
                    while ((tr + ti <= 4.0) && (--i > 0));
                    bits = (bits << 1) | (i == 0 ? 1 : 0);
                }
                while (bits < 0x100) bits = (bits << 1);
                line[xbyte] = (byte)bits;
            }
        }
    }


    class Program
    {
        private static List<Tuple<PointF, PointF, Color>> lines = new List<Tuple<PointF, PointF, Color>>();
        private static int ticks = 0;

        private static int indes = 10000;


        private static void run(Dictionary<string, Func<SpokeObject[], SpokeObject>> rv, Dictionary<string, SpokeType> vs)
        {



            RunApp ra;
           // ra = new RunApp("golf.spoke", rv, vs);
            ra = new RunApp("fallsfromlord.spoke", rv, vs);
            ra = new RunApp("liquid.spoke", rv, vs);

        //   ra = new RunApp("grideas.spoke", rv, vs);
          ra = new RunApp("sevens.spoke", rv, vs);
         
            ra = new RunApp("realSimple.spoke", rv, vs);
             ra = new RunApp("easy2.spoke", rv, vs);
             ra = new RunApp("easy.spoke", rv, vs);
             ra = new RunApp("prime.spoke", rv, vs);
             ra = new RunApp("tower.spoke", rv, vs);
              ra = new RunApp("simple.spoke", rv, vs);
             ra = new RunApp("isFo.spoke", rv, vs);
             ra = new RunApp("tester.spoke", rv, vs);
            
        }


        static void Main(string[] args) {
           // new MandelBrot();

            var ff = new Font(FontFamily.GenericMonospace, 9);
            var rv = new Dictionary
                <string, Func<SpokeObject[], SpokeObject>>()
                         {
                             {"write", (a) =>
                                              {
                                                  for (int index = 1; index < a.Length; index++)
                                                  {
                                                      var spokeObject = a[index];
#if !dontwrite
                                                      Console.Write(spokeObject.ToString() + " ");
#endif
                                                  }
                                                  return null;
                                              }
                                 }, {
                                 "getMouseX", (a) =>
                                             {
                                                 var vfd = new SpokeObject() { Type = ObjectType.Int, IntVal = 0 };
                                                 return vfd;
                                             }
                                 }, {
                                 "getMouseY", (a) =>
                                             {
                                                 var vfd = new SpokeObject() { Type = ObjectType.Int, IntVal = 0 };
                                                 return vfd;
                                             }
                                 }, {
                                 "getMouseClicked", (a) =>
                                             {
                                                 var vfd = new SpokeObject() { Type = ObjectType.Bool, BoolVal = false };
                                                 return vfd;
                                             }
                                 },{
                                 "readLine", (a) =>
                                                 {
                                                     return new SpokeObject()
                                                                {
                                                                    Type = ObjectType.String,
#if !dontwrite
                                                                    StringVal = Console.ReadLine()
#else
                                                                    StringVal=""
#endif
                                                                    };
                                                 }
                                 },{
                                 "read", (a) =>
                                             {
                                                 return new SpokeObject()
                                                            {
                                                                Type = ObjectType.String,
#if !dontwrite
                                                                StringVal = Console.Read().ToString()
#else
                                                                StringVal =""
#endif
                                                            };
                                             }
                                 },{"stringToInt", (a) =>
                                                    {
                                                        return new SpokeObject()
                                                                   {
                                                                       Type = ObjectType.Int,
                                                                       IntVal = int.Parse(a[1].StringVal)
                                                                   };
                                                    }
                                 },{
                                 "floatToInt", (a) =>
                                                   {
                                                       return new SpokeObject()
                                                                  {
                                                                      Type = ObjectType.Int,
                                                                      IntVal = (int) a[1].FloatVal
                                                                  };
                                                   }
                                 },{"debug", (a) =>
                                              {
                                                  return null;
                                              }
                                 },{"writeLine", (a) =>
                                                  {
                                                      for (int index = 1; index < a.Length; index++)
                                                      {
                                                          var spokeObject = a[index];
#if !dontwrite
                                                          Console.Write(spokeObject.ToString() + " ");
#endif
                                                      }
#if !dontwrite
                                                      Console.Write("\r\n");
#endif
                                                      return null;
                                                  }
                                 },{
                                 "clearConsole", (a) =>
                                                     {
#if !dontwrite
                                                         Console.Clear();
#endif
                                                         return null;
                                                     }
                                 },{
                                 "stringLength", (a) =>
                                                     {
                                                         return new SpokeObject()
                                                                    {
                                                                        Type = ObjectType.Int,
                                                                        IntVal = a[1].StringVal.Length
                                                                    };
                                                     }
                                 },{
                                 "setConsolePosition", (a) =>
                                                           {
#if !dontwrite
                                                               Console.SetCursorPosition(a[1].IntVal, a[2].IntVal);
#endif
                                                               return null;
                                                           }
                                 },
                             {
                                 "abs", (a) =>
                                            {
                                                switch (a[1].Type)
                                                {
                                                    case ObjectType.Int:
                                                        var c = a[1].IntVal;
                                                        return new SpokeObject()
                                                                   {IntVal = Math.Abs(c), Type = ObjectType.Int};

                                                        break;
                                                    case ObjectType.Float:
                                                        var cd = a[1].FloatVal;
                                                        return new SpokeObject()
                                                                   {FloatVal = Math.Abs(cd), Type = ObjectType.Float};

                                                        break;
                                                    default:
                                                        throw new ArgumentOutOfRangeException();
                                                }
                                            }
                                 },{
                                 "nextRandom", (a) =>
                                                   {

                                                   if (a.Length==2) {
                                                       return new SpokeObject()
                                                       {
                                                           Type = ObjectType.Int,
                                                           IntVal = rad.Next(a[1].IntVal)
                                                       };
                                                       
                                                   }
                                                       return new SpokeObject()
                                                                  {
                                                                      Type = ObjectType.Int,
                                                                      IntVal = rad.Next(a[1].IntVal, a[2].IntVal)
                                                                  };
                                                       return null;
                                                   }
                                 },{
                                 "rand", (a) =>
                                             {
                                                 var vfd = new SpokeObject()
                                                               {
                                                                   Type = ObjectType.Float,
                                                                   FloatVal = (float) rad.NextDouble()
                                                               };
                                                 return vfd;
                                             }
                                 },{
                                 "line", (a) =>
                                             {
                                                 Color col = Color.Pink;
                                                 switch ((int) a[5].FloatVal)
                                                 {
                                                     case 1:
                                                         col = Color.White;
                                                         break;
                                                     case 2:
                                                         col = Color.Blue;
                                                         break;
                                                     case 3:
                                                         col = Color.Green;
                                                         break;
                                                 }


                                                 lines.Add(
                                                     new Tuple<PointF, PointF, Color>(
                                                         new PointF(a[1].FloatVal + 10, a[2].FloatVal + 10),
                                                         new PointF(a[3].FloatVal + 10, a[4].FloatVal + 10), col));
                                                 return null;
                                             }
                                 },{
                                 "wait", (a) =>
                                             {
#if !dontwrite
                                                  Thread.Sleep(a[1].IntVal);
#else
                                             //Console.WriteLine("Waiting for " + a[1].IntVal + " milliseconds");
#endif                                  
                                                 return null;
                                             }
                                 },{
                                 "paintInternal", (a) =>
                                             {
                                                  
                                                 Bitmap bm = new Bitmap(850, 850);

                                                 var efd = Graphics.FromImage(bm);

                                                 efd.FillRectangle(Brushes.Black, 0, 0, 850, 850);
                                                 efd.DrawString(ticks++ + " Ticks", ff, Brushes.White, 0, 0);

                                                 if (lines != null)
                                                 {

                                                     for (int index = lines.Count - 1; index >= 0; index--)
                                                     {
                                                         var line = lines[index];
                                                         if (line.Item1 == line.Item2)
                                                         {
                                                             efd.DrawLine(new Pen(Color.Red), line.Item1,
                                                                          new PointF(line.Item2.X + 0.01f, line.Item2.Y + 0.01f));

                                                         }
                                                         else
                                                             efd.DrawLine(new Pen(line.Item3), line.Item1, line.Item2);
                                                     }
                                                     lines.Clear();

                                                 }
                                                 efd.Save();
                                                 Console.WriteLine("water" + indes + " Created");
                                                 bm.Save("C:\\water3\\" + indes++ + ".png", ImageFormat.Png);

                                                 return null;
                                             }
                                 },{"clone", (a) => {
                                                 return new SpokeObject() {
                                                                              AnonMethod = a[1].AnonMethod,
                                                                              BoolVal = a[1].BoolVal,
                                                                              IntVal = a[1].IntVal,
                                                                              FloatVal = a[1].FloatVal,
                                                                              StringVal = a[1].StringVal,
                                                                              Variables = a[1].Variables,
                                                                              ArrayItems = a[1].ArrayItems,
                                                                              Type = a[1].Type,
                                                                              ByRef = a[1].ByRef,
                                                                              ClassName = a[1].ClassName,
                                                                          };
                                             }
                                 }
                         };
            var vs = new Dictionary<string, SpokeType>(){
                             {"write", new SpokeType(ObjectType.Void)
                                 }, {
                                 "getMouseX", new SpokeType(ObjectType.Int)
                                 }, {
                                 "getMouseY", new SpokeType(ObjectType.Int)
                                 }, {
                                 "getMouseClicked", new SpokeType(ObjectType.Bool)
                                 },{
                                 "readLine", new SpokeType(ObjectType.String)
                                 },{
                                 "read", new SpokeType(ObjectType.String)
                                 },{"stringToInt", new SpokeType(ObjectType.Int)
                                 },{
                                 "floatToInt", new SpokeType(ObjectType.Int)
                                 },{"debug", new SpokeType(ObjectType.Void)
                                 },{"writeLine", new SpokeType(ObjectType.Void)
                                 },{
                                 "clearConsole", new SpokeType(ObjectType.Void)
                                 },{
                                 "stringLength", new SpokeType(ObjectType.Int)
                                 },{
                                 "setConsolePosition", new SpokeType(ObjectType.Void)
                                 },
                             {
                                 "abs", new SpokeType(ObjectType.Int)
                                 },{
                                 "nextRandom", new SpokeType(ObjectType.Int)
                                 },{
                                 "rand", new SpokeType(ObjectType.Float)
                                 },{
                                 "line", new SpokeType(ObjectType.Void)
                                 },{
                                 "wait",  new SpokeType(ObjectType.Void)
                                 },{
                                 "paintInternal",  new SpokeType(ObjectType.Void)
                                 },{
                                 "clone",  new SpokeType(ObjectType.Object)
                                 }
                         };



            run(rv, vs);

        }


        static Random rad = new Random();
    }
}
