# Reporting

Performing calculations will result in a report being generated in the format of choice. The most common format is
Markdown, which can be used to then output PDF reports.

## Text

Comments with a single `#` are not included in the report. If `##` is used to start a comment, it is included in the
report. Standard Markdown can be used to style the comment.

## Calculations

Variables are reported if a symbol is defined for that variable, but are not reported if a symbol is not defined. If a
reference is defined it will also be added to the report next to the calculation.

All variables with a description will be printed at the end of the calculation with their description.

```sunset
## #### Calculation of the plastic section modulus
# The two "##"s at the beginning of the comment above is used to signal that it will be included in the report.
# The "#### " following it means that a level 4 heading will be added as per standard Markdown.
# This line and the two lines above will not be included in the report as they begin with only a single #. 

## Calculate the **plastic** section modulus of the plate.
# The **plastic** is Markdown for "make 'plastic' bold".

@b = 150 {mm}
    d: Width of the plate.
@t = 10 {mm}
    d: Thickness of the plate.
    
plasticDenominator = 4                      # This variable will not be reported as it does not have a symbol defined

@Z_p {Example reference} = b * t ^ 2 / plasticDenominator
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
