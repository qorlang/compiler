namespace QorLang.Compiler.Parser.Nodes;

public class UsingNamespaceNode(
	string namespaceName
) : ASTNode
{
	public readonly string Name = namespaceName;

	public override bool Equals(object? obj)
	{
		return obj is UsingNamespaceNode other && Name == other.Name;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}