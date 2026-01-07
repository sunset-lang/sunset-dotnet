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

/// <summary>
/// Tests for required inputs - variable declarations without default values.
/// Required inputs must be provided when instantiating the containing element.
/// </summary>
[TestFixture]
public class RequiredInputsTests
{
    #region Parsing Tests

    [Test]
    public void Parse_RequiredInput_NoExpression_ParsesSuccessfully()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestElement1:
                                                   inputs:
                                                       RequiredValue {m}
                                                   outputs:
                                                       Result {m} = RequiredValue * 2
                                               end
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var element = environment.ChildScopes["$file"].ChildDeclarations["ReqTestElement1"] as ElementDeclaration;
        Assert.That(element, Is.Not.Null);
        
        var requiredInput = element!.ChildDeclarations["RequiredValue"] as VariableDeclaration;
        Assert.That(requiredInput, Is.Not.Null);
        Assert.That(requiredInput!.IsRequiredInput, Is.True);
        Assert.That(requiredInput.Expression, Is.Null);
    }

    [Test]
    public void Parse_OptionalInput_HasExpression_ParsesSuccessfully()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestElement2:
                                                   inputs:
                                                       OptionalValue {m} = 5 {m}
                                                   outputs:
                                                       Result {m} = OptionalValue * 2
                                               end
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var element = environment.ChildScopes["$file"].ChildDeclarations["ReqTestElement2"] as ElementDeclaration;
        Assert.That(element, Is.Not.Null);
        
        var optionalInput = element!.ChildDeclarations["OptionalValue"] as VariableDeclaration;
        Assert.That(optionalInput, Is.Not.Null);
        Assert.That(optionalInput!.IsRequiredInput, Is.False);
        Assert.That(optionalInput.Expression, Is.Not.Null);
    }

    [Test]
    public void Parse_MixedInputs_ParsesSuccessfully()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestRect1:
                                                   inputs:
                                                       Width {m}
                                                       Height {m} = 1 {m}
                                                   outputs:
                                                       Area {m^2} = Width * Height
                                               end
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var element = environment.ChildScopes["$file"].ChildDeclarations["ReqTestRect1"] as ElementDeclaration;
        Assert.That(element, Is.Not.Null);
        
        var widthInput = element!.ChildDeclarations["Width"] as VariableDeclaration;
        var heightInput = element.ChildDeclarations["Height"] as VariableDeclaration;
        
        Assert.That(widthInput!.IsRequiredInput, Is.True);
        Assert.That(heightInput!.IsRequiredInput, Is.False);
    }

    #endregion

    #region Type Checking - Required Input Validation

    [Test]
    public void TypeCheck_RequiredInputProvided_NamedArgument_NoError()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestElement3:
                                                   inputs:
                                                       RequiredValue {m}
                                                   outputs:
                                                       Result {m} = RequiredValue * 2
                                               end
                                               
                                               inst = ReqTestElement3(RequiredValue = 5 {m})
                                               output {m} = inst.Result
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.Errors, Is.Empty);
    }

    [Test]
    public void TypeCheck_RequiredInputProvided_PositionalArgument_NoError()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestElement4:
                                                   inputs:
                                                       RequiredValue {m}
                                                   outputs:
                                                       Result {m} = RequiredValue * 2
                                               end
                                               
                                               inst = ReqTestElement4(5 {m})
                                               output {m} = inst.Result
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.Errors, Is.Empty);
    }

    [Test]
    public void TypeCheck_RequiredInputNotProvided_GeneratesError()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestElement5:
                                                   inputs:
                                                       RequiredValue {m}
                                                   outputs:
                                                       Result {m} = RequiredValue * 2
                                               end
                                               
                                               inst = ReqTestElement5()
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.Errors.Count(), Is.EqualTo(1));
        Assert.That(environment.Log.Errors.First(), Is.TypeOf<RequiredInputNotProvidedError>());
        
        var error = environment.Log.Errors.First() as RequiredInputNotProvidedError;
        Assert.That(error!.Message, Does.Contain("RequiredValue"));
        Assert.That(error.Message, Does.Contain("ReqTestElement5"));
    }

    [Test]
    public void TypeCheck_MultipleRequiredInputs_AllProvided_NoError()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestRect2:
                                                   inputs:
                                                       Width {m}
                                                       Height {m}
                                                   outputs:
                                                       Area {m^2} = Width * Height
                                               end
                                               
                                               inst = ReqTestRect2(Width = 2 {m}, Height = 3 {m})
                                               result {m^2} = inst.Area
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors, Is.Empty);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var resultDecl = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = resultDecl!.GetResult(fileScope) as QuantityResult;

        Assert.That(result!.Result.BaseValue, Is.EqualTo(6.0).Within(0.001));
    }

    [Test]
    public void TypeCheck_MultipleRequiredInputs_OneMissing_GeneratesError()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestRect3:
                                                   inputs:
                                                       Width {m}
                                                       Height {m}
                                                   outputs:
                                                       Area {m^2} = Width * Height
                                               end
                                               
                                               inst = ReqTestRect3(Width = 2 {m})
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors.Count(), Is.EqualTo(1));
        Assert.That(environment.Log.Errors.First(), Is.TypeOf<RequiredInputNotProvidedError>());
        
        var error = environment.Log.Errors.First() as RequiredInputNotProvidedError;
        Assert.That(error!.Message, Does.Contain("Height"));
    }

    [Test]
    public void TypeCheck_MixedInputs_RequiredProvided_OptionalUseDefault_NoError()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestRect4:
                                                   inputs:
                                                       Width {m}
                                                       Height {m} = 1 {m}
                                                   outputs:
                                                       Area {m^2} = Width * Height
                                               end
                                               
                                               inst = ReqTestRect4(Width = 2 {m})
                                               result {m^2} = inst.Area
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors, Is.Empty);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var resultDecl = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = resultDecl!.GetResult(fileScope) as QuantityResult;

        // Width = 2m, Height defaults to 1m, Area = 2m^2
        Assert.That(result!.Result.BaseValue, Is.EqualTo(2.0).Within(0.001));
    }

    #endregion

    #region Evaluation Tests

    [Test]
    public void Evaluate_RequiredInputWithNamedArgument_CorrectValue()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestScaler1:
                                                   inputs:
                                                       Value {m}
                                                       Factor = 2
                                                   outputs:
                                                       Result {m} = Value * Factor
                                               end
                                               
                                               inst = ReqTestScaler1(Value = 5 {m}, Factor = 3)
                                               output {m} = inst.Result
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors, Is.Empty);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var outputDecl = fileScope!.ChildDeclarations["output"] as VariableDeclaration;
        var result = outputDecl!.GetResult(fileScope) as QuantityResult;

        // 5m * 3 = 15m
        Assert.That(result!.Result.BaseValue, Is.EqualTo(15.0).Within(0.001));
    }

    [Test]
    public void Evaluate_RequiredInputWithPositionalArgument_CorrectValue()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestScaler2:
                                                   inputs:
                                                       Value {m}
                                                       Factor = 2
                                                   outputs:
                                                       Result {m} = Value * Factor
                                               end
                                               
                                               inst = ReqTestScaler2(10 {m})
                                               output {m} = inst.Result
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors, Is.Empty);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var outputDecl = fileScope!.ChildDeclarations["output"] as VariableDeclaration;
        var result = outputDecl!.GetResult(fileScope) as QuantityResult;

        // 10m * 2 (default factor) = 20m
        Assert.That(result!.Result.BaseValue, Is.EqualTo(20.0).Within(0.001));
    }

    #endregion

    #region Element Type Required Inputs

    [Test]
    public void Parse_RequiredInputWithElementType_ParsesSuccessfully()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestPoint:
                                                   inputs:
                                                       X {m} = 0 {m}
                                                       Y {m} = 0 {m}
                                               end
                                               
                                               define ReqTestLine:
                                                   inputs:
                                                       Start {ReqTestPoint}
                                                       End {ReqTestPoint}
                                                   outputs:
                                                       DeltaX {m} = End.X - Start.X
                                                       DeltaY {m} = End.Y - Start.Y
                                               end
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var lineElement = environment.ChildScopes["$file"].ChildDeclarations["ReqTestLine"] as ElementDeclaration;
        Assert.That(lineElement, Is.Not.Null);
        
        var startInput = lineElement!.ChildDeclarations["Start"] as VariableDeclaration;
        var endInput = lineElement.ChildDeclarations["End"] as VariableDeclaration;
        
        Assert.That(startInput!.IsRequiredInput, Is.True);
        Assert.That(endInput!.IsRequiredInput, Is.True);
    }

    [Test]
    [Ignore("Blocked by: Required inputs with element type annotations (e.g., 'Start {ElementType}') need semantic analysis fix for accessing properties via dot operator")]
    public void TypeCheck_RequiredElementTypeInput_Provided_NoError()
    {
        var sourceFile = SourceFile.FromString("""
                                               define ReqTestPoint2:
                                                   inputs:
                                                       X {m} = 0 {m}
                                                       Y {m} = 0 {m}
                                               end
                                               
                                               define ReqTestLine2:
                                                   inputs:
                                                       Start {ReqTestPoint2}
                                                       End {ReqTestPoint2}
                                                   outputs:
                                                       DeltaX {m} = End.X - Start.X
                                               end
                                               
                                               startPt = ReqTestPoint2(X = 1 {m}, Y = 2 {m})
                                               endPt = ReqTestPoint2(X = 4 {m}, Y = 6 {m})
                                               line = ReqTestLine2(Start = startPt, End = endPt)
                                               result {m} = line.DeltaX
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.Errors, Is.Empty);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var resultDecl = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = resultDecl!.GetResult(fileScope) as QuantityResult;

        // DeltaX = 4m - 1m = 3m
        Assert.That(result!.Result.BaseValue, Is.EqualTo(3.0).Within(0.001));
    }

    #endregion
}
