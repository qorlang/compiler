using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Symbols.SymbolTables;

namespace QorLang.Compiler.NodeTagger;

public class MetadataImporter
{
    public (GlobalSymbolTable SymbolTable, List<CompilationError> Errors) ImportModule(string modulePath, TokenLocation importLocation)
    {
        List<CompilationError> errors = [];

        var fileInfo = new FileInfo(modulePath);

        if (!fileInfo.Exists)
        {
            errors.Add(
                new CompilationError(
                    $"Error QR317: Module {modulePath} not found.",
                    importLocation
                )
            );

            return (null!, errors);
        }

        var reader = new MetadataReader(fileInfo);
        GlobalSymbolTable symbolTable = new();

        foreach (var symbol in reader.YieldSymbols())
        {
            // technically a compiled module should never result in symbol errors
            // but since this API exists we might as well accumulate the errors
            // if they somehow exist
            errors.AddRange(symbolTable.AddSymbol(symbol, new TokenLocation($"<import: {modulePath}>", 0, 0)));
        }

        return (symbolTable, errors);
    }
}