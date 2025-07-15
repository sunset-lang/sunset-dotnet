# Element inheritance

Elements can inherit from other elements, copying the behaviour (the inputs and outputs) of that element and allowing it
to be extended.

For example, a reinforcement bar can be considered as having inherited from a circle.

```
Circle:
    inputs:
        Diameter <\phi> = 100 {mm} "Diameter of the circle"
    calculations:
        Area = (Pi * Diameter ^2) / 4 "Area of the circle"

Reinforcement(Circle):
    inputs:
        Diameter = parent "Diameter of the reinforcing bar"
   
    calculations: 
        Area = parent
```

The element inheriting from the other element must explicitly inherit all the properties of the parent element, and
either explicitly declare that they are as per the parent with the keyword `parent`, or they can override certain
aspects of the parent by re-defining them.

All inputs and calculations must be included in the child element for the sake of readability. Any elements that are not
inherited from explicitly will throw an error. The intent of this is to encourage full consideration of the inherited
properties and to encourage consideration of whether inherited properties are actually required.

> [!NOTE] Multiple inheritance was considered, but has been abandoned to allow for branching behaviour to be
> implemented. Interface-like behaviour is to be considered for multiple inheritance if required.

<!--

## Multiple inheritance

An element can inherit from multiple parents. If so, for parents that contain duplicate calculations they must explicitly define which element they are inheriting from with the `parent(Element)` syntax. This is automatically checked by the compiler

-->

## Element groups

Inherited elements can be grouped into categories to allow for the definition of inputs to contain a particular group of
element types.
