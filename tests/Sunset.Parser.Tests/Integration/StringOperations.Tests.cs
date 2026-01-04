using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class StringOperationsTests
{
    #region String Concatenation Tests

    [Test]
    public void Analyse_StringPlusString_ConcatenatesStrings()
    {
        var sourceFile = SourceFile.FromString("""
                                               Message = "hello " + "world"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Message", new StringResult("hello world"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_StringPlusQuantity_FormatsQuantity()
    {
        var sourceFile = SourceFile.FromString("""
                                               Length {m} = 100 {m}
                                               Message = "The length is " + Length
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Message", new StringResult("The length is 100 m"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_QuantityPlusString_FormatsQuantity()
    {
        var sourceFile = SourceFile.FromString("""
                                               Width {mm} = 50 {mm}
                                               Message = Width + " is the width"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Message", new StringResult("50 mm is the width"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_MultipleStringConcatenation_Works()
    {
        var sourceFile = SourceFile.FromString("""
                                               Part1 = "Hello"
                                               Part2 = " "
                                               Part3 = "World"
                                               Result = Part1 + Part2 + Part3
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Result", new StringResult("Hello World"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_StringWithDimensionlessQuantity_OmitsUnit()
    {
        var sourceFile = SourceFile.FromString("""
                                               Count = 42
                                               Message = "The count is " + Count
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Message", new StringResult("The count is 42"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    #endregion

    #region List Join Tests

    [Test]
    public void Analyse_JoinStringList_JoinsWithSeparator()
    {
        var sourceFile = SourceFile.FromString("""
                                               Words = ["hello", "world"]
                                               Sentence = Words.join(", ")
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Sentence", new StringResult("hello, world"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_JoinEmptyList_ReturnsEmptyString()
    {
        var sourceFile = SourceFile.FromString("""
                                               Words = []
                                               Sentence = Words.join(", ")
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Sentence", new StringResult(""));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_JoinSingleElement_ReturnsElement()
    {
        var sourceFile = SourceFile.FromString("""
                                               Words = ["hello"]
                                               Sentence = Words.join(", ")
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Sentence", new StringResult("hello"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_JoinWithEmptySeparator_ConcatenatesDirectly()
    {
        var sourceFile = SourceFile.FromString("""
                                               Chars = ["a", "b", "c"]
                                               Result = Chars.join("")
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Result", new StringResult("abc"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    #endregion

    #region String in Variable Declaration Tests

    [Test]
    public void Analyse_StringVariable_ValidType()
    {
        var sourceFile = SourceFile.FromString("""
                                               Name = "Sunset"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Name", new StringResult("Sunset"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_StringListVariable_ValidType()
    {
        var sourceFile = SourceFile.FromString("""
                                               Names = ["Alice", "Bob", "Charlie"]
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var fileScope = environment.ChildScopes["$file"];
        var variable = fileScope.ChildDeclarations["Names"] as VariableDeclaration;
        var result = variable?.GetResult(fileScope) as ListResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count, Is.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(new StringResult("Alice")));
        Assert.That(result[1], Is.EqualTo(new StringResult("Bob")));
        Assert.That(result[2], Is.EqualTo(new StringResult("Charlie")));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    #endregion

    #region Helper Methods

    private static void AssertVariableDeclaration(IScope scope, string variableName, IResult expectedValue)
    {
        if (scope.ChildDeclarations[variableName] is VariableDeclaration variableDeclaration)
        {
            var value = variableDeclaration.GetResult(scope);

            Assert.That(value, Is.Not.Null);
            Assert.That(value, Is.EqualTo(expectedValue));
        }
        else
        {
            Assert.Fail($"Expected variable {variableName} to be declared.");
        }
    }

    #endregion
}
