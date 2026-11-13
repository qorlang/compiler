using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

/// <summary>
/// Base class for binary operator expressions (operations with two operands).
/// </summary>
public abstract class BinaryOpExpr(Expr left, Expr right, string operatorSpecialName, TokenLocation location) : Expr(location)
{
	public readonly Expr Left = left;
	public readonly Expr Right = right;
	public readonly string OperatorSpecialName = operatorSpecialName;
}
