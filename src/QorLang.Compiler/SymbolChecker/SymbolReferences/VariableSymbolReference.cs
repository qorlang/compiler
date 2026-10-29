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

	public TypeReference DataType => Symbol switch
	{
		BoundFieldSymbol boundFieldSymbol => boundFieldSymbol.DataType,
		StaticFieldSymbol staticFieldSymbol => staticFieldSymbol.DataType,
		ArgumentOrLocalSymbol argumentOrLocalSymbol => argumentOrLocalSymbol.DataType,
		GlobalSymbol globalSymbol => globalSymbol.DataType,
		_ => throw new InvalidOperationException($"Unexpected symbol type: {Symbol.GetType().Name}")
	};
}