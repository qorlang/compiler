using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class LocalDeclarationStmt(
	string name,
	TypeReferenceNode dataType,
	DataProtection[] protections,
	Expr? initializer
) : CodeStmt
{
	public readonly string Name = name;
	public readonly TypeReferenceNode DataType = dataType;
	public readonly DataProtection[] Protections = protections;
	public readonly Expr? Initializer = initializer;

	public override bool Equals(object? obj)
	{
		if (obj is not LocalDeclarationStmt other) return false;
		return Name == other.Name &&
			DataType.Equals(other.DataType) &&
			Protections.SequenceEqual(other.Protections) &&
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
}