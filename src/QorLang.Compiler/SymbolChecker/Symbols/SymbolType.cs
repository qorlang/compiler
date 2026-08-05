namespace QorLang.Compiler.SymbolChecker.Symbols;

public enum SymbolType
{
	Argument,
	Local,
	Global,
	StaticField,
	BoundField,
	Function,
	StaticMethod,
	BoundMethod,
	Type
}