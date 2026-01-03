namespace CompilerTemplate.Compiler;

public static class Lexer
{
    public static List<Token> Lex(this string source)
    {
        var tokens = new List<Token>();
        var position = 0;

        while (true)
        {
            (var token, position) = NextToken(source, position);

            tokens.Add(token);

            if (token.Kind == TokenKind.EOF)
            {
                break;
            }
        }

        return tokens;
    }

    private static (Token, int) NextToken(string source, int position)
    {
        while (position < source.Length)
        {
            var c = source[position];
            if (c == ' ' || c == '\t' || c == '\r')
            {
                position++;
                continue;
            }

            break;
        }

        var lastPosition = position;

        if (position >= source.Length)
        {
            return (new Token(TokenKind.EOF, "", lastPosition), position);
        }

        var currentChar = source[position];

        if (currentChar == '\n')
        {
            position++;
            return (new Token(TokenKind.Newline, "\n", lastPosition), position);
        }

        position++;
        var token = currentChar switch
        {
            '=' => new Token(TokenKind.Equals, "=", lastPosition),
            '+' => new Token(TokenKind.Plus, "+", lastPosition),
            '-' => new Token(TokenKind.Minus, "-", lastPosition),
            '*' => new Token(TokenKind.Star, "*", lastPosition),
            '/' => new Token(TokenKind.Slash, "/", lastPosition),
            '%' => new Token(TokenKind.Percent, "%", lastPosition),
            '(' => new Token(TokenKind.LeftParen, "(", lastPosition),
            ')' => new Token(TokenKind.RightParen, ")", lastPosition),
            _ => LexNumberOrIdentifierOrError(source, position, lastPosition, currentChar, out position)
        };

        return (token, position);
    }

    private static Token LexNumberOrIdentifierOrError(string source, int position, int lastPosition, char firstChar, out int newPosition)
    {
        newPosition = position;

        if (char.IsDigit(firstChar))
        {
            while (newPosition < source.Length && char.IsDigit(source[newPosition]))
            {
                newPosition++;
            }

            return new Token(TokenKind.Number, source[lastPosition..newPosition], lastPosition);
        }

        if (char.IsLetter(firstChar) || firstChar == '_')
        {
            while (newPosition < source.Length)
            {
                char c = source[newPosition];
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    newPosition++;
                }
                else
                {
                    break;
                }
            }

            var text = source[lastPosition..newPosition];

            return text == "print"
                ? new Token(TokenKind.Print, text, lastPosition)
                : new Token(TokenKind.Identifier, text, lastPosition);
        }

        throw new Exception($"Lexer error at pos {lastPosition}: unexpected character '{firstChar}'");
    }
}
