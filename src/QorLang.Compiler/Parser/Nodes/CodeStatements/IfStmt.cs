using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class IfStmt(
	Expr condition,
	List<CodeStmt> thenBody,
	List<CodeStmt> elseBody,
	TokenLocation location
) : CodeStmt(location)
{
	public readonly Expr Condition = condition;
	public readonly List<CodeStmt> ThenBody = thenBody;
	public readonly List<CodeStmt> ElseBody = elseBody;

	public IfStmt(
		Expr condition,
		List<CodeStmt> thenBody,
		TokenLocation location
	) : this(condition, thenBody, [], location) { }

	public override bool Equals(object? obj)
	{
		if (obj is not IfStmt other) return false;
		return Condition.Equals(other.Condition) &&
			ThenBody.SequenceEqual(other.ThenBody) &&
			ElseBody.SequenceEqual(other.ElseBody);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Condition, ThenBody, ElseBody);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(IfStmt), condition = JsonDocument.Parse(Condition.ToString()).RootElement, thenBody = ThenBody.Select(b => JsonDocument.Parse(b.ToString()).RootElement).ToArray(), elseBody = ElseBody.Select(b => JsonDocument.Parse(b.ToString()).RootElement).ToArray() });
}
