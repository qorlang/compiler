using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class ErrorExpr() : Expr(default)
{
	public override bool Equals(object? obj)
	{
		return obj is ErrorExpr;
	}

	public override int GetHashCode()
	{
		return typeof(ErrorExpr).GetHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(ErrorExpr) });
}