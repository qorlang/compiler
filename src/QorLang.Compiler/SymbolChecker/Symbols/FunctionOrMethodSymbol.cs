using QorLang.Compiler.SymbolChecker.SymbolReferences;
using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public abstract class FunctionOrMethodSymbol(
	SymbolType type,
	string name,
	TypeReference returnType,
	string[] typeArgumentNames,
	ArgumentOrLocalSymbol[] signature,
	AccessLevel accessLevel
) : Symbol(type, name)
{
	public readonly TypeReference ReturnType = returnType;
	public readonly AccessLevel	AccessLevel = accessLevel;
	public readonly string[] TypeArgumentNames = typeArgumentNames;
	public readonly ArgumentOrLocalSymbol[] Signature = signature;
	public readonly string SignaturedName = $"{name}({string.Join(", ", signature.Select(s => s.DataType.FullyQualifiedName))})";
	public string InstantiatedSignaturedName => TypeArgumentNames.Length > 0 ? $"{SignaturedName}<{string.Join(", ", TypeArgumentNames)}>" : SignaturedName;
	public abstract string FullyQualifiedName { get; }
	public abstract string FullyQualifiedInstantiatedSignaturedName { get; }
}