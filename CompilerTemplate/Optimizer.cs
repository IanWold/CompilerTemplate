namespace CompilerTemplate;

public static class Optimizer
{
    public static BoundAstRoot Optimize(this BoundAstRoot root) =>
        root.FoldConstants();
}

public static class ConstantFolder
{
    public static BoundAstRoot FoldConstants(this BoundAstRoot root)
    {
        var constants = new Dictionary<int, int>();
        var statements = new List<BoundStatement>(root.Statements.Count);

        foreach (var statement in root.Statements)
        {
            switch (statement)
            {
                case BoundAssignStatement assignStatement:
                {
                    var folded = FoldExpression(assignStatement.Value, constants);

                    if (folded is BoundIntExpression ci)
                    {
                        constants[assignStatement.Id] = ci.Value;
                    }
                    else
                    {
                        constants.Remove(assignStatement.Id);
                    }

                    statements.Add(new BoundAssignStatement(assignStatement.Id, folded));
                    break;
                }

                case BoundPrintStatement printStatement:
                {
                    var folded = FoldExpression(printStatement.Value, constants);
                    statements.Add(new BoundPrintStatement(folded));
                    break;
                }

                default:
                    statements.Add(statement);
                    break;
            }
        }

        return root with { Statements = statements };
    }

    private static BoundExpression FoldExpression(BoundExpression expression, Dictionary<int, int> constants)
    {
        switch (expression)
        {
            case BoundIntExpression:
                return expression;

            case BoundVariableExpression variableExpression:
            {
                if (constants.TryGetValue(variableExpression.Id, out int c))
                {
                    return new BoundIntExpression(c);
                }

                return variableExpression;
            }

            case BoundBinaryExpression binaryExpression:
            {
                var left = FoldExpression(binaryExpression.Left, constants);
                var right = FoldExpression(binaryExpression.Right, constants);

                return (binaryExpression.Operator, left, right) switch
                {
                    (TokenKind.Plus, _, BoundIntExpression { Value : 0}) => left,
                    (TokenKind.Plus, BoundIntExpression { Value: 0 }, _) => right,
                    (TokenKind.Plus, BoundIntExpression leftInt, BoundIntExpression rightInt) => new BoundIntExpression(checked(leftInt.Value + rightInt.Value)),

                    (TokenKind.Minus, _, BoundIntExpression { Value: 0 }) => left,
                    (TokenKind.Minus, BoundIntExpression leftInt, BoundIntExpression rightInt) => new BoundIntExpression(checked(leftInt.Value - rightInt.Value)),

                    (TokenKind.Star, _, BoundIntExpression { Value: 1 }) => left,
                    (TokenKind.Star, BoundIntExpression { Value: 1 }, _) => right,
                    (TokenKind.Star, BoundIntExpression { Value: 0 }, BoundIntExpression { Value: 0 }) => new BoundIntExpression(0),
                    (TokenKind.Star, BoundIntExpression leftInt, BoundIntExpression rightInt) => new BoundIntExpression(checked(leftInt.Value * rightInt.Value)),

                    (TokenKind.Slash, _, BoundIntExpression { Value: 1 }) => left,
                    (TokenKind.Slash, BoundIntExpression leftInt, BoundIntExpression rightInt) => new BoundIntExpression(leftInt.Value / rightInt.Value),

                    _ => binaryExpression
                };
            }

            default:
                return expression;
        }
    }
}
