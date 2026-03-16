namespace QorLang.Compiler.Lexer;

public readonly struct Token(TokenType type, string value, TokenLocation position)
{
	public readonly TokenType Type { init; get; } = type;
	public readonly string Value { init; get; } = value;
	public readonly TokenLocation Location { init; get; } = position;

	public override string ToString()
	{
		return $"{Type}: {TokenRepr.ToString(this)} ({Location})";
	}
}

public enum TokenType
{
	ErrorToken, // ErrorToken is first so that default(Token)/default(TokenType) always has a type of ErrorToken
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
	// Logical "And" does not exist here because it would prevent && from being tokenized as two separate address/reference tokens.
	// The parser can still support && as the "And" operator by interpreting two consecutive '&' tokens (whose locations are exactly one character apart)
	// as a single '&&' operator.
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
	// The Right Shift operator is not implemented as a standalone token for reasons similar to that of the logical && operator.
	// In this case, a right shift operator might conflict with nested generics.
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
	EOF
}