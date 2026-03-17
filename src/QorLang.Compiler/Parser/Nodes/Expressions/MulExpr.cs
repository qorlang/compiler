using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class MulExpr(Expr left, Expr right) : Expr
{
	public readonly Expr Left = left;
	public readonly Expr Right = right;

	public override bool Equals(object? obj)
	{
		if (obj is not MulExpr other) return false;
		return Left.Equals(other.Left) && Right.Equals(other.Right);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(MulExpr), Left, Right);
	}

	public override string ToString()
	{
		var leftJson = JsonDocument.Parse(Left.ToString()).RootElement;
		var rightJson = JsonDocument.Parse(Right.ToString()).RootElement;
		return JsonSerializer.Serialize(new
		{
			type = nameof(MulExpr),
			left = leftJson,
			right = rightJson
		});
	}
}
