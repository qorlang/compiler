using System.Text.Json;
using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes;

public class ImportNode(string modulePath, TokenLocation location) : ASTNode(location)
{
	public readonly string ModulePath = modulePath;

	public override bool Equals(object? obj)
	{
		return obj is ImportNode other && ModulePath == other.ModulePath;
	}

	public override int GetHashCode()
	{
		return ModulePath.GetHashCode();
	}

	public override string ToString() => JsonSerializer.Serialize(new { type = nameof(ImportNode), modulePath = ModulePath });
}