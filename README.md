# Sunset .NET

This repository contains the .NET version of the Sunset compiler and runtime.

It is currently made up of a single project which contains everything, but will eventually be refactored into a number
of individual projects:

- `Sunset.Quantities`: Deals with the units and quantities handling
- `Sunset.Parser`: Lexing and parsing of Sunset code.
- `Sunset.Analyzer`: Static code analysis of the resulting syntax tree.
- `Sunset.Interpreter`: Evaluation of the syntax tree.

# Documentation

This repository also contains the [documentation site](https://sunset-lang.github.io/sunset-docs) of the Sunset
Language.

## Dependencies

- .NET 8+

## Unit terminology

The below is based on the SI system of units.

- **Base units**: The units that apply to only one dimensions, e.g. metres, kilograms. All base units are coherent.
- **Coherent units**: Units where there are no multipliers applied to the dimensions, e.g. metres, kilograms, pascals
- **Named/defined units**: The units that have been assigned a special name, e.g. metres, millimetres, pascals,
  kilopascals
- **Derived units**: Units that apply to more than one dimension, e.g. kilopascals, newtons

## Currently working on

- Unit type checking