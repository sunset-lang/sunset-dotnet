using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Parsing.Declarations;

public class SymbolName
{
    public readonly string Name;
    public readonly IToken[] Tokens;

    public SymbolName(IEnumerable<IToken> tokens)
    {
        var enumerable = tokens as IToken[] ?? tokens.ToArray();
        Tokens = enumerable;

        CheckSymbol();
        Name = string.Join(" ", enumerable.Select(t => t.ToString()));
    }

    public void CheckSymbol()
    {
        // TODO: Implement symbol checking
    }

    public override string ToString()
    {
        return Name;
    }
}