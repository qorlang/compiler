using QorLang.Compiler.SymbolChecker.SymbolReferences;
using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class FunctionSymbol(
	string name,
	TypeReference returnType,
	string[] typeArgumentNames,
	ArgumentOrLocalSymbol[] signature,
	AccessLevel accessLevel,
	string namespaceName
) : FunctionOrMethodSymbol(SymbolType.Function, name, returnType, typeArgumentNames, signature, accessLevel)
{
	public readonly string Namespace = namespaceName;
	public override string FullyQualifiedName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";
	public override string FullyQualifiedInstantiatedSignaturedName => string.IsNullOrEmpty(Namespace) ? InstantiatedSignaturedName : $"{Namespace}.{InstantiatedSignaturedName}";
}