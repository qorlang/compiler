using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class IndexAssignStmt(
	IndexExpr target,
	Expr value,
	TokenLocation location
) : CodeStmt(location)
{
	public readonly IndexExpr Target = target;
	public readonly Expr Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not IndexAssignStmt other) return false;
		return Target.Equals(other.Target) && Value.Equals(other.Value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target, Value);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(IndexAssignStmt), target = JsonDocument.Parse(Target.ToString()).RootElement, value = JsonDocument.Parse(Value.ToString()).RootElement });
}
