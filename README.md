<div align="center">

# 🔩 Compiler Template

</div>

This is a template for a compiler in C#. It implements a rudimentary language, including a simple CLI with [CommandLineParser](https://github.com/commandlineparser/commandline), [QBE](https://c9x.me/compile/) as a backend, and clang (`cc`) for linking. I've included a Linux QBE compilation for my own ease but this is easy to modify.

Implemented for the rudimenary language are the typical, canonical layers: lexer, parser, analyzer (only with a binder), optimizer (only with a constants folder), and lowering. Two AST representations are included and simple diagnostics collection, though diagnostics are not output.

The intent is that this is a jumping off point for languages.

### Example Language

The example language includes the following expressions:

* **Integers:** `0`, `123`
* **Variables:** `someVar`
* **Arithmetic:** `(5 + someVar) - (8 * someOtherVar)`

And the following statements:

* **Assignment**: `someVar = 5 - someOtherVar`
* **Print**: `print biggestVar / someVar`
