using System.Text;

namespace Sunset.Parser;

public static class StringBuilderUtilities
{
    public static StringBuilder AppendIndented(this StringBuilder builder, string toAppend, int indentLevel)
    {
        builder.Append(' ', indentLevel * 4);
        builder.Append(toAppend);
        return builder;
    }

    public static StringBuilder AppendIndented(this StringBuilder builder, char toAppend, int indentLevel)
    {
        builder.Append(' ', indentLevel * 4);
        builder.Append(toAppend);
        return builder;
    }

    public static StringBuilder AppendIndentedLine(this StringBuilder builder, string toAppend, int indentLevel)
    {
        builder.Append(' ', indentLevel * 4);
        builder.AppendLine(toAppend);
        return builder;
    }
}