using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class WhileStmt(
	Expr condition,
	CodeStmt[] body,
	TokenLocation location
) : CodeStmt(location)
{
	public readonly Expr Condition = condition;
	public readonly CodeStmt[] Body = body;

	public override bool Equals(object? obj)
	{
		if (obj is not WhileStmt other) return false;
		return Condition.Equals(other.Condition) && Body.SequenceEqual(other.Body);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Condition, NodeUtils.GetArrayHash(Body));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(WhileStmt), condition = JsonDocument.Parse(Condition.ToString()).RootElement, body = Body.Select(b => JsonDocument.Parse(b.ToString()).RootElement).ToArray() });
}
