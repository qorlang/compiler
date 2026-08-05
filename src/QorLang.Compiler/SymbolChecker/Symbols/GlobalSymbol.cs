namespace QorLang.Compiler.SymbolChecker.Symbols;

public class GlobalSymbol(string namespaceName, string name, TypeReference dataType) : Symbol(SymbolType.Global)
{
	public readonly string Namespace = namespaceName;
	public readonly string Name = name;
	public readonly TypeReference DataType = dataType;
	public readonly string FullyQualifiedName = string.IsNullOrEmpty(namespaceName) ? name : $"{namespaceName}.{name}";
}