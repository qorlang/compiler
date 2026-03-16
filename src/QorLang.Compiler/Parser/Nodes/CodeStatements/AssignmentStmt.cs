using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class AssignmentStmt(
	string name,
	Expr value
) : CodeStmt
{
	public readonly string Name = name;
	public readonly Expr Value = value;

	public override bool Equals(object? obj)
	{
		if (obj is not AssignmentStmt other) return false;
		return Name == other.Name && Value.Equals(other.Value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Value);
	}
}