namespace QorLang.Compiler.Lexer;

public readonly struct Token(TokenType type, string value, TokenLocation position)
{
	public readonly TokenType Type { init; get; } = type;
	public readonly string Value { init; get; } = value;
	public readonly TokenLocation Location { init; get; } = position;
}

public enum TokenType
{
	Identifier,
	Keyword,
	IntegerLiteral,
	FloatLiteral,
	StringLiteral,
	CharLiteral,
	Add,
	Sub,
	Asterisk,
	Div,
	Assign,
	Eq,
	NotEq,
	LessThan,
	GreaterThan,
	LessThanEqual,
	GreaterThanEqual,
	Mod,
	Power,
	And,
	Or,
	Not,
	BitwiseOr,
	BitwiseXor,
	BitwiseNot,
	AddAssign,
	SubAssign,
	MulAssign,
	DivAssign,
	ModAssign,
	Increment,
	Decrement,
	LeftShift,
	RightShift,
	LeftParen,
	RightParen,
	LeftBrace,
	RightBrace,
	LeftBracket,
	RightBracket,
	Comma,
	Semicolon,
	Ampersand,
	At,
	Dollar,
	Colon,
	Dot,
	LineComment,
	BlockComment,
	ErrorToken,
	EOF
}