using Sunset.Parser.Results.Types;
using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Analysis.TypeChecking;

public class TypeCheckPassData : IPassData
{
    public IResultType? AssignedType { get; set; }
    public IResultType? EvaluatedType { get; set; }
}