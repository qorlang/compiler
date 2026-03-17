using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class ThisExpr(TokenLocation location) : Expr(location)
{
	public override bool Equals(object? obj)
	{
		return obj is ThisExpr;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(ThisExpr));
	}

	public override string ToString()
	{
		return JsonSerializer.Serialize(new
		{
			type = nameof(ThisExpr)
		});
	}
}
