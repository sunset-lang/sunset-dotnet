# Runtime architecture

- Module - A module comprising a number of logically grouped together `SunsetFile`s.
- `SunsetFile` - Contains plain text Sunset Language code
- `Lexer` - Converts code into Tokens
- `Parser` - Converts Tokens into an Abstract Syntax Tree composed of statement nodes
- `Analyzer` - Evaluates references and checks types within the AST
- `Compiler` - Compiles the AST into simplified instruction files that flatten out references
- `Environment` - Contains all compiled informatio

See [this page](development.md) for more information the grammar of the Sunset Language.
