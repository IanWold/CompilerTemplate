namespace CompilerTemplate;

#region Ast

public sealed record AstRoot(List<AstStatement> Statements);

public abstract record AstStatement;
public sealed record AssignStatement(string Name, AstExpression Value) : AstStatement;
public sealed record PrintStatement(AstExpression Value) : AstStatement;

public abstract record AstExpression;
public sealed record IntExpression(int Value) : AstExpression;
public sealed record VaraibleExpression(string Name) : AstExpression;
public sealed record UnaryExpression(TokenKind Operator, AstExpression Right) : AstExpression;
public sealed record BinaryExpression(TokenKind Operator, AstExpression Left, AstExpression Right) : AstExpression;

#endregion

#region Bound Ast

public sealed record BoundAstRoot(IReadOnlyList<BoundStatement> Statements, IReadOnlyList<string> VariableNamesById, IReadOnlyList<Diagnostic> Diagnostics);

public abstract record BoundStatement;
public sealed record BoundAssignStatement(int Id, BoundExpression Value) : BoundStatement;
public sealed record BoundPrintStatement(BoundExpression Value) : BoundStatement;

public abstract record BoundExpression;
public sealed record BoundIntExpression(int Value) : BoundExpression;
public sealed record BoundVariableExpression(int Id) : BoundExpression;
public sealed record BoundBinaryExpression(TokenKind Operator, BoundExpression Left, BoundExpression Right) : BoundExpression;

#endregion
