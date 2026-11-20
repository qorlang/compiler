using QorLang.Compiler.Symbols;

namespace QorLang.Compiler.NodeTagger;

public class MetadataReader(FileInfo module)
{
    public IEnumerable<Symbol> YieldSymbols()
    {
        yield return null!;
    }
}