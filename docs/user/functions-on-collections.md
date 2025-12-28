# Functions on Collections

> **Status: Not Yet Implemented**
>
> Collection functions are planned for future implementation but are not currently functional. This document describes the intended functionality.

There are a number of functions that can be performed on collections to effectively loop over or aggregate data.

## Lists

### Iterators

`list.foreach(expression)`

Returns an array where `value` and `index` are used as keywords to reference the current element and its position:

```sunset
numbers = [1, 2, 3, 4, 5]
doubled = numbers.foreach(value * 2)  // Returns [2, 4, 6, 8, 10]
```

### Reducers

| Function | Description |
|----------|-------------|
| `list.min()` | Returns the minimum value in the list |
| `list.max()` | Returns the maximum value in the list |
| `list.average()` | Returns the average of all values |

```sunset
values = [10 {mm}, 20 {mm}, 30 {mm}]
minValue = values.min()      // 10 {mm}
maxValue = values.max()      // 30 {mm}
avgValue = values.average()  // 20 {mm}
```

### Filtering and Mapping

| Function | Description |
|----------|-------------|
| `list.where(condition)` | Returns elements matching the condition |
| `list.select(expression)` | Transforms each element |

```sunset
sizes = [10 {mm}, 25 {mm}, 30 {mm}, 45 {mm}]
largerThan20 = sizes.where(value > 20 {mm})  // [25 {mm}, 30 {mm}, 45 {mm}]
```

### Accessing Elements

| Syntax | Description |
|--------|-------------|
| `list[index]` | Access element by zero-based index |
| `list.first()` | Get the first element |
| `list.last()` | Get the last element |

```sunset
items = [12 {mm}, 16 {mm}, 20 {mm}]
first = items[0]        // 12 {mm}
second = items[1]       // 16 {mm}
firstItem = items.first()  // 12 {mm}
lastItem = items.last()    // 20 {mm}
```

## Dictionaries

### Iterators

`dictionary.foreach(expression)`

Iterates over all key-value pairs with `key` and `value` as keywords:

```sunset
materials = ["Steel": 250 {MPa}, "Aluminum": 270 {MPa}]
descriptions = materials.foreach(key + ": " + value)
```

### Accessing Values

| Syntax | Description |
|--------|-------------|
| `dict[key]` | Access value by exact key |
| `dict[~key]` | Linear interpolation between keys |
| `dict[~key-]` | Find value for nearest key below |
| `dict[~key+]` | Find value for nearest key above |

```sunset
// Exact key lookup
windSpeeds = ["A2": 45 {m/s}, "B1": 52 {m/s}]
speedA2 = windSpeeds["A2"]  // 45 {m/s}

// Interpolation (for numeric keys)
stressStrain = [0.001: 200 {MPa}, 0.002: 400 {MPa}]
interpolated = stressStrain[~0.0015]  // 300 {MPa}

// Find below/above
table = [10: 100, 20: 200, 30: 300]
belowValue = table[~25-]  // 200 (value at key 20)
aboveValue = table[~25+]  // 300 (value at key 30)
```

## Goal-Seek Iteration

> **Note:** Syntax to be confirmed.

A goal-seek style iteration capability is planned for dictionaries to find keys that produce a desired output value.
