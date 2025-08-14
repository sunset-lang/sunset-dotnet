# Parsing and analysis

The Sunset compiler and interpreter share the same architecture for parsing and static analysis of Sunset code. This
page describes this architecture. The Visitor pattern is used throughout, with each pass of the parser being a separate
implementation of `IVisitor`.

Following lexing and parsing into the AST, the main passes are in this order:

- Name resolution
- Type checking
- Unit checking
- Default quantity evaluation

The code may then be executed in a number of different ways to evaluate to different targets, including:

- Automated testing
- Printing a Markdown, HTML or PDF report
- Compiling to an F# class library
- Compiling to a GUI application

Each of these is a separate implementation of `IVisitor`.

## Metadata and the AST Typing Problem

Each pass can optionally store some metadata in the nodes of the AST. This includes type information, compile-time
evaluation of results, etc. To allow for simple storage of this information, each pass has an implementation of
`IPassData` which is stored in a `Dictionary<string, IPassData>` within each node. The use of a `string` as key to this
dictionary allows an arbitrary number of future visitors, as well as evaluation specific visitors to be implemented.