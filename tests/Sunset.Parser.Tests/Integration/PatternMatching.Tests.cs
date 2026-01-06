using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class PatternMatchingTests
{
    /// <summary>
    /// Gets errors excluding unit declaration warnings which are expected in simple test cases.
    /// </summary>
    private static IEnumerable<IOutputMessage> GetSignificantErrors(Environment env)
    {
        return env.Log.ErrorMessages
            .OfType<AttachedOutputMessage>()
            .Where(m => m.Error is not VariableUnitDeclarationError 
                     && m.Error is not VariableUnitEvaluationError);
    }

    [Test]
    public void Analyse_PatternMatchingWithRectangle_SelectsCorrectBranch()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2} = Width * Length
            end

            define Circle as Shape:
                inputs:
                    Radius = 1 {m}
                outputs:
                    return Area {m^2} = 3.14159 * Radius ^ 2
            end

            myShape {Shape} = Rectangle(2 {m}, 3 {m})

            result = 1 if myShape is Rectangle
                   = 2 if myShape is Circle
                   = 0 otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Console.WriteLine("All error messages:");
        foreach (var msg in env.Log.ErrorMessages)
        {
            if (msg is AttachedOutputMessage attached)
            {
                Console.WriteLine($"  Error: {attached.Error.GetType().Name}");
                if (attached.Error is Sunset.Parser.Errors.Syntax.UnexpectedSymbolError syntaxError)
                    Console.WriteLine($"    Token: '{syntaxError.StartToken}' at line {syntaxError.StartToken.LineStart}");
            }
            else
                Console.WriteLine($"  Message: {msg.GetType().Name}");
        }
        Assert.That(GetSignificantErrors(env), Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(1)); // Rectangle branch selected
    }

    [Test]
    public void Analyse_PatternMatchingWithCircle_SelectsCorrectBranch()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2} = Width * Length
            end

            define Circle as Shape:
                inputs:
                    Radius = 1 {m}
                outputs:
                    return Area {m^2} = 3.14159 * Radius ^ 2
            end

            myShape {Shape} = Circle(2 {m})

            result = 1 if myShape is Rectangle
                   = 2 if myShape is Circle
                   = 0 otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(GetSignificantErrors(env), Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(2)); // Circle branch selected
    }

    [Test]
    public void Analyse_PatternMatchingWithBinding_CanAccessElementProperties()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2} = Width * Length
            end

            define Circle as Shape:
                inputs:
                    Radius = 1 {m}
                outputs:
                    return Area {m^2} = 3.14159 * Radius ^ 2
            end

            myShape {Shape} = Rectangle(2 {m}, 3 {m})

            result {m^2} = rect.Width * rect.Length if myShape is Rectangle rect
                         = 3.14159 * circ.Radius ^ 2 if myShape is Circle circ
                         = error otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Assert.That(GetSignificantErrors(env), Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(6)); // 2 * 3 = 6
    }

    [Test]
    public void Analyse_PatternMatchingWithoutBinding_NoErrors()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2} = Width * Length
            end

            myShape {Shape} = Rectangle(2 {m}, 3 {m})

            result = 100 if myShape is Rectangle
                   = 0 otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(GetSignificantErrors(env), Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(100));
    }

    [Test]
    public void Analyse_PatternMatchingWithoutOtherwise_LogsError()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
            end

            myShape {Shape} = Rectangle(2 {m})

            result = 100 if myShape is Rectangle
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        
        // Should have an error about missing otherwise
        Assert.That(env.Log.ErrorMessages.Count, Is.GreaterThan(0));
        var hasPatternOtherwiseError = env.Log.ErrorMessages
            .OfType<Sunset.Parser.Errors.AttachedOutputMessage>()
            .Any(m => m.Error is PatternMatchingRequiresOtherwiseError);
        Assert.That(hasPatternOtherwiseError, Is.True);
    }

    [Test]
    [Ignore("Blocked by: TypeChecker now recognizes {Shape} as PrototypeType, but NameResolver still resolves myShape.Width to Rectangle.Width. Need to update NameResolver to respect prototype interface boundaries.")]
    public void Analyse_PrototypePropertyAccessWithoutPattern_LogsError()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2} = Width * Length
            end

            myShape {Shape} = Rectangle(2 {m}, 3 {m})

            // This should be an error - Width is not a property of Shape
            result = myShape.Width
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        
        // Should have an error about Width not being accessible
        Assert.That(env.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_PatternMatchingWithRegularConditions_NoErrors()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
            end

            define Circle as Shape:
                inputs:
                    Radius = 1 {m}
                outputs:
                    return Area {m^2} = 3.14159 * Radius ^ 2
            end

            myShape {Shape} = Rectangle(2 {m})
            x = 10

            // Mix regular conditions with pattern matching
            result = 1 if x > 20
                   = 2 if myShape is Rectangle
                   = 3 otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(GetSignificantErrors(env), Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(2)); // x > 20 is false, myShape is Rectangle is true
    }

    [Test]
    public void Analyse_PatternMatchingOtherwiseBranch_SelectsOtherwise()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
            end

            define Circle as Shape:
                inputs:
                    Radius = 1 {m}
                outputs:
                    return Area {m^2} = 3.14159 * Radius ^ 2
            end

            define Triangle as Shape:
                inputs:
                    Base = 1 {m}
                    Height = 1 {m}
                outputs:
                    return Area {m^2} = 0.5 * Base * Height
            end

            myShape {Shape} = Triangle(2 {m}, 3 {m})

            result = 1 if myShape is Rectangle
                   = 2 if myShape is Circle
                   = 0 otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(GetSignificantErrors(env), Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(0)); // Neither Rectangle nor Circle, so otherwise
    }

    [Test]
    public void Analyse_PatternMatchingWithNonPrototype_LogsError()
    {
        var source = """
            x = 5

            // This should be an error - x is not an element/prototype
            result = 100 if x is Rectangle
                   = 0 otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        
        // Should have an error about invalid pattern matching
        Assert.That(env.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_PatternBindingShadowsVariable_UsesBindingInBranch()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2} = Width * Length
            end

            myShape {Shape} = Rectangle(2 {m}, 3 {m})
            
            // 'r' exists as a variable outside
            r = 999

            // Pattern binding 'r' should shadow the outer 'r' in the branch
            result {m^2} = r.Width * r.Length if myShape is Rectangle r
                         = 0 {m^2} otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(GetSignificantErrors(env), Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(6)); // 2 * 3 = 6, using binding not outer r
    }

    [Test]
    public void Analyse_PatternMatchingInElement_CorrectResult()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Shape:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2} = Width * Length
            end

            define Circle as Shape:
                inputs:
                    Diameter = 2 {m}
                outputs:
                    return Area {m^2} = 3.14159 * (Diameter / 2) ^ 2
            end

            define AreaCalculator:
                inputs:
                    ShapeToCalculate {Shape} = Rectangle()
                outputs:
                    return Area {m^2} = rect.Width * rect.Length if ShapeToCalculate is Rectangle rect
                                      = 3.14159 * (circ.Diameter / 2) ^ 2 if ShapeToCalculate is Circle circ
                                      = error otherwise
            end

            RectangleInstance {Rectangle} = Rectangle(2 {m}, 3 {m})
            CircleInstance {Circle} = Circle(4 {m})

            RectangleCalculator = AreaCalculator(RectangleInstance)
            CircleCalculator = AreaCalculator(CircleInstance)
            
            RectangleArea {m^2} = RectangleCalculator.Area
            CircleArea {m^2} = CircleCalculator.Area
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Assert.That(GetSignificantErrors(env), Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        
        var rectAreaVar = fileScope!.ChildDeclarations["RectangleArea"] as VariableDeclaration;
        var rectAreaResult = rectAreaVar!.GetResult(fileScope) as QuantityResult;
        Assert.That(rectAreaResult, Is.Not.Null);
        Assert.That(rectAreaResult!.Result.BaseValue, Is.EqualTo(6)); // 2 * 3

        var circAreaVar = fileScope!.ChildDeclarations["CircleArea"] as VariableDeclaration;
        var circAreaResult = circAreaVar!.GetResult(fileScope) as QuantityResult;
        Assert.That(circAreaResult, Is.Not.Null);
        Assert.That(circAreaResult!.Result.BaseValue, Is.EqualTo(3.14159 * 4)); // 3.14159 * (4/2)^2 = 3.14159 * 4
    }

    [Test]
    [Ignore("Blocked by: Pattern matching with prototype inheritance uses 'myShape is Shape' which may require additional TypeChecker support for PrototypeType in pattern matching contexts.")]
    public void Analyse_PatternMatchingWithPrototypeInheritance_MatchesBasePrototype()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            prototype Polygon as Shape:
                outputs:
                    Sides
            end

            define Triangle as Polygon:
                inputs:
                    Base = 1 {m}
                    Height = 1 {m}
                outputs:
                    return Area {m^2} = 0.5 * Base * Height
                    Sides = 3
            end

            myShape {Shape} = Triangle(2 {m}, 4 {m})

            // Should match Shape since Triangle implements Polygon which implements Shape
            result = myShape.Area if myShape is Shape
                   = 0 {m^2} otherwise
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        var errors = GetSignificantErrors(env).ToList();
        Assert.That(errors, Is.Empty);

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(4)); // Matches Shape
    }
}
