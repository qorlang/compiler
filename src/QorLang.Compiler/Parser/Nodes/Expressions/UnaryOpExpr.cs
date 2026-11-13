using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

/// <summary>
/// Base class for unary operator expressions (operations with a single operand).
/// </summary>
public abstract class UnaryOpExpr(Expr target, string operatorSpecialName, TokenLocation location) : Expr(location)
{
	public readonly Expr Target = target;
	public readonly string OperatorSpecialName = operatorSpecialName;
}
