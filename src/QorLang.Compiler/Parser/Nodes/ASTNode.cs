namespace QorLang.Compiler.Parser.Nodes;

public abstract class ASTNode
{
	public abstract override bool Equals(object? obj);
	public abstract override int GetHashCode();
}