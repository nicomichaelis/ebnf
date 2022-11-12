using System;
using System.IO;

namespace Ebnf.Tools;

public class Program
{
    public static int Main(string[] args)
    {
        using var rdr = File.OpenText(args[0]);
        Scanner scan = new Scanner(new CharScanner(rdr, args[0]));
        Parser parse = new Parser(scan);
        parse.Syntax();
        return 0;
    }
}