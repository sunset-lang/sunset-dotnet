using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class PrototypeTests
{
    [Test]
    public void Analyse_BasicPrototype_NoErrors()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages, Is.Empty);
    }

    [Test]
    public void Analyse_ElementImplementingPrototype_NoErrors()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Square as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
            end
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages, Is.Empty);
    }

    [Test]
    public void Analyse_ElementMissingRequiredOutput_LogsError()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Square as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    // Missing Area output
                    Perimeter {m} = 4 * Width
            end
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_PrototypeInheritance_NoErrors()
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
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages, Is.Empty);
    }

    [Test]
    public void Analyse_PrototypeInheritanceCycle_LogsError()
    {
        var source = """
            prototype A as B:
            end

            prototype B as A:
            end
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_PrototypeInputInheritance_NoErrors()
    {
        var source = """
            prototype Rectangular:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Rectangular:
                outputs:
                    return Area {m^2} = Width * Length
            end

            // Use inherited defaults
            r = Rectangle()
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages, Is.Empty);
    }

    [Test]
    public void Analyse_MultiplePrototypeImplementation_NoErrors()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            prototype Rectangular:
                outputs:
                    Perimeter {m}
            end

            define Square as Shape, Rectangular:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
                    Perimeter {m} = 4 * Width
            end
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages, Is.Empty);
    }

    [Test]
    public void Analyse_EmptyPrototype_NoErrors()
    {
        var source = """
            prototype Printable:
            end

            define Report as Printable:
                inputs:
                    Title = "Untitled"
            end
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages, Is.Empty);
    }

    [Test]
    public void Analyse_PrototypeOutputOverride_LogsError()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            prototype InvalidPolygon as Shape:
                outputs:
                    Area {m^2}  // Error: trying to override
                    Sides
            end
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_ElementReturnMismatch_LogsError()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Square as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    Area {m^2} = Width ^ 2  // Not marked with return
                    return Perimeter {m} = 4 * Width  // Wrong output marked
            end
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));
        Assert.That(env.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    // TODO: These tests require full evaluation support and will be enabled later
    /*
    [Test]
    public void Evaluate_ElementWithPrototype_CorrectResult()
    {
        var source = """
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Square as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
            end

            s = Square(Width = 2 {m})
            result {m^2} = s.Area
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(4));
    }

    [Test]
    public void Evaluate_PrototypeInputInheritance_CorrectResult()
    {
        var source = """
            prototype Rectangular:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2}
            end

            define Rectangle as Rectangular:
                outputs:
                    return Area {m^2} = Width * Length
            end

            r = Rectangle()
            result {m^2} = r.Area
            """;

        var env = new Environment(SourceFile.FromString(source));
        env.Analyse();

        var fileScope = env.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = variable!.GetResult(fileScope) as QuantityResult;

        // Area should be 1 * 2 = 2 m^2
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(2));
    }
    */
}
