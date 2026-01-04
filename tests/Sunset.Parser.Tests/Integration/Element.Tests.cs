using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class ElementTests
{
    [Test]
    public void Parse_SingleElement_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               define Square:
                                                   inputs:
                                                       Width <w> {mm} = 100 {mm}
                                                       Length <l> {mm} = 200 {mm}
                                                   outputs:
                                                       Area <A> {mm^2} = Width * Length
                                               end
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var element = environment.ChildScopes["$file"].ChildDeclarations["Square"] as ElementDeclaration;
        // Check that the element is not null and has three child variables.
        Assert.That(element, Is.Not.Null);
        Assert.That(element.ChildDeclarations, Has.Count.EqualTo(3));
    }

    [Test]
    public void Parse_SingleElementWithInstanceAndAccess_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               define Square:
                                                   inputs:
                                                       Width <w> {mm} = 100 {mm}
                                                       Length <l> {mm} = 200 {mm}
                                                   outputs:
                                                       Area <A> {mm^2} = Width * Length
                                               end

                                               SquareInstance = Square(
                                                    Width = 200 {mm},
                                                    Length = 350 {mm}
                                                    )

                                               Result {mm^2} = SquareInstance.Area
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var resultDeclaration = environment.ChildScopes["$file"].ChildDeclarations["Result"] as VariableDeclaration;
        var result = resultDeclaration?.GetResult(fileScope!) as QuantityResult;
        Assert.That(result?.Result, Is.EqualTo(new Quantity(0.07, DefinedUnits.Metre * DefinedUnits.Metre)));
    }

    #region Default Return Value Tests

    [Test]
    public void Element_ImplicitReturn_ReturnsLastVariable()
    {
        // Implicit return: the last variable defined is the default return
        // When used in an expression, the element instance resolves to its default return value
        var sourceFile = SourceFile.FromString("""
                                               define Multiply:
                                                   inputs:
                                                       Value1 = 12
                                                       Value2 = 5
                                                   outputs:
                                                       Result = Value1 * Value2
                                               end

                                               Example = Multiply(Value1 = 12, Value2 = 5)
                                               Used = Example + 0
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var usedDeclaration = environment.ChildScopes["$file"].ChildDeclarations["Used"] as VariableDeclaration;
        var result = usedDeclaration?.GetResult(fileScope!) as QuantityResult;

        // The implicit return is the last variable (Result = 60)
        Assert.That(result?.Result.BaseValue, Is.EqualTo(60));
    }

    [Test]
    public void Element_ExplicitReturn_ReturnsMarkedVariable()
    {
        // Explicit return: use 'return' keyword to mark the default value
        // When used in an expression, the element instance resolves to the explicitly marked return value
        var sourceFile = SourceFile.FromString("""
                                               define Operation:
                                                   inputs:
                                                       Value1 = 12
                                                       Value2 = 5
                                                   outputs:
                                                       return Add = Value1 + Value2
                                                       Multiply = Value1 * Value2
                                               end

                                               Example = Operation(Value1 = 12, Value2 = 5)
                                               Used = Example + 0
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var usedDeclaration = environment.ChildScopes["$file"].ChildDeclarations["Used"] as VariableDeclaration;
        var result = usedDeclaration?.GetResult(fileScope!) as QuantityResult;

        // The explicit return is Add = 17 (not Multiply = 60)
        Assert.That(result?.Result.BaseValue, Is.EqualTo(17));
    }

    [Test]
    public void Element_MultipleReturn_ProducesError()
    {
        var sourceFile = SourceFile.FromString("""
                                               define Invalid:
                                                   inputs:
                                                       Value1 = 12
                                                   outputs:
                                                       return Add = Value1 + 5
                                                       return Multiply = Value1 * 5
                                               end
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // Should have an error for multiple return keywords
        var error = environment.Log.Errors.OfType<MultipleReturnError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected MultipleReturnError to be logged");
    }

    [Test]
    public void Element_DefaultReturnInExpression_CanBeUsed()
    {
        // Using element default return in an arithmetic expression
        var sourceFile = SourceFile.FromString("""
                                               define Square:
                                                   inputs:
                                                       X = 5
                                                   outputs:
                                                       Result = X * X
                                               end

                                               Doubled = Square(X = 4) * 2
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var doubledDeclaration = environment.ChildScopes["$file"].ChildDeclarations["Doubled"] as VariableDeclaration;
        var result = doubledDeclaration?.GetResult(fileScope!) as QuantityResult;

        // Square(X = 4) returns 16, * 2 = 32
        Assert.That(result?.Result.BaseValue, Is.EqualTo(32));
    }

    #endregion

    #region Partial Application (Re-instantiation) Tests

    [Test]
    public void Element_Reinstantiation_InheritsUnchangedProperties()
    {
        var sourceFile = SourceFile.FromString("""
                                               define Rectangle:
                                                   inputs:
                                                       Length {m} = 1 {m}
                                                       Width {m} = 2 {m}
                                                   outputs:
                                                       Area {m^2} = Length * Width
                                               end

                                               Rect1 = Rectangle(Length = 2 {m}, Width = 4 {m})
                                               Rect2 = Rect1(Length = 4 {m})
                                               Result {m^2} = Rect2.Area
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var resultDeclaration = environment.ChildScopes["$file"].ChildDeclarations["Result"] as VariableDeclaration;
        var result = resultDeclaration?.GetResult(fileScope!) as QuantityResult;

        // Rect2: Length = 4m (overridden), Width = 4m (inherited from Rect1)
        // Area = 4 * 4 = 16 m^2
        Assert.That(result?.Result.BaseValue, Is.EqualTo(16));
    }

    [Test]
    public void Element_Reinstantiation_IsImmutable()
    {
        // Re-instantiation should not modify the original instance
        var sourceFile = SourceFile.FromString("""
                                               define Counter:
                                                   inputs:
                                                       Value = 10
                                                   outputs:
                                                       Double = Value * 2
                                               end

                                               Original = Counter(Value = 5)
                                               Modified = Original(Value = 15)
                                               OriginalResult = Original.Double
                                               ModifiedResult = Modified.Double
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        
        var originalResultDecl = environment.ChildScopes["$file"].ChildDeclarations["OriginalResult"] as VariableDeclaration;
        var originalResult = originalResultDecl?.GetResult(fileScope!) as QuantityResult;
        
        var modifiedResultDecl = environment.ChildScopes["$file"].ChildDeclarations["ModifiedResult"] as VariableDeclaration;
        var modifiedResult = modifiedResultDecl?.GetResult(fileScope!) as QuantityResult;

        // Original should remain 5 * 2 = 10
        Assert.That(originalResult?.Result.BaseValue, Is.EqualTo(10));
        // Modified should be 15 * 2 = 30
        Assert.That(modifiedResult?.Result.BaseValue, Is.EqualTo(30));
    }

    [Test]
    public void Element_Reinstantiation_ChainedCalls()
    {
        // Re-instantiations can be chained
        var sourceFile = SourceFile.FromString("""
                                               define Point:
                                                   inputs:
                                                       X = 0
                                                       Y = 0
                                                   outputs:
                                                       Sum = X + Y
                                               end

                                               P1 = Point(X = 1, Y = 2)
                                               P2 = P1(X = 10)
                                               P3 = P2(Y = 20)
                                               Result = P3.Sum
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var resultDeclaration = environment.ChildScopes["$file"].ChildDeclarations["Result"] as VariableDeclaration;
        var result = resultDeclaration?.GetResult(fileScope!) as QuantityResult;

        // P1: X=1, Y=2, Sum=3
        // P2: X=10 (overridden), Y=2 (inherited), Sum=12
        // P3: X=10 (inherited), Y=20 (overridden), Sum=30
        Assert.That(result?.Result.BaseValue, Is.EqualTo(30));
    }

    #endregion
}