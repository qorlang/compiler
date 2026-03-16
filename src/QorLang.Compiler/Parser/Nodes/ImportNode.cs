namespace QorLang.Compiler.Parser.Nodes;

public class ImportNode(string modulePath) : ASTNode
{
	public readonly string ModulePath = modulePath;

	public override bool Equals(object? obj)
	{
		return obj is ImportNode other && ModulePath == other.ModulePath;
	}

	public override int GetHashCode()
	{
		return ModulePath.GetHashCode();
	}
}