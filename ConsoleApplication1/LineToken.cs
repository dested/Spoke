using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    public class LineToken
    {
        public List<IToken> Tokens;

        public LineToken(List<IToken> toks)
        {
            Tokens = toks;
        }
        public LineToken()
        {
            Tokens = new List<IToken>();
        }
        public override string ToString()
        {
return            Tokens.Aggregate("",(a,b)=>a+b.ToString());

        }
    }
}