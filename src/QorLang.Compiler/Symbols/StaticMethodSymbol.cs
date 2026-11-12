using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Symbols.References;
using QorLang.Compiler.Symbols.SymbolTables;

namespace QorLang.Compiler.Symbols;

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