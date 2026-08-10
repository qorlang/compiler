using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.SymbolChecker.Symbols;

namespace QorLang.Compiler.SymbolChecker;

public class ScopedSymbolTable(
	SymbolTable parent,
	string currentFileContext,
	string currentNamespaceContext = "",
	string currentTypeContext = ""
) : SymbolTable
{
	public readonly SymbolTable Parent = parent;
	public readonly string CurrentNamespaceContext = currentNamespaceContext;
	public readonly string CurrentTypeContext = currentTypeContext;
	public readonly string CurrentFileContext = currentFileContext;
	readonly Dictionary<string, Symbol> _symbols = [];

	public override List<CompilationError> AddSymbol(Symbol symbol, TokenLocation declarationLocation)
	{
		return [];
	}

	public bool TryGetSymbol(string name, out Symbol symbol)
	{
		symbol = null!;
		return true;
	}
}