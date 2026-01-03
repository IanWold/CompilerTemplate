using System.Text;
using CompilerTemplate.Compiler;

public static class Lowerer
{
    public static string Lower(this BoundAstRoot root)
    {
        var builder = new StringBuilder();
        var slots = new Dictionary<int, string>();

        var tempId = 0;

        builder.AppendLine("data $fmt_int = { b \"%d\\n\", b 0 }");
        builder.AppendLine();
        builder.AppendLine("export function w $main() {");
        builder.AppendLine("@start");

        for (int id = 0; id < root.VariableNamesById.Count; id++)
        {
            var slot = Temp(id);
            builder.AppendLine($"\t{slot} =l alloc4 4");
            slots[id] = slot;
        }

        foreach (var statement in root.Statements)
        {
            tempId = WriteStatement(statement, builder, slots, tempId);
        }

        builder.AppendLine("\tret 0");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static int WriteStatement(BoundStatement statement, StringBuilder builder, Dictionary<int, string> slots, int tempId)
    {
        switch (statement)
        {
            case BoundAssignStatement assignStatement:
            {
                (var valueTemp, tempId) = WriteExpression(assignStatement.Value, builder, slots, tempId);

                builder.AppendLine($"\tstorew {valueTemp}, {slots[assignStatement.Id]}");

                break;
            }

            case BoundPrintStatement printStatement:
            {
                (var valueTemp, tempId) = WriteExpression(printStatement.Value, builder, slots, tempId);
                builder.AppendLine($"\tcall $printf(l $fmt_int, ..., w {valueTemp})");

                break;
            }

            default:
                throw new NotSupportedException($"Unknown stmt: {statement.GetType().Name}");
        }

        return tempId;
    }

    private static (string, int) WriteExpression(BoundExpression expression, StringBuilder builder, Dictionary<int, string> slots, int tempId)
    {
        switch (expression)
        {
            case BoundIntExpression intExpression:
            {
                var temp = Temp(++tempId);
                builder.AppendLine($"\t{temp} =w copy {intExpression.Value}");

                return (temp, tempId);
            }

            case BoundVariableExpression variableExpression:
            {
                var temp = Temp(++tempId);
                builder.AppendLine($"\t{temp} =w loadw {slots[variableExpression.Id]}");
                
                return (temp, tempId);
            }

            case BoundUnaryExpression unaryExpression:
            {
                (var right, tempId) = WriteExpression(unaryExpression.Right, builder, slots, tempId);

                var zero = Temp(++tempId);
                builder.AppendLine($"\t{zero} =w copy 0");

                var temp = Temp(++tempId);
                builder.AppendLine($"\t{temp} =w sub {zero}, {right}");

                return (temp, tempId);
            }

            case BoundBinaryExpression binaryExpression:
            {
                (var left, tempId) = WriteExpression(binaryExpression.Left, builder, slots, tempId);
                (var right, tempId) = WriteExpression(binaryExpression.Right, builder, slots, tempId);
                var temp = Temp(++tempId);

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

            default:
                throw new NotSupportedException($"Unknown expr: {expression.GetType().Name}");
        }
    }

    private static string Temp(int id) => $"%t{id}";
}
