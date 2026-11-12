using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Symbols.SymbolTables;

public class ScopedSymbolTable(
	SymbolTable parent,
	string fileContext,
	string namespaceContext = "",
	TypeSymbol? typeContext = null
) : SymbolTable
{
	public readonly SymbolTable Parent = parent;
	public readonly string NamespaceContext = namespaceContext;
	public readonly TypeSymbol? TypeContext = typeContext;
	public readonly string FileContext = fileContext;
	readonly Dictionary<string, Symbol> _symbols = [];

	public override List<CompilationError> AddSymbol(Symbol symbol, TokenLocation declarationLocation)
	{
		return [];
	}

	public bool TryGetSymbol(string name, out Symbol[] matches)
	{
		matches = [];

		return true;
	}
}