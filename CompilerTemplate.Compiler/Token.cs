namespace CompilerTemplate.Compiler;

public enum TokenKind
{
    EOF,
    Newline,

    Number,
    Identifier,

    Print,

    Equals,
    Plus,
    Minus,
    Star,
    Slash,
    Percent,

    LeftParen,
    RightParen,
}

public readonly record struct Token(TokenKind Kind, string Text, int Pos);