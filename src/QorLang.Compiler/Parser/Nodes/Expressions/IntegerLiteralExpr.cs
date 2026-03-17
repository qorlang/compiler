using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class IntegerLiteralExpr(string value, TokenLocation location) : Expr(location)
{
	public readonly string Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not IntegerLiteralExpr other) return false;
		return Value == other.Value;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(IntegerLiteralExpr), Value);
	}

	public override string ToString()
	{
		return JsonSerializer.Serialize(new
		{
			type = nameof(IntegerLiteralExpr),
			value = Value
		});
	}
}
