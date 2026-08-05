using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

public class TypeReference(TypeSymbol typeSymbol, TypeReference[] typeArguments, IndirectionLayer[] indirectionLayers)
{
	public readonly TypeSymbol TypeSymbol = typeSymbol;
	public readonly TypeReference[] TypeArguments = typeArguments;
	public readonly IndirectionLayer[] IndirectionLayers = indirectionLayers;

	private readonly string _indirectionLayersRepr = string.Join(string.Empty, indirectionLayers.Select(layer => layer switch { IndirectionLayer.ArrayOf => "[]", IndirectionLayer.PointerTo => "&", _ => throw new ArgumentException() }));
	public string InstantiatedName => (
		TypeArguments.Length == 0 ? $"{TypeSymbol.Name}{_indirectionLayersRepr}" :
		$"{TypeSymbol.Name}<{string.Join(", ", TypeArguments.Select(t => t.FullyQualifiedName))}>{_indirectionLayersRepr}"
	);

	public string FullyQualifiedName => string.IsNullOrEmpty(TypeSymbol.Namespace) ? InstantiatedName : $"{TypeSymbol.Namespace}.{InstantiatedName}";
}