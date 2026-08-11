using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.SymbolChecker.Symbols;

// This is actually a reference to a type argument, so it cannot have type arguments itself.
// Its real type will be resolved by the JIT compiler when generics are instantiated.
public class TypeArgumentTypeSymbol(string name) : TypeSymbol(name, "", [], AccessLevel.Private);