using QorLang.Compiler.Lexer;

namespace QorLang.Compiler.Errors;

public readonly struct CompilationError(string message, TokenLocation location)
{
	public string Message { get; } = message;
	public TokenLocation Location { get; } = location;

	public override string ToString()
	{
		return $"At {Location}: {Message}";
	}
}