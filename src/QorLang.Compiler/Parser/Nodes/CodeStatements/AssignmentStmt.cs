using System.Text.Json;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class AssignmentStmt(
	Expr target,
	Expr value
) : CodeStmt
{
	public readonly Expr Target = target;
	public readonly Expr Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not AssignmentStmt other) return false;
		return Target == other.Target && Value.Equals(other.Value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target, Value);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(AssignmentStmt), name = Target, value = JsonDocument.Parse(Value.ToString()).RootElement });
}