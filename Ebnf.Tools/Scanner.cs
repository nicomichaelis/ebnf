using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Ebnf.Tools;

public record TextPosition(string SourceIdentifier, int Line, int Col)
{
}

public enum TokenType
{
    EndOfFile, Unknown, Runaway,
    Period, Bar, Assign,
    OpenParen, OpenBracket, OpenCurly, CloseParen, CloseBracket, CloseCurly,
    Identifer, Literal
}

public record Token(TextPosition Position, TokenType Type, string Text)
{
}

public class Scanner : IDisposable
{
    CharScanner Char { get; set; }
    StringBuilder m_Builder = new StringBuilder();
    Token? m_Current;
    Dictionary<int, TokenType> m_SingleCharTokens = new Dictionary<int, TokenType>()
    {
        ['.'] = TokenType.Period,
        ['|'] = TokenType.Bar,
        ['='] = TokenType.Assign,
        ['('] = TokenType.OpenParen,
        ['['] = TokenType.OpenBracket,
        ['{'] = TokenType.OpenCurly,
        [')'] = TokenType.CloseParen,
        [']'] = TokenType.CloseBracket,
        ['}'] = TokenType.CloseCurly,
    };

    public Scanner(CharScanner cscanner)
    {
        Char = cscanner;
    }

    public Token Current
    {
        get
        {
            if (m_Current == null) ReadToken();
            return m_Current!;
        }
    }

    public void ReadToken()
    {
        if (Char == null) throw new ObjectDisposedException(GetType().FullName);

        if (m_Current == null)
        {
            Char.GetChar(); // initial read
        }
        else if (m_Current.Type == TokenType.EndOfFile)
        {
            return;
        }

        m_Builder.Clear();

        SkipWhitespace();
        var currentChar = Char.Current;
        TextPosition tokenPos = Char.Position;

        if (Char.Current < 0)
        {
            m_Current = new Token(Char.Position, TokenType.EndOfFile, "");
        }
        else if (IsLetter(currentChar))
        {
            m_Current = ReadIdentifier(currentChar, tokenPos);
        }
        else if (IsQuote(currentChar))
        {
            m_Current = ReadLiteral(currentChar, tokenPos);
        }
        else if (m_SingleCharTokens.TryGetValue(currentChar, out var type))
        {
            m_Current = new Token(tokenPos, type, ((char)currentChar).ToString());
            Char.GetChar();
        }
        else
        {
            m_Current = new Token(tokenPos, TokenType.Unknown, ((char)currentChar).ToString());
            Char.GetChar();
        }
    }

    void SkipWhitespace()
    {
        while (IsWhitespace(Char.Current) || Char.IsLinebreak || Char.Current == '#')
        {
            if (Char.Current == '#')
            {
                while (Char.Current >= 0 && !Char.IsLinebreak) Char.GetChar();
            }
            else
                Char.GetChar();
        }
    }

    Token ReadIdentifier(int currentChar, TextPosition tokenPos)
    {
        do
        {
            m_Builder.Append((char)currentChar);
            currentChar = Char.GetChar();
        } while (IsLetter(currentChar) || IsDigit(currentChar));
        string text = m_Builder.ToString();

        return new Token(tokenPos, TokenType.Identifer, text);
    }

    Token ReadLiteral(int currentChar, TextPosition tokenPos)
    {
        var endQuote = currentChar;
        do
        {
            m_Builder.Append((char)currentChar);
            currentChar = Char.GetChar();
        } while ((endQuote != currentChar) && (currentChar >= 0) && (!Char.IsLinebreak));

        TokenType type = TokenType.Runaway;
        if (currentChar == endQuote)
        {
            m_Builder.Append((char)currentChar);
            currentChar = Char.GetChar();
            type = TokenType.Literal;
        }
        return new Token(tokenPos, type, m_Builder.ToString());
    }

    bool IsQuote(int ch)
    {
        return (ch == '\'') || (ch == '"');
    }

    bool IsWhitespace(int ch)
    {
        return (ch == ' ')
            || (ch == '\t')
            || (ch > 255 && CharUnicodeInfo.GetUnicodeCategory((char)ch) == UnicodeCategory.SpaceSeparator);
    }

    bool IsLetter(int ch)
    {
        return
              (ch >= 'a' & ch <= 'z')
            | (ch >= 'A' & ch <= 'Z')
            | (ch == '_')
            ;
    }

    bool IsDigit(int ch)
    {
        return (ch >= '0' && ch <= '9');
    }

    public void Dispose()
    {
        var temp = Char;
        try
        {
            if (temp != null) temp.Dispose();
        }
        finally
        {
            Char = null!;
            m_Builder = null!;
        }
    }
}
