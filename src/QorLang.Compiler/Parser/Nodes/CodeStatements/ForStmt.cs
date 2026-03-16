using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ForStmt(
	string iteratorName,
	Expr iterableExpression,
	List<CodeStmt> body
) : CodeStmt
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
}
