namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ContinueStmt : CodeStmt
{
	public override bool Equals(object? obj)
	{
		return obj is ContinueStmt;
	}

	public override int GetHashCode()
	{
		return typeof(ContinueStmt).GetHashCode();
	}
}
