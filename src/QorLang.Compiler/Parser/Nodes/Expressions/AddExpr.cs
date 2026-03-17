using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class AddExpr(Expr left, Expr right, TokenLocation location) : BinaryOpExpr(left, right, location)
{

	public override bool Equals(object? obj)
	{
		if (obj is not AddExpr other) return false;
		return Left.Equals(other.Left) && Right.Equals(other.Right);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(AddExpr), Left, Right);
	}

	public override string ToString()
	{
		var leftJson = JsonDocument.Parse(Left.ToString()).RootElement;
		var rightJson = JsonDocument.Parse(Right.ToString()).RootElement;
		return JsonSerializer.Serialize(new
		{
			type = nameof(AddExpr),
			left = leftJson,
			right = rightJson
		});
	}
}
