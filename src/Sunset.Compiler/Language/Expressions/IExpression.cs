namespace Sunset.Compiler.Language;

public interface IExpression
{
    public void Parse(Parser parser);

    public Token Token { get; set; }

    public IExpression GetExpression(Token token);
    
    public IExpression Left { get; set; }

}