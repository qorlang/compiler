using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class RefExpr(Expr target) : Expr
{
	public readonly Expr Target = target;

	public override bool Equals(object? obj)
	{
		if (obj is not RefExpr other) return false;
		return Target.Equals(other.Target);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(RefExpr), Target);
	}

	public override string ToString()
	{
		var targetJson = JsonDocument.Parse(Target.ToString()).RootElement;
		return JsonSerializer.Serialize(new
		{
			type = nameof(RefExpr),
			target = targetJson
		});
	}
}
