using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.CodeStatements;

namespace QorLang.Compiler.Parser.Nodes;

public class MethodDefinitionNode(
	string name,
	AccessLevel accessLevel,
	ArgDeclarationNode[] parameters,
	string[] typeParameters,
	TypeReferenceNode returnType,
	DataProtection protectionOnSelf,
	CodeStmt[] body,
	TokenLocation location
) : TypeMemberDeclarationNode(location)
{
	public readonly string Name = name;
	public readonly AccessLevel AccessLevel = accessLevel;
	public readonly ArgDeclarationNode[] Parameters = parameters;
	public readonly string[] TypeParameters = typeParameters;
	public readonly TypeReferenceNode ReturnType = returnType;
	public readonly DataProtection ProtectionOnSelf = protectionOnSelf;
	public readonly CodeStmt[] Body = body;

	public override bool Equals(object? obj)
	{
		if (obj is not FunctionDefinitionNode other) return false;
		return Name == other.Name && AccessLevel == other.AccessLevel &&
			Parameters.SequenceEqual(other.Parameters) &&
			TypeParameters.SequenceEqual(other.TypeParameters) &&
			ReturnType.Equals(other.ReturnType) &&
			Body.SequenceEqual(other.Body);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, AccessLevel, NodeUtils.GetArrayHash(Parameters), NodeUtils.GetArrayHash(TypeParameters), ReturnType, ProtectionOnSelf, NodeUtils.GetArrayHash(Body));
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(MethodDefinitionNode), name = Name, accessLevel = AccessLevel.ToString(), parameters = Parameters.Select(p => JsonDocument.Parse(p.ToString()).RootElement).ToArray(), typeParameters = TypeParameters, returnType = JsonDocument.Parse(ReturnType.ToString()).RootElement, protectionOnSelf = ProtectionOnSelf.ToString(), body = Body.Select(b => JsonDocument.Parse(b.ToString()).RootElement).ToArray() });
}