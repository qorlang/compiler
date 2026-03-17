using System.Text.Json;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ExprStmt(Expr expr) : CodeStmt
{
	public readonly Expr Expr = expr;

	public override bool Equals(object? obj)
	{
		if (obj is not ExprStmt other) return false;
		return Expr.Equals(other.Expr);
	}

	public override int GetHashCode()
	{
		return Expr.GetHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(ExprStmt), expr = JsonDocument.Parse(Expr.ToString()).RootElement });
}