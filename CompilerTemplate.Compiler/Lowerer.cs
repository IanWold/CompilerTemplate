using System.Text;
using CompilerTemplate.Compiler;

public static class Lowerer
{
    public static string Lower(this BoundAstRoot root)
    {
        var builder = new StringBuilder();
        var slots = new Dictionary<int, string>();

        var strings = new Dictionary<string, string>(root.Statements.SelectMany(GetStringExpressions).Distinct().Select((s, i) => new KeyValuePair<string, string>(s.Value, $"$string{i}")));

        foreach (var expression in strings)
        {
            builder.AppendLine($"data {expression.Value} = {{ b \"{expression.Key}\", b 0 }}");
        }

        builder.AppendLine("data $fmt_int = { b \"%d\\n\", b 0 }");
        builder.AppendLine("data $fmt_str = { b \"%s\\n\", b 0 }");
        builder.AppendLine();
        builder.AppendLine("export function w $main() {");
        builder.AppendLine("@start");

        foreach (var info in root.VariableInfos)
        {
            var slot = Temp(info.Id);
            builder.AppendLine(info.Type switch
            {
                TypeKind.Int => $"\t{slot} =l alloc4 4",
                TypeKind.String => $"\t{slot} =l alloc8 8",
                _ => throw new Exception("Unlowerable variable encountered.")
            });
            slots[info.Id] = slot;
        }

        var tempId = root.VariableInfos.Count;

        foreach (var statement in root.Statements)
        {
            tempId = WriteStatement(statement, builder, strings, slots, tempId);
        }

        builder.AppendLine("\tret 0");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static IEnumerable<BoundStringExpression> GetStringExpressions(BoundStatement statement) => statement switch
    {
        BoundAssignStatement assignStatement => GetStringExpressions(assignStatement.Value),
        BoundPrintStatement printStatement => GetStringExpressions(printStatement.Value),
        _ => throw new Exception()
    };

    private static IEnumerable<BoundStringExpression> GetStringExpressions(BoundExpression expression) => expression switch
    {
        BoundStringExpression stringExpression => [stringExpression],
        BoundBinaryExpression binaryExpression => [..GetStringExpressions(binaryExpression.Left), ..GetStringExpressions(binaryExpression.Right)],
        _ => []
    };

    private static int WriteStatement(BoundStatement statement, StringBuilder builder, Dictionary<string, string> strings, Dictionary<int, string> slots, int tempId)
    {
        switch (statement)
        {
            case BoundAssignStatement assignStatement:
            {
                (var valueTemp, tempId) = WriteExpression(assignStatement.Value, builder, strings, slots, tempId);
                var storeKeyword = assignStatement.Value.Type == TypeKind.Int
                    ? "storew"
                    : "storel";

                builder.AppendLine($"\t{storeKeyword} {valueTemp}, {slots[assignStatement.Id]}");
                break;
            }

            case BoundPrintStatement printStatement:
            {
                (var valueTemp, tempId) = WriteExpression(printStatement.Value, builder, strings, slots, tempId);
                var (formatKeyword, typeKeyword) = printStatement.Value.Type == TypeKind.Int
                    ? ("int", "w")
                    : ("str", "l");

                builder.AppendLine($"\tcall $printf(l $fmt_{formatKeyword}, ..., {typeKeyword} {valueTemp})");
                break;
            }

            default:
                throw new NotSupportedException($"Unknown statement: {statement.GetType().Name}");
        }

        return tempId;
    }

    private static (string, int) WriteExpression(BoundExpression expression, StringBuilder builder, Dictionary<string, string> strings, Dictionary<int, string> slots, int tempId)
    {
        switch (expression)
        {
            case BoundIntExpression intExpression:
            {
                var temp = Temp(++tempId);
                builder.AppendLine($"\t{temp} =w copy {intExpression.Value}");

                return (temp, tempId);
            }

            case BoundStringExpression stringExpression when strings.TryGetValue(stringExpression.Value, out var label):
            {
                var temp = Temp(++tempId);
                builder.AppendLine($"\t{temp} =l copy {label}");
                return (temp, tempId);
            }

            case BoundVariableExpression variableExpression:
            {
                var temp = Temp(++tempId);
                var loadKeyword = variableExpression.Type == TypeKind.Int
                    ? "=w loadw"
                    : "=l loadl";
                builder.AppendLine($"\t{temp} {loadKeyword} {slots[variableExpression.Id]}");
                
                return (temp, tempId);
            }

            case BoundUnaryExpression unaryExpression:
            {
                (var right, tempId) = WriteExpression(unaryExpression.Right, builder, strings, slots, tempId);

                var zero = Temp(++tempId);
                builder.AppendLine($"\t{zero} =w copy 0");

                var temp = Temp(++tempId);
                builder.AppendLine($"\t{temp} =w sub {zero}, {right}");

                return (temp, tempId);
            }

            case BoundBinaryExpression binaryExpression:
            {
                (var left, tempId) = WriteExpression(binaryExpression.Left, builder, strings, slots, tempId);
                (var right, tempId) = WriteExpression(binaryExpression.Right, builder, strings, slots, tempId);
                var temp = Temp(++tempId);

                (string, int) LowerInt()
                {
                    var operation = binaryExpression.Operator switch
                    {
                        TokenKind.Plus => "add",
                        TokenKind.Minus => "sub",
                        TokenKind.Star => "mul",
                        TokenKind.Slash => "div",
                        TokenKind.Percent => "rem",
                        _ => throw new NotSupportedException($"Unknown operator '{Enum.GetName(binaryExpression.Operator)}'")
                    };

                    builder.AppendLine($"\t{temp} =w {operation} {left}, {right}");
                    return (temp,tempId);
                }

                (string, int) LowerString()
                {
                    // TODO: Add support for string concatenation
                    throw new NotSupportedException("Compiler has not implemented string concatenation with +");
                }

                return binaryExpression.Type switch
                {
                    TypeKind.String => LowerString(),
                    TypeKind.Int => LowerInt(),
                    var unknown => throw new NotSupportedException($"Unknown expression type {Enum.GetName(unknown)}")
                };
            }

            default:
                throw new NotSupportedException($"Unknown expr: {expression.GetType().Name}");
        }
    }

    private static string Temp(int id) => $"%t{id}";
}
