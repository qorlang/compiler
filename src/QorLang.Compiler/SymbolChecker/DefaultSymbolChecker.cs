using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Parser.Nodes.CodeStatements;
using QorLang.Compiler.Parser.Nodes.Expressions;
using QorLang.Compiler.SymbolChecker.SymbolReferences;
using QorLang.Compiler.SymbolChecker.Symbols;

namespace QorLang.Compiler.SymbolChecker;

using SymbolCheckerResult = (
	GlobalSymbolTable GlobalSymbols,
	Dictionary<Symbol, ScopedSymbolTable> ScopedSymbolTables,
	Dictionary<ASTNode, Symbol> ResolvedNodeMap,
	Dictionary<Expr, TypeReference> ExprTypeMap,
	List<CompilationError> Errors
);

/// <summary>
/// Checks symbols and assigns inferred types to expressions. Does not perform assignment validity or type compatibility checks.
/// </summary>
/// <param name="nodes"></param>
public class DefaultSymbolChecker(IEnumerable<ASTNode> nodes)
{
	readonly ASTNode[] _nodes = [..nodes];
	readonly GlobalSymbolTable _symbols = new();

	// types can have scoped symbol tables too because their members do not need to be qualified and they may also have type arguments
	readonly Dictionary<Symbol, ScopedSymbolTable> _scopedSymbolTables = [];
	readonly Dictionary<ASTNode, SymbolReference> _resolvedNodeToReferenceMap = [with(ReferenceEqualityComparer.Instance)];
	readonly Dictionary<ASTNode, Symbol> _resolvedNodeToDeclarationMap = [with(ReferenceEqualityComparer.Instance)];
	readonly Dictionary<Expr, TypeReference> _exprTypeMap = [with(ReferenceEqualityComparer.Instance)];
	readonly List<CompilationError> _errors = [];

	TypeReference ResolveTypeReference(TypeReferenceNode node, bool allowVoid = false)
	{
		TypeSymbol typeBase;

		if (!_symbols.TryGetSymbol(node.TypeName, node.Location.File, out var returnTypeSymbolMatches))
		{
			_errors.Add(new CompilationError(
				$"Error QR302: Type '{node.TypeName}' is not defined.",
				node.Location
			));

			typeBase = TypeSymbol.ErrorType;
		}
		else if (returnTypeSymbolMatches[0].Type != SymbolType.Type)
		{
			_errors.Add(new CompilationError(
				$"Error QR303: '{node.TypeName}' is not a type.",
				node.Location
			));

			typeBase = TypeSymbol.ErrorType;
		}
		else if (returnTypeSymbolMatches[0] == TypeSymbol.VoidType && !allowVoid)
		{
			_errors.Add(new CompilationError(
				$"Error QR304: void cannot be used in this context. Please supply a concrete type.",
				node.Location
			));

			typeBase = TypeSymbol.ErrorType;
		}
		else
		{
			typeBase = (TypeSymbol) returnTypeSymbolMatches[0];
		}

		TypeReference[] typeArguments = [.. node.TypeArguments.Select(argNode => ResolveTypeReference(argNode))];

		// void may not have arguments or indirection layers

		if (typeBase == TypeSymbol.VoidType && (typeArguments.Length > 0 || node.IndirectionLayers.Length > 0))
		{
			_errors.Add(new CompilationError(
				$"Error QR313: void cannot have type arguments or indirection layers.",
				node.Location
			));
		}

		// check if type arguments are valid for the type base

		if (typeArguments.Length != typeBase.TypeArgumentNames.Length)
		{
			_errors.Add(new CompilationError(
				$"Error QR312: Type '{typeBase.FullyQualifiedName}' expects {typeBase.TypeArgumentNames.Length} type argument(s), but {typeArguments.Length} were provided.",
				node.Location
			));
		}

		TypeReference typeReference = new(typeBase, typeArguments, node.IndirectionLayers);

		return typeReference;
	}

	public SymbolCheckerResult Check(Func<string, (GlobalSymbolTable SymbolTable, List<CompilationError> Errors)> ImportModule)
	{
		string currentNamespace = string.Empty;

		foreach (var node in _nodes)
		{
			// Check all toplevel nodes

			switch (node)
			{
				case NamespaceNode namespaceNode:
					currentNamespace = namespaceNode.Name;
					break;
				case ImportNode importNode:
					var (symbolTable, errors) = ImportModule(importNode.ModulePath);

					if (errors.Count > 0)
					{
						_errors.AddRange(errors);
					}
					else
					{
						_errors.AddRange(_symbols.AddImport(symbolTable, importNode.Location));
					}

					break;
				case UsingNamespaceNode usingNode:
					_errors.AddRange(_symbols.UseNamespace(usingNode.Name, usingNode.Location));
					break;
				case TypeDefinitionNode typeDefNode:
					var typeSymbol = new TypeSymbol(typeDefNode.Name, currentNamespace, typeDefNode.TypeParameters, typeDefNode.AccessLevel);

					var symbolConflictErrors = _symbols.AddSymbol(typeSymbol, typeDefNode.Location);

					if (symbolConflictErrors.Count > 0)
					{
						_errors.AddRange(symbolConflictErrors);
					}
					else
					{
						_resolvedNodeToDeclarationMap[typeDefNode] = typeSymbol;
						// TODO: create scoped symbol table, visit type node members
					}

					break;
				case FunctionDefinitionNode functionDefinitionNode:
					var returnType = ResolveTypeReference(functionDefinitionNode.ReturnType, allowVoid: true);

					ArgumentOrLocalSymbol[] argumentSignature = [..functionDefinitionNode.Parameters.Select(
						arg => new ArgumentOrLocalSymbol(
							SymbolType.Argument,
							arg.Name,
							ResolveTypeReference(arg.DataType, allowVoid: false),
							arg.Protections
						)
					)];

					var functionSymbol = new FunctionSymbol(
						functionDefinitionNode.Name,
						returnType,
						functionDefinitionNode.TypeParameters,
						argumentSignature,
						functionDefinitionNode.AccessLevel,
						currentNamespace
					);

					var functionConflictErrors = _symbols.AddSymbol(functionSymbol, functionDefinitionNode.Location);

					if (functionConflictErrors.Count > 0)
					{
						_errors.AddRange(functionConflictErrors);
					}
					else
					{
						_resolvedNodeToDeclarationMap[functionDefinitionNode] = functionSymbol;
						// TODO: create scoped symbol table, visit function body
					}

					break;
				case GlobalDeclarationNode globalDeclarationNode:
					var dataType = ResolveTypeReference(globalDeclarationNode.DataType, allowVoid: false);

					var globalSymbol = new GlobalSymbol(
						globalDeclarationNode.Name,
						currentNamespace,
						dataType
					);

					var globalConflictErrors = _symbols.AddSymbol(globalSymbol, globalDeclarationNode.Location);

					if (globalConflictErrors.Count > 0)
					{
						_errors.AddRange(globalConflictErrors);
					}
					else
					{
						_resolvedNodeToDeclarationMap[globalDeclarationNode] = globalSymbol;
					}

					break;
				default:
					throw new NotImplementedException($"SymbolChecker.Check: Unhandled node type {node.GetType().Name}");
			}
		}

		return (_symbols, _scopedSymbolTables, _resolvedNodeToDeclarationMap, _exprTypeMap, _errors);
	}

	void VisitTypeDefinitionMembers(TypeSymbol enclosingType, ScopedSymbolTable symTable, TypeDefinitionNode typeDefNode)
	{
		foreach (var member in typeDefNode.MemberDeclarations)
		{
			switch (member)
			{
				case FieldDeclarationNode fieldNode:
					var fieldType = ResolveTypeReference(fieldNode.DataType, allowVoid: false);

					Symbol fieldSymbol;

					if (fieldNode.IsStatic)
					{
						fieldSymbol = new StaticFieldSymbol(
							fieldNode.Name,
							enclosingType,
							fieldType,
							fieldNode.Protections
						);
					}
					else
					{
						fieldSymbol = new BoundFieldSymbol(
							fieldNode.Name,
							enclosingType,
							fieldType,
							fieldNode.Protections
						);
					}

					var fieldConflictErrors = symTable.AddSymbol(fieldSymbol, fieldNode.Location);

					if (fieldConflictErrors.Count > 0)
					{
						_errors.AddRange(fieldConflictErrors);
					}
					else
					{
						_resolvedNodeToDeclarationMap[fieldNode] = fieldSymbol;
					}

					break;
				case MethodDefinitionNode methodNode:
					var returnType = ResolveTypeReference(methodNode.ReturnType, allowVoid: true);

					ArgumentOrLocalSymbol[] argumentSignature = [..methodNode.Parameters.Select(
						arg => new ArgumentOrLocalSymbol(
							SymbolType.Argument,
							arg.Name,
							ResolveTypeReference(arg.DataType, allowVoid: false),
							arg.Protections
						)
					)];

					FunctionOrMethodSymbol methodSymbol;

					if (methodNode.IsStatic)
					{
						methodSymbol = new StaticMethodSymbol(
							methodNode.Name,
							returnType,
							methodNode.TypeParameters,
							argumentSignature,
							methodNode.AccessLevel,
							enclosingType,
							methodNode.ProtectionOnSelf
						);
					}
					else
					{
						methodSymbol = new BoundMethodSymbol(
							methodNode.Name,
							returnType,
							methodNode.TypeParameters,
							argumentSignature,
							methodNode.AccessLevel,
							enclosingType,
							methodNode.ProtectionOnSelf
						);
					}

					var methodConflictErrors = symTable.AddSymbol(methodSymbol, methodNode.Location);

					if (methodConflictErrors.Count > 0)
					{
						_errors.AddRange(methodConflictErrors);
					}
					else
					{
						_resolvedNodeToDeclarationMap[methodNode] = methodSymbol;
						// TODO: create scoped symbol table, visit method body
						// these are the only scope symtables that will contain a file, namespace, AND type context
						// the bound methods will not come with a 'this' symbol as ThisExpr exists, but
						// only bound methods should allow 'this' to be used in their bodies.
					}

					break;
			}
		}
	}

	void VisitMethodBody(ScopedSymbolTable symTable, MethodDefinitionNode methodNode)
	{
		foreach (var statement in methodNode.Body)
		{
			VisitAndTagStmt(symTable, statement);
		}
	}

	void VisitAndTagStmt(ScopedSymbolTable symTable, CodeStmt statement)
	{
		switch (statement)
		{
			case LocalDeclarationStmt localDeclStmt:
				TypeReference localType;

				if (localDeclStmt.Initializer is not null)
				{
					VisitAndTagExpr(symTable, localDeclStmt.Initializer);
				}

				if (localDeclStmt.IsInference)
				{
					localType = _exprTypeMap[localDeclStmt.Initializer!];
				}
				else
				{
					localType = ResolveTypeReference(localDeclStmt.DataType!, allowVoid: false);
				}

				DataProtection[] protections;

				if (localDeclStmt.Protections is not null)
				{
					protections = localDeclStmt.Protections;
				}
				else
				{
					// default protections
					int ptrLevel = localType.IndirectionLayers.Count(IndirectionLayer.PointerTo);

					protections = new DataProtection[ptrLevel+1];

					Array.Fill(protections, DataProtection.ReadOnly);
				}


				var localSymbol = new ArgumentOrLocalSymbol(
					SymbolType.Local,
					localDeclStmt.Name,
					localType,
					protections
				);

				var localConflictErrors = symTable.AddSymbol(localSymbol, localDeclStmt.Location);

				if (localConflictErrors.Count > 0)
				{
					_errors.AddRange(localConflictErrors);
				}
				else
				{
					_resolvedNodeToDeclarationMap[localDeclStmt] = localSymbol;
				}

				break;
			case AssignStmt stmt:
				VisitAndTagExpr(symTable, stmt.Target);
				VisitAndTagExpr(symTable, stmt.Value);

				break;
			case IndexAssignStmt stmt:
				// need to validate that @seti exist and the signature matches, grab the @seti declaration, and then use the return value to set
				// just realized that there could theoretically be an IndexAddAssignStmt... so a better solution needs to

				break;
			case ReturnStmt stmt:
				if (stmt.Value is not null)
				{
					VisitAndTagExpr(symTable, stmt.Value);
				}

				break;
			case ForStmt stmt:
				var forScope = new ScopedSymbolTable(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

				if (stmt.Initializer is not null)
				{
					VisitAndTagStmt(forScope, stmt.Initializer);
				}

				if (stmt.Condition is not null)
				{
					VisitAndTagExpr(forScope, stmt.Condition);
				}

				if (stmt.Increment is not null)
				{
					VisitAndTagStmt(forScope, stmt.Increment);
				}

				foreach (var bodyStmt in stmt.Body)
				{
					VisitAndTagStmt(forScope, bodyStmt);
				}

				break;
			case WhileStmt stmt:
				var whileScope = new ScopedSymbolTable(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

				VisitAndTagExpr(symTable, stmt.Condition);

				foreach (var bodyStmt in stmt.Body)
				{
					VisitAndTagStmt(whileScope, bodyStmt);
				}

				break;
			case IfStmt stmt:
				var ifScope = new ScopedSymbolTable(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

				VisitAndTagExpr(symTable, stmt.Condition);

				foreach (var bodyStmt in stmt.ThenBody)
				{
					VisitAndTagStmt(ifScope, bodyStmt);
				}

				var elseScope = new ScopedSymbolTable(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

				foreach (var bodyStmt in stmt.ElseBody)
				{
					VisitAndTagStmt(elseScope, bodyStmt);
				}

				break;
			case ExprStmt stmt:
				VisitAndTagExpr(symTable, stmt.Expr);

				break;
		}
	}

	void VisitOpAssignment(ScopedSymbolTable symTable, Expr target, Expr value, string operatorSpecialName, TokenLocation location)
	{
		VisitAndTagExpr(symTable, target);
		VisitAndTagExpr(symTable, value);

		var targetType = _exprTypeMap[target];

		ValidateOperatorOverloadExists(targetType, operatorSpecialName, 1, location);
	}

	void ValidateOperatorOverloadExists(TypeReference definedOn, string operatorSpecialName, int argcount, TokenLocation location)
	{
		var targetMethodName = $"{definedOn.FullyQualifiedName}.@{operatorSpecialName}";

		_symbols.TryGetSymbol(targetMethodName, location.File, out var symbolMatches);
		BoundMethodSymbol[] targetMethodMatches  = [..symbolMatches.OfType<BoundMethodSymbol>()];

		if (symbolMatches.Length == 0)
		{
			_errors.Add(new CompilationError(
				$"Error QR305: Type '{definedOn.FullyQualifiedName}' does not have an instance @add method defined.",
				location
			));
		}
		else if (targetMethodMatches.All(targetFunction => targetFunction.Signature.Length != argcount))
		{
			if (argcount == 1)
			{
				_errors.Add(new CompilationError(
					$"Error QR307: Symbol '{targetMethodName}' must have exactly one argument.",
					location
				));
			}
			else
			{
				_errors.Add(new CompilationError(
					$"Error QR307: Symbol '{targetMethodName}' must have exactly {argcount} arguments.",
					location
				));
			}
		}
	}

	public void TagLiteralExpr(Expr expr, string literalType)
	{
		if (
			!_symbols.TryGetSymbol(literalType, expr.Location.File, out var stringTypeSymbol) ||
			stringTypeSymbol[0].Type != SymbolType.Type ||
			((TypeSymbol) stringTypeSymbol[0]).TypeArgumentNames.Length != 0)
		{
			_errors.Add(new CompilationError(
				$"Error QR310: Literal type '{literalType}' cannot be found.",
				expr.Location
			));

			_exprTypeMap[expr] = TypeReference.ErrorType;
		}
		else
		{
			var stringTypeRef = new TypeReference((TypeSymbol) stringTypeSymbol[0], [], []);

			_exprTypeMap[expr] = stringTypeRef;
		}
	}

	public void VisitAndTagExpr(ScopedSymbolTable symTable, Expr expr)
	{
		// we can't yet handle binary operators since we won't be able to assign overloads/calculate what the return type is
		// until we perform type checking, so for now we'll tag select root expr nodes and let the type checker fill in the gaps

		switch (expr)
		{
			case StringLiteralExpr:
				TagLiteralExpr(expr, "Qor.String");
				break;
			case IntegerLiteralExpr:
				TagLiteralExpr(expr, "Qor.Int32");
				break;
			case FloatLiteralExpr:
				TagLiteralExpr(expr, "Qor.Float64");
				break;
			case BooleanLiteralExpr:
				TagLiteralExpr(expr, "Qor.Bool");
				break;
			case CharLiteralExpr:
				TagLiteralExpr(expr, "Qor.Char");
				break;
			case ThisExpr:
				// 'this' is not really a bound field but is added to bound method symbol tables to support this functionality
				// however this does not count as a reference to go into _resolvedNodeToReferenceMap because it is not a real symbol
				// and this node will be special-cased by the code generator anyway

				if (!symTable.TryGetSymbol("this", out var thisSymbol))
				{
					_errors.Add(new CompilationError(
						$"Error QR308: 'this' cannot be used in this context.",
						expr.Location
					));

					_exprTypeMap[expr] = TypeReference.ErrorType;
				}
				else
				{
					var thisTypeRef = ((BoundFieldSymbol) thisSymbol[0]).DataType;

					_exprTypeMap[expr] = thisTypeRef;
				}

				break;
			case BinaryOpExpr binOpExpr:
				VisitAndTagExpr(symTable, binOpExpr.Left);
				VisitAndTagExpr(symTable, binOpExpr.Right);

				// TODO: needs method lookup

				break;

			// dot, ref, deref, and call may not be overloaded
			case CallExpr callExpr:
				// validate method exists, etc.
				break;
			case RefExpr refExpr:
				VisitAndTagExpr(symTable, refExpr.Target);
				break;
			case DerefExpr derefExpr:
				VisitAndTagExpr(symTable, derefExpr.Target);

				// ensure target is a pointer type

				if (_exprTypeMap[derefExpr.Target].IndirectionLayers[^1] != IndirectionLayer.PointerTo)
				{
					_errors.Add(new CompilationError(
						$"Error QR309: Cannot dereference a non-pointer type.",
						derefExpr.Location
					));

					_exprTypeMap[derefExpr] = TypeReference.ErrorType;
				}
				else
				{
					var targetType = _exprTypeMap[derefExpr.Target];

					var derefType = new TypeReference(
						targetType.TypeSymbol,
						targetType.TypeArguments,
						targetType.IndirectionLayers[..^1]
					);

					_exprTypeMap[derefExpr] = derefType;
				}

				break;
			case DotExpr dotExpr:
				VisitAndTagExpr(symTable, dotExpr.Target);

				NamespaceReference? targetNamespaceRef = null;
				TypeReference? targetTypeRef = null;
				bool isNamespaceProperty = false;
				bool isInstanceProperty = false;

				if (_exprTypeMap.ContainsKey(dotExpr.Target))
				{
					// instance attribute, as the Target is a valued expression and not a type reference

					targetTypeRef = _exprTypeMap[dotExpr.Target];
					isInstanceProperty = true;
				}
				else
				{
					if (_resolvedNodeToReferenceMap[dotExpr.Target] is NamespaceReference)
					{
						targetNamespaceRef = (NamespaceReference) _resolvedNodeToReferenceMap[dotExpr.Target];
						isNamespaceProperty = true;
					}
					else
					{
						targetTypeRef = (TypeReference) _resolvedNodeToReferenceMap[dotExpr.Target];
						isInstanceProperty = false;
					}
				}

				if (isNamespaceProperty)
				{
					if (!_symbols.TryGetNamespaceMember(
						targetNamespaceRef!,
						dotExpr.Property.Name,
						out var namespaceMemberMatches
					))
					{
						_errors.Add(new CompilationError(
							$"Error QR311: Namespace '{targetNamespaceRef!.FullyQualifiedName}' does not have a member named '{dotExpr.Property.Name}'.",
							dotExpr.Location
						));

						_exprTypeMap[dotExpr] = TypeReference.ErrorType;
					}
					else
					{
						if (namespaceMemberMatches.Length > 1)
						{
							// Now need to check if this is the target of a call expr.
							// need to handle case of ambiguous method reference
							// so if the lookup is ambiguous and this expression is
							// not the target of a call expression, then we should error out

							// then check for matches based on # of type parameters
							// and then check if it is properly static/instance
						}
						else
						{
							var varRef = new VariableSymbolReference(namespaceMemberMatches[0]);
							_resolvedNodeToReferenceMap[dotExpr] = varRef;
							_exprTypeMap[dotExpr] = varRef.DataType;
						}
					}
				}
				else
				{
					if (!_symbols.TryGetMember(
						targetTypeRef!,
						dotExpr.Property.Name,
						isInstanceProperty,
						out var propertySymbolMatches
					))
					{
						_errors.Add(new CompilationError(
							$"Error QR306: Type '{targetTypeRef!.FullyQualifiedName}' does not have a property named '{dotExpr.Property.Name}'.",
							dotExpr.Location
						));

						_exprTypeMap[dotExpr] = TypeReference.ErrorType;
					}
					else
					{
						if (propertySymbolMatches.Length > 1)
						{
							// Now need to check if this is the target of a call expr.
							// need to handle case of ambiguous method reference
							// so if the lookup is ambiguous and this expression is
							// not the target of a call expression, then we should error out

							// then check for matches based on # of type parameters
							// and then check if it is properly static/instance
						}
						else
						{
							var varRef = new VariableSymbolReference(propertySymbolMatches[0]);
							_resolvedNodeToReferenceMap[dotExpr] = varRef;
							_exprTypeMap[dotExpr] = varRef.DataType;
						}
					}
				}

				break;
			case IndexExpr indexExpr: // requires method lookup and containing expr for @geti
				break;
			case NameReferenceExpr nameReferenceExpr:
				if (!symTable.TryGetSymbol(
					nameReferenceExpr.Name,
					out var symbolMatches
				))
				{
					_errors.Add(new CompilationError(
						$"Error QR314: Symbol '{nameReferenceExpr.Name}' not found.",
						nameReferenceExpr.Location
					));

					_exprTypeMap[nameReferenceExpr] = TypeReference.ErrorType;
				}
				else
				{
					if (symbolMatches.Length > 1)
					{
						// Now need to check if this is the target of a call expr.
						// need to handle case of ambiguous method reference
						// so if the lookup is ambiguous and this expression is
						// not the target of a call expression, then we should error out

						// then check for matches based on # of type parameters
						// and then check if it is properly static/instance
					}
					else
					{
						var varRef = new VariableSymbolReference(symbolMatches[0]);
						_resolvedNodeToReferenceMap[nameReferenceExpr] = varRef;
						_exprTypeMap[nameReferenceExpr] = varRef.DataType;
					}
				}
				break;
			case UnaryOpExpr unaryOpExpr:
				VisitAndTagExpr(symTable, unaryOpExpr.Target);

				// requires method lookup
				break;
			default:
				throw new NotImplementedException($"SymbolChecker.VisitAndTagExpr: Unhandled expression type {expr.GetType().Name}");
		}
	}
}