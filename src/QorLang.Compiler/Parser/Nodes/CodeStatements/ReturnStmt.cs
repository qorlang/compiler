using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ReturnStmt(
	Expr? value
) : CodeStmt
{
	public readonly Expr? Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not ReturnStmt other) return false;
		return (Value?.Equals(other.Value) ?? other.Value is null);
	}

	public override int GetHashCode()
	{
		return Value?.GetHashCode() ?? 0;
	}
}