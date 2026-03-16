namespace QorLang.Compiler.Lexer;

public class DefaultLexer(string fileName, string source)
{
	public static readonly HashSet<string> Keywords =
	[
		"if", "else", "while", "for", "return",
		"let", "rw", "ro", "type", "pub", "break",
		"continue", "import", "using", "true", "false",
		"i8", "i16", "i32", "i64", "nint", "u8", "u16",
		"u32", "u64", "nuint", "f32", "f64", "string",
		"char", "bool", "void", "namespace", "nameof",
		"sizeof", "global", "this", "field"
	];

	readonly string _fileName = fileName;
	readonly string _source = source.Replace("\r\n", "\n").Replace("\r", "\n");
	int _currRow;
	int _currCol;
	string _currLine = string.Empty;

	Token GenerateErrorToken(string offendingToken)
	{
		return new Token(
			TokenType.ErrorToken,
			offendingToken,
			new TokenLocation(_fileName, _currRow+1, _currCol+1)
		);
	}

	/// <summary>
	/// Make an identifier or keyword given that the current character is a letter, underscore, or at symbol.
	/// </summary>
	/// <returns></returns>
	Token MakeIdentifierOrKeyword()
	{
		var startCol = _currCol;

		if (_currLine[_currCol] == '@')
		{
			_currCol++; // Allow the first character only to be an @
		}

		while (_currCol < _currLine.Length && (char.IsLetterOrDigit(_currLine[_currCol]) || _currLine[_currCol] == '_'))
		{
			_currCol++;
		}

		var value = _currLine[startCol.._currCol];

		var type = Keywords.Contains(value) ? TokenType.Keyword : TokenType.Identifier;

		return new Token(type, value, new TokenLocation(_fileName, _currRow + 1, startCol + 1));
	}

	/// <summary>
	/// Make an integer or float literal given that the current character is a digit.
	/// </summary>
	/// <returns></returns>
	Token MakeIntegerOrFloatLiteral()
	{
		var startCol = _currCol;

		while (_currCol < _currLine.Length && char.IsDigit(_currLine[_currCol]))
		{
			_currCol++;
		}

		if (_currCol < _currLine.Length && _currLine[_currCol] == '.')
		{
			_currCol++; // Skip the dot

			while (_currCol < _currLine.Length && char.IsDigit(_currLine[_currCol]))
			{
				_currCol++;
			}

			var value = _currLine[startCol.._currCol];
			return new Token(TokenType.FloatLiteral, value, new TokenLocation(_fileName, _currRow + 1, startCol + 1));
		}
		else
		{
			var value = _currLine[startCol.._currCol];
			return new Token(TokenType.IntegerLiteral, value, new TokenLocation(_fileName, _currRow + 1, startCol + 1));
		}
	}

	/// <summary>
	/// Parse a character from the current position in the source buffer, supporting \x0000 and \u0000 escape sequences.
	/// Advances _currCol past the parsed sequence.
	/// </summary>
	/// <returns></returns>
	(char Character, Token? Error) ParseCharacter()
	{
		if (_currLine[_currCol] != '\\')
		{
			return (_currLine[_currCol++], null);
		}

		if (_currCol + 1 >= _currLine.Length)
		{
			var error = GenerateErrorToken("Error QR107: Incomplete escape sequence");
			return ('\0', error);
		}

		char escapeChar = _currLine[_currCol + 1];

		if (escapeChar == 'x')
		{
			// \xHH format (2-4 hex digits)

			int hexStart = _currCol + 2;
			int hexEnd = hexStart;
			while (hexEnd < _currLine.Length && hexEnd < hexStart + 4 && "0123456789ABCDEFabcdef".Contains(_currLine[hexEnd]))
			{
				hexEnd++;
			}

			if (hexEnd == hexStart)
			{
				var error = GenerateErrorToken("Error QR107: \\x requires at least 2 hex digits");
				return ('\0', error);
			}

			string hexDigits = _currLine[hexStart..hexEnd];
			char result = (char)Convert.ToInt32(hexDigits, 16);
			_currCol = hexEnd;
			return (result, null);
		}
		else if (escapeChar == 'u')
		{
			// \uHHHH format (exactly 4 hex digits)

			if (_currCol + 6 > _currLine.Length)
			{
				var error = GenerateErrorToken("Error QR107: Incomplete \\u escape sequence (requires 4 hex digits)");
				return ('\0', error);
			}

			string hexDigits = _currLine[(_currCol + 2)..(_currCol + 6)];
			if (!hexDigits.All(c => "0123456789ABCDEFabcdef".Contains(c)))
			{
				var error = GenerateErrorToken($"Error QR106: Invalid hex digits in \\u escape sequence: {hexDigits}");
				return ('\0', error);
			}

			char result = (char)Convert.ToInt32(hexDigits, 16);
			_currCol += 6;
			return (result, null);
		}
		else
		{
			char result = escapeChar switch
			{
				'n' => '\n',
				'r' => '\r',
				't' => '\t',
				'\\' => '\\',
				'\'' => '\'',
				'"' => '"',
				'0' => '\0',
				'b' => '\b',
				'f' => '\f',
				'v' => '\v',
				_ => '!' // error case, unknown sequence
			};

			if (result != '!')
			{
				_currCol += 2;
				return (result, null);
			}
			
			var error = GenerateErrorToken($"Error QR105: Unknown escape sequence: \\{escapeChar}");
			return ('\0', error);
		}
	}

	/// <summary>
	/// Make a character literal given that the current character is a single quote.
	/// </summary>
	/// <returns></returns>
	Token MakeCharacterLiteral()
	{
		var startCol = _currCol;

		_currCol++; // Skip the opening quote

		if (_currCol >= _currLine.Length)
		{
			return GenerateErrorToken("Error QR103: Unterminated character literal");
		}

		var (character, error) = ParseCharacter();

		if (error is not null)
		{
			return error.Value;
		}

		if (_currCol >= _currLine.Length || _currLine[_currCol] != '\'')
		{
			return GenerateErrorToken("Error QR104: Character literals must end with a single quote");
		}

		_currCol++; // Skip the closing quote

		return new Token(TokenType.CharLiteral, character.ToString(), new TokenLocation(_fileName, _currRow + 1, startCol + 1));
	}

	/// <summary>
	/// Makes a string literal given that the current character is a double quote.
	/// </summary>
	/// <returns></returns>
	Token MakeStringLiteral()
	{
		var startCol = _currCol;

		_currCol++; // Skip the opening quote

		var value = string.Empty;

		while (_currCol < _currLine.Length)
		{
			if (_currLine[_currCol] == '"')
			{
				_currCol++; // Skip the closing quote
				return new Token(TokenType.StringLiteral, value, new TokenLocation(_fileName, _currRow + 1, startCol + 1));
			}
			else
			{
				var (character, error) = ParseCharacter();

				if (error is not null)
				{
					return error.Value;
				}

				value += character;
			}
		}

		return GenerateErrorToken("Error QR102: Unterminated string literal");
	}

	public IEnumerable<Token> Tokenize(bool includeComments = false)
	{
		var lines = _source.Split('\n', StringSplitOptions.None);

		bool inBlockComment = false;
		string blockCommentContent = string.Empty;

		for (_currRow = 0; _currRow < lines.Length; _currRow++)
		{
			_currLine = lines[_currRow];

			for (_currCol = 0; _currCol < _currLine.Length;)
			{
				var c = _currLine[_currCol];

				if (inBlockComment)
				{
					if (c == '*' && _currCol + 1 < _currLine.Length && _currLine[_currCol + 1] == '/')
					{
						inBlockComment = false;
						_currCol += 2;

						if (includeComments) yield return new Token(TokenType.BlockComment, blockCommentContent+"*/", new TokenLocation(_fileName, _currRow + 1, 1));

						continue;
					}
					else
					{
						_currCol++;
						blockCommentContent += c;
					}

					continue;
				}

				if (char.IsWhiteSpace(c))
				{
					_currCol++;
					continue;
				}
				else if (char.IsLetter(c) || c == '_' || (c == '@' && _currCol + 1 < _currLine.Length && (char.IsLetter(_currLine[_currCol + 1]) || _currLine[_currCol + 1] == '_')))
				{
					yield return MakeIdentifierOrKeyword();

					continue;
				}
				else if (char.IsDigit(c))
				{
					yield return MakeIntegerOrFloatLiteral();

					continue;
				}
				else if (c == '/' && _currCol + 1 < _currLine.Length && _currLine[_currCol + 1] == '/')
				{
					if (includeComments) yield return new Token(TokenType.LineComment, _currLine[_currCol..], new TokenLocation(_fileName, _currRow + 1, _currCol + 1));

					break; // Skip the rest of the line
				}
				else if (c == '/' && _currCol + 1 < _currLine.Length && _currLine[_currCol + 1] == '*')
				{
					_currCol += 2; // Skip the '/*'

					inBlockComment = true;
					blockCommentContent = "/*";
				}
				else if (c == '\'')
				{
					yield return MakeCharacterLiteral();

					continue;
				}
				else if (c == '"')
				{
					yield return MakeStringLiteral();

					continue;
				}
				else
				{
					// Handle operators and punctuation
					var startCol = _currCol;
					_currCol++;

					// Check for multi-character operators
					string opOrPunctuation = c.ToString();
					if (_currCol < _currLine.Length)
					{
						string twoCharOp = opOrPunctuation + _currLine[_currCol];
						if ((new[] { "==", "!=", "<=", ">=", "||", "++", "--", "+=", "-=", "*=", "/=", "%=", "<<" }).Contains(twoCharOp))
						{
							opOrPunctuation = twoCharOp;
							_currCol++;
						}
					}

					var tokenType = opOrPunctuation switch
					{
						"+" => TokenType.Add,
						"-" => TokenType.Sub,
						"*" => TokenType.Asterisk,
						"/" => TokenType.Div,
						"=" => TokenType.Assign,
						"==" => TokenType.Eq,
						"!=" => TokenType.NotEq,
						"<" => TokenType.LessThan,
						">" => TokenType.GreaterThan,
						"<=" => TokenType.LessThanEqual,
						">=" => TokenType.GreaterThanEqual,
						"%" => TokenType.Mod,
						"||" => TokenType.Or,
						"!" => TokenType.Not,
						"^" => TokenType.BitwiseXor,
						"&" => TokenType.Ampersand,
						"|" => TokenType.BitwiseOr,
						"~" => TokenType.BitwiseNot,
						"+=" => TokenType.AddAssign,
						"-=" => TokenType.SubAssign,
						"*=" => TokenType.MulAssign,
						"/=" => TokenType.DivAssign,
						"%=" => TokenType.ModAssign,
						"++" => TokenType.Increment,
						"--" => TokenType.Decrement,
						"<<" => TokenType.LeftShift,
						"(" => TokenType.LeftParen,
						")" => TokenType.RightParen,
						"{" => TokenType.LeftBrace,
						"}" => TokenType.RightBrace,
						"[" => TokenType.LeftBracket,
						"]" => TokenType.RightBracket,
						"," => TokenType.Comma,
						";" => TokenType.Semicolon,
						"@" => TokenType.At,
						"$" => TokenType.Dollar,
						":" => TokenType.Colon,
						"." => TokenType.Dot,
						_ => TokenType.ErrorToken
					};

					if (tokenType != TokenType.ErrorToken)
					{
						yield return new Token(tokenType, opOrPunctuation, new TokenLocation(_fileName, _currRow + 1, startCol + 1));
					}
					else
					{
						yield return GenerateErrorToken($"Error QR101: Unrecognized character: '{opOrPunctuation}'");
					}
				};
			}
		}

		yield return new Token(TokenType.EOF, string.Empty, new TokenLocation(_fileName, lines.Length + 1, 1));
	}
}