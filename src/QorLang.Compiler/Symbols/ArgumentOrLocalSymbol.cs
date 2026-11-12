using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Symbols.References;
using QorLang.Compiler.Symbols.SymbolTables;

namespace QorLang.Compiler.Symbols;

public class ArgumentOrLocalSymbol(
	SymbolType type,
	string name,
	TypeReference dataType,
	DataProtection[] protections
) : Symbol(type, name)
{
	public readonly TypeReference DataType = dataType;
	public readonly DataProtection[] Protections = protections;
}