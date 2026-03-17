using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public abstract class CodeStmt(TokenLocation location) : ASTNode(location);