using QorLang.Compiler.SymbolChecker.SymbolReferences;
using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class StaticMethodSymbol(
	string name,
	TypeReference returnType,
	string[] typeArgumentNames,
	ArgumentOrLocalSymbol[] signature,
	AccessLevel accessLevel,
	TypeSymbol enclosingType,
	DataProtection protectionOnSelf
) : FunctionOrMethodSymbol(SymbolType.StaticMethod, name, returnType, typeArgumentNames, signature, accessLevel)
{
	public readonly TypeSymbol EnclosingType = enclosingType;
	public readonly DataProtection ProtectionOnSelf = protectionOnSelf;
	public override string FullyQualifiedName => $"{EnclosingType.FullyQualifiedName}.{Name}";
	public override string FullyQualifiedInstantiatedSignaturedName => $"{EnclosingType.FullyQualifiedName}.{InstantiatedSignaturedName}";
}