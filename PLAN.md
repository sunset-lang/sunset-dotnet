# Standard Units Branch - Implementation Plan

## Current Status
- Branch: `standard-units`
- Tests: 150 passing, 14 failing, 1 skipped (Parser), 19 passing (Quantities), 6 failing (Markdown)

## Root Cause of Remaining Failures

The core issue is in `TypeChecker.Visit(UnitAssignmentExpression)` at `src/Sunset.Parser/Analysis/TypeChecking/TypeChecker.cs:251-265`:

```csharp
private IResultType? Visit(UnitAssignmentExpression dest)
{
    var resultType = Visit(dest.UnitExpression);

    // Only set the result type if it is a unit type.
    if (resultType is not UnitType unitType)
    {
        return null;  // <-- This returns null when it shouldn't
    }
    // ...
}
```

When visiting a binary expression like `m^2` inside a UnitAssignment:
1. `m` (NameExpression) resolves to UnitDeclaration
2. `Visit(UnitDeclaration)` returns `UnitType(m)` correctly
3. The binary expression `m^2` should return `UnitType(m^2)`
4. But something fails, causing `resultType` to not be a UnitType

## Investigation Steps

### Step 1: Debug Binary Expression with UnitType left operand
In `TypeChecker.Visit(BinaryExpression)` at lines 110-124:
```csharp
case UnitType leftUnitType:
{
    var resultUnit = rightResult switch
    {
        UnitType rightUnitType => BinaryUnitOperation(...),
        QuantityType rightQuantityType => BinaryUnitOperation(...),
        _ => null
    };
    return resultUnit == null ? null : new UnitType(resultUnit);
}
```

Add temporary debug logging to see:
- What `leftResult` and `rightResult` are for `m^2`
- Whether the pattern matching works correctly
- What `BinaryUnitOperation` returns

### Step 2: Check NameExpression resolution for unit symbols
In `TypeChecker.Visit(NameExpression)` at lines 183-204:
- Verify that `m` resolves to the stdlib's UnitDeclaration
- Verify that `Visit(UnitDeclaration)` returns `UnitType`

### Step 3: Fix the issue
Based on investigation, likely fixes:
1. The switch pattern matching might not be entering the correct case
2. The `BinaryUnitOperation` might be returning null unexpectedly
3. There might be an issue with how NameExpressions inside UnitAssignment are resolved

## Failing Tests to Fix (in order of priority)

### Priority 1: Core type checking issues
1. `DeclaredUnitMismatchError_DimensionMismatch_ReportsError` - declared unit not evaluated
2. `DeclaredUnitMismatchError_WrongUnit_ReportsError` - declared unit not evaluated
3. `ErrorMessage_DeclaredUnitMismatch_ContainsBothUnits` - depends on above
4. `ErrorLocation_MultiToken_HasStartAndEnd` - error not generated

### Priority 2: Trig function issues (likely related to deg unit)
5. `Analyse_Sin30Degrees_CorrectResult` - wrong value
6. `Analyse_SinWithDegrees_CorrectResult` - wrong value
7. `Analyse_CosWithDegrees_CorrectResult` - wrong value
8. `Analyse_TanWith45Degrees_CorrectResult` - wrong value

### Priority 3: Unit type comparison issues
9. `Analyse_ListType_CorrectType` - expects BaseCoherentUnit, got NamedUnit
10. `Analyse_ListIndexAccessWithUnits_CorrectResult` - unit mismatch

### Priority 4: Quantity comparison issues
11. `Analyse_ComplexCalculation_CorrectResult` - QuantityResult comparison
12. `Analyse_InvalidUnits_DoesNotEvaluate` - QuantityResult comparison
13. `Parse_SingleElementWithInstanceAndAccess_CorrectResult` - value mismatch

### Priority 5: Error message tests
14. `ErrorMessage_BinaryUnitMismatch_ContainsBothUnits` - error message content

## Quick Fix Approach

Before deep investigation, try this quick fix in `TypeChecker.Visit(BinaryExpression)`:

The issue might be that when `rightResult` is `QuantityType.Dimensionless` (from NumberConstant), the pattern `QuantityType rightQuantityType` matches, but then `BinaryUnitOperation` is called with a dimensionless unit which might cause issues with the Power operation.

Check if the Power operation special case at line 142-145 is being reached:
```csharp
if (dest is { Operator: TokenType.Power, Right: NumberConstant numberConstant })
{
    return leftUnit.Pow(numberConstant.Value);
}
```

This should work, but verify `dest.Right` is still a `NumberConstant` in the AST.

## Commands to Run

```bash
# Run specific failing test with verbose output
dotnet test tests/Sunset.Parser.Tests --filter "DeclaredUnitMismatchError_DimensionMismatch" -v detailed

# Run all tests
dotnet test sunset-dotnet.slnx
```

## Files to Examine

1. `src/Sunset.Parser/Analysis/TypeChecking/TypeChecker.cs` - main type checker
2. `src/Sunset.Parser/Expressions/UnitAssignmentExpression.cs` - unit assignment
3. `src/Sunset.Parser/Analysis/NameResolution/NameResolver.cs` - name resolution for units
4. `tests/Sunset.Parser.Tests/Integration/Errors/SemanticErrors.Tests.cs` - error tests
