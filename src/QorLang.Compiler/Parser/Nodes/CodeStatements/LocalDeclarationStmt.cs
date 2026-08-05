using System.Text.Json;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class LocalDeclarationStmt(
	string name,
	TypeReferenceNode? dataType,
	DataProtection[]? protections, // protections can be null due to inference
	Expr? initializer,
	TokenLocation location
) : CodeStmt(location)
{
	public readonly string Name = name;
	public readonly TypeReferenceNode? DataType = dataType;
	public readonly DataProtection[]? Protections = protections;
	public readonly Expr? Initializer = initializer;

	public bool IsDeclarationOnly => Initializer is null;
	public bool IsDefinition => !IsDeclarationOnly;
	public bool IsInference => DataType is null;

	public override bool Equals(object? obj)
	{
		if (obj is not LocalDeclarationStmt other) return false;
		return Name == other.Name &&
			(DataType?.Equals(other.DataType) ?? other.DataType is null) &&
			(Protections?.SequenceEqual(other.Protections) ?? other.Protections is null) &&
			(Initializer?.Equals(other.Initializer) ?? other.Initializer is null);
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(Name);
		hash.Add(DataType);
		hash.Add(Protections);
		hash.Add(Initializer);
		return hash.ToHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(LocalDeclarationStmt), name = Name, dataType = JsonDocument.Parse(DataType?.ToString() ?? "\"inferred\"").RootElement, protections = Protections?.Select(p => p.ToString())?.ToArray(), initializer = Initializer is not null ? JsonDocument.Parse(Initializer.ToString()).RootElement : JsonSerializer.SerializeToElement<object?>(null) });
}