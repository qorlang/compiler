using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class NameReferenceExpr(
	string name,
	TypeReferenceNode[] typeArguments,
	TokenLocation location
) : Expr(location)
{
	public readonly string Name = name;
	public readonly TypeReferenceNode[] TypeArguments = typeArguments;

	public override bool Equals(object? obj)
	{
		if (obj is not NameReferenceExpr other || obj.GetType() != GetType()) return false;
		return Name == other.Name && TypeArguments.SequenceEqual(other.TypeArguments);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, NodeUtils.GetArrayHash(TypeArguments));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(NameReferenceExpr), name = Name, genericArguments = TypeArguments.Select(g => JsonDocument.Parse(g.ToString()).RootElement).ToArray() });
}