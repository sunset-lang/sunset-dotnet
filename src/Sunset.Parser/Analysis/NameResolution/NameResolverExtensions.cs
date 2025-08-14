using Sunset.Parser.Abstractions;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

public static class NameResolverExtensions
{
    private const string PassDataKey = "NameResolver";

    public static IDeclaration? GetResolvedDeclaration(this IVisitable dest)
    {
        return dest.GetPassData<NamePassData>(PassDataKey).ResolvedDeclaration;
    }

    public static void SetResolvedDeclaration(this IVisitable dest, IDeclaration declaration)
    {
        dest.GetPassData<NamePassData>(PassDataKey).ResolvedDeclaration = declaration;
    }
}