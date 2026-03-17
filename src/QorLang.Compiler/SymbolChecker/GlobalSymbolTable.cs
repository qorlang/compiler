namespace QorLang.Compiler.SymbolChecker;

public class GlobalSymbolTable
{
	readonly GlobalSymbolTable[] importedModules = [];
	readonly Dictionary<string, Symbol> _symbols = [];

	public void AddImport(GlobalSymbolTable module)
	{
		
	}

	public void AddSymbol(Symbol symbol)
	{
		
	}

	public bool TryGetSymbol(string name, out Symbol? symbol)
	{
		return TryGetSymbol(name, string.Empty, out symbol);
	}

	public bool TryGetSymbol(string name, string currentNamespaceContext, out Symbol? symbol)
	{
		return TryGetSymbol(name, currentNamespaceContext, string.Empty, out symbol);
	}

	public bool TryGetSymbol(string name, string currentNamespaceContext, string currentTypeContext, out Symbol? symbol)
	{
		symbol = null;
		return false;
	}
}