namespace CompilerTemplate.Compiler;

public enum Severity { Error, Warning }

public sealed record Diagnostic(Severity Severity, string Message);
