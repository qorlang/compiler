using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ModAssignStmt(
	Expr target,
	Expr value,
	TokenLocation location
) : CodeStmt(location)
{
	public readonly Expr Target = target;
	public readonly Expr Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not ModAssignStmt other) return false;
		return Target == other.Target && Value.Equals(other.Value);
	}

	public override int GetHashCode() => HashCode.Combine(Target, Value);

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(ModAssignStmt), name = Target, value = JsonDocument.Parse(Value.ToString()).RootElement });
}
