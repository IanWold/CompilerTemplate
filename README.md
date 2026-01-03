<div align="center">

# 🔩 Compiler Template

</div>

This is a template for a compiler in C#. It implements a rudimentary language, including a simple CLI with [CommandLineParser](https://github.com/commandlineparser/commandline), [QBE](https://c9x.me/compile/) as a backend, and clang (`cc`) for linking. I've included a Linux QBE compilation for my own ease but this is easy to modify.

Implemented for the rudimenary language are the typical, canonical layers: lexer, parser, analyzer (only with a binder), optimizer (only with a constants folder), and lowering. Two AST representations are included and simple diagnostics collection, though diagnostics are not output.

The intent is that this is a jumping off point for languages.

### Running

The compiler can be run via:

```
CompilerTemplate -o program.out
```

And has an option to specify source directory:

```
CompilerTemplate -d /home/source/myproject
```

Or with `.exe` for Windows. A Linux build of QBE is included in the project, if you're using Windows you'll want to swap that out or install QBE. Also, I think `cc` only runs clang on Linux, Windows may want to modify Build.cs.

### Example Language

The example language includes the following expressions:

* **Integers:** `0`, `123`
* **Variables:** `someVar`
* **Arithmetic:** `(5 + someVar) - (8 * someOtherVar)`

And the following statements:

* **Assignment**: `someVar = 5 - someOtherVar`
* **Print**: `print biggestVar / someVar`

BNF:

```bnf
<sign>       ::= + | -
<identifier> ::= <letter> +(<letter> | <digit>)

<expression> ::= <term> +((+ | -) <term>)
<term>       ::= <factor> +((* | /) <factor>)
<factor>     ::= (+ | -) <factor> | <primary>
<primary>    ::= +<digit> | <identifier> | '(' <expression> ')'

<statement>  ::= <assignment> | <print>
<assignment> ::= <identifier> = <expression>
<print>      ::= print <expression>
```
