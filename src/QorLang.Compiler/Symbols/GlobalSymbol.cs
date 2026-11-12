using QorLang.Compiler.Symbols.References;
using QorLang.Compiler.Symbols.SymbolTables;

namespace QorLang.Compiler.Symbols;

public class GlobalSymbol(
	string name,
	string namespaceName,
	TypeReference dataType
) : Symbol(SymbolType.Global, name)
{
	public readonly string Namespace = namespaceName;
	public readonly TypeReference DataType = dataType;
	public readonly string FullyQualifiedName = string.IsNullOrEmpty(namespaceName) ? name : $"{namespaceName}.{name}";
}