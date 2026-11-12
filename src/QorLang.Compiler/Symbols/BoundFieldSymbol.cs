using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Symbols.References;
using QorLang.Compiler.Symbols.SymbolTables;

namespace QorLang.Compiler.Symbols;

public class BoundFieldSymbol(
	string name,
	TypeSymbol enclosingType,
	TypeReference dataType,
	Dictionary<AccessLevel, DataProtection[]> protections
) : Symbol(SymbolType.BoundField, name)
{
	public readonly TypeSymbol EnclosingType = enclosingType;
	public readonly TypeReference DataType = dataType;
	public readonly Dictionary<AccessLevel, DataProtection[]> Protections = protections;
	public readonly string FullyQualifiedName = $"{enclosingType.FullyQualifiedName}.{name}";
}