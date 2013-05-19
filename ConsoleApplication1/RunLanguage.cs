using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    public class TokenMacroPiece
    {
        public IToken[] Macro;
        public ParamEter[] Parameters;
        public List<LineToken> Lines;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Macro " + this.Macro.Aggregate("",
                                                                                  (m, l) => m + l.ToString()) +
                                                               "");


            sb.AppendLine(("(" +
                                             Parameters.Aggregate("", (a, b) => a + (b.ByRef ? "ref " : "") + b.Name + ",") + ")\r\n" +
                                             Lines.Aggregate("",
                                                               (a, b) =>
                                                               a + "  \t" +
                                                               b.Tokens.Aggregate("",
                                                                                  (m, l) => m + l.ToString()) +
                                                               "\r\n")));

            return sb.ToString();

        }

    }
    public struct ParamEter
    {
        public string Name;
        public int Index;
        public bool ByRef;
        public BuildLanguage.ITokenMacroParamType Type;
    }
    public class BuildLanguage
    {
        public BuildLanguage()
        {




        }


        public Tuple<List<Class>, List<TokenMacroPiece>> Run(string fs)
        {
            var words = getWords(fs);




            var lines = words.Aggregate(new List<LineToken>() { new LineToken() }, (old, n) =>
            {
                old.Last().Tokens.Add(n);
                if (n.Type == Token.NewLine)
                {
                    if (old.Any() && (old.Last().Tokens.Count == 0))
                    {
                        return old;
                    }


                    if (old.Any() && ((old.Last().Tokens.First().Type == Token.Tab || old.Last().Tokens.First().Type == Token.NewLine) && old.Last().Tokens.Count == 1))
                    {
                        old.Last().Tokens.Clear();
                        return old;
                    }
                    old.Add(new LineToken());
                    return old;
                }
                return old;
            });



            foreach (var a in lines)
            {
                if (!(!a.Tokens.Any() || (a.Tokens[0].Type == Token.Class || a.Tokens[0].Type == Token.Macro || a.Tokens[0].Type == Token.Tab)))
                {

                }
            }




            for (int index = lines.Count - 1; index >= 0; index--)
            {
                var lineToken = lines[index];
                if (lineToken.Tokens.Count == 0)
                {
                    lines.RemoveAt(index);
                    continue;
                }
                if (lineToken.Tokens[0].Type == Token.Tab)
                {
                    if (lineToken.Tokens.Count == 1)
                    {
                        lines.RemoveAt(index);
                        continue;
                    }
                    if (lineToken.Tokens[1].Type == Token.NewLine)
                    {
                        lines.RemoveAt(index);
                        continue;
                    }
                back:
                    if (lineToken.Tokens[1].Type == Token.Tab)
                    {
                        ((TokenTab)lineToken.Tokens[0]).TabIndex += ((TokenTab)lineToken.Tokens[1]).TabIndex;
                        lineToken.Tokens.RemoveAt(1);
                        goto back;
                    }

                }
            }


            int done = 0;
            int indes = 0;
            List<List<LineToken>> macros;
            List<TokenMacroPiece> allMacros = new List<TokenMacroPiece>();
            if (lines.Any() && lines[0].Tokens[0].Type == Token.Macro)
            {

                macros = lines.Aggregate(new List<List<LineToken>>() { },
                                         delegate(List<List<LineToken>> old, LineToken n)
                                         {
                                             if (n.Tokens.Count == 0)
                                             {
                                                 return old;
                                             }
                                             indes = indes + 1;
                                             if (done > 0 || n.Tokens[0].Type == Token.Class)
                                             {
                                                 if (done == 0)
                                                 {
                                                     done = indes - 2;
                                                 }
                                                 return old;
                                             }

                                             if (n.Tokens[0].Type == Token.Macro)
                                                 old.Add(new List<LineToken>());
                                             old.Last().Add(n);
                                             return old;
                                         });

                for (int i = done; i >= 0; i--)
                {
                    lines.RemoveAt(i);
                }


                foreach (List<LineToken> macro in macros)
                {
                    TokenMacroPiece mp = new TokenMacroPiece();

                    allMacros.Add(mp);

                    mp.Lines = new List<LineToken>();

                    TokenEnumerator en = new TokenEnumerator(macro.ToArray());
                    StaticMethods.Assert(en.Current.Type == Token.Macro, "notMacro?");
                    en.MoveNext();
                    List<IToken> mps = new List<IToken>();
                    int curLine = 0;

                    if (en.Current.Type == Token.SemiColon)
                    {
                        StaticMethods.Assert(en.Current.Type == Token.SemiColon, "");
                        en.MoveNext();
                        while (en.Current.Type != Token.SemiColon)
                        {
                            mps.Add(en.Current);
                            en.MoveNext();
                        }
                        mp.Macro = mps.ToArray();

                        StaticMethods.Assert(en.Current.Type == Token.SemiColon, "");
                        en.MoveNext();

                        StaticMethods.Assert(en.Current.Type == Token.AnonMethodStart, "");
                        en.MoveNext();

                        StaticMethods.Assert(en.Current.Type == Token.Bar, "");
                        en.MoveNext();
                        StaticMethods.Assert(en.Current.Type == Token.OpenParen, "");
                        en.MoveNext();


                        List<ParamEter> parameters_ = new List<ParamEter>();
                        if (en.Current.Type != Token.CloseParen)
                        {
                        pback2:
                            bool byRef = false;

                            if (((TokenWord)en.Current).Word.ToLower() == "ref")
                            {
                                byRef = true;
                                en.MoveNext();
                            }
                            parameters_.Add(new ParamEter() { ByRef = byRef, Name = ((TokenWord)en.Current).Word });
                            en.MoveNext();
                            switch (en.Current.Type)
                            {
                                case Token.CloseParen:
                                    en.MoveNext();
                                    break;
                                case Token.Comma:
                                    en.MoveNext();
                                    goto pback2;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        mp.Parameters = parameters_.ToArray();

                        curLine = 1;
                    }
                    else if (en.Current.Type == Token.OpenCurly)
                    {
                        en.MoveNext();
                        while (!(en.Current.Type == Token.CloseCurly && en.PeakNext().Type!= Token.CloseCurly))
                        {
                            if (en.Current.Type == Token.OpenCurly && en.PeakNext().Type == Token.OpenCurly)
                            {
                                en.MoveNext();
                            }
                            if (en.Current.Type == Token.CloseCurly && en.PeakNext().Type == Token.CloseCurly)
                            {
                                en.MoveNext();
                            }
                            mps.Add(en.Current);
                            en.MoveNext();
                        }
                        en.MoveNext();
                        mp.Macro = mps.ToArray().Trim((a) => a.Type == Token.NewLine, (a) => a.Type == Token.Tab);




                        StaticMethods.Assert(en.Current.Type == Token.Colon, "");
                        en.MoveNext();
                        StaticMethods.Assert(en.Current.Type == Token.OpenParen, "");
                        en.MoveNext();


                        List<ParamEter> parameters_ = new List<ParamEter>();
                        if (en.Current.Type != Token.CloseParen)
                        {
                        pback2:

                            ParamEter pm;
                            parameters_.Add(pm = new ParamEter() { ByRef = true, Name = ((TokenWord)en.Current).Word });
                            if (!pm.Name.StartsWith("$"))
                            {
                                throw new Exception("Bad macro param");
                            }
                            en.MoveNext();

                            if (en.Current.Type != Token.Equal)
                            {
                                throw new Exception("Bad macro param equals");
                            }
                            en.MoveNext();

                            pm.Type = consumeMacroParamValue(en);


                            switch (en.Current.Type)
                            {
                                case Token.CloseParen:
                                    en.MoveNext();
                                    break;
                                case Token.Comma:
                                    en.MoveNext();
                                    goto pback2;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        mp.Parameters = parameters_.ToArray();
                        curLine = en.LineIndex+1;

                    }
                    else throw new Exception("Cannot find macro");



                    for (int i = curLine; i < macro.Count; i++)
                    {
                        mp.Lines.Add(macro[i]);
                    }

                    if (mp.Lines[0].Tokens.Count >1&& mp.Lines[0].Tokens[0].Type == Token.Tab  && mp.Lines[0].Tokens[1].Type == Token.OpenCurly)
                    {
                        mp.Lines.RemoveAt(0);
                    }
                    if (mp.Lines[mp.Lines.Count-1].Tokens.Count > 1 && mp.Lines[mp.Lines.Count - 1].Tokens[0].Type == Token.Tab && mp.Lines[mp.Lines.Count - 1].Tokens[1].Type == Token.CloseCurly)
                    {
                        mp.Lines.RemoveAt(mp.Lines.Count - 1);
                    }


                }

            }

            var classes = lines.Aggregate(new List<List<LineToken>>() { }, (old, n) =>
            {
                if (n.Tokens.Count == 0)
                {
                    return old;
                }
                if (n.Tokens[0].Type == Token.Class)
                    old.Add(new List<LineToken>());
                old.Last().Add(n);
                return old;
            });


            List<Class> someClasses = new List<Class>();

            foreach (List<LineToken> @class in classes)
            {
                Class c = new Class();
                someClasses.Add(c);
                c.Name = ((TokenWord)@class[0].Tokens[1]).Word;
                for (int index = 1; index < @class.Count; index++)
                {
                    LineToken v = @class[index];
                    if (v.Tokens[0].Type != Token.Tab)
                        throw new AbandonedMutexException();

                    if (v.Tokens[1].Type != Token.Def)
                    {
                        c.Variables.Add(v);
                    }
                    else
                    {
                        var m = new Method();
                        if (v.Tokens[2] is TokenOpenParen)
                            m.Name = ".ctor";
                        else
                            m.Name = ((TokenWord)v.Tokens[2]).Word;

                        if (v.Tokens.Count != 5 + (v.Tokens[2] is TokenOpenParen ? 0 : 1))//tab def name openP closeP newline
                        {
                            for (int i = 3 + (v.Tokens[2] is TokenOpenParen ? 0 : 1); i < v.Tokens.Count - 2; i++)
                            {
                                m.paramNames.Add(((TokenWord)v.Tokens[i]).Word);
                                i++;
                            }
                        }
                        int tabl = ((TokenTab)v.Tokens[0]).TabIndex + 1;
                        index++;
                        for (; index < @class.Count; index++)
                        {
                            if (((TokenTab)@class[index].Tokens[0]).TabIndex < tabl)
                            {
                                index--;
                                break;
                            }
                            m.Lines.Add(@class[index]);
                        }
                        //index++;
                        c.Methods.Add(m);
                    }
                }

            }

            StringBuilder sb = new StringBuilder();

            foreach (var tokenMacroPiece in allMacros)
            {
                sb.Append(tokenMacroPiece.ToString());
            }

            sb.AppendLine();

            foreach (var someClass in someClasses)
            {
                Method ctor = someClass.Methods.FirstOrDefault(a => a.Name == ".ctor");
                if (ctor == null)
                {
                    someClass.Methods.Add(ctor = new Method());
                    ctor.Name = ".ctor";
                }
                for (int index = 0; index < someClass.Variables.Count; index++)
                {
                    var lineToken = someClass.Variables[index];
                    ((TokenTab)lineToken.Tokens[0]).TabIndex++;
                    someClass.VariableNames.Add(((TokenWord)lineToken.Tokens[1]).Word);
                    ctor.Lines.Insert(index, lineToken);
                }
                someClass.Variables.Clear();

                sb.AppendLine(someClass.ToString());
            }

            File.WriteAllText("C:\\spoke.txt", sb.ToString());
            //Console.WriteLine(sb);
            return new Tuple<List<Class>, List<TokenMacroPiece>>(someClasses, allMacros);
        }

        public interface ITokenMacroParamType
        {

        }
        public class MacroParamArray : ITokenMacroParamType
        {
            public string Type;

            public MacroParamArray(string word)
            {
                Type = word;
            }
        }
        public class MacroParamType : ITokenMacroParamType
        {
            public string Type;

            public MacroParamType(string word)
            {
                Type = word;
            }
        }
        public class MacroParamClosure : ITokenMacroParamType
        {
            public List<ITokenMacroParamType> Types = new List<ITokenMacroParamType>();

            public MacroParamClosure()
            {

            }

        }
        private ITokenMacroParamType consumeMacroParamValue(TokenEnumerator en)
        {

            ITokenMacroParamType type = null;
            if (en.Current.Type == Token.Word)
            {
                switch (((TokenWord)en.Current).Word.ToLower())
                {
                    case "array":
                        en.MoveNext();
                        if (en.Current.Type != Token.Less)
                            throw new Exception("bad array");

                        en.MoveNext();
                        if (en.Current.Type != Token.Word)
                            throw new Exception("bad array type");

                        type = new MacroParamArray(((TokenWord)en.Current).Word);

                        en.MoveNext();
                        if (en.Current.Type != Token.Greater)
                            throw new Exception("bad array");

                        en.MoveNext();
                        break;
                    case "closure":
                        en.MoveNext();
                        type = new MacroParamClosure();
                        if (en.Current.Type != Token.OpenParen)
                            throw new Exception("bad closure");
                        do
                        {
                            en.MoveNext();

                            var ty = consumeMacroParamValue(en);

                            ((MacroParamClosure)type).Types.Add(ty);
                            switch (en.Current.Type)
                            {
                                case Token.Comma:
                                    continue;
                                case Token.CloseParen:
                                    en.MoveNext();
                                    return type;
                                    break;
                            }
                        } while (true);

                        break;
                    default:


                        type = new MacroParamType(((TokenWord)en.Current).Word);

                        en.MoveNext();
                        break;
                }
            }
            return type;

        }

        private IToken addWord(string g, ref int firstHalfOfFloat)
        {
            int intf;
            if (int.TryParse(g, out intf))
            {
                if (firstHalfOfFloat > int.MinValue)
                {
                    int d = firstHalfOfFloat;
                    firstHalfOfFloat = int.MinValue;
                    return new TokenFloat(float.Parse(d + "." + intf));
                }
                firstHalfOfFloat = int.MinValue;
                return new TokenInt(intf);
            }


            switch (g.ToLower())
            {
                case "def":
                    return new TokenDef();
                    break;
                case "create":
                    return new TokenCreate();
                    break;
                case "return":
                    return new TokenReturn();
                    break;
                case "yield":
                    return new TokenYield();
                    break;
                case "class":
                    return new TokenClass();
                    break;
                case "macro":
                    return new TokenMacro();
                    break;
                case "if":
                    return new TokenIf();
                    break;
                case "else":
                    return new TokenElse();
                    break;
                case "false":
                    return new TokenFalse();
                case "true":
                    return new TokenTrue();
                default:
                    return new TokenWord(g);
                    break;
            }
        }
        public IEnumerable<IToken> getWords(string b)
        {
            int firstHalfOfFloat = int.MinValue;

            bool wasSpace = false;
            string addToWord = "";
            b = b.Replace("    ", "  \t");
            b = b.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            b = b.Replace("  \t ", " ");
            b = b.Replace("\r\n", "\r");
            b = b.Replace("\n", "\r");
            int tabCount = 0;
            string[] toks = new string[] { ";", "?", "[", "{", "}", "]", "^", "(", "|", ")", "\"", ",", ".", ";", ":", "&", "!", "+", "-", "/", "*", "<", ">", "=", "  \t", "\r" };
            bool breakTilNewLine = false;
            bool stringStart = false;
            for (int i = 0; i < b.Length; i++)
            {

                if (breakTilNewLine)
                {
                    if (b[i] == '\r')
                    {
                        breakTilNewLine = false;
                    }
                    continue;

                }
                if (b[i] != '\t' && tabCount > 0)
                {
                    yield return new TokenTab(tabCount);
                    tabCount = 0;
                }

                if (stringStart && b[i] != '\"')
                {

                    addToWord += b[i];
                    continue;
                }
                switch (b[i])
                {
                    case '@':
                        if (b[i+1] == '@')
                            breakTilNewLine = true;
                        break;
                    case ' ':

                        if (wasSpace || addToWord.Length == 0)
                        {
                            continue;
                        }
                        yield return addWord(addToWord, ref firstHalfOfFloat);
                        addToWord = "";
                        break;
                    case '\t':
                        tabCount++;
                        break;
                    default:
                        if (!stringStart && toks.Contains(b[i].ToString()) && addToWord.Length > 0)
                        {
                            int fb;
                            int df;
                            if (b[i] == '.' && int.TryParse(addToWord, out fb) && int.TryParse(b[i + 1].ToString(), out df))
                            {
                                i++;
                                firstHalfOfFloat = fb;
                            }
                            else
                                yield return addWord(addToWord, ref firstHalfOfFloat);
                            addToWord = "";
                        }
                        switch (b[i])
                        {
                            case '[':
                                yield return new TokenOpenSquare();
                                break;
                            case '{':
                                yield return new TokenOpenCurly();
                                break;
                            case '}':

                                yield return new TokenCloseCurly();
                                break;
                            case '|':

                                if (b[i + 1] == '|')
                                {
                                    i++;
                                    yield return new TokenDoubleOr();
                                }
                                else
                                    yield return new TokenBar();
                                break;
                            case ']':
                                yield return new TokenCloseSquare();

                                break;
                            case '!': if (b[i + 1] == '=')
                                {
                                    i++;
                                    yield return new TokenNotEqual();
                                }
                                else
                                    yield return new TokenNot();

                                break;
                            case '(':
                                yield return new TokenOpenParen();

                                break;
                            case ')':
                                yield return new TokenCloseParen();

                                break;
                            case '\"':

                                if (stringStart)
                                {
                                    yield return new TokenString(addToWord);
                                }
                                addToWord = "";
                                stringStart = !stringStart;

                                break;
                            case ',':
                                yield return new TokenComma();

                                break;
                            case '.':
                                yield return new TokenPeriod();

                                break;
                            case '+':
                                yield return new TokenPlus();
                                break;
                            case '-':
                                yield return new TokenMinus();

                                break;
                            case '/':
                                yield return new TokenDivide();

                                break;
                            case '*':
                                yield return new TokenMulitply();

                                break;
                            case '<':
                                yield return new TokenLess();

                                break;
                            case '>':
                                yield return new TokenGreater();

                                break;
                            case ';':
                                yield return new TokenSemiColon();

                                break;
                            case '?':
                                yield return new TokenQuestionMark();

                                break;
                            case ':':
                                yield return new TokenColon();

                                break;
                            case '^':
                                yield return new TokenCarot();

                                break;
                            case '=':

                                if (b[i + 1] == '>')
                                {
                                    i++;
                                    yield return new TokenAnonMethodStart();
                                }
                                else
                                    if (b[i + 1] == '=')
                                    {
                                        i++;
                                        yield return new TokenDoubleEqual();
                                    }
                                    else
                                        yield return new TokenEqual();

                                break;


                            case '&':

                                if (b[i + 1] == '&')
                                {
                                    i++;
                                    yield return new TokenDoubleAnd();
                                }
                                else
                                    yield return new TokenAmpersand();

                                break;

                            case '\r':
                                yield return new TokenNewLine();

                                break;
                            default:

                                addToWord += b[i];

                                break;
                        }

                        break;
                }
            }

        }

    }
}