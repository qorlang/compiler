namespace QorLang.Compiler.Parser.Nodes;

public class TypeDefinitionNode(string name, AccessLevel accessLevel, string[] typeParameters) : ASTNode
{
	public readonly string Name = name;
	public readonly AccessLevel AccessLevel = accessLevel;
	public readonly string[] TypeParameters = typeParameters;
	public readonly List<TypeLevelDeclarationNode> Declarations = [];

	public override bool Equals(object? obj)
	{
		if (obj is not TypeDefinitionNode other) return false;
		return Name == other.Name &&
			AccessLevel == other.AccessLevel &&
			TypeParameters.SequenceEqual(other.TypeParameters) &&
			Declarations.SequenceEqual(other.Declarations);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, AccessLevel, NodeUtils.GetArrayHash(TypeParameters), Declarations);
	}
}