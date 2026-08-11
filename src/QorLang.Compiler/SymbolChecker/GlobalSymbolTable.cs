using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.SymbolChecker.Symbols;

namespace QorLang.Compiler.SymbolChecker;

public class GlobalSymbolTable : SymbolTable
{
	readonly List<GlobalSymbolTable> importedModules = [];
	readonly Dictionary<string, Symbol> _symbols = [];
	readonly Dictionary<string, List<string>> _usedNamespaces = [];

	public List<CompilationError> AddImport(GlobalSymbolTable module, TokenLocation importLocation)
	{
		List<CompilationError> errors = [];

		// check for name conflicts
		foreach (var fqSymbolName in module._symbols.Keys)
		{
			if (_symbols.ContainsKey(fqSymbolName))
			{
				errors.Add(
					new CompilationError(
						$"Error QR301: Module import conflict: Symbol '{fqSymbolName}' already exists in the current global scope.",
						importLocation
					)
				);
			}
		}

		importedModules.Add(module);

		return errors;
	}

	public override List<CompilationError> AddSymbol(Symbol symbol, TokenLocation declarationLocation)
	{
		List<CompilationError> errors = [];

		var fullyQualifiedName = symbol switch
		{
			BoundFieldSymbol boundFieldSymbol => boundFieldSymbol.FullyQualifiedName,
			StaticFieldSymbol staticFieldSymbol => staticFieldSymbol.FullyQualifiedName,
			BoundMethodSymbol boundMethodSymbol => boundMethodSymbol.FullyQualifiedInstantiatedSignaturedName,
			StaticMethodSymbol staticMethodSymbol => staticMethodSymbol.FullyQualifiedInstantiatedSignaturedName,
			FunctionSymbol functionSymbol => functionSymbol.FullyQualifiedInstantiatedSignaturedName,
			GlobalSymbol globalSymbol => globalSymbol.FullyQualifiedName,
			TypeSymbol typeSymbol => typeSymbol.FullyQualifiedName,
			_ => throw new InvalidOperationException($"Cannot add symbol of type {symbol.GetType().Name} to GlobalSymbolTable")
		};

		// check for name conflicts
		if (_symbols.ContainsKey(fullyQualifiedName))
		{
			errors.Add(
				new CompilationError(
					$"Error QR301: Symbol '{fullyQualifiedName}' is defined twice in the global scope.",
					declarationLocation
				)
			);
		}
		else
		{
			_symbols[fullyQualifiedName] = symbol;
		}

		return errors;
	}

	// implements the 'using' directive
	public List<CompilationError> UseNamespace(string namespaceName, TokenLocation usageLocation)
	{
		List<CompilationError> errors = [];

		var file = usageLocation.File;

		if (!_usedNamespaces.ContainsKey(file))
		{
			_usedNamespaces[file] = [];
		}

		if (_usedNamespaces[file].Contains(namespaceName))
		{
			errors.Add(
				new CompilationError(
					$"Error QR304: The namespace '{namespaceName}' is imported multiple times in the same file.",
					usageLocation
				)
			);
		}
		else
		{
			_usedNamespaces[file].Add(namespaceName);
		}

		return errors;
	}

	public bool TryGetSymbol(string name, string currentFile, out Symbol[] matches)
	{
		return TryGetSymbol(name, currentFile, string.Empty, out matches);
	}

	public bool TryGetSymbol(string name, string currentFile, string currentNamespaceContext, out Symbol[] matches)
	{
		return TryGetSymbol(name, currentFile, currentNamespaceContext, string.Empty, out matches);
	}

	public bool TryGetSymbol(string name, string currentFile, string currentNamespaceContext, string currentTypeContext, out Symbol[] matches)
	{
		matches = [];
		return false;
	}
}