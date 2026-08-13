using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.SymbolChecker.SymbolReferences;
using QorLang.Compiler.SymbolChecker.Symbols;

namespace QorLang.Compiler.SymbolChecker;

// TODO: need to somehow support method/function types
public class GlobalSymbolTable : SymbolTable
{
	readonly List<GlobalSymbolTable> importedModules = [];
	// there needs to be a better way to organize this to lookup by type context, namespace context, etc.
	// readonly Dictionary<string, Symbol> _symbols = [];
	readonly Dictionary<string, List<string>> _usedNamespaces = [];

	public List<CompilationError> AddImport(GlobalSymbolTable module, TokenLocation importLocation)
	{
		List<CompilationError> errors = [];

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

		return [];
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
		matches = [];
		return false;
	}

	public bool TryGetSymbol(string name, string currentFile, TypeSymbol? currentTypeContext, out Symbol[] matches)
	{
		matches = [];
		return false;
	}

	public bool TryGetMember(
		TypeReference fromType,
		string name,
		bool instance,
		out Symbol[] matches
	)
	{
		matches = [];
		return false;
	}
}