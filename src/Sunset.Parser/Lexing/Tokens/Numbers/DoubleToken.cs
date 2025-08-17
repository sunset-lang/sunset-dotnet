namespace Sunset.Parser.Lexing.Tokens.Numbers;

public class DoubleToken(double value, int positionStart, int positionEnd, int lineStart, int columnEnd)
    : NumberTokenBase<double>(value, positionStart, positionEnd, lineStart, columnEnd);