namespace QorLang.Compiler.SymbolChecker;

public class ScopedSymbolTable(
	GlobalSymbolTable parent,
	string currentNamespaceContext = "",
	string currentTypeContext = ""
)
{
	public readonly GlobalSymbolTable Parent = parent;
	readonly string CurrentNamespaceContext = currentNamespaceContext;
	readonly string CurrentTypeContext = currentTypeContext;
	readonly Dictionary<string, Symbol> _symbols = [];

	public void AddSymbol(Symbol symbol)
	{
		
	}

	public bool TryGetSymbol(string name, out Symbol? symbol)
	{
		return Parent.TryGetSymbol(name, CurrentNamespaceContext, CurrentTypeContext, out symbol);
	}
}