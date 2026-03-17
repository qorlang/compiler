using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ReturnStmt(
	Expr? value,
	TokenLocation location
) : CodeStmt(location)
{
	public readonly Expr? Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not ReturnStmt other) return false;
		return (Value?.Equals(other.Value) ?? other.Value is null);
	}

	public override int GetHashCode()
	{
		return Value?.GetHashCode() ?? 0;
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(ReturnStmt), value = Value is not null ? JsonDocument.Parse(Value.ToString()).RootElement : JsonSerializer.SerializeToElement<object?>(null) });
}