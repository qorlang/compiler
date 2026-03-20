using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ForStmt(
	CodeStmt? initializer,
	Expr? condition,
	CodeStmt? increment,
	CodeStmt[] body,
	TokenLocation location
) : CodeStmt(location)
{
	public readonly CodeStmt? Initializer = initializer;
	public readonly Expr? Condition = condition;
	public readonly CodeStmt? Increment = increment;
	public readonly CodeStmt[] Body = body;

	public override bool Equals(object? obj)
	{
		if (obj is not ForStmt other) return false;

		return Equals(Initializer, other.Initializer) &&
			   Equals(Condition, other.Condition) &&
			   Equals(Increment, other.Increment) &&
			   Body.SequenceEqual(other.Body);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Initializer, Condition, Increment, NodeUtils.GetArrayHash(Body));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(ForStmt), initializer = JsonDocument.Parse(Initializer?.ToString() ?? "null").RootElement, condition = JsonDocument.Parse(Condition?.ToString() ?? "null").RootElement, increment = JsonDocument.Parse(Increment?.ToString() ?? "null").RootElement, body = Body.Select(b => JsonDocument.Parse(b.ToString()).RootElement).ToArray() });
}
