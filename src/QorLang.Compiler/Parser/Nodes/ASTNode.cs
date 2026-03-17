using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes;

public abstract class ASTNode(TokenLocation location)
{
	public readonly TokenLocation Location = location;

	public abstract override bool Equals(object? obj);
	public abstract override int GetHashCode();
	
	public abstract override string ToString();
}