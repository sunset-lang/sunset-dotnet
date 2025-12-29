using Sunset.Parser.BuiltIns;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

public class NamePassData : IPassData
{
    public IDeclaration? ResolvedDeclaration { get; set; }

    /// <summary>
    /// If this call expression targets a built-in function, stores the function instance.
    /// </summary>
    public IBuiltInFunction? BuiltInFunction { get; set; }
}