using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class NotEqualExpr(Expr left, Expr right, TokenLocation location) : BinaryOpExpr(left, right, location)
{

	public override bool Equals(object? obj)
	{
		if (obj is not NotEqualExpr other) return false;
		return Left.Equals(other.Left) && Right.Equals(other.Right);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Left, Right);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(NotEqualExpr), left = JsonDocument.Parse(Left.ToString()).RootElement, right = JsonDocument.Parse(Right.ToString()).RootElement });
}
