namespace QorLang.Compiler.Parser.Nodes.Expressions;

public abstract class CallExpr(
	Expr target,
	Expr[] arguments
) : ASTNode
{
	public readonly Expr Target = target;
	public readonly Expr[] Arguments = arguments;

	public override bool Equals(object? obj)
	{
		if (obj is not CallExpr other || obj.GetType() != GetType()) return false;
		return Target.Equals(other.Target) && Arguments.SequenceEqual(other.Arguments);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target, Arguments);
	}
}