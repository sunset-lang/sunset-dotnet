using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens.Numbers;

public class IntToken(int value, int positionStart, int positionEnd, int lineStart, int columnEnd, SourceFile file)
    : NumberTokenBase<int>(value, positionStart, positionEnd, lineStart, columnEnd, file);