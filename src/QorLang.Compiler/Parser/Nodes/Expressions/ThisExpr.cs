using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class ThisExpr : Expr
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
