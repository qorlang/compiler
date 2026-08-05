using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class DotExpr(
	Expr target,
	string propertyName,
	TypeReferenceNode[] typeArguments,
	TokenLocation location
) : Expr(location)
{
	public readonly Expr Target = target;
	public readonly NameReferenceExpr Property = new(propertyName, typeArguments, location);

	public override bool Equals(object? obj)
	{
		if (obj is not DotExpr other || obj.GetType() != GetType()) return false;
		return Target.Equals(other.Target) && Property.Name == other.Property.Name && Property.TypeArguments.SequenceEqual(other.Property.TypeArguments);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target, Property.Name, NodeUtils.GetArrayHash(Property.TypeArguments));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(DotExpr), target = JsonDocument.Parse(Target.ToString()).RootElement, propertyName = Property.Name, typeParameters = Property.TypeArguments.Length > 0 ? Property.TypeArguments.Select(t => JsonDocument.Parse(t.ToString()).RootElement).ToArray() : null });
}