using System.Text.Json;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes;

public class GlobalDeclarationNode(
	string name,
	TypeReferenceNode dataType,
	Dictionary<AccessLevel, DataProtection[]> protections,
	Expr? initializer
) : ASTNode
{
	public readonly string Name = name;
	public readonly TypeReferenceNode DataType = dataType;
	public readonly Dictionary<AccessLevel, DataProtection[]> Protections = protections;
	public readonly Expr? Initializer = initializer;

	public override bool Equals(object? obj)
	{
		if (obj is not GlobalDeclarationNode other) return false;
		if (Name != other.Name || !DataType.Equals(other.DataType)) return false;
		if (Protections.Count != other.Protections.Count) return false;
		foreach (var kvp in Protections)
		{
			if (!other.Protections.TryGetValue(kvp.Key, out var otherProtections) || !kvp.Value.SequenceEqual(otherProtections))
				return false;
		}
		return Initializer?.Equals(other.Initializer) ?? other.Initializer is null;
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
		hash.Add(Initializer);
		return hash.ToHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(GlobalDeclarationNode), name = Name, dataType = JsonDocument.Parse(DataType.ToString()).RootElement, protections = Protections.ToDictionary(k => k.Key.ToString(), v => v.Value.Select(p => p.ToString()).ToArray()), initializer = Initializer is not null ? JsonDocument.Parse(Initializer.ToString()).RootElement : JsonSerializer.SerializeToElement<object?>(null) });
}