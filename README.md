# Compiler Template

This is a template for a compiler in C#. It implements a rudimentary language, including a simple CLI with [CommandLineParser](https://github.com/commandlineparser/commandline), [QBE](https://c9x.me/compile/) as a backend, and clang (`cc`) for linking. I've included a Linux QBE compilation for my own ease but this is easy to modify.

Implemented for the rudimenary language are the typical, canonical layers: lexer, parser, analyzer (only with a binder), optimizer (only with a constants folder), and lowering. Two AST representations are included and simple diagnostics collection, though diagnostics are not output.

The intent is that this is a jumping off point for languages.
