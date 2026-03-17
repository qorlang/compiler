using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes;

public class TypeDefinitionNode(string name, AccessLevel accessLevel, string[] typeParameters, TypeMemberDeclarationNode[] memberDeclarations) : ASTNode
{
	public readonly string Name = name;
	public readonly AccessLevel AccessLevel = accessLevel;
	public readonly string[] TypeParameters = typeParameters;
	public readonly TypeMemberDeclarationNode[] MemberDeclarations = memberDeclarations;

	public override bool Equals(object? obj)
	{
		if (obj is not TypeDefinitionNode other) return false;
		return Name == other.Name &&
			AccessLevel == other.AccessLevel &&
			TypeParameters.SequenceEqual(other.TypeParameters) &&
			MemberDeclarations.SequenceEqual(other.MemberDeclarations);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, AccessLevel, NodeUtils.GetArrayHash(TypeParameters), NodeUtils.GetArrayHash(MemberDeclarations));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(TypeDefinitionNode), name = Name, accessLevel = AccessLevel.ToString(), typeParameters = TypeParameters, memberDeclarations = MemberDeclarations.Select(m => JsonDocument.Parse(m.ToString()).RootElement).ToArray() });
}