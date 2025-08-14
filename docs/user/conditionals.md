## Conditionals

Conditions are expressions that evaluate to a `true` or `false` variable. They can be used as either statements or expressions.
They use the following operators:

- Value comparisons:
  - `==` Equal to
  - `!=` Not equal to
  - `<` Less than
  - `<=` Less than or equal to
  - `>` Greater than
  - `>=` Greater than or equal to
- Type comparisons:
  - `is` Types are equivalent
  - `is not` Types are not equivalent
- Combinations:
  - `and`, `or`
  - `not`

### In-variable `if` statements

A conditional can affect a single variable, where the calculations of a variable are interchanged depending on the
result of a **condition**.

These take one of two forms:

#### Conditional form

```sunset
variable = 
  if condition:
    expression
  else if condition:
    expression
  ... # Multiple else if statements allowable prior to else statement
  else:
    expression
  end
  
  metadata: values
```

#### Comparison form

```sunset
variable = 
  if variable:
    comparison:
      expression
    ... # Multiple comparison statments allowable prior to else statement
  else:
      expression
  end
```

`comparison` can be a single comparison such as `> 20 {mm}` or can optionally include additional combined comparisons
like `> 20 {mm} or < 10 {mm}`

Rules:

- All of the `expression`s must evaluate to the same units or type.
- `if` statements are executed sequentially, and are exited once one of them is found to be true.
- There must be an `else` statement at the end of the statements.

For example:

```
@x = 
  if y < 20 {mm}:
    A + B * C         # Note that this must evaluate to {MPa}, as the other conditions all evaluate to the same units
  else if y < 30 {mm}:
    35 {MPa} "Description override for this particular branch" {Reference override}
  else:
    40 {MPa}
      d: Default description if not picked up in one of the branches
      r: Default reference if not picked up in one of the above branches
  end
```

In comparison form:

```
@x = 
  if y:
    > 20 {mm}:
      A + B * C         # Note that this must evaluate to {MPa}, as the other conditions all evaluate to the same units
    < 30 {mm}:
      35 {MPa} "Description override for this particular branch" {Reference override}
    else:
      40 {MPa}
        d: Default description if not picked up in one of the branches
        r: Default reference if not picked up in one of the above branches
  end
```

> **Design notes:** The colon at the end of the `if`, `else if` and `else` statements are not strictly necessary, but
> are put in
> place to simulate Python syntax, and to allow a user to transition across to more general programming languages more
> readily.
>
> The `else if` is used instead of a plain `if` for the sake of readability so that the `if` and `end` clearly show the
> limits of the entire condition block.
>
> The `end` statement is used to align with the multi-variable syntax noted below. Even without these, as the
> expressions are single lined the tab indentation is not required for parsing the overall syntax.

### Multi-variable `if` statements

Conditional statements can also be used across a number of different variables should more complex calculations be
required.

The syntax for this is:

```
element:
  inputs:
    ...
  calculations:
    ...
    
    if condition:
      variable = expression
      ...
    else if condition:
      variable = expression
      ...
    else:
      variable = expression
      ...
    end
    
    ...
```

Rules:

- Both the conditional and comparison forms of conditional statements may be used.
- All non-anonymous variables must be defined in the same manner between each condition and must have the same
  dimensions in each of the conditions.

> **Design notes:** The `end` statement is required here to differentiate between the conditional block and the next
> variable definition below.

### Conditions on Options

If an [`Option`](options.md) is provided as the variable in a comparison `if` statement, the `else` statement may be
omitted if all the options are included in the comparisons.

For example, the below is valid as all four options are included in the `if` statement comparisons.:

```
options BoltTypes = ["4.6/S", "8.8/S", "8.8/TB", "8.8/TF"]
...
if BoltTypes:
  is "4.6/S":
    ...
  is "8.8/S":
    ...
  is "8.8/TB":
    ...
  is "8.8/TF":
    ...
end
```

> **Design note:** The aim in doing this is to prevent errors from creeping into the software as additional types are
> added in. One should explicitly implement behaviour for all options.
