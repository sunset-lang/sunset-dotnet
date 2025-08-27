using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Parsing.Declarations;

public class SymbolName
{
    public readonly string Name;

    /// <summary>
    ///     If the symbol can also be used as a name, use it
    /// </summary>
    public readonly StringToken? NameToken;

    public readonly IToken[] Tokens;

    public SymbolName(IEnumerable<IToken> tokens)
    {
        var enumerable = tokens as IToken[] ?? tokens.ToArray();
        Tokens = enumerable;

        CheckSymbol();
        Name = string.Join(" ", enumerable.Select(t => t.ToString()));
        // Remove spaces if they occur after a backslash
        Name = Name.Replace("\\ ", "\\");

        // If the symbol can also be used as a name (i.e. it is made up of a single token, note that it can also be used as a name.
        if (Tokens.Length == 1 && Tokens.First() is StringToken nameToken)
        {
            NameToken = nameToken;
        }
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