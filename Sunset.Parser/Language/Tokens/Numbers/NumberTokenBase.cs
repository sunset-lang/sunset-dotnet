using System.Numerics;

namespace Sunset.Parser.Language.Tokens.Numbers;

public abstract class NumberTokenBase<T>(T value, int positionStart, int positionEnd, int lineStart, int columnEnd)
    : ValueTokenBase<T>(value, TokenType.Number, positionStart, positionEnd, lineStart, columnEnd),
        INumberToken where T : INumber<T>;