using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

public class NamePassData : IPassData
{
    public IDeclaration? ResolvedDeclaration { get; set; }
}