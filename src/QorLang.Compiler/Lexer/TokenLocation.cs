namespace QorLang.Compiler.Lexer;

public readonly struct TokenLocation(string file, int line, int column)
{
	public string File { get; } = file;
	public int Line { get; } = line;
	public int Column { get; } = column;

	public override string ToString() => $"{File}:{Line}:{Column}";
}