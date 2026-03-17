using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Parser.Nodes.CodeStatements;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Parser;

using ParserResult = (List<ASTNode> Nodes, List<CompilationError> Errors);

public class DefaultParser(IEnumerable<Token> tokens)
{
	IEnumerator<Token> _tokenEnumerator = tokens.GetEnumerator();
	List<CompilationError> _errors = [];
	bool _currentDeclIsPublic = false;

	static CompilationError GenerateError(string message, Token offendingToken)
	{
		return new CompilationError(message, offendingToken.Location);
	}

	Token GetNextToken()
	{
		if (_tokenEnumerator.MoveNext())
		{
			if (_tokenEnumerator.Current.Type == TokenType.ErrorToken)
			{
				_errors.Add(GenerateError(_tokenEnumerator.Current.Value, _tokenEnumerator.Current));

				return GetNextToken();
			}
			
			return _tokenEnumerator.Current;
		}
		else
		{
			throw new InvalidOperationException("No more tokens available");
		}
	}

	void SkipToNextStatement()
	{
		while (true)
		{
			var token = GetNextToken();

			if (token.Type == TokenType.Semicolon)
			{
				break;
			}
		}
	}

	ErrorNode ErrorOutOfCurrentContext()
	{
		SkipToNextStatement();

		return new();
	}

	ErrorStmt ErrorOutOfCurrentCodeStmt()
	{
		SkipToNextStatement();

		return new();
	}

	ErrorNode ErrorOutOfCurrentCodeBlock()
	{
		while (true)
		{
			var token = GetNextToken();

			if (token.Type == TokenType.RightBrace)
			{
				break;
			}
		}

		return new();
	}

	void RemoveLastError()
	{
		if (_errors.Count > 0)
		{
			_errors.RemoveAt(_errors.Count - 1);
		}
	}

	bool ExpectNextToken(TokenType expectedType, out Token parsedToken)
	{
		var token = GetNextToken();

		parsedToken = token;

		if (token.Type != expectedType)
		{
			_errors.Add(GenerateError($"Error QR202: Expected {TokenRepr.ToString(expectedType)}, but got {TokenRepr.ToString(token.Type)} instead.", token));

			return false;
		}

		return true;
	}

	bool ExpectNextToken(TokenType expectedType)
	{
		return ExpectNextToken(expectedType, out _);
	}

	bool ExpectKeyword(string expectedKeyword)
	{
		var token = GetNextToken();

		if (token.Type != TokenType.Keyword || token.Value != expectedKeyword)
		{
			_errors.Add(GenerateError($"Error QR203: Expected keyword '{expectedKeyword}', but got {TokenRepr.ToString(token)} instead.", token));

			return false;
		}

		return true;
	}

	/// <summary>
	/// Parses an import given that the last consumed token was the 'import' keyword.
	/// </summary>
	/// <returns></returns>
	ASTNode ParseImport()
	{
		if (!ExpectNextToken(TokenType.StringLiteral, out var modulePathToken)) return ErrorOutOfCurrentContext();

		return new ImportNode(modulePathToken.Value);
	}

	/// <summary>
	/// Parses a namespace declaration given that the last consumed token was the 'namespace' keyword.
	/// </summary>
	/// <returns></returns>
	ASTNode ParseNamespace()
	{
		if (!ExpectNextToken(TokenType.Identifier, out var nameToken)) return ErrorOutOfCurrentContext();

		string namespaceName = nameToken.Value;

		while (true)
		{
			var currToken = GetNextToken();

			if (currToken.Type == TokenType.Dot)
			{
				if (!ExpectNextToken(TokenType.Identifier, out var nextPartToken)) return ErrorOutOfCurrentContext();

				namespaceName += "." + nextPartToken.Value;
			}
			else
			{
				if (currToken.Type != TokenType.Semicolon)
				{
					_errors.Add(GenerateError($"Error QR202: Expected '.' or ';' in namespace declaration, but got {TokenRepr.ToString(currToken)} instead.", currToken));

					return ErrorOutOfCurrentContext();
				}

				break;
			}
		}

		return new NamespaceNode(namespaceName);
	}

	ASTNode ParseUsing()
	{
		if (!ExpectNextToken(TokenType.Identifier, out var namespaceToken)) return ErrorOutOfCurrentContext();

		string namespaceName = namespaceToken.Value;

		while (true)
		{
			var currToken = GetNextToken();

			if (currToken.Type == TokenType.Dot)
			{
				if (!ExpectNextToken(TokenType.Identifier, out var nextPartToken)) return ErrorOutOfCurrentContext();

				namespaceName += "." + nextPartToken.Value;
			}
			else
			{
				if (currToken.Type != TokenType.Semicolon)
				{
					_errors.Add(GenerateError($"Error QR202: Expected '.' or ';' in using statement, but got {TokenRepr.ToString(currToken)} instead.", currToken));

					return ErrorOutOfCurrentContext();
				}

				break;
			}
		}

		return new UsingNamespaceNode(namespaceName);
	}

	/// <summary>
	/// Parses a simple protection list of the form $[rw, ro, rw&, ...] given that the last consumed token was the opening bracket.
	/// </summary>
	/// <returns></returns>
	DataProtection[]? ParseSimpleProtectionList(int maxRefLevel, bool allowReadOnly = false)
	{		
		var setProtections = new bool[maxRefLevel + 1];
		var protections = new DataProtection[maxRefLevel + 1];

		Array.Fill(protections, DataProtection.ReadOnly);

		while (true)
		{
			var nextToken = GetNextToken();

			int refLevel = 0;

			while (nextToken.Type == TokenType.Asterisk)
			{
				refLevel++;

				nextToken = GetNextToken();
			}

			if (refLevel > maxRefLevel)
			{
				_errors.Add(GenerateError($"Error QR213: Too many reference levels specified in data protection list. The associated type only has {maxRefLevel} reference levels.", nextToken));

				return null;
			}

			if (setProtections[refLevel])
			{
				_errors.Add(GenerateError($"Error QR214: Duplicate protection modifier set for reference level {refLevel}.", nextToken));

				return null;
			}

			if (nextToken.Type != TokenType.Keyword)
			{
				_errors.Add(GenerateError($"Error QR202: Expected protection modifier in data protection list, but got {TokenRepr.ToString(nextToken)} instead.", nextToken));

				return null;
			}

			if (nextToken.Value == "rw")
			{
				protections[refLevel] = DataProtection.ReadWrite;
				setProtections[refLevel] = true;

				var tokenAfterProtDef = GetNextToken();

				if (tokenAfterProtDef.Type == TokenType.Comma)
				{
					continue;
				}
				else if (tokenAfterProtDef.Type == TokenType.RightBracket)
				{
					break;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected ',' or ']' after protection modifier, but got {TokenRepr.ToString(tokenAfterProtDef)} instead.", tokenAfterProtDef));

					return null;
				}
			}
			else if (nextToken.Value == "ro")
			{
				if (!allowReadOnly)
				{
					_errors.Add(GenerateError($"Error QR212: Read-only protection is applied by default. Remove the 'ro' modifier.", nextToken));

					return null;
				}

				protections[refLevel] = DataProtection.ReadOnly;
				setProtections[refLevel] = true;

				var tokenAfterProtDef = GetNextToken();

				if (tokenAfterProtDef.Type == TokenType.Comma)
				{
					continue;
				}
				else if (tokenAfterProtDef.Type == TokenType.RightBracket)
				{
					break;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected ',' or ']' after protection modifier, but got {TokenRepr.ToString(tokenAfterProtDef)} instead.", tokenAfterProtDef));

					return null;
				}
			}
			else
			{
				_errors.Add(GenerateError($"Error QR202: Expected 'rw' or 'ro' in data protection list, but got keyword '{nextToken.Value}' instead.", nextToken));

				return null;
			}
		}

		return [..protections];
	}

	/// <summary>
	/// Parses a complex protection definition of the form { pub [rw, rw&, ...], [rw, rw&, ...] } given that the last consumed token was the opening left brace.
	/// </summary>
	/// <returns></returns>
	Dictionary<AccessLevel, DataProtection[]>? ParseComplexProtection(int maxRefLevel)
	{
		Dictionary<AccessLevel, DataProtection[]> protections = [];

		while (true)
		{
			var nextToken = GetNextToken();

			if (nextToken.Type == TokenType.Keyword && nextToken.Value == "pub")
			{
				if (protections.ContainsKey(AccessLevel.Public))
				{
					_errors.Add(GenerateError($"Error QR206: Duplicate 'pub' modifier in protection definition.", nextToken));

					return protections;
				}

				if (!ExpectNextToken(TokenType.LeftBracket)) return null;

				var pubProtections = ParseSimpleProtectionList(maxRefLevel, allowReadOnly: true);

				if (pubProtections is null) return null;

				protections[AccessLevel.Public] = pubProtections;

				var afterPubProtToken = GetNextToken();

				if (afterPubProtToken.Type == TokenType.Comma)
				{
					continue;
				}
				else if (afterPubProtToken.Type == TokenType.RightBrace)
				{
					break;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected ',' or '}}' after public protection definition, but got {TokenRepr.ToString(afterPubProtToken)} instead.", afterPubProtToken));

					return null;
				}
			}
			else if (nextToken.Type == TokenType.LeftBracket)
			{
				if (protections.ContainsKey(AccessLevel.Private))
				{
					_errors.Add(GenerateError($"Error QR218: Duplicate private protection definition in protection definition.", nextToken));

					return protections;
				}

				var privProtections = ParseSimpleProtectionList(maxRefLevel, allowReadOnly: true);

				if (privProtections is null) return null;

				protections[AccessLevel.Private] = privProtections;

				var afterPrivProtToken = GetNextToken();

				if (afterPrivProtToken.Type == TokenType.Comma)
				{
					continue;
				}
				else if (afterPrivProtToken.Type == TokenType.RightBrace)
				{
					break;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected ',' or '}}' after private protection definition, but got {TokenRepr.ToString(afterPrivProtToken)} instead.", afterPrivProtToken));

					return null;
				}
			}
			else
			{
				_errors.Add(GenerateError($"Error QR202: Expected 'pub' or '[' in protection definition, but got {TokenRepr.ToString(nextToken)} instead.", nextToken));

				return null;
			}
		}

		return protections;
	}

	(ASTNode typeRefNode, Token lastUnprocessedToken) ParseTypeReference()
	{
		var token = GetNextToken();

		string typeName;
		List<TypeReferenceNode> genericArgs = [];
		Token currToken;


		if (token.Type == TokenType.Identifier)
		{
			typeName = token.Value;
		}
		else if (token.Type == TokenType.Keyword)
		{
			typeName = token.Value switch
			{
				"i8" => "Qor.Int8",
				"i16" => "Qor.Int16",
				"i32" => "Qor.Int32",
				"i64" => "Qor.Int64",
				"u8" => "Qor.UInt8",
				"u16" => "Qor.UInt16",
				"u32" => "Qor.UInt32",
				"u64" => "Qor.UInt64",
				"f32" => "Qor.Float32",
				"f64" => "Qor.Float64",
				"bool" => "Qor.Bool",
				"string" => "Qor.String",
				"void" => "::void",
				_ => "::error_type"
			};

			if (typeName == "::error_type")
			{
				_errors.Add(GenerateError($"Error QR202: Expected type name, but got keyword '{token.Value}' instead.", token));

				return (ErrorOutOfCurrentContext(), default);
			}

			if (typeName == "::void")
			{
				return (TypeReferenceNode.Void(), GetNextToken());
			}

			currToken = GetNextToken();
			
			goto AfterTypeResolved;
		}
		else
		{
			_errors.Add(GenerateError($"Error QR202: Expected type name, but got {TokenRepr.ToString(token)} instead.", token));

			return (ErrorOutOfCurrentContext(), default);
		}

		while (true)
		{
			currToken = GetNextToken();

			if (currToken.Type == TokenType.Dot)
			{
				if (!ExpectNextToken(TokenType.Identifier, out var nextPartToken)) return (ErrorOutOfCurrentContext(), default);

				typeName += "." + nextPartToken.Value;
			}
			else break;
		}

		if (currToken.Type == TokenType.LessThan)
		{
			while (true)
			{
				var (genericArgNode, lastUnprocessedToken) = ParseTypeReference();

				if (genericArgNode is ErrorNode) return (genericArgNode, default);

				genericArgs.Add((TypeReferenceNode) genericArgNode);

				var nextToken = lastUnprocessedToken;

				if (nextToken.Type == TokenType.Comma)
				{
					continue;
				}
				else if (nextToken.Type == TokenType.GreaterThan)
				{
					currToken = GetNextToken();
					break;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected ',' or '>' in generic argument list, but got {TokenRepr.ToString(currToken)} instead.", currToken));

					return (ErrorOutOfCurrentContext(), default);
				}
			}
		}

		AfterTypeResolved:

		List<IndirectionLayer> indirectionLayers = [];

		while (true)
		{
			if (currToken.Type == TokenType.Ampersand)
			{
				indirectionLayers.Add(IndirectionLayer.PointerTo);
			}
			else if (currToken.Type == TokenType.LeftBracket)
			{
				indirectionLayers.Add(IndirectionLayer.ArrayOf);
				
				if (!ExpectNextToken(TokenType.RightBracket)) return (ErrorOutOfCurrentContext(), default);
			}
			else
			{
				break; 
			}

			currToken = GetNextToken();
		}

		return (new TypeReferenceNode(typeName, [..indirectionLayers], [..genericArgs]), currToken);
	}

	/// <summary>
	/// Parses a single function argument given that the last consumed token was the argument name (an identifier).
	/// </summary>
	/// <param name="argName"></param>
	/// <returns></returns>
	(ASTNode argDeclNode, Token lastUnprocessedToken) ParseSingleFunctionArg(string argName)
	{
		if (!ExpectNextToken(TokenType.Colon)) return (ErrorOutOfCurrentContext(), default);

		var (typeNodeResult, lastUnprocessedToken) = ParseTypeReference();

		if (typeNodeResult is ErrorNode) return (typeNodeResult, default);

		var typeNode = (TypeReferenceNode) typeNodeResult;

		var currToken = lastUnprocessedToken;

		if (currToken.Type == TokenType.Dollar)
		{
			if (!ExpectNextToken(TokenType.LeftBracket)) return (ErrorOutOfCurrentContext(), default);

			var protections = ParseSimpleProtectionList(typeNode.RefLevel);

			if (protections is null) return (ErrorOutOfCurrentContext(), default);

			return (new ArgDeclarationNode(argName, typeNode, protections), GetNextToken());
		}

		var defaultProtections = new DataProtection[typeNode.RefLevel + 1];
		Array.Fill(defaultProtections, DataProtection.ReadOnly);

		return (new ArgDeclarationNode(argName, typeNode, defaultProtections), lastUnprocessedToken);
	}

	Expr ParseExpression(Token firstToken)
	{
		return new ErrorExpr();
	}

	CodeStmt ParseReturnStatement()
	{
		var token = GetNextToken();

		if (token.Type == TokenType.Semicolon)
		{
			return new ReturnStmt(null);
		}
		else
		{
			var exprNode = ParseExpression(token);

			if (!ExpectNextToken(TokenType.Semicolon)) return ErrorOutOfCurrentCodeStmt();

			return new ReturnStmt(exprNode);
		}
	}

	CodeStmt ParseLocalDeclaration()
	{
		// TODO: Implement local variable declaration parsing
		return ErrorOutOfCurrentCodeStmt();
	}

	CodeStmt ParseIfStatement()
	{
		// TODO: Implement if statement parsing
		return ErrorOutOfCurrentCodeStmt();
	}

	CodeStmt ParseWhileStatement()
	{
		// TODO: Implement while loop parsing
		return ErrorOutOfCurrentCodeStmt();
	}

	CodeStmt ParseForStatement()
	{
		// TODO: Implement for loop parsing
		return ErrorOutOfCurrentCodeStmt();
	}

	/// <summary>
	/// Parses a brace-delimited block of statements, such as a function body or the body of an if or while statement, given that the last consumed token was the opening left brace.
	/// </summary>
	/// <returns></returns>
	List<CodeStmt> ParseBraceEnclosedBody(bool isLoopBody = false)
	{
		List<CodeStmt> statements = [];

		bool returnEncountered = false;

		while (true)
		{
			var token = GetNextToken();

			if (token.Type == TokenType.RightBrace)
			{
				break;
			}

			if (returnEncountered)
			{
				_errors.Add(GenerateError($"Error QR215: Unreachable code detected. A 'return' statement has already been encountered in this block.", token));

				SkipToNextStatement();

				continue;
			}

			if (token.Type == TokenType.Keyword)
			{
				if (token.Value == "let")
				{
					var localDeclStmt = ParseLocalDeclaration();

					statements.Add(localDeclStmt);
				}
				else if (token.Value == "if")
				{
					var ifStmt = ParseIfStatement();

					statements.Add(ifStmt);
				}
				else if (token.Value == "while")
				{
					var whileStmt = ParseWhileStatement();

					statements.Add(whileStmt);
				}
				else if (token.Value == "for")
				{
					var forStmt = ParseForStatement();

					statements.Add(forStmt);
				}
				else if (token.Value == "break")
				{
					if (!isLoopBody)
					{
						_errors.Add(GenerateError($"Error QR216: 'break' statements may only be used within loops.", token));

						SkipToNextStatement();

						continue;
					}

					statements.Add(new BreakStmt());
				}
				else if (token.Value == "continue")
				{
					if (!isLoopBody)
					{
						_errors.Add(GenerateError($"Error QR217: 'continue' statements may only be used within loops.", token));

						SkipToNextStatement();

						continue;
					}

					statements.Add(new ContinueStmt());
				}
				else if (token.Value == "return")
				{
					var returnStmt = ParseReturnStatement();

					statements.Add(returnStmt);

					returnEncountered = true;

					continue; // the ParseReturnStatement method already handles the semicolon, so we can skip the check at the end of the loop
				}
				else
				{
					_errors.Add(GenerateError($"Error QR210: Keyword '{token.Value}' may not begin a function-level statement.", token));

					return statements;
				}
			}
			else
			{
				
				_errors.Add(GenerateError($"Error QR210: Token {TokenRepr.ToString(token)} may not begin a function-level statement.", token));

				return statements;
			}


			if (!ExpectNextToken(TokenType.Semicolon))
			{
				SkipToNextStatement();
			}
		}

		return statements;
	}

	/// <summary>
	/// Parses a function definition given that the last consumed token was an identifier (the function name).
	/// </summary>
	/// <param name="functionName"></param>
	/// <returns></returns>
	ASTNode ParseFunctionDefinition(string functionName, bool isMethod = false)
	{
		AccessLevel accessLevel = _currentDeclIsPublic ? AccessLevel.Public : AccessLevel.Private;
		_currentDeclIsPublic = false;

		List<string> typeParameters = [];
		List<ArgDeclarationNode> parameters = [];

		var token = GetNextToken();

		if (token.Type == TokenType.LessThan)
		{
			while (true)
			{
				var typeParamToken = GetNextToken();

				if (typeParamToken.Type != TokenType.Identifier)
				{
					_errors.Add(GenerateError($"Error QR202: Expected identifier in type parameter list, but got {TokenRepr.ToString(typeParamToken)} instead.", typeParamToken));

					return ErrorOutOfCurrentContext();
				}

				typeParameters.Add(typeParamToken.Value);

				var nextToken = GetNextToken();

				if (nextToken.Type == TokenType.Comma)
				{
					continue;
				}
				else if (nextToken.Type == TokenType.GreaterThan)
				{
					break;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected ',' or '>' in type parameter list, but got {TokenRepr.ToString(nextToken)} instead.", nextToken));

					return ErrorOutOfCurrentContext();
				}
			}

			if (!ExpectNextToken(TokenType.LeftParen))
			{
				return ErrorOutOfCurrentContext();
			}
		}
		else
		{
			if (token.Type != TokenType.LeftParen)
			{
				_errors.Add(GenerateError($"Error QR202: Expected '<' or '(' after function name, but got {TokenRepr.ToString(token)} instead.", token));

				return ErrorOutOfCurrentContext();
			}
		}

		while (true)
		{
			token = GetNextToken();

			if (token.Type == TokenType.RightParen)
			{
				break;
			}
			else if (token.Type == TokenType.Identifier)
			{
				var (node, lastUnprocessedToken) = ParseSingleFunctionArg(token.Value);

				if (node is ErrorNode) return node;

				parameters.Add((ArgDeclarationNode) node);

				var nextToken = lastUnprocessedToken;

				if (nextToken.Type == TokenType.Comma)
				{
					continue;
				}
				else if (nextToken.Type == TokenType.RightParen)
				{
					break;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected ',' or ')' after parameter declaration, but got {TokenRepr.ToString(nextToken)} instead.", nextToken));

					return ErrorOutOfCurrentContext();
				}
			}
			else
			{
				_errors.Add(GenerateError($"Error QR202: Expected identifier or ')' in parameter list, got {TokenRepr.ToString(token)}.", token));

				return ErrorOutOfCurrentContext();
			}
		}

		var tokenAfterParamList = GetNextToken();

		DataProtection selfProt = DataProtection.ReadOnly;

		if (isMethod)
		{
			if (tokenAfterParamList.Type == TokenType.LeftBracket)
			{
				var protectionDef = ParseSimpleProtectionList(0);

				if (protectionDef is null) return ErrorOutOfCurrentContext();

				selfProt = protectionDef[0];

				tokenAfterParamList = GetNextToken();
			}
		}

		TypeReferenceNode returnType;

		if (tokenAfterParamList.Type != TokenType.Colon)
		{
			returnType = TypeReferenceNode.Void();

			if (tokenAfterParamList.Type != TokenType.LeftBrace)
			{
				_errors.Add(GenerateError($"Error QR202: Expected ':' or '{{' after parameter list, but got {TokenRepr.ToString(tokenAfterParamList)} instead.", tokenAfterParamList));

				return ErrorOutOfCurrentContext();
			}
		}
		else
		{
			var (parsedType, lastUnprocessedToken) = ParseTypeReference();

			if (parsedType is ErrorNode) return parsedType;

			returnType = (TypeReferenceNode) parsedType;

			if (lastUnprocessedToken.Type != TokenType.LeftBrace)
			{
				_errors.Add(GenerateError($"Error QR202: Expected '{{' after return type, but got {TokenRepr.ToString(lastUnprocessedToken)} instead.", lastUnprocessedToken));

				return ErrorOutOfCurrentContext();
			}
		}

		// now we can parse the function body

		var body = ParseBraceEnclosedBody();

		if (isMethod)
		{
			return new MethodDefinitionNode(functionName, accessLevel, [..parameters], [..typeParameters], returnType, selfProt, [..body]);
		}

		return new FunctionDefinitionNode(functionName, accessLevel, [..parameters], [..typeParameters], returnType, [..body]);
	}

	/// <summary>
	/// Parses a field declaration given that the last consumed token was the field keyword.
	/// </summary>
	/// <returns></returns>
	ASTNode ParseFieldDeclaration()
	{
		bool fieldIsPublic = _currentDeclIsPublic;
		_currentDeclIsPublic = false;

		if (!ExpectNextToken(TokenType.Identifier, out var nameToken)) return ErrorOutOfCurrentContext();

		if (!ExpectNextToken(TokenType.Colon)) return ErrorOutOfCurrentContext();

		var (typeNodeResult, lastUnprocessedToken) = ParseTypeReference();

		if (typeNodeResult is ErrorNode) return typeNodeResult;

		TypeReferenceNode typeNode = (TypeReferenceNode) typeNodeResult;

		Dictionary<AccessLevel, DataProtection[]> protections;

		Token nextToken;

		if (lastUnprocessedToken.Type == TokenType.Dollar)
		{
			if (!ExpectNextToken(TokenType.LeftBracket)) return ErrorOutOfCurrentContext();

			var simpleProts = ParseSimpleProtectionList(typeNode.RefLevel);

			if (simpleProts is null) return ErrorOutOfCurrentContext();

			if (fieldIsPublic)
			{
				protections = new Dictionary<AccessLevel, DataProtection[]>
				{
					[AccessLevel.Public] = simpleProts,
					[AccessLevel.Private] = simpleProts
				};
			}
			else
			{
				protections = new Dictionary<AccessLevel, DataProtection[]>
				{
					[AccessLevel.Public] = [..Enumerable.Repeat(DataProtection.NoAccess, typeNode.RefLevel + 1)],
					[AccessLevel.Private] = simpleProts
				};
			}

			nextToken = GetNextToken();
		}
		else if (lastUnprocessedToken.Type == TokenType.LeftBrace)
		{
			if (fieldIsPublic)
			{
				_errors.Add(GenerateError($"Error QR219: Cannot mix complex data protections and a blanket 'pub' modifier. Consider removing the 'pub' modifier or using a simple protection list instead.", lastUnprocessedToken));

				return ErrorOutOfCurrentContext();
			}

			var complexProts = ParseComplexProtection(typeNode.RefLevel);

			if (complexProts is null) return ErrorOutOfCurrentContext();

			protections = complexProts;

			nextToken = GetNextToken();
		}
		else
		{
			protections = fieldIsPublic
				? new Dictionary<AccessLevel, DataProtection[]>
				{
					[AccessLevel.Public] = [..Enumerable.Repeat(DataProtection.ReadOnly, typeNode.RefLevel + 1)],
					[AccessLevel.Private] = [..Enumerable.Repeat(DataProtection.ReadOnly, typeNode.RefLevel + 1)]
				}
				: new Dictionary<AccessLevel, DataProtection[]>
				{
					[AccessLevel.Public] = [..Enumerable.Repeat(DataProtection.NoAccess, typeNode.RefLevel + 1)],
					[AccessLevel.Private] = [..Enumerable.Repeat(DataProtection.ReadOnly, typeNode.RefLevel + 1)]
				};

			nextToken = lastUnprocessedToken;
		}
		
		if (nextToken.Type != TokenType.Semicolon)
		{
			_errors.Add(GenerateError($"Error QR202: Expected ';' after global variable declaration, but got {TokenRepr.ToString(nextToken)} instead.", nextToken));

			return ErrorOutOfCurrentContext();
		}

		return new FieldDeclarationNode(nameToken.Value, typeNode, protections);		
	}

	/// <summary>
	/// Parses a type declaration given that the last consumed token was the 'type' keyword.
	/// </summary>
	/// <returns></returns>
	ASTNode ParseTypeDeclaration()
	{
		bool typeIsPublic = _currentDeclIsPublic;
		_currentDeclIsPublic = false;

		if (!ExpectNextToken(TokenType.Identifier, out var nameToken)) return ErrorOutOfCurrentContext();

		List<string> typeParameters = [];

		var token = GetNextToken();

		if (token.Type == TokenType.LessThan)
		{
			while (true)
			{
				var typeParamToken = GetNextToken();

				if (typeParamToken.Type != TokenType.Identifier)
				{
					_errors.Add(GenerateError($"Error QR202: Expected identifier in type parameter list, but got {TokenRepr.ToString(typeParamToken)} instead.", typeParamToken));

					return ErrorOutOfCurrentContext();
				}

				typeParameters.Add(typeParamToken.Value);

				var nextToken = GetNextToken();

				if (nextToken.Type == TokenType.Comma)
				{
					continue;
				}
				else if (nextToken.Type == TokenType.GreaterThan)
				{
					break;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected ',' or '>' in type parameter list, but got {TokenRepr.ToString(nextToken)} instead.", nextToken));

					return ErrorOutOfCurrentContext();
				}
			}

			if (!ExpectNextToken(TokenType.LeftBrace))
			{
				return ErrorOutOfCurrentContext();
			}
		}
		else if (token.Type != TokenType.LeftBrace)
		{
			_errors.Add(GenerateError($"Error QR202: Expected '<' or '{{' after type name, but got {TokenRepr.ToString(token)} instead.", token));

			return ErrorOutOfCurrentContext();
		}

		List<TypeMemberDeclarationNode> memberDeclarations = [];

		while (true)
		{
			var nextToken = GetNextToken();

			if (nextToken.Type == TokenType.RightBrace)
			{
				if (_currentDeclIsPublic)
				{
					_errors.Add(GenerateError($"Error QR202: Expected member declaration after 'pub' modifier, but got '}}' instead.", nextToken));

					return ErrorOutOfCurrentCodeBlock();
				}

				break;
			}
			else if (nextToken.Type == TokenType.Keyword)
			{
				if (nextToken.Value == "type")
				{
					_errors.Add(GenerateError($"Error QR211: Nested type declarations are not allowed.", nextToken));

					return ErrorOutOfCurrentCodeBlock();
				}
				else if (nextToken.Value == "pub")
				{
					if (_currentDeclIsPublic)
					{
						_errors.Add(GenerateError($"Error QR206: Duplicate 'pub' modifier applied to declaration.", nextToken));

						return ErrorOutOfCurrentCodeBlock();
					}

					_currentDeclIsPublic = true;
				}
				else if (nextToken.Value == "field")
				{
					var fieldNodeResult = ParseFieldDeclaration();

					if (fieldNodeResult is ErrorNode) continue;

					memberDeclarations.Add((FieldDeclarationNode) fieldNodeResult);
				}
				else
				{
					_errors.Add(GenerateError($"Error QR202: Expected declaration in type body, but got keyword '{nextToken.Value}' instead.", nextToken));

					return ErrorOutOfCurrentCodeBlock();
				}
			}
			else if (nextToken.Type == TokenType.Identifier)
			{
				var methodNodeResult = ParseFunctionDefinition(nextToken.Value, isMethod: true);

				if (methodNodeResult is ErrorNode) continue;

				memberDeclarations.Add((MethodDefinitionNode) methodNodeResult);
			}
			else
			{
				_errors.Add(GenerateError($"Error QR202: Expected declaration in type body, but got {TokenRepr.ToString(nextToken)} instead.", nextToken));

				return ErrorOutOfCurrentCodeBlock();
			}
		}


		return new TypeDefinitionNode(nameToken.Value, typeIsPublic ? AccessLevel.Public : AccessLevel.Private, [..typeParameters], [..memberDeclarations]);
	}

	/// <summary>
	/// Parses a global variable declaration given that the last consumed token was the 'global' keyword.
	/// </summary>
	/// <returns></returns>
	ASTNode ParseGlobalDeclaration()
	{
		bool globalIsPublic = _currentDeclIsPublic;
		_currentDeclIsPublic = false;

		if (!ExpectNextToken(TokenType.Identifier, out var nameToken)) return ErrorOutOfCurrentContext();

		if (!ExpectNextToken(TokenType.Colon)) return ErrorOutOfCurrentContext();

		var (typeNodeResult, lastUnprocessedToken) = ParseTypeReference();

		if (typeNodeResult is ErrorNode) return typeNodeResult;

		TypeReferenceNode typeNode = (TypeReferenceNode) typeNodeResult;

		Dictionary<AccessLevel, DataProtection[]> protections;

		Token nextToken;

		if (lastUnprocessedToken.Type == TokenType.Dollar)
		{
			if (!ExpectNextToken(TokenType.LeftBracket)) return ErrorOutOfCurrentContext();

			var simpleProts = ParseSimpleProtectionList(typeNode.RefLevel);

			if (simpleProts is null) return ErrorOutOfCurrentContext();

			if (globalIsPublic)
			{
				protections = new Dictionary<AccessLevel, DataProtection[]>
				{
					[AccessLevel.Public] = simpleProts,
					[AccessLevel.Private] = simpleProts
				};
			}
			else
			{
				protections = new Dictionary<AccessLevel, DataProtection[]>
				{
					[AccessLevel.Public] = [..Enumerable.Repeat(DataProtection.NoAccess, typeNode.RefLevel + 1)],
					[AccessLevel.Private] = simpleProts
				};
			}

			nextToken = GetNextToken();
		}
		else if (lastUnprocessedToken.Type == TokenType.LeftBrace)
		{
			if (globalIsPublic)
			{
				_errors.Add(GenerateError($"Error QR219: Cannot mix complex data protections and a blanket 'pub' modifier. Consider removing the 'pub' modifier or using a simple protection list instead.", lastUnprocessedToken));

				return ErrorOutOfCurrentContext();
			}

			var complexProts = ParseComplexProtection(typeNode.RefLevel);

			if (complexProts is null) return ErrorOutOfCurrentContext();

			protections = complexProts;

			nextToken = GetNextToken();
		}
		else
		{
			protections = globalIsPublic
				? new Dictionary<AccessLevel, DataProtection[]>
				{
					[AccessLevel.Public] = [..Enumerable.Repeat(DataProtection.ReadOnly, typeNode.RefLevel + 1)],
					[AccessLevel.Private] = [..Enumerable.Repeat(DataProtection.ReadOnly, typeNode.RefLevel + 1)]
				}
				: new Dictionary<AccessLevel, DataProtection[]>
				{
					[AccessLevel.Public] = [..Enumerable.Repeat(DataProtection.NoAccess, typeNode.RefLevel + 1)],
					[AccessLevel.Private] = [..Enumerable.Repeat(DataProtection.ReadOnly, typeNode.RefLevel + 1)]
				};

			nextToken = lastUnprocessedToken;
		}
				
		Expr? initializer = null;

		if (nextToken.Type == TokenType.Eq)
		{
			var initializerToken = GetNextToken();

			initializer = ParseExpression(initializerToken);

			nextToken = GetNextToken();
		}
		
		if (nextToken.Type != TokenType.Semicolon)
		{
			_errors.Add(GenerateError($"Error QR202: Expected ';' after global variable declaration, but got {TokenRepr.ToString(nextToken)} instead.", nextToken));

			return ErrorOutOfCurrentContext();
		}

		return new GlobalDeclarationNode(nameToken.Value, typeNode, protections, initializer);
	}

	public ParserResult ParseModule()
	{
		List<ASTNode> nodes = [];

		_errors.Clear();

		var lastToken = default(Token);
		var token = default(Token);

		try {
			while (true)
			{
				lastToken = token;
				token = GetNextToken();

				if (token.Type == TokenType.EOF)
				{
					if (_currentDeclIsPublic)
					{
						_errors.Add(GenerateError($"Error QR202: Expected declaration after 'pub' modifier, but got EOF instead.", token));
					}

					break;
				}

				if (token.Type == TokenType.Keyword)
				{
					if (_currentDeclIsPublic && token.Value != "type" && token.Value != "global")
					{
						if (token.Value == "pub") _errors.Add(GenerateError($"Error QR206: Duplicate 'pub' modifier applied to declaration.", token));
						else _errors.Add(GenerateError($"Error QR202: Expected declaration after 'pub', but got '{token.Value}' instead.", token));

						_currentDeclIsPublic = false;

						SkipToNextStatement();
						continue;
					}

					if (token.Value == "type")
					{
						var typeNode = ParseTypeDeclaration();

						nodes.Add(typeNode);
						continue; // there is no semicolon after a type declaration, so we can skip the check at the end of the loop
					}
					else if (token.Value == "global")
					{
						var globalNode = ParseGlobalDeclaration();

						nodes.Add(globalNode);

						continue; // ParseGlobalDeclaration handles the semicolon itself, so we can skip the check at the end of the loop
					}
					else if (token.Value == "import")
					{
						var importNode = ParseImport();

						nodes.Add(importNode);
					}
					else if (token.Value == "namespace")
					{
						if (nodes.OfType<NamespaceNode>().Any())
						{
							_errors.Add(GenerateError($"Error QR205: A single source file may only include one namespace declaration.", token));

							SkipToNextStatement();
							continue;
						}

						if (nodes.Count > 0)
						{
							_errors.Add(GenerateError($"Error QR204: Namespace declarations must be the first statements in a source file.", token));

							SkipToNextStatement();
							continue;
						}

						var namespaceNode = ParseNamespace();

						nodes.Add(namespaceNode);

						continue; // ParseNamespace() already handles the semicolon, so we can skip the check at the end of the loop
					}
					else if (token.Value == "using")
					{
						if (nodes.Count > 0 && nodes[0] is not NamespaceNode)
						{
							_errors.Add(GenerateError($"Error QR207: 'using' declarations must appear before all other statements except for an optional namespace declaration.", token));

							SkipToNextStatement();
							continue;
						}

						var usingNode = ParseUsing();

						nodes.Add(usingNode);

						continue; // ParseUsing() already handles the semicolon, so we can skip the check at the end of the loop
					}
					else if (token.Value == "pub")
					{
						// _currentDeclIsPublic must be false here since it was handled in an earlier case

						_currentDeclIsPublic = true;

						continue; // 'pub' is not a complete statement on its own, so we can skip the semicolon check at the end of the loop
					}
					else
					{
						_errors.Add(GenerateError($"Error QR208: Unexpected keyword '{token.Value}'.", token));

						SkipToNextStatement();
						continue;
					}
				}
				else if (token.Type == TokenType.Identifier)
				{
					var funcNode = ParseFunctionDefinition(token.Value);

					nodes.Add(funcNode);

					continue; // No semicolon expected after a function definition, so we can skip the check at the end of the loop
				}

				else if (_currentDeclIsPublic)
				{
					_errors.Add(GenerateError($"Error QR202: Expected declaration after 'pub', but got '{token.Value}' instead.", token));

					_currentDeclIsPublic = false;

					SkipToNextStatement();
					continue;
				}
				else
				{
					_errors.Add(GenerateError($"Error QR210: A(n) {TokenRepr.ToString(token.Type)} may not begin a top-level statement.", token));

					SkipToNextStatement();

					continue;
				}

				if (!ExpectNextToken(TokenType.Semicolon))
				{
					SkipToNextStatement();
				}
			}
		} catch (InvalidOperationException)
		{
			_errors.Add(new CompilationError("Error QR201: Unexpected end of input.", lastToken.Location));
		}

		return (nodes, _errors);
	}
}