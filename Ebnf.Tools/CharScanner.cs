using System;
using System.IO;

namespace Ebnf.Tools;

public class CharScanner
{
    TextReader _Reader;
    readonly string _sourceIdent;
    readonly bool _LeaveOpen;

    int _CurChar;
    int _Line;
    int _Col;

    public CharScanner(TextReader reader, string sourceIdent, int initialLine, int initialCol, bool leaveOpen)
    {
        _Reader = reader;
        _sourceIdent = sourceIdent;
        _Line = initialLine;
        _Col = initialCol;
        _CurChar = -1;
        IsLinebreak = true;
        _LeaveOpen = leaveOpen;
    }

    public CharScanner(TextReader reader, string sourceIdent, bool leaveOpen = false) : this(reader, sourceIdent, 0, 0, leaveOpen)
    {

    }

    public TextPosition Position => new TextPosition(_sourceIdent, _Line, _Col);

    public int Current => _CurChar;
    public bool IsLinebreak { get; private set; }

    public int GetChar()
    {
        if (_Reader == null) throw new ObjectDisposedException(GetType().FullName);

        if (IsLinebreak)
        {
            _Col = 0;
            _Line = _Line + 1;
            IsLinebreak = false;
        }

        _Col = _Col + 1;
        ReadOne();

        switch (_CurChar)
        {
            case '\n':
            case 0x000B: // vertical tab
            case 0x000C: // form feed
            case 0x0085: // next line
            case 0x2028: // line separator
            case 0x2029: // paragraph separator
                IsLinebreak = true;
                break;
            case '\r':
                IsLinebreak = true;
                if (_Reader.Peek() == '\n')
                {
                    var temp = _CurChar;
                    ReadOne();
                    _CurChar = temp;
                }
                break;
            default:
                break;
        }
        return _CurChar;

        void ReadOne()
        {
            _CurChar = _Reader.Read();
        }
    }

    public void Dispose()
    {
        if (_Reader != null)
        {
            try
            {
                if (!_LeaveOpen) _Reader.Dispose();
            }
            finally
            {
                _Reader = null!;
            }
        }
    }
}
