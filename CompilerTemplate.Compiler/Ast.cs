namespace CompilerTemplate.Compiler;

#region Ast

public record AstRoot(List<AstStatement> Statements);

public abstract record AstStatement;
public record AssignStatement(string Name, AstExpression Value) : AstStatement;
public record PrintStatement(AstExpression Value) : AstStatement;

public abstract record AstExpression;
public record IntExpression(int Value) : AstExpression;
public record StringExpression(string Value) : AstExpression;
public record VariableExpression(string Name) : AstExpression;
public record UnaryExpression(TokenKind Operator, AstExpression Right) : AstExpression;
public record BinaryExpression(TokenKind Operator, AstExpression Left, AstExpression Right) : AstExpression;

#endregion

#region Bound Ast

public record BoundVariableInfo(int Id, string Name, TypeKind Type);

public record BoundAstRoot(IReadOnlyList<BoundStatement> Statements, IReadOnlyList<BoundVariableInfo> VariableInfos, IReadOnlyList<Diagnostic> Diagnostics);

public abstract record BoundStatement;
public record BoundAssignStatement(int Id, BoundExpression Value) : BoundStatement;
public record BoundPrintStatement(BoundExpression Value) : BoundStatement;

public abstract record BoundExpression(TypeKind Type);
public abstract record BoundLiteralExpression(TypeKind Type) : BoundExpression(Type);
public record BoundIntExpression(int Value) : BoundLiteralExpression(TypeKind.Int);
public record BoundStringExpression(string Value) : BoundLiteralExpression(TypeKind.String);
public record BoundVariableExpression(int Id, TypeKind Type) : BoundExpression(Type);
public record BoundUnaryExpression(TokenKind Operator, BoundExpression Right) : BoundExpression(Right.Type);
public record BoundBinaryExpression(TokenKind Operator, TypeKind Type, BoundExpression Left, BoundExpression Right) : BoundExpression(Type);

#endregion
