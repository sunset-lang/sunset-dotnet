using System.Numerics;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens.Numbers;

public abstract class NumberTokenBase<T>(
    T value,
    int positionStart,
    int positionEnd,
    int lineStart,
    int columnEnd,
    SourceFile file)
    : ValueTokenBase<T>(value, TokenType.Number, positionStart, positionEnd, lineStart, columnEnd, file),
        INumberToken where T : INumber<T>;