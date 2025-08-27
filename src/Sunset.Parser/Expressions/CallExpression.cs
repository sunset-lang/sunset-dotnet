using Sunset.Parser.Errors;
using Sunset.Parser.Parsing;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

internal class CallExpression(IExpression target, List<Argument> arguments) : IExpression
{
    public IExpression Target { get; } = target;
    public List<Argument> Arguments { get; } = arguments;
    public Dictionary<string, IPassData> PassData { get; } = [];
    public List<IError> Errors { get; } = [];
}