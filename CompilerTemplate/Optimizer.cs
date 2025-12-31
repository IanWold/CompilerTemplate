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

    private static BoundExpression FoldExpression(BoundExpression expression, Dictionary<int, int> constants) => expression switch
    {
        BoundIntExpression =>
            expression,

        BoundVariableExpression variableExpression =>
            constants.TryGetValue(variableExpression.Id, out int c)
            ? new BoundIntExpression(c)
            : variableExpression,

        BoundUnaryExpression u =>
            (u.Operator, FoldExpression(u.Right, constants)) switch
            {
                (TokenKind.Plus, BoundIntExpression { Value: var rightInt }) => new BoundIntExpression(rightInt),
                (TokenKind.Plus, var right) => right,
                
                (TokenKind.Minus, BoundIntExpression { Value: var rightInt }) => new BoundIntExpression(checked(-rightInt)),

                var (o, r) => new BoundUnaryExpression(o, r)
            },

        BoundBinaryExpression binaryExpression =>
            (binaryExpression.Operator, FoldExpression(binaryExpression.Left, constants), FoldExpression(binaryExpression.Right, constants)) switch
            {
                (TokenKind.Plus, var left, BoundIntExpression { Value: 0 }) => left,
                (TokenKind.Plus, BoundIntExpression { Value: 0 }, var right) => right,
                (TokenKind.Plus, BoundIntExpression leftInt, BoundIntExpression rightInt) => new BoundIntExpression(checked(leftInt.Value + rightInt.Value)),

                (TokenKind.Minus, var left, BoundIntExpression { Value: 0 }) => left,
                (TokenKind.Minus, BoundIntExpression leftInt, BoundIntExpression rightInt) => new BoundIntExpression(checked(leftInt.Value - rightInt.Value)),

                (TokenKind.Star, var left, BoundIntExpression { Value: 1 }) => left,
                (TokenKind.Star, BoundIntExpression { Value: 1 }, var right) => right,
                (TokenKind.Star, BoundIntExpression { Value: 0 }, BoundIntExpression { Value: 0 }) => new BoundIntExpression(0),
                (TokenKind.Star, BoundIntExpression leftInt, BoundIntExpression rightInt) => new BoundIntExpression(checked(leftInt.Value * rightInt.Value)),

                (TokenKind.Slash, var left, BoundIntExpression { Value: 1 }) => left,
                (TokenKind.Slash, BoundIntExpression leftInt, BoundIntExpression rightInt) => new BoundIntExpression(leftInt.Value / rightInt.Value),

                var (o, l, r) => new BoundBinaryExpression(o, l, r)
            },

        _ => expression,
    };
}
