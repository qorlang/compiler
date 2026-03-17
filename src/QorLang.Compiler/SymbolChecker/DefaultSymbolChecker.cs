using QorLang.Compiler.Errors;
using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker;

using SymbolCheckerResult = (
	GlobalSymbolTable GlobalSymbols,
	Dictionary<ASTNode, ScopedSymbolTable> ScopedSymbolTables,
	List<CompilationError> Errors
);

public class DefaultSymbolChecker(IEnumerable<ASTNode> nodes)
{
	readonly ASTNode[] _nodes = [..nodes];
	readonly GlobalSymbolTable _symbols = new();
	readonly Dictionary<ASTNode, ScopedSymbolTable> _scopedSymbolTables = [];
	readonly List<CompilationError> _errors = [];

	public SymbolCheckerResult Check()
	{
		return (_symbols, _scopedSymbolTables, _errors);
	}
}