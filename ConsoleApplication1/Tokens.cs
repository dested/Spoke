using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{

    public enum Token
    {
        OpenSquare = '[', CloseSquare = ']', OpenCurly = '{', CloseCurly = '}', OpenParen = '(', CloseParen = ')', String = '\"', Comma = ',', Period = '.', Plus = '+', Minus = '-', Divide = '/', Mulitply = '*', Greater = '>', Less = '<', Equal = '=', Tab = 0, Def = 1, Class = 2, Create = 3, Return = 4, Word = 5, Int = 6, Float = 7, If = 8, Else = 9,
        NewLine = 10, DoubleEqual = 12,
        AnonMethodStart = 11,
        Bar = '|',
        Yield = 17,
        EndOfCodez = 13,
        Macro = 20,
        DoubleAnd = 15,
        DoubleOr = 16,
        Not = '!',
        NotEqual = 14,
        True = 18,
        False = 19,
        Ampersand = '&',
        SemiColon = ';', Colon = ':',
        QuestionMark = '?',
        Carot = '^'
    }
    public class TokenOpenSquare : IToken { public Token Type { get { return Token.OpenSquare; } } public TokenOpenSquare() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenCloseSquare : IToken { public Token Type { get { return Token.CloseSquare; } } public TokenCloseSquare() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenNot : IToken { public Token Type { get { return Token.Not; } } public TokenNot() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenOpenCurly : IToken { public Token Type { get { return Token.OpenCurly; } } public TokenOpenCurly() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenCloseCurly : IToken { public Token Type { get { return Token.CloseCurly; } } public TokenCloseCurly() { } public override string ToString() { return ((char)Type).ToString(); } }

    public class TokenColon : IToken { public Token Type { get { return Token.Colon; } } public TokenColon() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenSemiColon : IToken { public Token Type { get { return Token.SemiColon; } } public TokenSemiColon() { } public override string ToString() { return ((char)Type).ToString(); } }

    public class TokenOpenParen : IToken { public Token Type { get { return Token.OpenParen; } } public TokenOpenParen() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenCloseParen : IToken { public Token Type { get { return Token.CloseParen; } } public TokenCloseParen() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenString : IToken
    {
        public readonly string _value;
        public Token Type { get { return Token.String; } }
        public TokenString(string value)
        {
            _value = value;
        }

        public override string ToString() { return "'" + _value + "'"; }
    }
    public class TokenInt : IToken
    {
        public readonly int _value;
        public Token Type { get { return Token.Int; } }
        public TokenInt(int value)
        {
            _value = value;
        }

        public override string ToString() { return _value.ToString(); }
    }
    public class TokenFloat : IToken
    {
        public readonly float _value;
        public Token Type { get { return Token.Float; } }
        public TokenFloat(float value)
        {
            _value = value;
        }

        public override string ToString() { return _value.ToString(); }
    }
    public class TokenEndOfCodez : IToken { public Token Type { get { return Token.EndOfCodez; } } public TokenEndOfCodez() { } public override string ToString() { return "\r\nEOF"; } }


    public class TokenComma : IToken { public Token Type { get { return Token.Comma; } } public TokenComma() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenPeriod : IToken { public Token Type { get { return Token.Period; } } public TokenPeriod() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenPlus : IToken { public Token Type { get { return Token.Plus; } } public TokenPlus() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenQuestionMark : IToken { public Token Type { get { return Token.QuestionMark; } } public TokenQuestionMark() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenMinus : IToken { public Token Type { get { return Token.Minus; } } public TokenMinus() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenDivide : IToken { public Token Type { get { return Token.Divide; } } public TokenDivide() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenMulitply : IToken { public Token Type { get { return Token.Mulitply; } } public TokenMulitply() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenGreater : IToken { public Token Type { get { return Token.Greater; } } public TokenGreater() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenLess : IToken { public Token Type { get { return Token.Less; } } public TokenLess() { } public override string ToString() { return ((char)Type).ToString(); } }

    public class TokenAnonMethodStart : IToken { public Token Type { get { return Token.AnonMethodStart; } } public TokenAnonMethodStart() { } public override string ToString() { return "=>"; } }

    public class TokenAmpersand : IToken { public Token Type { get { return Token.Ampersand; } } public TokenAmpersand() { } public override string ToString() { return ((char)Type).ToString(); } }


    public class TokenDoubleAnd : IToken { public Token Type { get { return Token.DoubleAnd; } } public TokenDoubleAnd() { } public override string ToString() { return "&&"; } }
    public class TokenDoubleOr : IToken { public Token Type { get { return Token.DoubleOr; } } public TokenDoubleOr() { } public override string ToString() { return "||"; } }
    public class TokenEqual : IToken { public Token Type { get { return Token.Equal; } } public TokenEqual() { } public override string ToString() { return ((char)Type).ToString(); } }
    public class TokenDoubleEqual : IToken { public Token Type { get { return Token.DoubleEqual; } } public TokenDoubleEqual() { } public override string ToString() { return "=="; } }
    public class TokenNotEqual : IToken { public Token Type { get { return Token.NotEqual; } } public TokenNotEqual() { } public override string ToString() { return "!="; } }
    public class TokenNewLine : IToken { public Token Type { get { return Token.NewLine; } } public TokenNewLine() { } public override string ToString() { return ""; } }
    public class TokenDef : IToken { public Token Type { get { return Token.Def; } } public TokenDef() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenClass : IToken { public Token Type { get { return Token.Class; } } public TokenClass() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenMacro : IToken { public Token Type { get { return Token.Macro; } } public TokenMacro() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenCreate : IToken { public Token Type { get { return Token.Create; } } public TokenCreate() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenReturn : IToken { public Token Type { get { return Token.Return; } } public TokenReturn() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenIf : IToken { public Token Type { get { return Token.If; } } public TokenIf() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenElse : IToken { public Token Type { get { return Token.Else; } } public TokenElse() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenTrue : IToken { public Token Type { get { return Token.True; } } public TokenTrue() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenFalse : IToken { public Token Type { get { return Token.False; } } public TokenFalse() { } public override string ToString() { return Type.ToString() + " "; } }
    public class TokenBar : IToken
    {
        public Token Type { get { return Token.Bar; } } public TokenBar() { }
        public override string ToString()
        {
            return ((char)Type).ToString();
        }
    }
    public class TokenYield : IToken { public Token Type { get { return Token.Yield; } } public TokenYield() { } public override string ToString() { return "Yield" + " "; } }
    public class TokenCarot : IToken { public Token Type { get { return Token.Carot; } } public TokenCarot() { } public override string ToString() { return "^"; } }

    public class TokenTab : IToken
    {
        public int TabIndex { get; set; }
        public Token Type { get { return Token.Tab; } }
        public TokenTab(int tabIndex)
        {
            TabIndex = tabIndex;
        }
        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < TabIndex; i++)
            {
                s += "  \t";
            }
            return s;
        }

    }
    public class TokenWord : IToken
    {
        public string Word { get; set; }
        public Token Type { get { return Token.Word; } }
        public TokenWord(string word)
        {

            Word = word;
        }
        public override string ToString()
        {
            return Word;
        }
    }
    public interface IToken
    {
        Token Type { get; }
    }
}
