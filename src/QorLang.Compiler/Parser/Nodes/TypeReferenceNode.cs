using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes;

public class TypeReferenceNode(
	string typeName,
	IndirectionLayer[] indirectionLayers,
	TypeReferenceNode[] typeArguments
) : ASTNode
{
	public readonly string TypeName = typeName;

	/// <summary>
	/// The innermost indirection layer (closest to the base type) is listed first.
	/// </summary>
	public readonly IndirectionLayer[] IndirectionLayers = indirectionLayers;
	public readonly TypeReferenceNode[] TypeArguments = typeArguments;

	public bool IsVoid => TypeName == "::void" && IndirectionLayers.Length == 0 && TypeArguments.Length == 0;
	
	/// <summary>
	/// For compat with old parser logic which didn't support mixed array/ptr indirections.
	/// </summary>
	public int RefLevel => IndirectionLayers.Count(IndirectionLayer.PointerTo);

	public static TypeReferenceNode Void() => new("::void", [], []);

	public override bool Equals(object? obj)
	{
		if (obj is not TypeReferenceNode other) return false;
		return TypeName == other.TypeName &&
			IndirectionLayers.SequenceEqual(other.IndirectionLayers) &&
			TypeArguments.SequenceEqual(other.TypeArguments);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(TypeName, IndirectionLayers, TypeArguments);
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(TypeReferenceNode), typeName = TypeName, indirectionLayers = IndirectionLayers.Select(il => il.ToString()).ToArray(), typeArguments = TypeArguments });
}