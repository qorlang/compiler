using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class BooleanLiteralExpr(string value, TokenLocation location) : Expr(location)
{
	public readonly string Value = value;
	public bool IsTrue => Value == "true";

	public override bool Equals(object? obj)
	{
		if (obj is not BooleanLiteralExpr other) return false;
		return Value == other.Value;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(BooleanLiteralExpr), Value);
	}

	public override string ToString()
	{
		return JsonSerializer.Serialize(new { type = nameof(BooleanLiteralExpr), value = Value });
	}
}
