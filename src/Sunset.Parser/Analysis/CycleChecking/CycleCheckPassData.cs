using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.CycleChecking;

public class CycleCheckPassData : IPassData
{
    public DependencyCollection? Dependencies { get; set; }
}