using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public class DotExpr(
	Expr target,
	string propertyName,
	TypeReferenceNode[] typeArguments
) : Expr
{
	public readonly Expr Target = target;
	public readonly string PropertyName = propertyName;
	public readonly TypeReferenceNode[] TypeArguments = typeArguments;

	public override bool Equals(object? obj)
	{
		if (obj is not DotExpr other || obj.GetType() != GetType()) return false;
		return Target.Equals(other.Target) && PropertyName == other.PropertyName && TypeArguments.SequenceEqual(other.TypeArguments);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target, PropertyName, NodeUtils.GetArrayHash(TypeArguments));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(DotExpr), target = JsonDocument.Parse(Target.ToString()).RootElement, propertyName = PropertyName, typeParameters = TypeArguments.Length > 0 ? TypeArguments.Select(t => JsonDocument.Parse(t.ToString()).RootElement).ToArray() : null });
}