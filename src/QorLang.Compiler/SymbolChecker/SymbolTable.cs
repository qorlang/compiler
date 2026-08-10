using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.SymbolChecker.Symbols;

namespace QorLang.Compiler.SymbolChecker;

public abstract class SymbolTable
{
	public abstract List<CompilationError> AddSymbol(Symbol symbol, TokenLocation declarationLocation);
}