namespace QorLang.Compiler.SymbolChecker.SymbolReferences;

public class NamespaceReference(string name) : SymbolReference
{
	public readonly string Name = name;
}