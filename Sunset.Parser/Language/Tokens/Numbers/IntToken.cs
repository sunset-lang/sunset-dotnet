namespace Northrop.Common.Sunset.Language;

public class IntToken(int value, int positionStart, int positionEnd, int lineStart, int columnEnd)
    : NumberTokenBase<int>(value, positionStart, positionEnd, lineStart, columnEnd);