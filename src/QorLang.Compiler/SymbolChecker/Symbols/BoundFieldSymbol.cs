using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class BoundFieldSymbol(TypeReference parentType, TypeReference dataType, string name, Dictionary<AccessLevel, DataProtection[]> protections) : Symbol(SymbolType.BoundField)
{
	public readonly TypeReference ParentType = parentType;
	public readonly TypeReference DataType = dataType;
	public readonly string Name = name;
	public readonly Dictionary<AccessLevel, DataProtection[]> Protections = protections;
	public readonly string FullyQualifiedName = $"{parentType.FullyQualifiedName}.{name}";
}