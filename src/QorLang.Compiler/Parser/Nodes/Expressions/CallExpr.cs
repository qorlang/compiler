using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class CallExpr(
	Expr target,
	Expr[] arguments,
	TokenLocation location
) : Expr(location)
{
	public readonly Expr Target = target;
	public readonly Expr[] Arguments = arguments;

	public override bool Equals(object? obj)
	{
		if (obj is not CallExpr other || obj.GetType() != GetType()) return false;
		return Target.Equals(other.Target) && Arguments.SequenceEqual(other.Arguments);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target, NodeUtils.GetArrayHash(Arguments));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(CallExpr), target = JsonDocument.Parse(Target.ToString()).RootElement, arguments = Arguments.Select(a => JsonDocument.Parse(a.ToString()).RootElement).ToArray() });
}