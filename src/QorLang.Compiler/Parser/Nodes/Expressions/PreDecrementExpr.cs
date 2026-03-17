using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class PreDecrementExpr(Expr target) : Expr
{
	public readonly Expr Target = target;

	public override bool Equals(object? obj)
	{
		if (obj is not PreDecrementExpr other) return false;
		return Target.Equals(other.Target);
	}

	public override int GetHashCode()
	{
		return Target.GetHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(PreDecrementExpr), target = JsonDocument.Parse(Target.ToString()).RootElement });
}
