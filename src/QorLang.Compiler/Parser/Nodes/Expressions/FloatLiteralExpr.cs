using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class FloatLiteralExpr(string value, TokenLocation location) : Expr(location)
{
	public readonly string Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not FloatLiteralExpr other) return false;
		return Value == other.Value;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(FloatLiteralExpr), Value);
	}

	public override string ToString()
	{
		return JsonSerializer.Serialize(new
		{
			type = nameof(FloatLiteralExpr),
			value = Value
		});
	}
}
