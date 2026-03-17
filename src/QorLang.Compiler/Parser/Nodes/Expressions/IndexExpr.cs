using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class IndexExpr(Expr target, Expr index) : Expr
{
	public readonly Expr Target = target;
	public readonly Expr Index = index;

	public override bool Equals(object? obj)
	{
		if (obj is not IndexExpr other) return false;
		return Target.Equals(other.Target) && Index.Equals(other.Index);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target, Index);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(IndexExpr), target = JsonDocument.Parse(Target.ToString()).RootElement, index = JsonDocument.Parse(Index.ToString()).RootElement });
}
