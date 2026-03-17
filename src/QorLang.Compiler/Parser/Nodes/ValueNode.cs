using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes;

public abstract class ValueNode(TokenLocation location) : ASTNode(location);