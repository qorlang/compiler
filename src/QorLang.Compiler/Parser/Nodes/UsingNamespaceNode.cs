using System.Text.Json;

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

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(UsingNamespaceNode), name = Name });
}