using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Symbols;

public abstract class SymbolTable
{
	public abstract List<CompilationError> AddSymbol(Symbol symbol, TokenLocation declarationLocation);
}