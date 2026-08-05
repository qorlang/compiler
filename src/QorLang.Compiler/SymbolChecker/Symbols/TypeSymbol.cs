using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class TypeSymbol(string namespaceName, string name, string[] typeArgumentNames, AccessLevel accessLevel) : Symbol(SymbolType.Type)
{
	public readonly string Namespace = namespaceName;
	public readonly string Name = name;
	public readonly string[] TypeArgumentNames = typeArgumentNames;
	public readonly AccessLevel AccessLevel = accessLevel;
	public readonly string InstantiatedName = typeArgumentNames.Length == 0 ? name : $"{name}<{string.Join(", ", typeArgumentNames)}>";
	public string FullyQualifiedName => string.IsNullOrEmpty(Namespace) ? InstantiatedName : $"{Namespace}.{InstantiatedName}";
}