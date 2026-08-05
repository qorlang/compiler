namespace QorLang.Compiler.SymbolChecker.Symbols;

public class FunctionSymbol(SymbolType type, TypeReference returnType, string[] typeArgumentNames, string name, ArgumentOrLocalSymbol[] signature) : Symbol(type)
{
	public readonly TypeReference ReturnType = returnType;
	public readonly string[] TypeArgumentNames = typeArgumentNames;
	public readonly string Name = name;
	public readonly ArgumentOrLocalSymbol[] Signature = signature;
	public readonly string SignaturedName = $"{name}({string.Join(", ", signature.Select(s => s.DataType.FullyQualifiedName))})";
	public string InstantiatedName => TypeArgumentNames.Length > 0 ? $"{SignaturedName}<{string.Join(", ", TypeArgumentNames)}>" : SignaturedName;
}