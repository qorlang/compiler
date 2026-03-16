namespace QorLang.Compiler.Parser.Nodes;

public class TypeReferenceNode(
	string typeName,
	int refLevel,
	int arrayLevel,
	TypeReferenceNode[] typeArguments
) : ASTNode
{
	public readonly string TypeName = typeName;
	public readonly int RefLevel = refLevel;
	public readonly int ArrayLevel = arrayLevel;
	public readonly TypeReferenceNode[] TypeArguments = typeArguments;

	public bool IsVoid => TypeName == "::void" && RefLevel == 0 && TypeArguments.Length == 0;

	public static TypeReferenceNode Void() => new("::void", 0, 0, []);

	public override bool Equals(object? obj)
	{
		if (obj is not TypeReferenceNode other) return false;
		return TypeName == other.TypeName &&
			RefLevel == other.RefLevel &&
			ArrayLevel == other.ArrayLevel &&
			TypeArguments.SequenceEqual(other.TypeArguments);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(TypeName, RefLevel, ArrayLevel, TypeArguments);
	}
}