using System;
using System.Linq;

namespace Ebnf.Tools;

public class Parser
{
    Scanner Scanner { get; }
    Token? LastError { get; set; }

    public Parser(Scanner scanner)
    {
        Scanner = scanner;
    }

    public void Syntax()
    {
        while (Scanner.Current.Type == TokenType.Identifer)
        {
            Production();
        }
        Expect(TokenType.EndOfFile);
    }

    public void Production()
    {
        Expect(TokenType.Identifer);
        Expect(TokenType.Assign);
        Expression();
        Expect(TokenType.Period);
    }

    public void Expression()
    {
        Term();
        while (Scanner.Current.Type == TokenType.Bar)
        {
            Scanner.ReadToken();
            Term();
        }
    }

    public void Term()
    {
        Factor();
        var curt = Scanner.Current.Type;
        while (curt == TokenType.Identifer || curt == TokenType.Literal || curt == TokenType.OpenParen || curt == TokenType.OpenBracket || curt == TokenType.OpenCurly)
        {
            Factor();
            curt = Scanner.Current.Type;
        }
    }

    public void Factor()
    {
        Token tok = Scanner.Current;
        if (Expect(TokenType.Identifer, TokenType.Literal, TokenType.OpenParen, TokenType.OpenBracket, TokenType.OpenCurly))
        {
            switch (tok.Type)
            {
                case TokenType.OpenParen:
                    Expression();
                    Expect(TokenType.CloseParen);
                    break;
                case TokenType.OpenBracket:
                    Expression();
                    Expect(TokenType.CloseBracket);
                    break;
                case TokenType.OpenCurly:
                    Expression();
                    Expect(TokenType.CloseCurly);
                    break;
                case TokenType.Identifer:
                    break;
                case TokenType.Literal:
                    break;
                default: throw new NotImplementedException();
            }
        }
    }

    private bool Expect(params TokenType[] types)
    {
        var curt = Scanner.Current.Type;
        bool res = types.Any(z => z == curt);
        if (!res)
        {
            Mark($"'{(string.Join("', '", types))}' expected");
        }
        else Scanner.ReadToken();
        return res;
    }

    private void Sync(params TokenType[] types)
    {
        while (!(Scanner.Current.Type == TokenType.EndOfFile) && !types.Any(z => z == Scanner.Current.Type))
        {
            Scanner.ReadToken();
        }
    }

    private void Mark(string message)
    {
        var cur = Scanner.Current;
        if (LastError != cur)
        {
            Console.WriteLine($"{cur.Position.SourceIdentifier} ({cur.Position.Line}, {cur.Position.Col}): {message}");
        }
        LastError = cur;
    }
}