using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class TypeSymbol(
	string name,
	string namespaceName,
	string[] typeArgumentNames,
	AccessLevel accessLevel
) : Symbol(SymbolType.Type, name)
{
	public readonly string Namespace = namespaceName;
	public readonly string[] TypeArgumentNames = typeArgumentNames;
	public readonly AccessLevel AccessLevel = accessLevel;
	public readonly string InstantiatedName = typeArgumentNames.Length == 0 ? name : $"{name}<{string.Join(", ", typeArgumentNames)}>";
	public string FullyQualifiedName => string.IsNullOrEmpty(Namespace) ? InstantiatedName : $"{Namespace}.{InstantiatedName}";

	public static readonly TypeSymbol ErrorType = new("::error_type", string.Empty, [], AccessLevel.Public);
	public static readonly TypeSymbol VoidType = new("::void", string.Empty, [], AccessLevel.Public);
}