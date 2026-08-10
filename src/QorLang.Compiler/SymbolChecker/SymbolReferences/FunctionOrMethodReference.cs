using QorLang.Compiler.SymbolChecker.Symbols;

namespace QorLang.Compiler.SymbolChecker.SymbolReferences;

public class FunctionOrMethodReference(FunctionOrMethodSymbol funcSymbol, TypeReference[] typeArguments) : SymbolReference
{
	public readonly FunctionOrMethodSymbol FuncSymbol = funcSymbol;
	public readonly TypeReference[] TypeArguments = typeArguments;

	public string FullyQualifiedInstantiatedSignaturedName => (
		TypeArguments.Length > 0 ?
			$"{FuncSymbol.FullyQualifiedName}<{string.Join(", ", TypeArguments.Select(t => t.FullyQualifiedName))}>" :
			FuncSymbol.FullyQualifiedName
	) + $"({string.Join(", ", FuncSymbol.Signature.Select(t => t.DataType.FullyQualifiedName))})";
}