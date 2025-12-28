# Reporting

Performing calculations will result in a report being generated in the format of choice. The most common format is
Markdown, which can be used to then output PDF reports.

## Text

Use `///` to start a documentation comment that will be included in the report. Regular comments with `//` are not included. Standard Markdown can be used to style documentation comments.

## Calculations

Variables are reported if a symbol is defined for that variable, but are not reported if a symbol is not defined. If a
reference is defined it will also be added to the report next to the calculation.

All variables with a description will be printed at the end of the calculation with their description.

```sunset
/// #### Calculation of the plastic section modulus
// The "///" at the beginning of the comment above signals that it will be included in the report.
// The "#### " following it means that a level 4 heading will be added as per standard Markdown.
// This line and the two lines above will not be included in the report as they begin with "//".

/// Calculate the **plastic** section modulus of the plate.
// The **plastic** is Markdown for "make 'plastic' bold".

@b = 150 {mm}
    d: Width of the plate.
@t = 10 {mm}
    d: Thickness of the plate.

plasticDenominator = 4  // This variable will not be reported as it does not have a symbol defined

@Z_p {Example reference} = b * t^2 / plasticDenominator
    d: Plastic section modulus.
```

This will result in the following report:

> ### Calculation of the plastic section modulus
>
> Calculate the plastic section modulus of the plate.
> $$
>
>> \begin{alignedat}{2}
>> b &= 150 \text{ mm} \\
>> d &= 10 \text{ mm} \\
>> Z_p &= 3,750 \text{ mm}^2 &\quad\text{(Example reference)}
>> \end{alignedat}
>> $$
>
> Where:
>
> - $b$: Width of the plate.
> - $d$: Thickness of the plate.
> - $Z_p$: Plastic section modulus. (Example reference)
