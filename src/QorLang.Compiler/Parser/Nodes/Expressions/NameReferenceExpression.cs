using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes;

public abstract class NameReferenceExpression(
	string name,
	TypeReferenceNode[] typeArguments
) : ASTNode
{
	public readonly string Name = name;
	public readonly TypeReferenceNode[] GenericArguments = typeArguments;

	public override bool Equals(object? obj)
	{
		if (obj is not NameReferenceExpression other || obj.GetType() != GetType()) return false;
		return Name == other.Name && GenericArguments.SequenceEqual(other.GenericArguments);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, NodeUtils.GetArrayHash(GenericArguments));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(NameReferenceExpression), name = Name, genericArguments = GenericArguments.Select(g => JsonDocument.Parse(g.ToString()).RootElement).ToArray() });
}