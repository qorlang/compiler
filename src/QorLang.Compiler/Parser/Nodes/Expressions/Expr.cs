using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.Expressions;

public abstract class Expr(TokenLocation location) : ASTNode(location);