using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class WhileStmt(
	Expr condition,
	List<CodeStmt> body
) : CodeStmt
{
	public readonly Expr Condition = condition;
	public readonly List<CodeStmt> Body = body;

	public override bool Equals(object? obj)
	{
		if (obj is not WhileStmt other) return false;
		return Condition.Equals(other.Condition) && Body.SequenceEqual(other.Body);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Condition, Body);
	}
}
