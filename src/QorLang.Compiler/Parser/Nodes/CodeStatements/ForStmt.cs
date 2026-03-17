using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ForStmt(
	string iteratorName,
	Expr iterableExpression,
	List<CodeStmt> body,
	TokenLocation location
) : CodeStmt(location)
{
	public readonly string IteratorName = iteratorName;
	public readonly Expr IterableExpression = iterableExpression;
	public readonly List<CodeStmt> Body = body;

	public override bool Equals(object? obj)
	{
		if (obj is not ForStmt other) return false;
		return IteratorName == other.IteratorName &&
			IterableExpression.Equals(other.IterableExpression) &&
			Body.SequenceEqual(other.Body);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(IteratorName, IterableExpression, Body);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(ForStmt), iteratorName = IteratorName, iterableExpression = JsonDocument.Parse(IterableExpression.ToString()).RootElement, body = Body.Select(b => JsonDocument.Parse(b.ToString()).RootElement).ToArray() });
}
