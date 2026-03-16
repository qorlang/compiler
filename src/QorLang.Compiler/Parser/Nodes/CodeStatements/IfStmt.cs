using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class IfStmt(
	Expr condition,
	List<CodeStmt> thenBody,
	List<CodeStmt> elseBody
) : CodeStmt
{
	public readonly Expr Condition = condition;
	public readonly List<CodeStmt> ThenBody = thenBody;
	public readonly List<CodeStmt> ElseBody = elseBody;

	public IfStmt(
		Expr condition,
		List<CodeStmt> thenBody
	) : this(condition, thenBody, []) { }

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
}
