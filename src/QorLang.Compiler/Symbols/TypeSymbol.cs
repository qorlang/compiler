using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Symbols.SymbolTables;

namespace QorLang.Compiler.Symbols;

public class TypeSymbol(
	string name,
	string namespaceName,
	TypeParameterDeclarationNode[] typeArguments,
	AccessLevel accessLevel
) : Symbol(SymbolType.Type, name)
{
	public readonly string Namespace = namespaceName;
	public readonly TypeParameterDeclarationNode[] TypeArguments = typeArguments;
	public readonly string[] TypeArgumentNames = [..typeArguments.Select(node => node.Name)];
	public readonly AccessLevel AccessLevel = accessLevel;
	public readonly string InstantiatedName = typeArguments.Length == 0 ? name : $"{name}<{string.Join(", ", typeArguments)}>";
	public string FullyQualifiedName => string.IsNullOrEmpty(Namespace) ? InstantiatedName : $"{Namespace}.{InstantiatedName}";

	public static readonly TypeSymbol ErrorType = new("::error_type", string.Empty, [], AccessLevel.Public);
	public static readonly TypeSymbol VoidType = new("::void", string.Empty, [], AccessLevel.Public);
}