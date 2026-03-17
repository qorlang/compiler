using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes;

public abstract class TypeMemberDeclarationNode(TokenLocation location) : ASTNode(location);