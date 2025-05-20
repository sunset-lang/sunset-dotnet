namespace Sunset.Compiler.Language;

public interface IInfixExpression : IExpression
{
    public IExpression Right { get; set; }
}