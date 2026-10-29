namespace QorLang.Compiler.SymbolChecker.SymbolReferences;

public class NamespaceReference(string enclosingNamespace,string name) : SymbolReference
{
	public readonly string EnclosingNamespace = enclosingNamespace;
	public readonly string Name = name;
	public readonly string FullyQualifiedName = $"{enclosingNamespace}.{name}";
}