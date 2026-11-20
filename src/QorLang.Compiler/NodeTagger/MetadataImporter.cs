using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Symbols.SymbolTables;

namespace QorLang.Compiler.NodeTagger;

public class MetadataImporter
{
    public (GlobalSymbolTable SymbolTable, List<CompilationError> Errors) ImportModule(string modulePath, TokenLocation importLocation)
    {
        List<CompilationError> errors = [];

        errors.Add(
            new CompilationError(
                $"Error QR317: Module {modulePath} not found.",
                importLocation
            )
        );

        return (null!, errors);
    }
}