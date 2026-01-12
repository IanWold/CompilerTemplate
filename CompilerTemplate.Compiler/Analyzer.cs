namespace CompilerTemplate.Compiler;

public static class Analyzer
{
    public static BoundAstRoot Analyze(this AstRoot root) =>
        root.Bind();
}

public static class Binder
{
    public static BoundAstRoot Bind(this AstRoot root)
    {
        var diagnostics = new List<Diagnostic>();
        var bound = new List<BoundStatement>();

        var variables = new List<BoundVariableInfo?>();

        foreach (var statement in root.Statements)
        {
            switch (statement)
            {
                case AssignStatement assignStatement:
                {
                    var rhs = BindExpression(assignStatement.Value, variables, out var bindDiagnostics);
                    diagnostics.AddRange(bindDiagnostics);

                    var id = GetOrCreateVariableId(assignStatement.Name, variables);
                    if (id == variables.Count)
                    {
                        variables.Add(new (id, assignStatement.Name, rhs.Type));
                    }

                    bound.Add(new BoundAssignStatement(id, rhs));

                    break;
                }
                case PrintStatement printStatement:
                {
                    var expression = BindExpression(printStatement.Value, variables, out var bindDiagnostics);
                    diagnostics.AddRange(bindDiagnostics);

                    bound.Add(new BoundPrintStatement(expression));

                    break;
                }
                default:
                    diagnostics.Add(new(Severity.Error, $"Unknown statement type: {statement.GetType().Name}"));
                    break;
            }
        }

        return new BoundAstRoot(bound, [..variables.Where(v => v is not null)!], diagnostics);
    }

    private static BoundExpression BindExpression(AstExpression expression, List<BoundVariableInfo?> variables, out List<Diagnostic> diagnostics)
    {
        diagnostics = [];

        switch (expression)
        {
            case IntExpression intExpression:
            {
                if (intExpression.Value < int.MinValue || intExpression.Value > int.MaxValue)
                {
                    diagnostics.Add(new(Severity.Error, $"Integer literal out of 32-bit range: {intExpression.Value}"));
                }

                return new BoundIntExpression(intExpression.Value);
            }

            case StringExpression stringExpression:
                return new BoundStringExpression(stringExpression.Value);

            case VariableExpression variableExpression:
            {
                if (variables.SingleOrDefault(n => n?.Name == variableExpression.Name) is not (int id, _, TypeKind type))
                {
                    diagnostics.Add(new(Severity.Error, $"Use of undefined variable '{variableExpression.Name}'."));
                    return new BoundIntExpression(0);
                }

                if (!variables.Any(v => v?.Id == id))
                {
                    diagnostics.Add(new(Severity.Error, $"Variable '{variableExpression.Name}' used before assignment."));
                }

                return new BoundVariableExpression(id, type);
            }

            case UnaryExpression unaryExpression:
            {
                if (unaryExpression.Operator is not TokenKind.Plus and not TokenKind.Minus)
                {
                    diagnostics.Add(new(Severity.Error, $"Unknown unary operator '{unaryExpression.Operator}'."));
                }

                var right = BindExpression(unaryExpression.Right, variables, out var rightDiagnostics);
                diagnostics.AddRange(rightDiagnostics);

                return new BoundUnaryExpression(unaryExpression.Operator, right);
            }

            case BinaryExpression binaryExpression:
            {
                if (binaryExpression.Operator is not TokenKind.Plus and not TokenKind.Minus and not TokenKind.Star and not TokenKind.Slash)
                {
                    diagnostics.Add(new(Severity.Error, $"Unknown binary operator '{binaryExpression.Operator}'."));
                }

                var left = BindExpression(binaryExpression.Left, variables, out var leftDiagnostics);
                var right = BindExpression(binaryExpression.Right, variables, out var rightDiagnostics);
                diagnostics.AddRange([..leftDiagnostics, ..rightDiagnostics]);

                if (binaryExpression.Operator is TokenKind.Slash or TokenKind.Percent && right is BoundIntExpression { Value: 0 })
                {
                    diagnostics.Add(new(Severity.Error, "Division by zero."));
                }

                var inferredType = left.Type == right.Type
                    ? left.Type
                    : TypeKind.Invalid;

                if (inferredType == TypeKind.Invalid)
                {
                    diagnostics.Add(new(Severity.Error, "Type mismatch."));
                }

                if (inferredType == TypeKind.String && binaryExpression.Operator != TokenKind.Plus)
                {
                    diagnostics.Add(new(Severity.Error, $"Invalid operation on string: {Enum.GetName(binaryExpression.Operator)}."));
                }

                return new BoundBinaryExpression(binaryExpression.Operator, inferredType, left, right);
            }

            default:
                diagnostics.Add(new(Severity.Error, $"Unknown expression type: {expression.GetType().Name}"));
                return new BoundIntExpression(0);
        }
    }

    private static int GetOrCreateVariableId(string name, List<BoundVariableInfo?> variables) =>
        variables.SingleOrDefault(n => n?.Name == name) is (int id, _, _)
        ? id
        : variables.Count;
}
