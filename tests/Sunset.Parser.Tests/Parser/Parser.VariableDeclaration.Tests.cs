using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserVariableDeclarationTests
{
    /// <summary>
    /// Gets a variable declaration by running full analysis pipeline.
    /// This is needed because unit symbols are resolved during name resolution.
    /// </summary>
    private VariableDeclaration GetAnalyzedVariableDeclaration(string input)
    {
        var sourceFile = SourceFile.FromString(input);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        if (fileScope is null)
        {
            throw new Exception("File scope not found.");
        }

        var declaration = fileScope.ChildDeclarations.Values.OfType<VariableDeclaration>().FirstOrDefault();
        if (declaration is null)
        {
            throw new Exception("Variable declaration not found.");
        }

        return declaration;
    }

    [Test]
    public void GetVariableDeclaration_WithValidInput_CorrectDeclaration()
    {
        var variable = GetAnalyzedVariableDeclaration("area <A> {mm^2} = 100 {mm} * 200 {mm}");
        var stringRepresentation = DebugPrinter.Singleton.PrintVariableDeclaration(variable);

        Assert.That(stringRepresentation, Is.EqualTo("area <A> {mm^2} = (* (assign 100 mm) (assign 200 mm))"));
    }

    [Test]
    public void GetVariableDeclaration_WithComplexUnit_CorrectDeclaration()
    {
        var variable = GetAnalyzedVariableDeclaration("force <F> {kN} = 100 {kg} * 200 {m} / (400 {s})^2");
        var stringRepresentation = DebugPrinter.Singleton.PrintVariableDeclaration(variable);

        Assert.That(stringRepresentation,
            Is.EqualTo("force <F> {kN} = (/ (* (assign 100 kg) (assign 200 m)) (^ (assign 400 s) 2))"));
    }

    [Test]
    public void GetVariableDeclaration_WithGreekLetter_CorrectDeclaration()
    {
        var variable = GetAnalyzedVariableDeclaration("phi <\\phi> = 35");

        Assert.That(variable.Variable.Symbol, Is.EqualTo("\\phi"));
    }
}
