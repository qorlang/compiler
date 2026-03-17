using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class UnaryMinusExpr(Expr target) : Expr
{
	public readonly Expr Target = target;

	public override bool Equals(object? obj)
	{
		if (obj is not UnaryMinusExpr other) return false;
		return Target.Equals(other.Target);
	}

	public override int GetHashCode()
	{
		return Target.GetHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(UnaryMinusExpr), target = JsonDocument.Parse(Target.ToString()).RootElement });
}
