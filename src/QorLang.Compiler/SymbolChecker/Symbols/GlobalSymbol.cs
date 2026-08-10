using QorLang.Compiler.SymbolChecker.SymbolReferences;

namespace QorLang.Compiler.SymbolChecker.Symbols;

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