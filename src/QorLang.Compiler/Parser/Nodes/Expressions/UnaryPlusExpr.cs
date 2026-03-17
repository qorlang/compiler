using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class UnaryPlusExpr(Expr target) : Expr
{
	public readonly Expr Target = target;

	public override bool Equals(object? obj)
	{
		if (obj is not UnaryPlusExpr other) return false;
		return Target.Equals(other.Target);
	}

	public override int GetHashCode()
	{
		return Target.GetHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(UnaryPlusExpr), target = JsonDocument.Parse(Target.ToString()).RootElement });
}
