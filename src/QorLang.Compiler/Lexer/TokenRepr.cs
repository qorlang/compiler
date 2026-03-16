using System.Text.Json;

namespace QorLang.Compiler.Lexer;

public static class TokenRepr
{
	public static string ToString(Token token)
	{
		return token.Type switch
		{
			TokenType.Identifier => $"identifier '{token.Value}'",
			TokenType.Keyword => $"keyword '{token.Value}'",
			TokenType.IntegerLiteral => $"integer literal {token.Value}",
			TokenType.FloatLiteral => $"float literal {token.Value}",
			TokenType.StringLiteral => $"string literal {JsonSerializer.Serialize(token.Value)}",
			TokenType.CharLiteral => $"character literal {JsonSerializer.Serialize(token.Value)}",
			_ => ToString(token.Type)
		};
	}

	public static string ToString(TokenType tokenType)
	{
		return tokenType switch
		{
			TokenType.Identifier => "identifier",
			TokenType.Keyword => "keyword",
			TokenType.IntegerLiteral => "integer literal",
			TokenType.FloatLiteral => "float literal",
			TokenType.StringLiteral => "string literal",
			TokenType.CharLiteral => "character literal",
			TokenType.Add => "'+'",
			TokenType.Sub => "'-'",
			TokenType.Asterisk => "'*'",
			TokenType.Div => "'/'",
			TokenType.Assign => "'='",
			TokenType.Eq => "'=='",
			TokenType.NotEq => "'!='",
			TokenType.LessThan => "'<'",
			TokenType.GreaterThan => "'>'",
			TokenType.LessThanEqual => "'<='",
			TokenType.GreaterThanEqual => "'>='",
			TokenType.Mod => "'%'",
			TokenType.Power => "'**'",
			TokenType.And => "'&&'",
			TokenType.Or => "'||'",
			TokenType.Not => "'!'",
			TokenType.BitwiseOr => "'|'",
			TokenType.BitwiseXor => "'^'",
			TokenType.BitwiseNot => "'~'",
			TokenType.AddAssign => "'+='",
			TokenType.SubAssign => "'-='",
			TokenType.MulAssign => "'*='",
			TokenType.DivAssign => "'/='",
			TokenType.ModAssign => "'%='",
			TokenType.Increment => "'++'",
			TokenType.Decrement => "'--'",
			TokenType.LeftShift => "'<<'",
			TokenType.RightShift => "'>>'",
			TokenType.LeftParen => "'('",
			TokenType.RightParen => "')'",
			TokenType.LeftBrace => "'{'",
			TokenType.RightBrace => "'}'",
			TokenType.LeftBracket => "'['",
			TokenType.RightBracket => "']'",
			TokenType.Comma => "','",
			TokenType.Semicolon => "';'",
			TokenType.Ampersand => "'&'",
			TokenType.At => "'@'",
			TokenType.Dollar => "'$'",
			TokenType.Colon => "':'",
			TokenType.Dot => "'.'",
			TokenType.LineComment => "line comment",
			TokenType.BlockComment => "block comment",
			TokenType.ErrorToken => "<error token>",
			TokenType.EOF => "end of file",
			_ => throw new ArgumentOutOfRangeException(nameof(tokenType), $"Unknown token type: {tokenType}")
		};
	}
}