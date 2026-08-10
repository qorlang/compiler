using QorLang.Compiler.SymbolChecker.Symbols;

namespace QorLang.Compiler.SymbolChecker.SymbolReferences;

public class VariableSymbolReference : SymbolReference
{
	public readonly Symbol Symbol;

	public VariableSymbolReference(Symbol symbol)
	{
		if (symbol is not BoundFieldSymbol and not StaticFieldSymbol and not ArgumentOrLocalSymbol and not GlobalSymbol)
		{
			throw new ArgumentException($"Symbol must be a variable symbol, but was {symbol.GetType().Name}");
		}

		Symbol = symbol;
	}
}