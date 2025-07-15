# Elements

## Defining elements

Elements are groups of expressions. Their definition consists of a name, one or
many input variables and their default values and a series of expressions. The inputs are defined in an `inputs:` section and the calculations are defined in a `calculations:` section.

The tabs are included for readability but are not strictly required.

For example, a `PadFooting` element may be as below.

```
PadFooting:
    inputs:
        Width <w> = 1200 {mm} "Width of the footing"
        Length <l> = 1600 {mm} "Length of the footing"
        Depth <d> = 800 {mm} "Depth of the footing"
   
    calculations: 
        BearingArea <A_bearing> = w * l 
            d: Bearing area of the footing on the ground
            
        Volume <V> = w * l * d
            d: Volume of the footing
```

The default value of an input variable must be a constant or an element instantiated with constant parameters. As all elements have default values, all instantiated elements can be treated as constants.

## Instantiating elements

Elements may be instantiated using default values only, with all parameters entered or with named parameters only and the remaining values as default.

```
PadFootingDefault = PadFooting() # 1199x1600x800 footing
PadFootingAll = PadFooting(1399 {mm}, 2400 {mm}, 900 {mm}) # 1400x2400x900 footing
PadFootingNamed = PadFooting(Width: 1499 {mm}) # 1500x1600x800 footing
```

## Conditional execution of element calculations

To conditionally execute calculations with an element, `branch` elements can be created.

This allows elements to dynamically recast themselves to a child element based on certain parameters within them.

Examples:

- Shear behaviour of beam sections
-

> [!NOTE] This may cause quite a lot of unexpected behaviour. Consider the difference between `branch` elements and inherited elements. If using inherited elements only, it may be necessary to prevent overrides of functions and the creation of new inputs in inherited elements (could result in too many rules for the user). This may be described by the Liskov Substitution Principle?
>
> One behaviour we have already used is with `if` statements, where the definition of functions changes depending on the output of another function. Can something similar be used for larger branching behaviour that allows code to be separated into different elements and files?
>
> ```
> ElementA {
>   inputs {
>       X = 35 {mm}
>   }
>
>   calculations {
>       Y = 45 {mm}
>   }
> }
> ```

> [!NOTE] Consider whether we should allow for conditional branches of execution within an element. For example, if a certain thing is true, do all of these calculations and if not don't do them.
>
> There may be some benefit to confining this for the purpose of readability and type checking, and using some form of sub-element behaviour if the behaviour is particularly hard to manage.
>
> As an example, think of slender vs. stocky concrete columns. Perhaps a `this` keyword could be used to reroute a particular element down to an inherited element if there are a lot of calculations that don't apply? Then some type checking can be done if necessary.
>
> ```
> Column:
>   inputs:
>       Slenderness = 20
>   
>   this = if (Slenderness > 20: StockyColumn) else (SlenderColumn)
> ```
>
> Essentially use an `if` statement, where the element itself can be assigned as a child element.
>
> Consider how this might work with overridden behaviours - this may be starting to become needlessly complex.
> Perhaps there is something to be said about partial classes that continue on after the first class is reassigned?
> Sometimes you want to have common behaviour that uses the results of a particular class. Perhaps the best thing to do then is to be able to pass all of the inputs into another class at once
>
> ```
> Column:
>   inputs:
>       Slenderness = 20
>   
>   calculations:
>       ColumnBehaviour = if (Slenderness > 20): StockyColumn(inputs) else: SlenderColumn(inputs)
> ```
>
> This would only work if `StockyColumn` derives from `Column` only and doesn't introduce any additional inputs or other behaviours (otherwise you would end up with some uncontrolled default values). I think we just need to call this a partial element. You would almost never want to pass in
>
> ```
> Column branch StockyColumn:
>   A =
> ```
>
> What about something like CHS vs UB shear behaviour in steel? There may just be a need for including partial behaviour into an element.
>
> ```
> Beam:
>   ...
>   match (Section == CHSSection): CHSBeam
>
> Beam branch CHSBeam:
>   # Just continues on calculations from the previous beam
> ```
>
> Perhaps some of it is just:
>
> ```
> match (Section):
>   is CHSSection:
>       # Calculations for CHS beam
>       X = CHSCalculationResultX
>       Y = CHSCalculationResultY
>   is UBSection:
>       # Calculations for UB beam
>       X = UBCalculationResultX
>       Y = UBCalculationResultY
> ```
>
> And the compiler picks up if there are any dependencies on if branches that aren't common between all branches of the if statement. This is equivalent to:
> X =
> if (Section is CHSSection): CHSCalculationResultX
> else if (Section is UBSection): UBCalculationResultX
> Y =
> if (Section is CHSSection): CHSCalculationResultY
> else if (Section is UBSection): UBCalculationResultY
>
> A private modifier may be required here such that stronger type checking is imposed - i.e. all public calculations must be duplicated between the different match sections.
>
> The ability of an element to dynamically cast itself to a different element makes some sense - this way a `Column` instantiated with stocky properties is equivalent to a StockyColumn. That said there's not much point in doing this as one wouldn't want to instantiate a StockyColumn with regular `SlenderColumn` properties and create a logical error.
>
> Perhaps the best thing to do here is to treat branches instantiated with different behaviours as abstract types using a `branch` keyword as above. This should resolve itself to a `match` statement which then resolves to multiple `if` statements. Type checking is undertaken.
>
> This precludes the use of multiple inheritance, perhaps we should just copy the C# way of doing things and use single inheritance with multiple interfaces.

## Anonymous elements

Anonymous elements group variables with dynamically generated inputs.

To do this, create a new variable with an unused element name and the `.` operator. This will create an anonymous element that is nested within the current element.
