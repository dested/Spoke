using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    //    [System.Diagnostics.DebuggerStepThrough]
    public class TokenEnumerator : IEnumerator<IToken>, IEnumerable<IToken>
    {
        private LineToken[] lines_;
        public TokenEnumerator(LineToken[] lines)
        {
            lines_ = lines;
        }
        public void Dispose()
        {
            lines_ = null;
        }
        public TokenEnumerator Clone()
        {


            LineToken[] ts = new LineToken[lines_.Length ];
            for (int index = 0; index < lines_.Length;index++ )
            {
                var lineToken = lines_[index];
                ts[index] = new LineToken(new List<IToken>(lineToken.Tokens));
            }



            TokenEnumerator tn = new TokenEnumerator(ts);
            tn.lineIndex = lineIndex;
            tn.tokenIndex = tokenIndex;
            return tn;
        }

        public LineToken CurrentLine
        {
            get
            {


                return lines_[lineIndex];
            }
        }
        public List<LineToken> CurrentLines
        {
            get
            {
                int start = lineIndex + 1;
                List<LineToken> lmn = new List<LineToken>(3);
                lmn.Add(lines_[lineIndex]);
                int ind = 0;

            fb:

                var d = lmn[ind].Tokens;
                if (d[d.Count - 1].Type == Token.AnonMethodStart || d[d.Count - 2].Type == Token.AnonMethodStart)
                {
                    int curta = ((TokenTab)lines_[lineIndex].Tokens[0]).TabIndex;


                fm:
                    if (start < lines_.Length)
                        if (((TokenTab)lines_[start++].Tokens[0]).TabIndex == curta)
                        {
                            lmn.Add(lines_[start - 1]);
                            ind++;
                            goto fb;
                        }
                        else goto fm;

                }
                return lmn;
            }
        }

        public IToken[] Next5()
        {
            List<IToken> its = new List<IToken>();
            for (int i = 0; i < 5; i++)
            {
                its.Add(PeakNext(i));
            }
            return its.ToArray();
        }



        public IToken PeakNext(int g = 1)
        {
            int peakLineIndex = lineIndex;
            int peakTokenIndex = tokenIndex;
            IToken cur;
        s:

            if (lines_[peakLineIndex].Tokens.Count - 1 == peakTokenIndex)
            {
                peakLineIndex++;
                peakTokenIndex = 0;
            }
            else peakTokenIndex++;

            if (peakLineIndex >= lines_.Length )
            {
                return null;
            }
            cur = lines_[peakLineIndex].Tokens[peakTokenIndex];
            g--;
            if (g == 0)
            {
                return cur;
            }
            goto s;

        }




        private int lineIndex = 0;
        public int tokenIndex = 0;
        public IEnumerable<IToken> OutstandingLine
        {
            get
            {
                var lmn = new List<IToken>();
                for (int i = tokenIndex; i < lines_[lineIndex].Tokens.Count; i++)
                {
                    lmn.Add(lines_[lineIndex].Tokens[i]);
                }
                int start = lineIndex + 1;


            fb:
                if (lmn.LastOrDefault(a => a.Type != Token.NewLine) == null)
                {
                    return lmn;
                }
                if (lmn.Last(a => a.Type != Token.NewLine).Type == Token.AnonMethodStart)
                {
                    int curta = ((TokenTab)lines_[lineIndex].Tokens[0]).TabIndex;

                fm:
                    if (start < lines_.Length )
                        if (((TokenTab)lines_[start++].Tokens[0]).TabIndex == curta)
                        {
                            //                            start -= 1;
                            lmn.AddRange(lines_[start - 1].Tokens);
                            goto fb;
                        }
                        else
                            goto fm;

                }
                return lmn;
            }
        }

        public bool MoveNext()
        {
            if (lines_.Length  == lineIndex)
            {
                return true;
            }

            if (lines_[lineIndex].Tokens.Count - 1 == tokenIndex)
            {
                ++lineIndex;
                tokenIndex = 0;
            }
            else tokenIndex++;

            return lineIndex < lines_.Length ;
        }

        public void Reset()
        {
            //            lineIndex = 0;
            //            tokenIndex = 0;
        }

        public IToken Current
        {
            get
            {

                if (lines_.Length  == lineIndex)
                {

                    return new TokenEndOfCodez();
                }

                return lines_[lineIndex].Tokens[tokenIndex];
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public int LineIndex
        {
            get { return lineIndex; }
            
        }

        IEnumerator<IToken> IEnumerable<IToken>.GetEnumerator()
        {
            return this;
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        public void PutBack(int g = 1)
        {
        vf:


            if (tokenIndex == 0)
            {
                lineIndex--;
                tokenIndex = lines_[lineIndex].Tokens.Count - 1;
            }
            else
            {
                tokenIndex--;
            }

            g--;
            if (g == 0)
            {
                return;
            }
            goto vf;

        }

        public IToken getFirstInLine()
        {
            return lines_[lineIndex].Tokens[0];
        }

        public void Set(TokenEnumerator tm)
        {
            this.tokenIndex = tm.tokenIndex;
            this.lineIndex = tm.lineIndex;
        }

     }
}