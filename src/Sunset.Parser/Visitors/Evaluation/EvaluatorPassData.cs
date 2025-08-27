using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Quantities.Quantities;

namespace Sunset.Parser.Visitors.Evaluation;

public class EvaluatorPassData : IPassData
{
    /// <summary>
    /// A dictionary containing the results of each function when evaluated in each scope.
    /// </summary>
    public Dictionary<IScope, IResult?> Results { get; } = [];
}