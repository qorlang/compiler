namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ErrorStmt : CodeStmt
{
	public override bool Equals(object? obj)
	{
		return obj is ErrorStmt;
	}

	public override int GetHashCode()
	{
		return typeof(ErrorStmt).GetHashCode();
	}
}