using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes;

public class NamespaceNode(string name) : ASTNode
{
	public readonly string Name = name;

	public override bool Equals(object? obj)
	{
		return obj is NamespaceNode other && Name == other.Name;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(NamespaceNode), name = Name });
}