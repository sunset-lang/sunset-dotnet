# Variables

Thus far, we've performed numeric calculations using Sunset. However, we have run into two problems.

1. With just numbers and no names, it's quite difficult to keep track of which numbers mean what.
2. If we change any of the calculations, we need to manually update all the following calculations to use the values
   from the previous calculations. This is tedious work and you won't be able to check many different options before the
   time runs out.

You go back to the Sunset Reference Manual and find a section on **variables**.

> ### Using variables
> Variables allow you to assign a **name** to the result of a particular calculation.
>
> For a name to be valid, it must:
> - Only have letters, numbers and underscores in it
> - Start with a letter or underscore
>
> To assign a variable start a line with the name of the variable and it's units in curly braces `{units}`, followed by
> the equals sign `=` and then the calculation.
>
> For example, the following will create new variables `VariableA`, `VariableB` and `VariableC`.
>
> ```sunset
> // Variables don't need to have units if the calculations don't have units. {} is also acceptable.
> VariableA = 45 + 12 * 8              
> 
> // If they do have units, the units must be specified after the variable name and before the = sign.
> VariableB {kg} = 100 {kg} + 45 {kg}  
> 
> // If there is just one number and no calculation, units also don't need to be declared.
> VariableC = 35 {m}
> ```
>
> #### Naming conventions
>
> By convention, variables should be named using `PascalCase`. Variable names can be made up of one or more full words
> without abbreviations, with each word starting with a capital letter.
>
> ```sunset
> // Good, valid names
> ConcreteStrength
> WindSpeed
> FloorLevel
> 
> // Bad, valid names
> Concrete_Strength  // Variable words are separated with an underscore
> windSpeed          // Variable should start with a capital letter
> Floorlevel         // All words should start with a capital letter
> 
> // Invalid names
> CostIn$            // Contains a symbol, should be CostInDollars
> 32MpaConcrete      // Starts with a number, should be Concrete32MPa
> ```

Let's set up the calculations from before using variables instead of just the numeric calculations, starting with the
combined mass of the crate and cabbages. Recall that the crate had a mass of 30kg and the cabbages had a mass of 150kg.
We should call our variables meaningful names: let's try `CrateMass` and `CabbageMass`, and our total mass `TotalMass`.

You'll need to first set up the variables for `CrateMass` and `CabbageMass`, followed by the calculation for
`TotalMass`.

```sunset
CrateMass = 30 {kg}
CabbageMass = 150 {kg}
TotalMass {kg} = CrateMass + CabbageMass
```

Now let's move on to the calculation of the velocity `v_0` (which we'll call `InitialVelocity`), which is c