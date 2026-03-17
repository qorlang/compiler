using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class LogicalOrExpr(Expr left, Expr right) : Expr
{
	public readonly Expr Left = left;
	public readonly Expr Right = right;

	public override bool Equals(object? obj)
	{
		if (obj is not LogicalOrExpr other) return false;
		return Left.Equals(other.Left) && Right.Equals(other.Right);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Left, Right);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(LogicalOrExpr), left = JsonDocument.Parse(Left.ToString()).RootElement, right = JsonDocument.Parse(Right.ToString()).RootElement });
}
