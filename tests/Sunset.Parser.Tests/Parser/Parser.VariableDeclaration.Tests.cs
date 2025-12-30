using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserVariableDeclarationTests
{
    [Test]
    public void GetVariableDeclaration_WithValidInput_CorrectDeclaration()
    {
        // Use Environment to get access to standard library units
        var sourceFile = SourceFile.FromString("area <A> {mm^2} = 100 {mm} * 200 {mm}");
        var environment = new Environment(sourceFile);
        environment.Analyse(); // Run name resolution and type checking

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var variable = fileScope!.ChildDeclarations.GetValueOrDefault("area") as VariableDeclaration;
        Assert.That(variable, Is.Not.Null);

        var stringRepresentation = DebugPrinter.Singleton.PrintVariableDeclaration(variable!);
        // Unit names are now fully qualified paths from the standard library
        Assert.That(stringRepresentation, Is.EqualTo("area <A> {} = (* (assign 100 $env.$stdlib.mm) (assign 200 $env.$stdlib.mm))"));
    }

    [Test]
    public void GetVariableDeclaration_WithComplexUnit_CorrectDeclaration()
    {
        // Use Environment to get access to standard library units
        var sourceFile = SourceFile.FromString("force <F> {kN} = 100 {kg} * 200 {m} / (400 {s})^2");
        var environment = new Environment(sourceFile);
        environment.Analyse(); // Run name resolution and type checking

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var variable = fileScope!.ChildDeclarations.GetValueOrDefault("force") as VariableDeclaration;
        Assert.That(variable, Is.Not.Null);

        var stringRepresentation = DebugPrinter.Singleton.PrintVariableDeclaration(variable!);
        // Unit names are now fully qualified paths from the standard library
        Assert.That(stringRepresentation,
            Is.EqualTo("force <F> {} = (/ (* (assign 100 $env.$stdlib.kg) (assign 200 $env.$stdlib.m)) (^ (assign 400 $env.$stdlib.s) 2))"));
    }

    [Test]
    public void GetVariableDeclaration_WithGreekLetter_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
                                        phi <\phi> = 35
                                        """));
        var variable = parser.GetVariableDeclaration(new FileScope("$", null));

        Assert.That(variable.Variable.Symbol, Is.EqualTo("\\phi"));
    }
}