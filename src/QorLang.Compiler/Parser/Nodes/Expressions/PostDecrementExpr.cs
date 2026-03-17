using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class PostDecrementExpr(Expr target, TokenLocation location) : UnaryOpExpr(target, location)
{

	public override bool Equals(object? obj)
	{
		if (obj is not PostDecrementExpr other) return false;
		return Target.Equals(other.Target);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(PostDecrementExpr), target = JsonDocument.Parse(Target.ToString()).RootElement });
}