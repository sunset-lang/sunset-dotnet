using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class OptionTests
{
    [Test]
    public void Option_ValidQuantityOption_NoErrors()
    {
        var source = SourceFile.FromString("""
            option Size {m}:
                10 {m}
                20 {m}
            end
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors, Is.Empty);
        
        // Verify the option is in the file scope
        var fileScope = env.ChildScopes["$file"];
        Assert.That(fileScope.ChildDeclarations.ContainsKey("Size"), Is.True);
        Assert.That(fileScope.ChildDeclarations["Size"], Is.TypeOf<OptionDeclaration>());
    }

    [Test]
    public void Option_TextOption_NoErrors()
    {
        var source = SourceFile.FromString("""
            option Methods {text}:
                "SVG"
                "Typst"
            end
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors, Is.Empty);
    }

    [Test]
    public void Option_NumberOption_NoErrors()
    {
        var source = SourceFile.FromString("""
            option Scale {number}:
                1
                2
                5
            end
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors, Is.Empty);
    }

    [Test]
    public void Option_InferredType_NoErrors()
    {
        var source = SourceFile.FromString("""
            option Size:
                10 {m}
                20 {m}
            end
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors, Is.Empty);
    }

    [Test]
    public void Option_EmptyOption_ProducesError()
    {
        var source = SourceFile.FromString("""
            option Empty {m}:
            end
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors.OfType<EmptyOptionError>(), Is.Not.Empty);
    }

    [Test]
    public void Option_MixedTypes_ProducesError()
    {
        var source = SourceFile.FromString("""
            option Invalid {m}:
                10 {m}
                "text"
            end
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors.OfType<OptionValueTypeMismatchError>(), Is.Not.Empty);
    }

    [Test]
    public void Option_UsedAsTypeAnnotation_NoErrors()
    {
        var source = SourceFile.FromString("""
            option Size {m}:
                10 {m}
                20 {m}
            end
            
            x {Size} = 10 {m}
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors, Is.Empty);
        
        // Verify the variable has the option type
        var fileScope = env.ChildScopes["$file"] as FileScope;
        var xDecl = fileScope!.ChildDeclarations["x"] as VariableDeclaration;
        var xType = xDecl!.GetAssignedType();
        Assert.That(xType, Is.TypeOf<OptionType>());
    }

    [Test]
    public void Option_VariableCanBeUsedInArithmetic()
    {
        var source = SourceFile.FromString("""
            option Size {m}:
                10 {m}
                20 {m}
            end
            
            x {Size} = 10 {m}
            y {m} = x * 2
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors, Is.Empty);
        
        var fileScope = env.ChildScopes["$file"] as FileScope;
        var yDecl = fileScope!.ChildDeclarations["y"] as VariableDeclaration;
        var result = yDecl!.GetResult(fileScope) as QuantityResult;
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(20));
    }

    [Test]
    public void Option_InElementInput_NoErrors()
    {
        var source = SourceFile.FromString("""
            option Size {m}:
                10 {m}
                20 {m}
            end
            
            define Rectangle:
                inputs:
                    Width {Size} = 10 {m}
                    Length {Size} = 20 {m}
                outputs:
                    Area {m^2} = Width * Length
            end
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors, Is.Empty);
    }

    [Test]
    public void Option_TextOptionUsedInElement_NoErrors()
    {
        var source = SourceFile.FromString("""
            option DrawingMethods {text}:
                "SVG"
                "Typst"
            end
            
            define Shape:
                inputs:
                    Method {DrawingMethods} = "SVG"
                outputs:
                    Description = Method
            end
            """);
        var env = new Environment(source);
        env.Analyse();

        Assert.That(env.Log.Errors, Is.Empty);
    }

    [Test]
    public void Option_TypeChecking_CompatibleWithUnderlyingType()
    {
        var source = SourceFile.FromString("""
            option Size {m}:
                10 {m}
                20 {m}
            end
            
            x {Size} = 10 {m}
            y {m} = x
            """);
        var env = new Environment(source);
        env.Analyse();

        // Option type should be compatible with its underlying type (metres)
        Assert.That(env.Log.Errors, Is.Empty);
    }
}
