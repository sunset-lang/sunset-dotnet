# Functions on Collections

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

Dictionaries are key-value collections that support exact key lookup and interpolation-based access for numeric keys.

### Creating Dictionaries

```sunset
// Dictionary with numeric keys
temperatures = [0: 20, 100: 100, 200: 180]

// Dictionary with string keys
windSpeeds = ["A2": 45 {m/s}, "B1": 52 {m/s}]

// Empty dictionary
emptyDict = [:]

// Dictionary with expressions
x = 10
y = 20
data = [0: x, 100: y, 200: x + y]
```

### Accessing Values

| Syntax | Description | Status |
|--------|-------------|--------|
| `dict[key]` | Access value by exact key | ✅ Implemented |
| `dict[~key]` | Linear interpolation between keys | ✅ Implemented |
| `dict[~key-]` | Find value for largest key ≤ lookup key | ✅ Implemented |
| `dict[~key+]` | Find value for smallest key ≥ lookup key | ✅ Implemented |

#### Exact Key Lookup

```sunset
windSpeeds = ["A2": 45 {m/s}, "B1": 52 {m/s}]
speedA2 = windSpeeds["A2"]  // 45 {m/s}

temps = [0: 20, 100: 100, 200: 180]
t100 = temps[100]  // 100
```

#### Linear Interpolation

Use the `~` prefix for linear interpolation between adjacent keys. The lookup key must be within the range of existing keys.

```sunset
// Linear interpolation between keys
stressStrain = [0: 0, 100: 100]
interpolated = stressStrain[~50]  // 50 (halfway between 0 and 100)

// Works with units too
temps = [0: 20 {kg}, 100: 100 {kg}]
t25 = temps[~25]  // 40 {kg} (20 + 0.25 * (100-20))
```

#### Floor/Ceiling Lookup

Use `~key-` to find the value for the largest key less than or equal to the lookup key (floor), or `~key+` to find the value for the smallest key greater than or equal to the lookup key (ceiling).

```sunset
table = [0: 10, 100: 100, 200: 180]

// Find below (floor): largest key <= 150 is 100
belowValue = table[~150-]  // 100 (value at key 100)

// Find above (ceiling): smallest key >= 150 is 200
aboveValue = table[~150+]  // 180 (value at key 200)
```

### Iterators

> **Status: Not Yet Implemented**

`dictionary.foreach(expression)`

Iterates over all key-value pairs with `key` and `value` as keywords:

```sunset
materials = ["Steel": 250 {MPa}, "Aluminum": 270 {MPa}]
descriptions = materials.foreach(key + ": " + value)
```

## Goal-Seek Iteration

> **Note:** Syntax to be confirmed.

A goal-seek style iteration capability is planned for dictionaries to find keys that produce a desired output value.
