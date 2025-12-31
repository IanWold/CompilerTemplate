namespace CompilerTemplate;

public static class Parser
{
    public static AstRoot Parse(this List<Token> tokens)
    {
        var statements = new List<AstStatement>();
        var position = 0;
        
		while (TryMatch(tokens, position, TokenKind.Newline, out position)) { }

        while (Peek(tokens, position).Kind != TokenKind.EOF)
        {
			(var statement, position) = ParseStatement(tokens, position);
            statements.Add(statement);

            if (Peek(tokens, position).Kind == TokenKind.Newline)
            {
                while (TryMatch(tokens, position, TokenKind.Newline, out position)) { }
            }
            else if (Peek(tokens, position).Kind != TokenKind.EOF)
            {
                throw new Exception($"Parse error at pos {Peek(tokens, position).Pos}: expected newline or EOF after statement, got {Peek(tokens, position).Kind}");
            }
        }

        return new AstRoot(statements);
    }

    private static Token Peek(List<Token> tokens, int position, int offset = 0)
    {
        var index = Math.Max(position + offset, 0);
        return index >= tokens.Count
            ? tokens[^1]
            : tokens[index];
    }

    private static (Token, int) Consume(List<Token> tokens, int position, TokenKind kind, string? message = null) =>
        Peek(tokens, position).Kind == kind
		? (Peek(tokens, position), position + 1)
		: throw new Exception(message ?? $"Parse error at pos {Peek(tokens, position).Pos}: expected {kind}, got {Peek(tokens, position).Kind}");

    private static bool TryMatch(List<Token> tokens, int position, TokenKind kind, out int newPosition)
    {
		newPosition = position;

        if (Peek(tokens, position).Kind == kind)
        {
			newPosition++;
        }

        return newPosition > position;
    }

    private static (AstStatement, int) ParseStatement(List<Token> tokens, int position)
    {
        if (Peek(tokens, position).Kind == TokenKind.Print)
        {
            (_, position) = Consume(tokens, position, TokenKind.Print);
            (var expression, position) = ParseExpression(tokens, position);

            return (new PrintStatement(expression), position);
        }

        if (Peek(tokens, position).Kind == TokenKind.Identifier && Peek(tokens, position, 1).Kind == TokenKind.Equals)
        {
            (var name, position) = Consume(tokens, position, TokenKind.Identifier);
            (_, position) = Consume(tokens, position, TokenKind.Equals);
            (var expression, position) = ParseExpression(tokens, position);

            return (new AssignStatement(name.Text, expression), position);
        }

        throw new Exception($"Parse error at pos {Peek(tokens, position).Pos}: expected 'print' or assignment, got {Peek(tokens, position).Kind}");
    }

    // Expression grammar:
    // expr    := term (('+'|'-') term)*
    // term    := factor (('*'|'/') factor)*
    // factor  := ('+'|'-') factor | primary
    // primary := NUMBER | IDENT | '(' expr ')'

    private static (AstExpression, int) ParseExpression(List<Token> tokens, int position)
    {
        (var left, position) = ParseTerm(tokens, position);

        while (true)
        {
            if (TryMatch(tokens, position, TokenKind.Plus, out position))
            {
                (var right, position) = ParseTerm(tokens, position);
                left = new BinaryExpression(TokenKind.Plus, left, right);

                continue;
            }

            if (TryMatch(tokens, position, TokenKind.Minus, out position))
            {
                (var right, position) = ParseTerm(tokens, position);
                left = new BinaryExpression(TokenKind.Minus, left, right);

                continue;
            }

            break;
        }

        return (left, position);
    }

    private static (AstExpression, int) ParseTerm(List<Token> tokens, int position)
    {
        (var left, position) = ParseFactor(tokens, position);

        while (true)
        {
            if (TryMatch(tokens, position, TokenKind.Star, out position))
            {
                (var right, position) = ParseFactor(tokens, position);
                left = new BinaryExpression(TokenKind.Star, left, right);

                continue;
            }

            if (TryMatch(tokens, position, TokenKind.Slash, out position))
            {
                (var right, position) = ParseFactor(tokens, position);
                left = new BinaryExpression(TokenKind.Slash, left, right);

                continue;
            }

            break;
        }

        return (left, position);
    }

    private static (AstExpression, int) ParseFactor(List<Token> tokens, int position)
    {
        if (TryMatch(tokens, position, TokenKind.Plus, out position))
        {
            (var expression, position) = ParseFactor(tokens, position);
            return (new UnaryExpression(TokenKind.Plus, expression), position);
        }

        if (TryMatch(tokens, position, TokenKind.Minus, out position))
        {
            (var expression, position) = ParseFactor(tokens, position);
            return (new UnaryExpression(TokenKind.Minus, expression), position);
        }

        return ParsePrimary(tokens, position);
    }

    private static (AstExpression, int) ParsePrimary(List<Token> tokens, int position)
    {
        if (Peek(tokens, position).Kind == TokenKind.Number)
        {
            (var token, position) = Consume(tokens, position, TokenKind.Number);
            if (!int.TryParse(token.Text, out int value))
            {
                throw new Exception($"Parse error at pos {token.Pos}: invalid int literal '{token.Text}'");
            }

            return (new IntExpression(value), position);
        }

        if (Peek(tokens, position).Kind == TokenKind.Identifier)
        {
            (var token, position) = Consume(tokens, position, TokenKind.Identifier);
            return (new VaraibleExpression(token.Text), position);
        }

        if (TryMatch(tokens, position, TokenKind.LeftParen, out position))
        {
            (var expression, position) = ParseExpression(tokens, position);
            (_, position) = Consume(tokens, position, TokenKind.RightParen, $"Parse error at pos {Peek(tokens, position).Pos}: expected ')'");

            return (expression, position);
        }

        throw new Exception($"Parse error at pos {Peek(tokens, position).Pos}: expected number, identifier, or '('; got {Peek(tokens, position).Kind}");
    }
}
