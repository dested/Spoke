using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    public class Class
    {

        public List<LineToken> Variables = new List<LineToken>();
        public List<string> VariableNames = new List<string>();
        public List<Method> Methods = new List<Method>();
        public string Name;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Class " + Name);

            sb.AppendLine("  \tVariables:" +
                          VariableNames .Aggregate("\r\n",
                                                   (n, t) =>
                                                   n + ("  \t  \t" + t +"\r\n")));
            sb.AppendLine("  \tMethods:" +
                          Methods.Aggregate("\r\n",
                                            (n, t) =>
                                            n +
                                            ("  \t  \t" + "Method " + t.Name + " (" +
                                             t.paramNames.Aggregate("", (a, b) => a + b + ",") + ")\r\n" +
                                             t.Lines.Aggregate("",
                                                               (a, b) =>
                                                               a + "  \t" +
                                                               b.Tokens.Aggregate("",
                                                                                  (m, l) => m + l.ToString() ) +
                                                               "\r\n")) ));

            return sb.ToString();
            return base.ToString();
        }
    }
}