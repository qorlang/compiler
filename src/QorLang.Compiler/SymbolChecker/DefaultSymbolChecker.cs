using QorLang.Compiler.Errors;
using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Parser.Nodes.Expressions;
using QorLang.Compiler.SymbolChecker.Symbols;

namespace QorLang.Compiler.SymbolChecker;

using SymbolCheckerResult = (
	GlobalSymbolTable GlobalSymbols,
	Dictionary<FunctionSymbol, ScopedSymbolTable> ScopedSymbolTables,
	Dictionary<ASTNode, Symbol> ResolvedNodeMap,
	Dictionary<Expr, TypeReference> ExprTypeMap,
	List<CompilationError> Errors
);

public class DefaultSymbolChecker(IEnumerable<ASTNode> nodes)
{
	readonly ASTNode[] _nodes = [..nodes];
	readonly GlobalSymbolTable _symbols = new();
	readonly Dictionary<FunctionSymbol, ScopedSymbolTable> _scopedSymbolTables = [];
	readonly Dictionary<ASTNode, Symbol> _resolvedNodeMap = [with(ReferenceEqualityComparer.Instance)];
	readonly Dictionary<Expr, TypeReference> _exprTypeMap = [with(ReferenceEqualityComparer.Instance)];
	readonly List<CompilationError> _errors = [];

	public SymbolCheckerResult Check()
	{
		return (_symbols, _scopedSymbolTables, _resolvedNodeMap, _exprTypeMap, _errors);
	}
}