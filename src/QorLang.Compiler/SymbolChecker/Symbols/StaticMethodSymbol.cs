using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class StaticMethodSymbol(
	TypeReference returnType,
	string[] typeArgumentNames,
	string name,
	ArgumentOrLocalSymbol[] signature,
	TypeSymbol enclosingType,
	DataProtection protectionOnSelf
) : FunctionSymbol(SymbolType.StaticMethod, returnType, typeArgumentNames, name, signature)
{
	public readonly TypeSymbol EnclosingType = enclosingType;
	public readonly DataProtection ProtectionOnSelf = protectionOnSelf;
	public string FullyQualifiedName => $"{EnclosingType.FullyQualifiedName}.{InstantiatedName}";
}