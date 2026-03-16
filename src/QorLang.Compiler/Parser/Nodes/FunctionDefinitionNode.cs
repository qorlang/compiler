using QorLang.Compiler.Parser.Nodes.CodeStatements;

namespace QorLang.Compiler.Parser.Nodes;

public class FunctionDefinitionNode(
	string name,
	AccessLevel accessLevel,
	ArgDeclarationNode[] parameters,
	string[] typeParameters,
	TypeReferenceNode returnType,
	CodeStmt[] body
) : ASTNode
{
	public readonly string Name = name;
	public readonly AccessLevel AccessLevel = accessLevel;
	public readonly ArgDeclarationNode[] Parameters = parameters;
	public readonly string[] TypeParameters = typeParameters;
	public readonly TypeReferenceNode ReturnType = returnType;
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
		return HashCode.Combine(Name, AccessLevel, Parameters, TypeParameters, ReturnType, Body);
	}
}