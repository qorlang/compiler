using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class DerefExpr(Expr target, TokenLocation location) : UnaryOpExpr(target, location)
{

	public override bool Equals(object? obj)
	{
		if (obj is not DerefExpr other) return false;
		return Target.Equals(other.Target);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(DerefExpr), Target);
	}

	public override string ToString()
	{
		var targetJson = JsonDocument.Parse(Target.ToString()).RootElement;
		return JsonSerializer.Serialize(new
		{
			type = nameof(DerefExpr),
			target = targetJson
		});
	}
}
