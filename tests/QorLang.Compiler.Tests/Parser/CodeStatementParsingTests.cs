using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser.Nodes;
using QorLang.Compiler.Parser.Nodes.CodeStatements;
using QorLang.Compiler.Parser.Nodes.Expressions;

namespace QorLang.Compiler.Tests.Parser;

public class CodeStatementParsingTests
{
	#region Expression Statement Tests

	[Fact]
	public void Parse_FunctionCallAsStatement_ReturnsExprStmt()
	{
		var code = """
		main()
		{
			some_func();
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var callExpr = Assert.IsType<CallExpr>(stmt.Expr);
		var targetExpr = Assert.IsType<NameReferenceExpr>(callExpr.Target);
		Assert.Equal("some_func", targetExpr.Name);
		Assert.Empty(callExpr.Arguments);
	}

	[Fact]
	public void Parse_FunctionCallWithArguments_ReturnsExprStmtWithCallExpr()
	{
		var code = """
		main()
		{
			some_func(42, 3.14, "hello");
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var callExpr = Assert.IsType<CallExpr>(stmt.Expr);
		
		Assert.Equal(3, callExpr.Arguments.Length);
		Assert.IsType<IntegerLiteralExpr>(callExpr.Arguments[0]);
		Assert.IsType<FloatLiteralExpr>(callExpr.Arguments[1]);
		Assert.IsType<StringLiteralExpr>(callExpr.Arguments[2]);
	}

	[Fact]
	public void Parse_MethodCallAsStatement_ReturnsExprStmt()
	{
		var code = """
		main()
		{
			obj.some_method();
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var callExpr = Assert.IsType<CallExpr>(stmt.Expr);
		var dotExpr = Assert.IsType<DotExpr>(callExpr.Target);
		Assert.Equal("some_method", dotExpr.PropertyName);
	}

	[Fact]
	public void Parse_PostIncrementAsStatement_ReturnsExprStmt()
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
		var incExpr = Assert.IsType<PostIncrementExpr>(stmt.Expr);
		var target = Assert.IsType<NameReferenceExpr>(incExpr.Target);
		Assert.Equal("x", target.Name);
	}

	[Fact]
	public void Parse_PostDecrementAsStatement_ReturnsExprStmt()
	{
		var code = """
		main()
		{
			counter--;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var decExpr = Assert.IsType<PostDecrementExpr>(stmt.Expr);
		var target = Assert.IsType<NameReferenceExpr>(decExpr.Target);
		Assert.Equal("counter", target.Name);
	}

	[Fact]
	public void Parse_PreIncrementAsStatement_ReturnsExprStmt()
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
		var incExpr = Assert.IsType<PreIncrementExpr>(stmt.Expr);
		var target = Assert.IsType<NameReferenceExpr>(incExpr.Target);
		Assert.Equal("x", target.Name);
	}

	[Fact]
	public void Parse_PreDecrementAsStatement_ReturnsExprStmt()
	{
		var code = """
		main()
		{
			--y;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ExprStmt>(funcNode.Body.First());
		var decExpr = Assert.IsType<PreDecrementExpr>(stmt.Expr);
		var target = Assert.IsType<NameReferenceExpr>(decExpr.Target);
		Assert.Equal("y", target.Name);
	}

	#endregion

	#region Assignment Statement Tests

	[Fact]
	public void Parse_SimpleAssignment_ReturnsAssignmentStmt()
	{
		var code = """
		main()
		{
			x = 42;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var target = Assert.IsType<NameReferenceExpr>(stmt.Target);
		var value = Assert.IsType<IntegerLiteralExpr>(stmt.Value);
		Assert.Equal("x", target.Name);
		Assert.Equal("42", value.Value);
	}

	[Fact]
	public void Parse_AssignmentWithExpression_ReturnsAssignmentStmt()
	{
		var code = """
		main()
		{
			result = a + b;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var target = Assert.IsType<NameReferenceExpr>(stmt.Target);
		Assert.IsType<AddExpr>(stmt.Value);
		Assert.Equal("result", target.Name);
	}

	[Fact]
	public void Parse_MemberAssignment_ReturnsAssignmentStmt()
	{
		var code = """
		main()
		{
			obj.some_field = 100;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var target = Assert.IsType<DotExpr>(stmt.Target);
		Assert.Equal("some_field", target.PropertyName);
		var value = Assert.IsType<IntegerLiteralExpr>(stmt.Value);
		Assert.Equal("100", value.Value);
	}

	[Fact]
	public void Parse_IndexAssignment_ReturnsAssignmentStmt()
	{
		var code = """
		main()
		{
			arr[0] = 99;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var target = Assert.IsType<IndexExpr>(stmt.Target);
		var indexValue = Assert.IsType<IntegerLiteralExpr>(target.Index);
		Assert.Equal("0", indexValue.Value);
	}

	[Fact]
	public void Parse_ChainedMemberAssignment_ReturnsAssignmentStmt()
	{
		var code = """
		main()
		{
			obj.nested.some_field = 50;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<AssignmentStmt>(funcNode.Body.First());
		var target = Assert.IsType<DotExpr>(stmt.Target);
		Assert.Equal("some_field", target.PropertyName);
		var baseObj = Assert.IsType<DotExpr>(target.Target);
		Assert.Equal("nested", baseObj.PropertyName);
	}

	#endregion

	#region Local Variable Declaration Tests

	[Fact]
	public void Parse_SimpleLocalDeclaration_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let x: i32;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());
		Assert.Equal("x", stmt.Name);
		Assert.Equal("Qor.Int32", stmt.DataType.TypeName);
		Assert.Null(stmt.Initializer);
	}

	[Fact]
	public void Parse_LocalDeclarationWithInitializer_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let x: i32 = 42;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());
		Assert.Equal("x", stmt.Name);
		Assert.Equal("Qor.Int32", stmt.DataType.TypeName);
		var initValue = Assert.IsType<IntegerLiteralExpr>(stmt.Initializer);
		Assert.Equal("42", initValue.Value);
	}

	[Fact]
	public void Parse_LocalDeclarationWithExpressionInitializer_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let result: i32 = a + b * 2;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());
		Assert.Equal("result", stmt.Name);
		Assert.IsType<AddExpr>(stmt.Initializer);
	}

	[Fact]
	public void Parse_LocalDeclarationWithRefType_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let ptr: i32&;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());

		Assert.Equal("ptr", stmt.Name);
		Assert.Equal("Qor.Int32", stmt.DataType.TypeName);
		Assert.Single(stmt.DataType.IndirectionLayers);
	}

	[Fact]
	public void Parse_MultipleLocalDeclarations_ReturnsMultipleStmts()
	{
		var code = """
		main()
		{
			let x: i32 = 10;
			let y: f64 = 3.14;
			let z: string = "hello";
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		Assert.Equal(3, funcNode.Body.Length);
		
		var stmt1 = Assert.IsType<LocalDeclarationStmt>(funcNode.Body[0]);
		Assert.Equal("x", stmt1.Name);
		
		var stmt2 = Assert.IsType<LocalDeclarationStmt>(funcNode.Body[1]);
		Assert.Equal("y", stmt2.Name);
		
		var stmt3 = Assert.IsType<LocalDeclarationStmt>(funcNode.Body[2]);
		Assert.Equal("z", stmt3.Name);
	}

	[Fact]
	public void Parse_LocalDeclarationWithDataProtection_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let ptr: i32& $[rw];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());
		Assert.Equal("ptr", stmt.Name);
		Assert.NotEmpty(stmt.Protections);
	}

	#endregion

	#region Return Statement Tests

	[Fact]
	public void Parse_ReturnWithoutValue_ReturnsReturnStmt()
	{
		var code = """
		main()
		{
			return;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ReturnStmt>(funcNode.Body.First());
		Assert.Null(stmt.Value);
	}

	[Fact]
	public void Parse_ReturnWithLiteralValue_ReturnsReturnStmt()
	{
		var code = """
		main()
		{
			return 42;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ReturnStmt>(funcNode.Body.First());
		var value = Assert.IsType<IntegerLiteralExpr>(stmt.Value);
		Assert.Equal("42", value.Value);
	}

	[Fact]
	public void Parse_ReturnWithExpression_ReturnsReturnStmt()
	{
		var code = """
		main()
		{
			return a + b * c;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ReturnStmt>(funcNode.Body.First());
		Assert.NotNull(stmt.Value);
		var expr_value = Assert.IsType<AddExpr>(stmt.Value);
		Assert.NotNull(expr_value);
	}

	[Fact]
	public void Parse_ReturnWithVariable_ReturnsReturnStmt()
	{
		var code = """
		main()
		{
			let result: i32 = 100;
			
			return result;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		Assert.Equal(2, funcNode.Body.Length);
		var return_stmt = Assert.IsType<ReturnStmt>(funcNode.Body[1]);
		var value = Assert.IsType<NameReferenceExpr>(return_stmt.Value);
		Assert.Equal("result", value.Name);
	}

	[Fact]
	public void Parse_UnreachableCode_ReturnsError()
	{
		var code = """
		main()
		{
			return 1;
			x = 2;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		// Should get unreachable code error
		Assert.NotEmpty(errors);
		Assert.Contains("QR215", errors.First().Message);
	}

	#endregion

	#region Break/Continue Statement Tests

	[Fact]
	public void Parse_BreakStatementOutsideLoop_ReturnsError()
	{
		var code = """
		main()
		{
			break;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.NotEmpty(errors);
		Assert.Contains("QR216", errors.First().Message);
	}

	[Fact]
	public void Parse_ContinueStatementOutsideLoop_ReturnsError()
	{
		var code = """
		main()
		{
			continue;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.NotEmpty(errors);
		Assert.Contains("QR217", errors.First().Message);
	}

	#endregion

	#region Mixed Statement Tests

	[Fact]
	public void Parse_MixedStatements_ReturnsCorrectStatementTypes()
	{
		var code = """
		main()
		{
			let x: i32 = 10;
			y = x + 5;
			x++;
			some_func();
			return y;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		Assert.Equal(5, funcNode.Body.Length);
		
		Assert.IsType<LocalDeclarationStmt>(funcNode.Body[0]);
		Assert.IsType<AssignmentStmt>(funcNode.Body[1]);
		Assert.IsType<ExprStmt>(funcNode.Body[2]);
		Assert.IsType<ExprStmt>(funcNode.Body[3]);
		Assert.IsType<ReturnStmt>(funcNode.Body[4]);
	}

	[Fact]
	public void Parse_DeclarationsAndAssignments_ReturnsCorrectSequence()
	{
		var code = """
		main()
		{
			let a: i32 = 1;
			let b: i32 = 2;
			c = a + b;
			a = 10;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		Assert.Equal(4, funcNode.Body.Length);
		
		var decl1 = Assert.IsType<LocalDeclarationStmt>(funcNode.Body[0]);
		Assert.Equal("a", decl1.Name);
		
		var decl2 = Assert.IsType<LocalDeclarationStmt>(funcNode.Body[1]);
		Assert.Equal("b", decl2.Name);
		
		Assert.IsType<AssignmentStmt>(funcNode.Body[2]);
		Assert.IsType<AssignmentStmt>(funcNode.Body[3]);
	}

	#endregion

	#region Error Cases

	[Fact]
	public void Parse_AssignmentToLiteral_ReturnsError()
	{
		var code = """
		main()
		{
			42 = x;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.NotEmpty(errors);
		Assert.Contains("QR203", errors.First().Message);
	}

	[Fact]
	public void Parse_LocalDeclarationMissingType_ReturnsError()
	{
		var code = """
		main()
		{
			x;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.NotEmpty(errors);
	}

	[Fact]
	public void Parse_StatementMissingSemicolon_ReturnsError()
	{
		var code = """
		main()
		{
			x = 42
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.NotEmpty(errors);
		Assert.Contains("QR202", errors.First().Message);
	}

	#endregion

	#region Protection Tests for Local Declarations

	[Fact]
	public void Parse_LocalDeclarationWithReadWriteProtection_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let ptr: i32& $[*rw];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());
		Assert.Equal("ptr", stmt.Name);
		Assert.Equal("Qor.Int32", stmt.DataType.TypeName);
		Assert.Single(stmt.DataType.IndirectionLayers, IndirectionLayer.PointerTo);
		Assert.Equal(DataProtection.ReadOnly, stmt.Protections[0]);
		Assert.Equal(DataProtection.ReadWrite, stmt.Protections[1]);
	}

	[Fact]
	public void Parse_LocalDeclarationArrayWithProtection_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let arr: i32[] $[rw];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());
		Assert.Equal("arr", stmt.Name);
		Assert.NotEmpty(stmt.Protections);
	}

	[Fact]
	public void Parse_LocalDeclarationPointerToArrayWithProtection_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let ptr_arr: i32&[] $[*rw];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());
		Assert.Equal("ptr_arr", stmt.Name);
		Assert.Equal("Qor.Int32", stmt.DataType.TypeName);
		Assert.Equal(IndirectionLayer.PointerTo, stmt.DataType.IndirectionLayers[0]);
		Assert.Equal(IndirectionLayer.ArrayOf, stmt.DataType.IndirectionLayers[1]);
		Assert.Equal(DataProtection.ReadOnly, stmt.Protections[0]);
		Assert.Equal(DataProtection.ReadWrite, stmt.Protections[1]);
	}

	[Fact]
	public void Parse_LocalDeclarationWithInitializerAndProtection_ReturnsLocalDeclarationStmt()
	{
		var code = """
		main()
		{
			let ptr: i32& $[rw] = other_ptr;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<LocalDeclarationStmt>(funcNode.Body.First());
		Assert.Equal("ptr", stmt.Name);
		Assert.NotEmpty(stmt.Protections);
		Assert.NotNull(stmt.Initializer);
	}

	#endregion

	#region Control Flow Tests - If Statements

	[Fact]
	public void Parse_SimpleIfStatement_ReturnsIfStmt()
	{
		var code = """
		main()
		{
			if (x > 0)
			{
				y = 1;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<IfStmt>(funcNode.Body.First());
		Assert.NotEmpty(stmt.ThenBody);
		Assert.Empty(stmt.ElseBody);
	}

	[Fact]
	public void Parse_IfStatementWithElse_ReturnsIfStmtWithElseBranch()
	{
		var code = """
		main()
		{
			if (x > 0)
			{
				y = 1;
			}
			else
			{
				y = -1;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<IfStmt>(funcNode.Body.First());
		Assert.NotEmpty(stmt.ThenBody);
		Assert.NotEmpty(stmt.ElseBody);
	}

	[Fact]
	public void Parse_IfStatementWithElseIfChain_ReturnsNestedIfStmts()
	{
		var code = """
		main()
		{
			if (x < 0)
			{
				y = -1;
			}
			else if (x == 0)
			{
				y = 0;
			}
			else
			{
				y = 1;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<IfStmt>(funcNode.Body.First());
		Assert.NotEmpty(stmt.ThenBody);
		Assert.NotEmpty(stmt.ElseBody);
	}

	[Fact]
	public void Parse_IfStatementWithComparison_ReturnsIfStmtWithComparisonCondition()
	{
		var code = """
		main()
		{
			if (x == 42)
			{
				result = true;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<IfStmt>(funcNode.Body.First());
		Assert.IsType<EqualExpr>(stmt.Condition);
	}

	[Fact]
	public void Parse_IfStatementWithMultipleStatementsInBranch_ReturnsIfStmtWithMultipleStatements()
	{
		var code = """
		main()
		{
			if (flag)
			{
				x: i32 = 10;
				y: i32 = 20;
				z = x + y;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<IfStmt>(funcNode.Body.First());
		Assert.Equal(3, stmt.ThenBody.Length);
	}

	#endregion

	#region Control Flow Tests - While Statements

	[Fact]
	public void Parse_SimpleWhileStatement_ReturnsWhileStmt()
	{
		var code = """
		main()
		{
			while (x < 10)
			{
				x++;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<WhileStmt>(funcNode.Body.First());
		Assert.NotEmpty(stmt.Body);
	}

	[Fact]
	public void Parse_WhileStatementWithMultipleStatements_ReturnsWhileStmtWithMultipleStatements()
	{
		var code = """
		main()
		{
			while (count < 5)
			{
				value = count * 2;
				count++;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<WhileStmt>(funcNode.Body.First());
		Assert.Equal(2, stmt.Body.Length);
	}

	[Fact]
	public void Parse_WhileStatementWithBreak_ReturnsWhileStmtWithBreakStmt()
	{
		var code = """
		main()
		{
			while (true)
			{
				if (x > 100)
				{
					break;
				}
				x++;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var whileStmt = Assert.IsType<WhileStmt>(funcNode.Body.First());
		Assert.NotEmpty(whileStmt.Body);
	}

	[Fact]
	public void Parse_WhileStatementWithContinue_ReturnsWhileStmtWithContinueStmt()
	{
		var code = """
		main()
		{
			while (x < 100)
			{
				if (x % 2 == 0)
				{
					continue;
				}
				process(x);
				x++;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<WhileStmt>(funcNode.Body.First());
		Assert.NotEmpty(stmt.Body);
	}

	#endregion

	#region Control Flow Tests - For Statements

	[Fact]
	public void Parse_SimpleForStatement_ReturnsForStmt()
	{
		var code = """
		main()
		{
			for (i: i32 = 0; i < 10; i++)
			{
				result = result + i;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ForStmt>(funcNode.Body.First());
		Assert.NotNull(stmt.Initializer);
		Assert.NotNull(stmt.Condition);
		Assert.NotNull(stmt.Increment);
		Assert.NotEmpty(stmt.Body);
	}

	[Fact]
	public void Parse_ForStatementWithMultipleBodyStatements_ReturnsForStmtWithMultipleStatements()
	{
		var code = """
		main()
		{
			for (i: i32 = 0; i < 10; i++)
			{
				x = arr[i];
				sum = sum + x;
				count++;
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var stmt = Assert.IsType<ForStmt>(funcNode.Body.First());
		Assert.Equal(3, stmt.Body.Length);
	}

	[Fact]
	public void Parse_ForStatementWithBreak_ReturnsForStmtWithBreakStatement()
	{
		var code = """
		main()
		{
			for (i: i32 = 0; i < items.length; i++)
			{
				if (items[i] == target)
				{
					break;
				}
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var forStmt = Assert.IsType<ForStmt>(funcNode.Body.First());
		Assert.NotEmpty(forStmt.Body);
	}

	[Fact]
	public void Parse_NestedForLoops_ReturnsNestedForStatements()
	{
		var code = """
		main()
		{
			for (i: i32 = 0; i < rows; i++)
			{
				for (j: i32 = 0; j < cols; j++)
				{
					matrix[i][j] = 0;
				}
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = Assert.IsType<FunctionDefinitionNode>(nodes.First());
		var outerFor = Assert.IsType<ForStmt>(funcNode.Body.First());
		Assert.NotEmpty(outerFor.Body);
		var innerFor = Assert.IsType<ForStmt>(outerFor.Body.First());
		Assert.NotEmpty(innerFor.Body);
	}

	#endregion
}
