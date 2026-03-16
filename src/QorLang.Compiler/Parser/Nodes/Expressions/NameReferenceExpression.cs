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
		return HashCode.Combine(Name, GenericArguments);
	}
}