namespace QorLang.Compiler.Lexer;

public readonly struct TokenLocation(string file, int line, int column)
{
	public readonly string File { init; get; } = file;
	public readonly int Line { init; get; } = line;
	public readonly int Column { init; get; } = column;

	public override string ToString() => $"{File}:{Line}:{Column}";
}