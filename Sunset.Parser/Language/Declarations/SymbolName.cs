using Sunset.Parser.Language.Tokens;

namespace Sunset.Parser.Language.Declarations;

public class SymbolName
{
    public readonly IToken[] Tokens;
    public readonly string Name;

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