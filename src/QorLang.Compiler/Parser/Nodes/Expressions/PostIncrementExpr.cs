using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class PostIncrementExpr(Expr target) : Expr
{
	public readonly Expr Target = target;

	public override bool Equals(object? obj)
	{
		if (obj is not PostIncrementExpr other) return false;
		return Target.Equals(other.Target);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(PostIncrementExpr), target = JsonDocument.Parse(Target.ToString()).RootElement });
}