using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class BreakStmt(TokenLocation location) : CodeStmt(location)
{
	public override bool Equals(object? obj)
	{
		return obj is BreakStmt;
	}

	public override int GetHashCode()
	{
		return typeof(BreakStmt).GetHashCode();
	}

	public override string ToString() => System.Text.Json.JsonSerializer.Serialize(new { type = nameof(BreakStmt) });
}
