using System.Text.Json;

namespace QorLang.Compiler.Parser.Nodes;

public class ErrorNode : ASTNode
{
	public override bool Equals(object? obj)
	{
		return obj is ErrorNode;
	}

	public override int GetHashCode()
	{
		return typeof(ErrorNode).GetHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(ErrorNode) });
}