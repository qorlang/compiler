using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ContinueStmt(TokenLocation location) : CodeStmt(location)
{
	public override bool Equals(object? obj)
	{
		return obj is ContinueStmt;
	}

	public override int GetHashCode()
	{
		return typeof(ContinueStmt).GetHashCode();
	}

	public override string ToString() => System.Text.Json.JsonSerializer.Serialize(new { type = nameof(ContinueStmt) });
}
