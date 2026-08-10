using QorLang.Compiler.SymbolChecker.SymbolReferences;
using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class StaticFieldSymbol(
	string name,
	TypeSymbol enclosingType,
	TypeReference dataType,
	Dictionary<AccessLevel, DataProtection[]> protections
) : Symbol(SymbolType.StaticField, name)
{
	public readonly TypeSymbol EnclosingType = enclosingType;
	public readonly TypeReference DataType = dataType;
	public readonly Dictionary<AccessLevel, DataProtection[]> Protections = protections;
	public readonly string FullyQualifiedName = $"{enclosingType.FullyQualifiedName}.{name}";
}