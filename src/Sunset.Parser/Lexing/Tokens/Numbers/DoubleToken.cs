using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens.Numbers;

public class DoubleToken(
    double value,
    int positionStart,
    int positionEnd,
    int lineStart,
    int columnEnd,
    SourceFile file)
    : NumberTokenBase<double>(value, positionStart, positionEnd, lineStart, columnEnd, file);