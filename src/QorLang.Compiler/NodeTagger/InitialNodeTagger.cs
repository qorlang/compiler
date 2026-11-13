using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Parser.Nodes.CodeStatements;
using QorLang.Compiler.Parser.Nodes.Expressions;
using QorLang.Compiler.Symbols;
using QorLang.Compiler.Symbols.References;
using QorLang.Compiler.Symbols.SymbolTables;

namespace QorLang.Compiler.NodeTagger;

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
public class InitialNodeTagger(IEnumerable<ASTNode> nodes)
{
	readonly ASTNode[] _nodes = [..nodes];
	readonly GlobalSymbolTable _symbols = new();

	// types can have scoped symbol tables too because their members do not need to be qualified, and they may also have type arguments
	readonly Dictionary<Symbol, ScopedSymbolTable> _scopedSymbolTables = [];
	readonly Dictionary<ASTNode, SymbolReference> _resolvedNodeToReferenceMap = [with(ReferenceEqualityComparer.Instance)];
	readonly Dictionary<ASTNode, Symbol> _resolvedNodeToDeclarationMap = [with(ReferenceEqualityComparer.Instance)];
	readonly Dictionary<Expr, TypeReference> _exprTypeMap = [with(ReferenceEqualityComparer.Instance)];
	readonly List<CompilationError> _errors = [];

	delegate bool TypeSymbolGetter(TypeReferenceNode bode, out Symbol[] typeSymbolMatches);

	TypeReference ResolveTypeReference(TypeReferenceNode node, bool allowVoid = false)
	{
		return ResolveTypeReference((n, out s) => _symbols.TryGetSymbol(n.TypeName, n.Location.File, out s), node, allowVoid);
	}

	TypeReference ResolveTypeReference(ScopedSymbolTable symTable, TypeReferenceNode node, bool allowVoid = false)
	{
		return ResolveTypeReference((n, out s) => symTable.TryGetSymbol(n.TypeName, out s), node, allowVoid);
	}

	void AddTypeParameterSymbols(ScopedSymbolTable symTable, TypeParameterDeclarationNode[] typeParameters)
	{
		// assume typeParameters are distinct, we only have to validate that there are no name conflicts
		foreach (var typeParam in typeParameters)
		{
			// this will handle name conflict errors automatically
			_errors.AddRange(symTable.AddSymbol(new TypeArgumentTypeSymbol(typeParam.Name), typeParam.Location));
		}
	}

	TypeReference ResolveTypeReference(TypeSymbolGetter symbolGetter, TypeReferenceNode node, bool allowVoid = false)
	{
		TypeSymbol typeBase;

		if (!symbolGetter(node, out var typeSymbolMatches))
		{
			_errors.Add(new CompilationError(
				$"Error QR302: Type '{node.TypeName}' is not defined.",
				node.Location
			));

			typeBase = TypeSymbol.ErrorType;
		}
		else if (typeSymbolMatches[0].Type != SymbolType.Type)
		{
			_errors.Add(new CompilationError(
				$"Error QR303: '{node.TypeName}' is not a type.",
				node.Location
			));

			typeBase = TypeSymbol.ErrorType;
		}
		else if (typeSymbolMatches[0] == TypeSymbol.VoidType && !allowVoid)
		{
			_errors.Add(new CompilationError(
				$"Error QR304: void cannot be used in this context. Please supply a concrete type.",
				node.Location
			));

			typeBase = TypeSymbol.ErrorType;
		}
		else
		{
			typeBase = (TypeSymbol) typeSymbolMatches[0];
		}

		TypeReference[] typeArguments = [.. node.TypeArguments.Select(argNode => ResolveTypeReference(symbolGetter, argNode))];

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

	// creates and/or returns the existing delegate type for a specified function reference
	TypeReference GetDelegateType(FunctionOrMethodReference funcRef)
	{
		return null!;
	}

	public SymbolCheckerResult Check(Func<string, (GlobalSymbolTable SymbolTable, List<CompilationError> Errors)> ImportModule)
	{
		var currentNamespace = string.Empty;

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

						ScopedSymbolTable symTable = new(_symbols, typeDefNode.Location.File, currentNamespace, typeSymbol);

						AddTypeParameterSymbols(symTable, typeDefNode.TypeParameters);
						VisitTypeDefinitionMembers(typeSymbol, symTable, typeDefNode);
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

						ScopedSymbolTable symTable = new(_symbols, functionDefinitionNode.Location.File, currentNamespace);
						AddTypeParameterSymbols(symTable, functionDefinitionNode.TypeParameters);

						VisitFunctionBody(symTable, functionDefinitionNode);
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
		// A few notes about constructing the `thisType`.
		// We can pass empty arrays for the typeArguments and indirectionLayers
		// of the type arguments of the 'this' type because typeArguments themselves
		// may not be defined with further typeArguments or indirection layers
		// We also use the symbol table to grab these type arguments' symbols, but this
		// is probably a bad idea considering the very next/previous method up the chain
		// (the caller of this method) has access to the TypeArgumentTypeSymbol objects
		// of the type parameters
		TypeReference thisType = new TypeReference(
			enclosingType,
			[
				..enclosingType.TypeArgumentNames
					.Select(name =>
					{
						// We assume the symbol exists here & is a TypeSymbol, which is dangerous!
						symTable.TryGetSymbol(name, out var symbolMatches);

						return new TypeReference((TypeSymbol)symbolMatches[0], [], []);
					})
			],
			[]
		);

		foreach (var member in typeDefNode.MemberDeclarations)
		{
			switch (member)
			{
				case FieldDeclarationNode fieldNode:
					var fieldType = ResolveTypeReference(symTable, fieldNode.DataType, allowVoid: false);

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
					var returnType = ResolveTypeReference(symTable, methodNode.ReturnType, allowVoid: true);

					ArgumentOrLocalSymbol[] argumentSignature = [..methodNode.Parameters.Select(
						arg => new ArgumentOrLocalSymbol(
							SymbolType.Argument,
							arg.Name,
							ResolveTypeReference(symTable, arg.DataType, allowVoid: false),
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

						// these are the only scope symtables that will contain a file, namespace, AND type context
						// Bound methods will not come with a 'this' symbol, as 'this' is referenced through ThisExpr
						// and not NameReferenceExpr, but only bound methods should allow 'this' to be used in their bodies
						ScopedSymbolTable methodSymTable = new(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

						AddTypeParameterSymbols(methodSymTable, methodNode.TypeParameters);
						VisitMethodBody(methodSymTable, methodNode, methodNode.IsStatic ? null : thisType);
					}

					break;
			}
		}
	}

	void VisitMethodBody(ScopedSymbolTable symTable, MethodDefinitionNode methodNode, TypeReference? thisType)
	{
		foreach (var statement in methodNode.Body)
		{
			VisitAndTagStmt(symTable, statement, thisType);
		}
	}

	void VisitFunctionBody(ScopedSymbolTable symTable, FunctionDefinitionNode functionNode)
	{
		foreach (var statement in functionNode.Body)
		{
			VisitAndTagStmt(symTable, statement, null);
		}
	}

	void VisitAndTagStmt(ScopedSymbolTable symTable, CodeStmt statement, TypeReference? thisType)
	{
		switch (statement)
		{
			case LocalDeclarationStmt localDeclStmt:
				TypeReference localType;

				if (localDeclStmt.Initializer is not null)
				{
					VisitAndTagExpr(symTable, localDeclStmt.Initializer, thisType);
				}

				if (localDeclStmt.IsInference)
				{
					localType = _exprTypeMap[localDeclStmt.Initializer!];
				}
				else
				{
					localType = ResolveTypeReference(symTable, localDeclStmt.DataType!, allowVoid: false);
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

			// NOTE: For all the below assign statements (AssignStmt...ModAssignStmt), we don't
			// actually know if we're allowed to assign to the LHS based off of the
			// DataProtection on the LHS, but this will be checked by the DataProtectionChecker
			// in the fifth phase after the complete Binder, which itself runs after this phase

			case AssignStmt stmt:
				VisitAndTagExpr(symTable, stmt.Value, thisType);

				switch (stmt.Target)
				{
					case IndexExpr indexExpr:
						// need to validate that @seti exists and the signature matches
						VisitAndTagExpr(symTable, indexExpr.Target, thisType);
						VisitAndTagExpr(symTable, indexExpr.Index, thisType);

						var indexableTargetType = _exprTypeMap[indexExpr.Target];

						// NOTE: we don't even know if indexableTargetType is resolved here
						// the variable could have been declared by inference, for example

						ValidateOperatorOverloadExists(indexableTargetType, "seti", 2, indexExpr.Location);

						// this eventually compiles down to something like:
						// indexable.@seti(index, value)
						break;
					case var target:
						VisitAndTagExpr(symTable, target, thisType);
						break;
				}

				break;
			case AddAssignStmt stmt:
				VisitOpAssignment(symTable, stmt.Target, stmt.Value, "add", stmt.Location, thisType);

				break;
			case SubAssignStmt stmt:
				VisitOpAssignment(symTable, stmt.Target, stmt.Value, "sub", stmt.Location, thisType);

				break;
			case MulAssignStmt stmt:
				VisitOpAssignment(symTable, stmt.Target, stmt.Value, "mul", stmt.Location, thisType);

				break;
			case DivAssignStmt stmt:
				VisitOpAssignment(symTable, stmt.Target, stmt.Value, "div", stmt.Location, thisType);
				break;
			case ModAssignStmt stmt:
				VisitOpAssignment(symTable, stmt.Target, stmt.Value, "mod", stmt.Location, thisType);

				break;
			case ReturnStmt stmt:
				if (stmt.Value is not null)
				{
					VisitAndTagExpr(symTable, stmt.Value, thisType);
				}

				break;
			case ForStmt stmt:
				var forScope = new ScopedSymbolTable(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

				if (stmt.Initializer is not null)
				{
					VisitAndTagStmt(forScope, stmt.Initializer, thisType);
				}

				if (stmt.Condition is not null)
				{
					VisitAndTagExpr(forScope, stmt.Condition, thisType);
				}

				if (stmt.Increment is not null)
				{
					VisitAndTagStmt(forScope, stmt.Increment, thisType);
				}

				foreach (var bodyStmt in stmt.Body)
				{
					VisitAndTagStmt(forScope, bodyStmt, thisType);
				}

				break;
			case WhileStmt stmt:
				var whileScope = new ScopedSymbolTable(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

				VisitAndTagExpr(symTable, stmt.Condition, thisType);

				foreach (var bodyStmt in stmt.Body)
				{
					VisitAndTagStmt(whileScope, bodyStmt, thisType);
				}

				break;
			case IfStmt stmt:
				var ifScope = new ScopedSymbolTable(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

				VisitAndTagExpr(symTable, stmt.Condition, thisType);

				foreach (var bodyStmt in stmt.ThenBody)
				{
					VisitAndTagStmt(ifScope, bodyStmt, thisType);
				}

				var elseScope = new ScopedSymbolTable(symTable, symTable.FileContext, symTable.NamespaceContext, symTable.TypeContext);

				foreach (var bodyStmt in stmt.ElseBody)
				{
					VisitAndTagStmt(elseScope, bodyStmt, thisType);
				}

				break;
			case ExprStmt stmt:
				VisitAndTagExpr(symTable, stmt.Expr, thisType);

				break;

		}
	}

	void VisitOpAssignment(ScopedSymbolTable symTable, Expr target, Expr value, string operatorSpecialName, TokenLocation location, TypeReference? thisType)
	{
		VisitAndTagExpr(symTable, value, thisType);

		switch (target)
		{
			case IndexExpr indexExpr:
				// need to validate that @geti, @seti exists and the signature matches
				VisitAndTagExpr(symTable, indexExpr.Target, thisType);
				VisitAndTagExpr(symTable, indexExpr.Index, thisType);

				var indexableTargetType = _exprTypeMap[indexExpr.Target];

				// NOTE: we don't even know if indexableTargetType is resolved here
				// the variable could have been declared by inference, for example

				ValidateOperatorOverloadExists(indexableTargetType, "geti", 1, indexExpr.Location);
				ValidateOperatorOverloadExists(indexableTargetType, "seti", 2, indexExpr.Location);

				// this eventually compiles down to something like:
				// let index = <index-expression>
				// let indexable = <indexed-expression>
				// let newValue = indexable.@geti(index).@add(<value-expression>)
				// indexable.@seti(index, newValue)

				// at this time, we are unable to see if a suitable @add exists,
				// because until we definitively bind @geti, we cannot be sure of
				// the type of its return value, which is the type that @add must
				// exist on. A later phase must resolve this after definitive binding.
				break;
			default:
				VisitAndTagExpr(symTable, target, thisType);

				var targetType = _exprTypeMap[target];

				// NOTE: we don't even know if targetType is resolved here
				// the variable could have been declared by inference, for example

				ValidateOperatorOverloadExists(targetType, operatorSpecialName, 1, location);

				// this eventually compiles down to something like
				// target = target.@add(value)
				// Since target must be either a DotExpr or NameReferenceExpr here, there
				// is no possibility of accidentally/wrongly re-evaluating an expression twice
				// the only assignment type that permits a dynamic expression on the LHS
				// is the indexed assignment type, but that is handled above.
				break;
		}
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
		else if (targetMethodMatches.All(targetMethod => targetMethod.Signature.Length != argcount))
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

	void HandleNamedExprSymbolMatch(Symbol? symbolMatch, ScopedSymbolTable symTable, Expr expr, TypeReferenceNode[] typeArguments)
	{
		switch (symbolMatch)
		{
			case FunctionOrMethodSymbol func:
				TypeReference[] typeArgs = [..typeArguments.Select(arg => ResolveTypeReference(symTable, arg))];

				var funcRef = new FunctionOrMethodReference(func, typeArgs);

				_resolvedNodeToReferenceMap[expr] = funcRef;
				_exprTypeMap[expr] = GetDelegateType(funcRef);

				break;
			case not null:
				var varRef = new VariableSymbolReference(symbolMatch);

				_resolvedNodeToReferenceMap[expr] = varRef;
				_exprTypeMap[expr] = varRef.DataType;

				break;
		}
	}

	void TagLiteralExpr(Expr expr, string literalType)
	{
		if (
			!_symbols.TryGetSymbol(literalType, expr.Location.File, out var literalTypeSymbol) ||
			literalTypeSymbol[0].Type != SymbolType.Type ||
			((TypeSymbol) literalTypeSymbol[0]).TypeArgumentNames.Length != 0)
		{
			_errors.Add(new CompilationError(
				$"Error QR310: Literal type '{literalType}' cannot be found.",
				expr.Location
			));

			_exprTypeMap[expr] = TypeReference.ErrorType;
		}
		else
		{
			var literalTypeRef = new TypeReference((TypeSymbol) literalTypeSymbol[0], [], []);

			_exprTypeMap[expr] = literalTypeRef;
		}
	}

	void VisitAndTagCallExpr(ScopedSymbolTable symTable, CallExpr callExpr, TypeReference? thisType)
	{
		// only a small subset of exprs are valid call targets, so we handle them here
		// additionally, info regarding the nature of the CallExpr is useful in evaluating some
		// exprs, which could be invalid if they aren't targets of a CallExpr

		switch (callExpr.Target)
		{
			// DerefExpr and IndexExpr can never have ambiguity because if they are valid targets
			// and resolve to function type, then they will always resolve to a concrete memory
			// address, rather than an ambiguous overload reference
			// For now, we'll just visit and tag the expr, and the type checker will ensure
			// that they actually resolve to a delegate or callable type
			case DerefExpr derefExpr:
				VisitAndTagExpr(symTable, derefExpr.Target, thisType);
				break;
			case IndexExpr indexExpr:
				VisitAndTagExpr(symTable, indexExpr.Target, thisType);
				break;

			// NameReferenceExpr and DotExpr can be ambiguous if they resolve to multiple overloads
			case NameReferenceExpr nameRefExpr:
				// then check for matches based on # of type parameters

				VisitAndTagExpr(symTable, nameRefExpr, thisType);
				break;
			case DotExpr dotExpr:
				// then check for matches based on # of type parameters
				// and then check if it is properly static/instance

				VisitAndTagExpr(symTable, dotExpr.Target, thisType);
				break;
		}
	}

	public void VisitAndTagExpr(ScopedSymbolTable symTable, Expr expr, TypeReference? thisType)
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

				if (thisType is null)
				{
					_errors.Add(new CompilationError(
						$"Error QR308: 'this' cannot be used in this context.",
						expr.Location)
					);
				}
				else
				{
					_exprTypeMap[expr] = thisType;
				}

				break;
			case BinaryOpExpr binOpExpr:
				VisitAndTagExpr(symTable, binOpExpr.Left, thisType);
				VisitAndTagExpr(symTable, binOpExpr.Right, thisType);

				var leftType = _exprTypeMap[binOpExpr.Left];

				// NOTE: We don't even know if leftType exists, because the type could
				// be ambiguous due to an inference, for example

				ValidateOperatorOverloadExists(leftType, binOpExpr.OperatorSpecialName, 1, binOpExpr.Location);

				break;

			// dot, ref, deref, and call may not be overloaded
			case CallExpr callExpr:
				VisitAndTagCallExpr(symTable, callExpr, thisType);
				break;
			case RefExpr refExpr:
				VisitAndTagExpr(symTable, refExpr.Target, thisType);
				break;
			case DerefExpr derefExpr:
				VisitAndTagExpr(symTable, derefExpr.Target, thisType);

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
				VisitAndTagExpr(symTable, dotExpr.Target, thisType);

				NamespaceReference? targetNamespaceRef = null;
				TypeReference? targetTypeRef = null;
				bool isNamespaceProperty = false;
				bool isInstanceProperty = false;

				if (_exprTypeMap.TryGetValue(dotExpr.Target, out TypeReference? value))
				{
					// instance attribute, as the Target is a valued expression and not a type reference

					targetTypeRef = value;
					isInstanceProperty = true;
				}
				else
				{
					if (_resolvedNodeToReferenceMap[dotExpr.Target] is NamespaceReference reference)
					{
						targetNamespaceRef = reference;
						isNamespaceProperty = true;
					}
					else
					{
						targetTypeRef = (TypeReference) _resolvedNodeToReferenceMap[dotExpr.Target];
						isInstanceProperty = false;
					}
				}

				Symbol? dotExprSymbolMatch = null;

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
							// this can never be the target of a call expression.
							foreach (var match in namespaceMemberMatches)
							{
								if (((FunctionOrMethodSymbol) match).TypeArgumentNames.Length == dotExpr.Property.TypeArguments.Length)
								{
									if (dotExprSymbolMatch != null)
									{
										_errors.Add(new CompilationError(
											$"Error QR315: Namespace '{targetNamespaceRef!.FullyQualifiedName}' has multiple members named '{dotExpr.Property.Name}'. The ambiguous reference cannot be resolved.",
											dotExpr.Location
										));

										_exprTypeMap[expr] = TypeReference.ErrorType;

										break;
									}

									dotExprSymbolMatch = match;
								}
							}

							if (dotExprSymbolMatch == null)
							{
								_errors.Add(new CompilationError(
									$"Error QR316: Namespace '{targetNamespaceRef!.FullyQualifiedName}' has no member named '{dotExpr.Property.Name}' that takes {dotExpr.Property.TypeArguments.Length} type arguments.",
									dotExpr.Location
								));

								_exprTypeMap[expr] = TypeReference.ErrorType;
							}
						}
						else
						{
							dotExprSymbolMatch = namespaceMemberMatches[0];
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
							// this can never be the target of a call expression.
							foreach (var match in propertySymbolMatches)
							{
								if (((FunctionOrMethodSymbol) match).TypeArgumentNames.Length == dotExpr.Property.TypeArguments.Length)
								{
									if (dotExprSymbolMatch != null)
									{
										_errors.Add(new CompilationError(
											$"Error QR315: Type '{targetTypeRef!.FullyQualifiedName}' has multiple members named '{dotExpr.Property.Name}'. The ambiguous reference cannot be resolved.",
											dotExpr.Location
										));

										_exprTypeMap[expr] = TypeReference.ErrorType;

										break;
									}

									dotExprSymbolMatch = match;
								}
							}

							if (dotExprSymbolMatch == null)
							{
								_errors.Add(new CompilationError(
									$"Error QR316: Type '{targetTypeRef!.FullyQualifiedName}' has no member named '{dotExpr.Property.Name}' that takes {dotExpr.Property.TypeArguments.Length} type arguments.",
									dotExpr.Location
								));

								_exprTypeMap[expr] = TypeReference.ErrorType;
							}
						}
						else
						{
							dotExprSymbolMatch = propertySymbolMatches[0];
						}

						if (dotExprSymbolMatch != null)
						{
							// actually... this could be a function...
							var varRef = new VariableSymbolReference(propertySymbolMatches[0]);
							_resolvedNodeToReferenceMap[dotExpr] = varRef;
							_exprTypeMap[dotExpr] = varRef.DataType;
						}
					}
				}

				HandleNamedExprSymbolMatch(dotExprSymbolMatch, symTable, dotExpr, dotExpr.Property.TypeArguments);

				break;
			case IndexExpr indexExpr: // requires method lookup and containing expr for @geti
				VisitAndTagExpr(symTable, indexExpr.Target, thisType);
				VisitAndTagExpr(symTable, indexExpr.Index, thisType);

				var indexableTargetType = _exprTypeMap[indexExpr.Target];

				// NOTE: we don't even know if indexableTargetType is resolved here
				// the variable could have been declared by inference, for example

				ValidateOperatorOverloadExists(indexableTargetType, "geti", 1, indexExpr.Location);
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
					Symbol? symbolMatch = null;

					if (symbolMatches.Length > 1)
					{
						// this can never be the target of a call expression.
						foreach (var match in symbolMatches)
						{
							if (((FunctionOrMethodSymbol) match).TypeArgumentNames.Length == nameReferenceExpr.TypeArguments.Length)
							{
								if (symbolMatch != null)
								{
									_errors.Add(new CompilationError(
										$"Error QR315: There are multiple symbols named '{nameReferenceExpr.Name}'. The ambiguous reference cannot be resolved.",
										nameReferenceExpr.Location
									));

									_exprTypeMap[expr] = TypeReference.ErrorType;

									break;
								}

								symbolMatch = match;
							}
						}

						if (symbolMatch == null)
						{
							_errors.Add(new CompilationError(
								$"Error QR316: There are no symbols named named '{nameReferenceExpr.Name}' that take {nameReferenceExpr.TypeArguments.Length} type arguments.",
								nameReferenceExpr.Location
							));

							_exprTypeMap[expr] = TypeReference.ErrorType;
						}
					}
					else
					{
						symbolMatch = symbolMatches[0];
					}

					HandleNamedExprSymbolMatch(symbolMatch, symTable, nameReferenceExpr, nameReferenceExpr.TypeArguments);
				}
				break;
			case UnaryOpExpr unaryOpExpr:
				VisitAndTagExpr(symTable, unaryOpExpr.Target, thisType);

				// requires method lookup

				var unaryOpTargetType = _exprTypeMap[unaryOpExpr.Target];

				// NOTE: We don't even know if unaryOpTargetType exists, because the type could
				// be ambiguous due to an inference, for example

				ValidateOperatorOverloadExists(unaryOpTargetType, unaryOpExpr.OperatorSpecialName, 0, unaryOpExpr.Location);
				break;
			default:
				throw new NotImplementedException($"SymbolChecker.VisitAndTagExpr: Unhandled expression type {expr.GetType().Name}");
		}
	}
}