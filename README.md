# Sunset .NET

This repository contains the .NET version of the Sunset compiler and runtime.

It is made up of the following projects:

- `Sunset.Parser`: Lexing and parsing of Sunset code.
- `Sunset.Analyzer`: Static code analysis of the resulting syntax tree.
- `Sunset.Interpreter`: Evaluation of the syntax tree.

## Dependencies

- .NET 8+

## Unit terminology

The below is based on the SI system of units.

- **Base units**: The units that apply to only one dimensions, e.g. metres, kilograms. All base units are coherent.
- **Coherent units**: Units where there are no multipliers applied to the dimensions, e.g. metres, kilograms, pascals
- **Named units**: The units that have been assigned a special name, e.g. metres, millimetres, pascals, kilopascals
- **Derived units**: Units that apply to more than one dimension, e.g. kilopascals, newtons