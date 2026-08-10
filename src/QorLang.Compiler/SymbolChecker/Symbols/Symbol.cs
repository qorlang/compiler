namespace QorLang.Compiler.SymbolChecker.Symbols;

public class Symbol(SymbolType type, string name)
{
	public readonly SymbolType Type = type;
	public readonly string Name = name;
}