using System.Text.Json;
using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Parser.Nodes.CodeStatements;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Tests.Parser;

public class ExpressionParsingTests
{
	#region Literal Expression Tests

	[Theory]
	[InlineData("0")]
	[InlineData("42")]
	[InlineData("999999")]
	// [InlineData("0x1A")]
	// [InlineData("0xFF")]
	public void Parse_IntegerLiterals_ReturnsIntegerLiteralExpr(string value)
	{
		var code = $$"""
		main()
		{
			x = {{value}};
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<IntegerLiteralExpr>(stmt.Value);

		Assert.Equal(value, expr.Value);
	}

	[Theory]
	[InlineData("1.0")]
	[InlineData("3.14")]
	[InlineData("0.5")]
	// [InlineData("1e10")]
	// [InlineData("1.5e-3")]
	public void Parse_FloatLiterals_ReturnsFloatLiteralExpr(string value)
	{
		var code = $$"""
		main()
		{
			x = {{value}};
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<FloatLiteralExpr>(stmt.Value);

		Assert.Equal(value, expr.Value);
	}

	[Theory]
	[InlineData("\"hello\"")]
	[InlineData("\"\"")]
	[InlineData("\"with spaces\"")]
	[InlineData("\"newline\\n\"")]
	public void Parse_StringLiterals_ReturnsStringLiteralExpr(string value)
	{
		var code = $$"""
		main()
		{
			x = {{value}};
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<StringLiteralExpr>(stmt.Value);

		Assert.Equal(JsonDocument.Parse(value).RootElement.GetString(), expr.Value);
	}

	[Theory]
	[InlineData('a')]
	[InlineData('0')]
	[InlineData(' ')]
	public void Parse_CharLiterals_ReturnsCharLiteralExpr(char value)
	{
		var code = $$"""
		main()
		{
			x = '{{value}}';
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<CharLiteralExpr>(stmt.Value);

		Assert.Equal(value.ToString(), expr.Value);
	}

	[Theory]
	[InlineData("true")]
	[InlineData("false")]
	public void Parse_BooleanLiterals_ReturnsBooleanLiteralExpr(string value)
	{
		var code = $$"""
		main()
		{
			x = {{value}};
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<BooleanLiteralExpr>(stmt.Value);

		Assert.Equal(value, expr.Value);
	}

	#endregion

	#region Identifier and Reference Tests

	[Fact]
	public void Parse_SimpleIdentifier_ReturnsNameReferenceExpr()
	{
		var code = """
		main()
		{
			x = myVar;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<NameReferenceExpr>(stmt.Value);

		Assert.Equal("myVar", expr.Name);
		Assert.Empty(expr.TypeArguments);
	}

	[Fact]
	public void Parse_IdentifierWithTypeParameter_ReturnsNameReferenceExpr()
	{
		var code = """
		main()
		{
			x = Vec<i32>;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<NameReferenceExpr>(stmt.Value);

		Assert.Equal("Vec", expr.Name);
		Assert.Single(expr.TypeArguments);
	}

	[Fact]
	public void Parse_IdentifierWithMultipleTypeParameters_ReturnsNameReferenceExpr()
	{
		var code = """
		main()
		{
			x = Map<string, i32>;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<NameReferenceExpr>(stmt.Value);

		Assert.Equal("Map", expr.Name);
		Assert.Equal(2, expr.TypeArguments.Length);
	}

	[Fact]
	public void Parse_ThisKeyword_ReturnsThisExpr()
	{
		var code = """
		main()
		{
			x = this;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = stmt.Value;

		Assert.IsType<ThisExpr>(expr);
	}

	#endregion

	#region Arithmetic Operator Tests

	[Fact]
	public void Parse_AdditionExpression_ReturnsAddExpr()
	{
		var code = """
		main()
		{
			x = 10 + 20;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<AddExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_SubtractionExpression_ReturnsSubExpr()
	{
		var code = """
		main()
		{
			x = 30 - 15;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<SubExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_MultiplicationExpression_ReturnsMulExpr()
	{
		var code = """
		main()
		{
			x = 5 * 6;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<MulExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_DivisionExpression_ReturnsDivExpr()
	{
		var code = """
		main()
		{
			x = 100 / 10;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<DivExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_ModuloExpression_ReturnsModExpr()
	{
		var code = """
		main()
		{
			x = 17 % 5;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<ModExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_ArithmeticPrecedence_MultiplicationBeforeAddition()
	{
		var code = """
		main()
		{
			x = 2 + 3 * 4;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<AddExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<MulExpr>(expr.Right);
	}

	#endregion

	#region Bitwise Operator Tests

	[Fact]
	public void Parse_BitwiseAndExpression_ReturnsBitwiseAndExpr()
	{
		var code = """
		main()
		{
			x = 12 & 10;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<BitwiseAndExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_BitwiseOrExpression_ReturnsBitwiseOrExpr()
	{
		var code = """
		main()
		{
			x = 12 | 10;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<BitwiseOrExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_BitwiseXorExpression_ReturnsBitwiseXorExpr()
	{
		var code = """
		main()
		{
			x = 12 ^ 10;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<BitwiseXorExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_LeftShiftExpression_ReturnsLeftShiftExpr()
	{
		var code = """
		main()
		{
			x = 8 << 2;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LeftShiftExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_RightShiftExpression_ReturnsRightShiftExpr()
	{
		var code = """
		main()
		{
			x = 8 >> 2;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<RightShiftExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	#endregion

	#region Logical Operator Tests

	[Fact]
	public void Parse_LogicalAndExpression_ReturnsLogicalAndExpr()
	{
		var code = """
		main()
		{
			x = true && false;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LogicalAndExpr>(stmt.Value);
	}

	[Fact]
	public void Parse_LogicalOrExpression_ReturnsLogicalOrExpr()
	{
		var code = """
		main()
		{
			x = true || false;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LogicalOrExpr>(stmt.Value);
	}

	#endregion

	#region Comparison Operator Tests

	[Fact]
	public void Parse_EqualityExpression_ReturnsEqualExpr()
	{
		var code = """
		main()
		{
			x = 10 == 10;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<EqualExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_InequalityExpression_ReturnsNotEqualExpr()
	{
		var code = """
		main()
		{
			x = 10 != 20;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<NotEqualExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_LessThanExpression_ReturnsLessThanExpr()
	{
		var code = """
		main()
		{
			x = 5 < 10;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LessThanExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_LessThanEqualExpression_ReturnsLessThanEqualExpr()
	{
		var code = """
		main()
		{
			x = 5 <= 10;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LessThanEqualExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_GreaterThanExpression_ReturnsGreaterThanExpr()
	{
		var code = """
		main()
		{
			x = 10 > 5;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<GreaterThanExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	[Fact]
	public void Parse_GreaterThanEqualExpression_ReturnsGreaterThanEqualExpr()
	{
		var code = """
		main()
		{
			x = 10 >= 5;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<GreaterThanEqualExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	#endregion

	#region Unary Operator Tests

	[Fact]
	public void Parse_UnaryPlusExpression_ReturnsUnaryPlusExpr()
	{
		var code = """
		main()
		{
			x = +42;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<UnaryPlusExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Target);
	}

	[Fact]
	public void Parse_UnaryMinusExpression_ReturnsUnaryMinusExpr()
	{
		var code = """
		main()
		{
			x = -42;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<UnaryMinusExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Target);
	}

	[Fact]
	public void Parse_LogicalNotExpression_ReturnsLogicalNotExpr()
	{
		var code = """
		main()
		{
			x = !true;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LogicalNotExpr>(stmt.Value);
	}

	[Fact]
	public void Parse_BitwiseNotExpression_ReturnsBitwiseNotExpr()
	{
		var code = """
		main()
		{
			x = ~42;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<BitwiseNotExpr>(stmt.Value);

		Assert.IsType<IntegerLiteralExpr>(expr.Target);
	}

	[Fact]
	public void Parse_PreIncrementExpression_ReturnsPreIncrementExpr()
	{
		var code = """
		main()
		{
			++x;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<PreIncrementExpr>(stmt.Expr);

		Assert.IsType<NameReferenceExpr>(expr.Target);
	}

	[Fact]
	public void Parse_PreDecrementExpression_ReturnsPreDecrementExpr()
	{
		var code = """
		main()
		{
			--x;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<PreDecrementExpr>(stmt.Expr);

		Assert.IsType<NameReferenceExpr>(expr.Target);
	}

	[Fact]
	public void Parse_PostIncrementExpression_ReturnsPostIncrementExpr()
	{
		var code = """
		main()
		{
			x++;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<PostIncrementExpr>(stmt.Expr);

		Assert.IsType<NameReferenceExpr>(expr.Target);
	}

	[Fact]
	public void Parse_PostDecrementExpression_ReturnsPostDecrementExpr()
	{
		var code = """
		main()
		{
			x--;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<PostDecrementExpr>(stmt.Expr);

		Assert.IsType<NameReferenceExpr>(expr.Target);
	}

	[Fact]
	public void Parse_ReferenceExpression_ReturnsRefExpr()
	{
		var code = """
		main()
		{
			ptr = &x;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<RefExpr>(stmt.Value);

		Assert.IsType<NameReferenceExpr>(expr.Target);
	}

	[Fact]
	public void Parse_DereferenceExpression_ReturnsDerefExpr()
	{
		var code = """
		main()
		{
			x = *ptr;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<DerefExpr>(stmt.Value);

		Assert.IsType<NameReferenceExpr>(expr.Target);
	}

	#endregion

	#region Member Access Tests

	[Fact]
	public void Parse_SimpleMemberAccess_ReturnsDotExpr()
	{
		var code = """
		main()
		{
			x = obj.some_field;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<DotExpr>(stmt.Value);

		Assert.Equal("some_field", expr.PropertyName);
		Assert.IsType<NameReferenceExpr>(expr.Target);
		Assert.Empty(expr.TypeArguments);
	}

	[Fact]
	public void Parse_MemberAccessWithTypeArgument_ReturnsDotExprWithTypeParameters()
	{
		var code = """
		main()
		{
			x = obj.Method<int>;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<DotExpr>(stmt.Value);

		Assert.Equal("Method", expr.PropertyName);
		Assert.Single(expr.TypeArguments);
	}

	[Fact]
	public void Parse_ChainedMemberAccess_ReturnsNestedDotExpr()
	{
		var code = """
		main()
		{
			x = obj.field1.field2;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<DotExpr>(stmt.Value);

		Assert.Equal("field2", expr.PropertyName);
		Assert.IsType<DotExpr>(expr.Target);

		var innerDot = Assert.IsType<DotExpr>(expr.Target);
		Assert.Equal("field1", innerDot.PropertyName);
	}

	#endregion

	#region Indexing Tests

	[Fact]
	public void Parse_IndexExpression_ReturnsIndexExpr()
	{
		var code = """
		main()
		{
			x = arr[0];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<IndexExpr>(stmt.Value);

		Assert.IsType<NameReferenceExpr>(expr.Target);
		Assert.IsType<IntegerLiteralExpr>(expr.Index);
	}

	[Fact]
	public void Parse_IndexExpressionWithIdentifier_ReturnsIndexExpr()
	{
		var code = """
		main()
		{
			x = arr[i];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<IndexExpr>(stmt.Value);

		Assert.IsType<NameReferenceExpr>(expr.Index);
	}

	[Fact]
	public void Parse_ChainedIndexing_ReturnsNestedIndexExpr()
	{
		var code = """
		main()
		{
			x = matrix[i][j];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<IndexExpr>(stmt.Value);

		Assert.IsType<IndexExpr>(expr.Target);
	}

	#endregion

	#region Function Call Tests

	[Fact]
	public void Parse_FunctionCallNoArguments_ReturnsCallExpr()
	{
		var code = """
		main()
		{
			func();
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<CallExpr>(stmt.Expr);

		Assert.IsType<NameReferenceExpr>(expr.Target);
		Assert.Empty(expr.Arguments);
	}

	[Fact]
	public void Parse_FunctionCallWithArgument_ReturnsCallExprWithArgs()
	{
		var code = """
		main()
		{
			func(42);
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<CallExpr>(stmt.Expr);

		Assert.Single(expr.Arguments);
		Assert.IsType<IntegerLiteralExpr>(expr.Arguments[0]);
	}

	[Fact]
	public void Parse_FunctionCallWithMultipleArguments_ReturnsCallExprWithArgs()
	{
		var code = """
		main()
		{
			func(1, 2, 3);
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<CallExpr>(stmt.Expr);

		Assert.Equal(3, expr.Arguments.Length);
	}

	[Fact]
	public void Parse_MemberFunctionCall_ReturnsCallExprWithDotTarget()
	{
		var code = """
		main()
		{
			obj.method();
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<CallExpr>(stmt.Expr);

		Assert.IsType<DotExpr>(expr.Target);
	}

	[Fact]
	public void Parse_FunctionCallWithComplexArguments_ParsesCorrectly()
	{
		var code = """
		main()
		{
			func(a + b, c * d);
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<CallExpr>(stmt.Expr);

		Assert.Equal(2, expr.Arguments.Length);
		Assert.IsType<AddExpr>(expr.Arguments[0]);
		Assert.IsType<MulExpr>(expr.Arguments[1]);
	}

	#endregion

	#region Operator Precedence Tests

	[Fact]
	public void Parse_AdditionAndMultiplication_RespectsPrecedence()
	{
		var code = """
		main()
		{
			x = 2 + 3 * 4;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<AddExpr>(stmt.Value);

		Assert.IsType<MulExpr>(expr.Right);
	}

	[Fact]
	public void Parse_BitwiseAndComparisonPrecedence()
	{
		var code = """
		main()
		{
			x = a < b & c > d;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<BitwiseAndExpr>(stmt.Value);

		Assert.IsType<LessThanExpr>(expr.Left);
		Assert.IsType<GreaterThanExpr>(expr.Right);
	}

	[Fact]
	public void Parse_LogicalOperatorsPrecedence()
	{
		var code = """
		main()
		{
			x = a && b || c;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LogicalOrExpr>(stmt.Value);

		Assert.IsType<LogicalAndExpr>(expr.Left);
	}

	[Fact]
	public void Parse_ComplexExpression_ParsesPrecedenceCorrectly()
	{
		var code = """
		main()
		{
			x = a + b * c - d / e;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
	}

	[Fact]
	public void Parse_UnaryWithBinaryPrecedence()
	{
		var code = """
		main()
		{
			x = -a * b;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<MulExpr>(stmt.Value);

		Assert.IsType<UnaryMinusExpr>(expr.Left);
	}

	#endregion

	#region Parenthesized Expression Tests

	[Fact]
	public void Parse_ParenthesizedExpression_ParsesCorrectly()
	{
		var code = """
		main()
		{
			x = (42);
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());

		Assert.IsType<IntegerLiteralExpr>(stmt.Value);
	}

	[Fact]
	public void Parse_ParenthesizedToOverridePrecedence()
	{
		var code = """
		main()
		{
			x = (2 + 3) * 4;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<MulExpr>(stmt.Value);

		Assert.IsType<AddExpr>(expr.Left);
		Assert.IsType<IntegerLiteralExpr>(expr.Right);
	}

	#endregion

	#region Mixed Expression Tests

	[Fact]
	public void Parse_MemberAccessAndCall_ParsesCorrectly()
	{
		var code = """
		main()
		{
			obj.method(arg1, arg2);
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var expr = Assert.IsType<CallExpr>(stmt.Expr);

		Assert.IsType<DotExpr>(expr.Target);
		Assert.Equal(2, expr.Arguments.Length);
	}

	[Fact]
	public void Parse_IndexingAndMemberAccess_ParsesCorrectly()
	{
		var code = """
		main()
		{
			x = arr[0].some_field;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<DotExpr>(stmt.Value);

		Assert.IsType<IndexExpr>(expr.Target);
	}

	[Fact]
	public void Parse_CallAndIndexing_ParsesCorrectly()
	{
		var code = """
		main()
		{
			x = func()[0];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<IndexExpr>(stmt.Value);

		Assert.IsType<CallExpr>(expr.Target);
	}

	[Fact]
	public void Parse_ReferenceAndDereference_ParsesCorrectly()
	{
		var code = """
		main()
		{
			x = *&x;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<DerefExpr>(stmt.Value);

		Assert.IsType<RefExpr>(expr.Target);
	}

	#endregion

	#region Error Recovery Tests

	[Fact]
	public void Parse_MissingOperand_GeneratesError()
	{
		var code = """
		main()
		{
			10 +;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.NotEmpty(errors);
	}

	[Fact]
	public void Parse_UnmatchedParenthesis_GeneratesError()
	{
		var code = """
		main()
		{
			(10 + 20;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.NotEmpty(errors);
	}

	[Fact]
	public void Parse_InvalidOperator_GeneratesError()
	{
		var code = """
		main()
		{
			10 @@ 20;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.NotEmpty(errors);
	}

	#endregion

	#region Type Parameter Tests

	[Fact]
	public void Parse_NestedTypeParameters_ParsesCorrectly()
	{
		var code = """
		main()
		{
			x = Dict<string, Vec<int>>;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<NameReferenceExpr>(stmt.Value);

		Assert.Equal("Dict", expr.Name);
		Assert.Equal(2, expr.TypeArguments.Length);
	}

	[Fact]
	public void Parse_MemberAccessWithNestedTypeParams_ParsesCorrectly()
	{
		var code = """
		main()
		{
			x = Core.Array.Alloc<Vec<int>>;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<DotExpr>(stmt.Value);

		Assert.Equal("Alloc", expr.PropertyName);
		Assert.Single(expr.TypeArguments);
	}

	#endregion

	#region Edge Case and Complex Expression Tests

	[Fact]
	public void Parse_ParenthesesOverridePrecedence_ReturnsCorrectStructure()
	{
		var code = """
		main()
		{
			x = (a + b) * c;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<MulExpr>(stmt.Value);

		Assert.IsType<AddExpr>(expr.Left);
	}

	[Fact]
	public void Parse_ChainedUnaryOperators_ParsesCorrectly()
	{
		var code = """
		main()
		{
			x = -+5;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<UnaryMinusExpr>(stmt.Value);

		Assert.IsType<UnaryPlusExpr>(expr.Target);
	}

	[Fact]
	public void Parse_ComplexMixedOperatorPrecedence_RespectsPrecedence()
	{
		var code = """
		main()
		{
			x = a + b * c - d / e;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<SubExpr>(stmt.Value);

		var addExpr = Assert.IsType<AddExpr>(expr.Left);
		Assert.IsType<MulExpr>(addExpr.Right);
		Assert.IsType<DivExpr>(expr.Right);
	}

	[Fact]
	public void Parse_BitwiseWithLogicalOperators_RespectsPrecedence()
	{
		var code = """
		main()
		{
			x = a | b & c || d;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LogicalOrExpr>(stmt.Value);

		Assert.IsType<BitwiseOrExpr>(expr.Left);
	}

	[Fact]
	public void Parse_ShiftWithComparisonOperators_RespectsPrecedence()
	{
		var code = """
		main()
		{
			x = a << 2 > b >> 1;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<GreaterThanExpr>(stmt.Value);

		Assert.IsType<LeftShiftExpr>(expr.Left);
		Assert.IsType<RightShiftExpr>(expr.Right);
	}

	[Fact]
	public void Parse_DoubleNegation_ParsesCorrectly()
	{
		var code = """
		main()
		{
			x = !!true;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LogicalNotExpr>(stmt.Value);

		Assert.IsType<LogicalNotExpr>(expr.Target);
	}

	[Fact]
	public void Parse_UnaryOperatorWithBinaryExpression_RespectsPrecedence()
	{
		var code = """
		main()
		{
			x = -a + b;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<AddExpr>(stmt.Value);

		Assert.IsType<UnaryMinusExpr>(expr.Left);
	}

	[Fact]
	public void Parse_MultipleComparisons_ParsesAsChained()
	{
		var code = """
		main()
		{
			x = a < b && b < c;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<LogicalAndExpr>(stmt.Value);

		Assert.IsType<LessThanExpr>(expr.Left);
		Assert.IsType<LessThanExpr>(expr.Right);
	}

	[Fact]
	public void Parse_BitwiseXorWithEqualityOperators_RespectsPrecedence()
	{
		var code = """
		main()
		{
			x = a == b ^ c != d;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var expr = Assert.IsType<BitwiseXorExpr>(stmt.Value);

		Assert.IsType<EqualExpr>(expr.Left);
		Assert.IsType<NotEqualExpr>(expr.Right);
	}

	#endregion
}
