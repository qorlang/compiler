using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Errors;

public readonly struct CompilationError(string message, TokenLocation location)
{
	public readonly string Message { init; get; } = message;
	public readonly TokenLocation Location { init; get; } = location;

	public override string ToString()
	{
		return $"At {Location}: {Message}";
	}
}