using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes;

public class TypeParameterDeclarationNode(string name, TokenLocation location) : ASTNode(location)
{
    public readonly string Name = name;
    
    public override bool Equals(object? obj)
    {
        return (obj is TypeParameterDeclarationNode node) && Name.Equals(node.Name);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(new { type = nameof(FunctionDefinitionNode), name = Name });
    }
}