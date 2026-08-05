using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class ArgumentOrLocalSymbol(SymbolType type, TypeReference dataType, string name, FunctionSymbol method, DataProtection[] protections) : Symbol(type)
{
	public readonly TypeReference DataType = dataType;
	public readonly string Name = name;
	public readonly FunctionSymbol MethodScope = method;
	public readonly DataProtection[] Protections = protections;
}