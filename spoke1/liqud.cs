using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConsoleApplication1;

namespace spoke1
{
    public class liqud
    {
        private readonly Func<Tuple<Point, bool>> _mouseStatus;


        public void run(object sender, DoWorkEventArgs doWorkEventArgs)
        {

            

            var rv = new Dictionary
<string, Func<SpokeObject[], SpokeObject>>()
                         {
                             {
                                 "write", (a) =>
                                              {
                                                  for (int index = 1; index < a.Length; index++)
                                                  {
                                                      var spokeObject = a[index];
                                                      Console.Write(spokeObject.ToString() + " ");
                                                  }
                                                  return null;
                                              }
                                 },
                             {
                                 "readLine", (a) =>
                                                 {
                                                     return new SpokeObject()
                                                                {
                                                                    Type = ObjectType.String,
                                                                    StringVal = Console.ReadLine()
                                                                };
                                                 }
                                 },
                             {
                                 "read", (a) =>
                                             {
                                                 return new SpokeObject()
                                                            {
                                                                Type = ObjectType.String,
                                                                StringVal = Console.Read().ToString()
                                                            };
                                             }
                                 },
                             {
                                 "stringToInt", (a) =>
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
                                                                       IntVal = (int)a[1].FloatVal
                                                                   };
                                                    }
                                 },
                             {
                                 "debug", (a) =>
                                              {
                                                  return null;
                                              }
                                 },
                             {
                                 "writeLine", (a) =>
                                                  {
                                                      for (int index = 1; index < a.Length; index++)
                                                      {
                                                          var spokeObject = a[index];
                                                          Console.Write(spokeObject.ToString() + " ");
                                                      }
                                                      Console.Write("\r\n");
                                                      return null;
                                                  }
                                 },
                             {
                                 "clearConsole", (a) =>
                                                     {
                                                         Console.Clear();
                                                         return null;
                                                     }
                                 },
                             {
                                 "stringLength", (a) =>
                                                     {
                                                         return new SpokeObject()
                                                                    {
                                                                        Type = ObjectType.Int,
                                                                        IntVal = a[1].StringVal.Length
                                                                    };
                                                     }
                                 },
                             {
                                 "setConsolePosition", (a) =>
                                                           {
                                                               Console.SetCursorPosition(a[1].IntVal, a[2].IntVal);
                                                               return null;
                                                           }
                                 },
                             
                             {
                                 "abs", (a) =>
                                            {
                                                switch (a[1].Type)
                                                {
                                                    case ObjectType.Int: var c = a[1].IntVal;
                                                        return new SpokeObject() { IntVal = Math.Abs(c), Type = ObjectType.Int };

                                                        break;
                                                    case ObjectType.Float: var cd = a[1].FloatVal;
                                                        return new SpokeObject() { FloatVal = Math.Abs(cd),Type=ObjectType.Float };

                                                        break;
                                                    default:
                                                        throw new ArgumentOutOfRangeException();
                                                }
                                            }
                                 },
                             {
                                 "nextRandom", (a) =>
                                                   {
                                                       return new SpokeObject()
                                                                  {
                                                                      Type = ObjectType.Int,
                                                                      IntVal = rad.Next(a[1].IntVal, a[2].IntVal)
                                                                  };
                                                       return null;
                                                   }
                                 },
                             {
                                 "rand", (a) =>
                                             {
                                                 var vfd = new SpokeObject() { Type = ObjectType.Float, FloatVal = (float)rad.NextDouble() };
                                                 return vfd;
                                             }
                                 }, {
                                 "getMouseX", (a) =>
                                             {
                                                 var vfd = new SpokeObject() { Type = ObjectType.Int, IntVal = _mouseStatus().Item1.X };
                                                 return vfd;
                                             }
                                 }, {
                                 "getMouseY", (a) =>
                                             {
                                                 var vfd = new SpokeObject() { Type = ObjectType.Int, IntVal = _mouseStatus().Item1.Y };
                                                 return vfd;
                                             }
                                 }, {
                                 "getMouseClicked", (a) =>
                                             {
                                                 var vfd = new SpokeObject() { Type = ObjectType.Bool, BoolVal = _mouseStatus().Item2 };
                                                 return vfd;
                                             }
                                 }
                                 
                                 
                                 ,{
                                 "line", (a) =>
                                             {
                                                 lines.Add(new Tuple<PointF, PointF>(new PointF(a[1].FloatVal + 10, a[2].FloatVal + 10), new PointF(a[3].FloatVal + 10, a[4].FloatVal + 10)));
                                                 return null;
                                             }
                                 },{
                                 "wait", (a) =>
                                             {
                                                 ((BackgroundWorker)sender).ReportProgress(0,lines);
                                                 lines=new List<Tuple<PointF, PointF>>();
                                                 return null;
                                             }
                                 }, 
                         };




            lines = new List<Tuple<PointF, PointF>>();

            try
            {
                var ra = new RunApp(@"liquid.spoke");
                ((BackgroundWorker)sender).ReportProgress(0,lines);
              //  ra.run(rv);

                Console.Write("DonSe");
            }
            catch (Exception er)
            {
                Console.WriteLine(er);

                Application.Exit();

            }




        }
        static Random rad = new Random();
        private List<Tuple<PointF, PointF>> lines;

        public liqud(Func<Tuple<Point, bool>> mouseStatus)
        {
            _mouseStatus = mouseStatus;
        }
    }
}
