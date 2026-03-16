namespace QorLang.Compiler.Parser.Nodes;

public class ArgDeclarationNode(
	string name,
	TypeReferenceNode dataType,
	DataProtection[] protections
) : ASTNode
{
	public readonly string Name = name;
	public readonly TypeReferenceNode DataType = dataType;
	public readonly DataProtection[] Protections = protections;

	public override bool Equals(object? obj)
	{
		if (obj is not ArgDeclarationNode other) return false;
		return Name == other.Name && DataType.Equals(other.DataType) && Protections.SequenceEqual(other.Protections);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, DataType, Protections);
	}
}