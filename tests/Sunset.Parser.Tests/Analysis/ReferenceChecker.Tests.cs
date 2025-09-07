using Sunset.Markdown.Extensions;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Analysis;

[TestFixture]
public class ReferenceCheckerTests
{
    [Test]
    public void Parse_SingleCircularReference_DetectsError()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 13 + y
                                               y = 25 + x
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.ChildScopes["$file"].ChildDeclarations["x"].ContainsError<CircularReferenceError>());
        Assert.That(environment.ChildScopes["$file"].ChildDeclarations["y"].ContainsError<CircularReferenceError>());
    }

    [Test]
    public void Parse_IndirectCircularReference_DetectsError()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 13 + z
                                               y = 12 + x
                                               z = 11 + y
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.Multiple(() =>
        {
            Assert.That(environment.ChildScopes["$file"].ChildDeclarations["x"]
                .ContainsError<CircularReferenceError>());
            Assert.That(environment.ChildScopes["$file"].ChildDeclarations["y"]
                .ContainsError<CircularReferenceError>());
            Assert.That(environment.ChildScopes["$file"].ChildDeclarations["z"]
                .ContainsError<CircularReferenceError>());
        });
    }

    [Test]
    public void Parse_PartialCircularReference_DetectsError()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 13 + z
                                               y = 12 + x
                                               z = 11 + y
                                               a = 45
                                               b = 12 + a
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.Multiple(() =>
        {
            Assert.That(environment.ChildScopes["$file"].ChildDeclarations["x"]
                .ContainsError<CircularReferenceError>());
            Assert.That(environment.ChildScopes["$file"].ChildDeclarations["y"]
                .ContainsError<CircularReferenceError>());
            Assert.That(environment.ChildScopes["$file"].ChildDeclarations["z"]
                .ContainsError<CircularReferenceError>());
            Assert.That(environment.ChildScopes["$file"].ChildDeclarations["a"].ContainsError<CircularReferenceError>(),
                Is.False);
            Assert.That(environment.ChildScopes["$file"].ChildDeclarations["b"].ContainsError<CircularReferenceError>(),
                Is.False);
        });
    }
}