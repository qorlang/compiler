using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Parser.Nodes.CodeStatements;

public class ErrorStmt() : CodeStmt(default)
{
	public override bool Equals(object? obj)
	{
		return obj is ErrorStmt;
	}

	public override int GetHashCode()
	{
		return typeof(ErrorStmt).GetHashCode();
	}

	public override string ToString() => System.Text.Json.JsonSerializer.Serialize(new { type = nameof(ErrorStmt) });
}