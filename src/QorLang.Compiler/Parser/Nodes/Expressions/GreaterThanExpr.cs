using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class GreaterThanExpr(Expr left, Expr right, TokenLocation location) : BinaryOpExpr(left, right, location)
{

	public override bool Equals(object? obj)
	{
		if (obj is not GreaterThanExpr other) return false;
		return Left.Equals(other.Left) && Right.Equals(other.Right);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(GreaterThanExpr), Left, Right);
	}

	public override string ToString()
	{
		var leftJson = JsonDocument.Parse(Left.ToString()).RootElement;
		var rightJson = JsonDocument.Parse(Right.ToString()).RootElement;
		return JsonSerializer.Serialize(new
		{
			type = nameof(GreaterThanExpr),
			left = leftJson,
			right = rightJson
		});
	}
}
