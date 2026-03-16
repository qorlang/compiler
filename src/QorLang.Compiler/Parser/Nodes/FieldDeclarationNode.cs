namespace QorLang.Compiler.Parser.Nodes;

public class FieldDeclarationNode(
	string name,
	TypeReferenceNode dataType,
	Dictionary<AccessLevel, DataProtection[]> protections
) : TypeLevelDeclarationNode
{
	public readonly string Name = name;
	public readonly TypeReferenceNode DataType = dataType;
	public readonly Dictionary<AccessLevel, DataProtection[]> Protections = protections;

	public override bool Equals(object? obj)
	{
		if (obj is not FieldDeclarationNode other) return false;
		if (Name != other.Name || !DataType.Equals(other.DataType)) return false;
		if (Protections.Count != other.Protections.Count) return false;
		foreach (var kvp in Protections)
		{
			if (!other.Protections.TryGetValue(kvp.Key, out var otherProtections) || !kvp.Value.SequenceEqual(otherProtections))
				return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(Name);
		hash.Add(DataType);
		foreach (var kvp in Protections.OrderBy(x => x.Key))
		{
			hash.Add(kvp.Key);
			hash.Add(kvp.Value);
		}
		return hash.ToHashCode();
	}
}