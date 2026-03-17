using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class StringLiteralExpr(string value, TokenLocation location) : Expr(location)
{
	public readonly string Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not StringLiteralExpr other) return false;
		return Value == other.Value;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(StringLiteralExpr), Value);
	}

	public override string ToString()
	{
		return JsonSerializer.Serialize(new
		{
			type = nameof(StringLiteralExpr),
			value = Value
		});
	}
}
