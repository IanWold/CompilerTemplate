namespace CompilerTemplate;

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

    LeftParen,
    RightParen,
}

public readonly record struct Token(TokenKind Kind, string Text, int Pos);