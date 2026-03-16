namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class BreakStmt : CodeStmt
{
	public override bool Equals(object? obj)
	{
		return obj is BreakStmt;
	}

	public override int GetHashCode()
	{
		return typeof(BreakStmt).GetHashCode();
	}
}
