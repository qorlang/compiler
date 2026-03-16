using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.Tests.Parser;

public class TopLevelStatements
{
	[Theory]
	[InlineData("SomeNamespace")]
	[InlineData("SomeNamespace.NestedNamespace")]
	[InlineData("@SomeNamespace")]
	[InlineData("SomeNamespace.@NestedNamespace")]
	[InlineData("SomeNamespace.NestedNamespace.Namespace3")]
	public void Parse_ValidNamespaceDeclaration_ReturnsNamespaceNode(string namespaceName)
	{
		string code = $"""
		namespace {namespaceName};
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		Assert.Single(nodes, new NamespaceNode(namespaceName));
	}

	[Theory]
	[InlineData("SomeNamespace")]
	[InlineData("SomeNamespace.NestedNamespace")]
	[InlineData("@SomeNamespace")]
	[InlineData("SomeNamespace.@NestedNamespace")]
	[InlineData("SomeNamespace.NestedNamespace.Namespace3")]
	public void Parse_ValidUsing_ReturnsUsingNamespaceNode(string namespaceName)
	{
		string code = $"""
		using {namespaceName};
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		Assert.Single(nodes, new UsingNamespaceNode(namespaceName));
	}

	[Theory]
	[InlineData("1InvalidNamespace")]
	[InlineData("SomeNamespace..Nested")]
	[InlineData("SomeNamespace.!Invalid")]
	[InlineData("")]
	[InlineData(" ")]
	public void Parse_InvalidNamespaceDeclaration_ReturnsError(string namespaceName)
	{
		string code = $"""
		namespace {namespaceName};
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(nodes);
		Assert.Single(errors);
	}

	[Theory]
	[InlineData("1InvalidNamespace")]
	[InlineData("SomeNamespace..Nested")]
	[InlineData("SomeNamespace.!Invalid")]
	[InlineData("")]
	[InlineData(" ")]
	public void Parse_InvalidUsing_ReturnsError(string namespaceName)
	{
		string code = $"""
		using {namespaceName};
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(nodes);
		Assert.Single(errors);
	}

	[Fact]
	public void Parse_MultipleNamespaceDeclarations_ReturnsError()
	{
		string code = """
		namespace Namespace1;
		namespace Namespace2;
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR205", errors.First().Message);
	}

	[Fact]
	public void Parse_StatementBeforeNamespace_ReturnsError()
	{
		string code = """
		using Qor;
		namespace MyNamespace;
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR204", errors.First().Message);
	}

	[Fact]
	public void Parse_ValidImport_ReturnsImportNode()
	{
		string code = """
		import "libqor/core";
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes, new ImportNode("libqor/core"));
	}

	[Theory]
	[InlineData("libqor/core")]
	[InlineData("libqor/threads")]
	[InlineData("myassembly")]
	public void Parse_ValidImportPaths_ReturnsImportNode(string path)
	{
		string code = $"""
		import "{path}";
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes, new ImportNode(path));
	}

	[Fact]
	public void Parse_ImportMissingPath_ReturnsError()
	{
		string code = """
		import ;
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(nodes);
		Assert.Single(errors);
	}

	[Fact]
	public void Parse_MultipleImports_SuccessfullyParsed()
	{
		string code = """
		import "libqor/core";
		import "libqor/threads";
		import "mylib/utils";
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Equal(3, nodes.Count());
		Assert.Contains(new ImportNode("libqor/core"), nodes);
		Assert.Contains(new ImportNode("libqor/threads"), nodes);
		Assert.Contains(new ImportNode("mylib/utils"), nodes);
	}

	[Fact]
	public void Parse_SimpleFunctionDefinition_ReturnsFunctionNode()
	{
		string code = """
		main() {
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes);

		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal("main", funcNode.Name);
		Assert.Equal(AccessLevel.Private, funcNode.AccessLevel);
		Assert.Empty(funcNode.Parameters);
		Assert.Empty(funcNode.TypeParameters);
		Assert.True(funcNode.ReturnType.IsVoid);
		Assert.Empty(funcNode.Body);
	}

	[Fact]
	public void Parse_PublicFunctionDefinition_HasPublicAccessLevel()
	{
		string code = """
		pub myFunc()
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes);

		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal(AccessLevel.Public, funcNode.AccessLevel);
	}

	[Fact]
	public void Parse_FunctionWithReturnType_CorrectlyParsed()
	{
		string code = """
		getNumber() : i32
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal("Qor.Int32", funcNode.ReturnType.TypeName);
	}

	[Fact]
	public void Parse_FunctionWithParameters_CorrectlyParsed()
	{
		string code = """
		add(a: i32, b: i32) : i32
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal(2, funcNode.Parameters.Length);
		Assert.Equal("a", funcNode.Parameters[0].Name);
		Assert.Equal("b", funcNode.Parameters[1].Name);
	}

	[Fact]
	public void Parse_FunctionWithTypeParameters_CorrectlyParsed()
	{
		string code = """
		identity<T>(value: T) : T
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Single(funcNode.TypeParameters);
		Assert.Equal("T", funcNode.TypeParameters[0]);
	}

	[Fact]
	public void Parse_FunctionWithMultipleTypeParameters_CorrectlyParsed()
	{
		string code = """
		map<K, V>(key: K, value: V) : bool
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal(2, funcNode.TypeParameters.Length);
		Assert.Equal("K", funcNode.TypeParameters[0]);
		Assert.Equal("V", funcNode.TypeParameters[1]);
	}

	[Fact]
	public void Parse_FunctionWithReturnVoid_IsVoidType()
	{
		string code = """
		do_something()
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.True(funcNode.ReturnType.IsVoid);
	}

	[Fact]
	public void Parse_MultipleFunctionDefinitions_AllParsedSuccessfully()
	{
		string code = """
		func1()
		{

		}

		func2()
		{

		}

		func3()
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Equal(3, nodes.Count());
	}

	[Fact]
	public void Parse_DuplicatePubModifier_ReturnsError()
	{
		string code = """
		pub pub myFunc()
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR206", errors.First().Message);
	}

	[Fact]
	public void Parse_PubModifierWithoutDeclaration_ReturnsError()
	{
		string code = """
		pub;
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR202", errors.First().Message);
		Assert.Contains("Expected declaration after 'pub'", errors.First().Message);
	}

	// Function statement tests
	[Fact]
	public void Parse_ReturnStatement_CorrectlyParsed()
	{
		string code = """
		test()
		{
			return 42;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Single(funcNode.Body);
	}

	[Fact]
	public void Parse_BreakOutsideLoop_ReturnsError()
	{
		string code = """
		test()
		{
			break;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR216", errors.First().Message);
	}

	[Fact]
	public void Parse_ContinueOutsideLoop_ReturnsError()
	{
		string code = """
		test()
		{
			continue;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR217", errors.First().Message);
	}

	[Fact]
	public void Parse_UnreachableCodeAfterReturn_ReturnsError()
	{
		string code = """
		test()
		{
			return;
			let x: i32 = 5;
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR215", errors.First().Message);
	}

	[Theory]
	[InlineData("i8")]
	[InlineData("i16")]
	[InlineData("i32")]
	[InlineData("i64")]
	[InlineData("u8")]
	[InlineData("u16")]
	[InlineData("u32")]
	[InlineData("u64")]
	[InlineData("f32")]
	[InlineData("f64")]
	[InlineData("bool")]
	[InlineData("string")]
	public void Parse_BuiltInTypeReference_CorrectlyResolved(string typeName)
	{
		string code = $$"""
		test() : {{typeName}}
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal(typeName switch {
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
			_ => throw new ArgumentException($"Unknown type name: {typeName}")
		}, funcNode.ReturnType.TypeName);
	}

	[Fact]
	public void Parse_CustomTypeReference_CorrectlyResolved()
	{
		string code = """
		test() : MyType
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal("MyType", funcNode.ReturnType.TypeName);
	}

	[Fact]
	public void Parse_DottedTypeReference_CorrectlyResolved()
	{
		string code = """
		test() : Qor.Collections.List
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal("Qor.Collections.List", funcNode.ReturnType.TypeName);
	}

	[Fact]
	public void Parse_GenericTypeReference_CorrectlyResolved()
	{
		string code = """
		test() : List<i32>
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal("List", funcNode.ReturnType.TypeName);
		Assert.Single(funcNode.ReturnType.TypeArguments);
	}

	[Fact]
	public void Parse_ReferenceTypeReference_CorrectlyResolved()
	{
		string code = """
		test() : MyType&
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal(1, funcNode.ReturnType.RefLevel);
	}

	[Fact]
	public void Parse_ArrayTypeReference_CorrectlyResolved()
	{
		string code = """
		test() : i32[]
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal(1, funcNode.ReturnType.ArrayLevel);
	}

	[Fact]
	public void Parse_MultiDimensionalArrayTypeReference_CorrectlyResolved()
	{
		string code = """
		test() : i32[][]
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var funcNode = nodes.First() as FunctionDefinitionNode;
		Assert.NotNull(funcNode);
		Assert.Equal(2, funcNode.ReturnType.ArrayLevel);
	}

	[Fact]
	public void Parse_ReferenceToArrayType_ReturnsError()
	{
		string code = """
		test(arr: i32[]&)
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR211", errors.First().Message);
	}

	[Fact]
	public void Parse_PubBeforeImport_ReturnsError()
	{
		string code = """
		pub import "module";
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR202", errors.First().Message);
		Assert.Contains("Expected declaration after 'pub'", errors.First().Message);
	}

	[Fact]
	public void Parse_PubBeforeUsingStatement_ReturnsError()
	{
		string code = """
		pub using Qor;
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR202", errors.First().Message);
		Assert.Contains("Expected declaration after 'pub'", errors.First().Message);
	}

	[Fact]
	public void Parse_PubBeforeNamespace_ReturnsError()
	{
		string code = """
		pub namespace MyNamespace;
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR202", errors.First().Message);
		Assert.Contains("Expected declaration after 'pub'", errors.First().Message);
	}

	[Fact]
	public void Parse_PubAppliedToType_ReturnsPublicType()
	{
		string code = """
		pub type MyType
		{

		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal(AccessLevel.Public, typeNode.AccessLevel);
	}

	[Fact]
	public void Parse_PubAppliedToGlobal_ReturnsPublicGlobal()
	{
		string code = """
		pub global myGlobal: i32;
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes);

		var globalNode = nodes.First() as GlobalDeclarationNode;
		Assert.NotNull(globalNode);
		Assert.True(globalNode.Protections.ContainsKey(AccessLevel.Public));
	}

	// Global protection syntax tests - simple
	[Fact]
	public void Parse_SimpleGlobalPublicWithReadWriteProtection_ReturnsReadWriteProtectionForAll()
	{
		string code = """
		pub global other: Other $[rw];
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes);

		var globalNode = nodes.First() as GlobalDeclarationNode;
		Assert.NotNull(globalNode);
		Assert.Equal("other", globalNode.Name);
		
		Assert.True(globalNode.Protections.ContainsKey(AccessLevel.Public));
		Assert.Equal(DataProtection.ReadWrite, globalNode.Protections[AccessLevel.Public][0]);
		
		// Simple syntax applies to all access levels
		Assert.True(globalNode.Protections.ContainsKey(AccessLevel.Private));
		Assert.Equal(DataProtection.ReadWrite, globalNode.Protections[AccessLevel.Private][0]);
	}

	[Fact]
	public void Parse_ReadOnlyInSimpleSyntax_ReturnsError()
	{
		string code = """
		global config: string $[ro];
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR212", errors.First().Message);
	}

	// Global protection syntax tests - complex
	[Fact]
	public void Parse_ComplexGlobalWithDifferentAccessProtections_ReturnsCorrectProtections()
	{
		string code = """
		global state: State& {
			pub [ro, ro&],
			[rw, rw&]
		};
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes);

		var globalNode = nodes.First() as GlobalDeclarationNode;
		Assert.NotNull(globalNode);
		Assert.Equal("state", globalNode.Name);
		
		// Type has one reference level (State&), so protections array should have length 2
		// Index 0 = base type protection, Index 1 = first ref (&) protection
		Assert.True(globalNode.Protections.ContainsKey(AccessLevel.Public));
		Assert.Equal(2, globalNode.Protections[AccessLevel.Public].Length);
		Assert.Equal(DataProtection.ReadOnly, globalNode.Protections[AccessLevel.Public][0]);
		Assert.Equal(DataProtection.ReadOnly, globalNode.Protections[AccessLevel.Public][1]);
		
		Assert.True(globalNode.Protections.ContainsKey(AccessLevel.Private));
		Assert.Equal(2, globalNode.Protections[AccessLevel.Private].Length);
		Assert.Equal(DataProtection.ReadWrite, globalNode.Protections[AccessLevel.Private][0]);
		Assert.Equal(DataProtection.ReadWrite, globalNode.Protections[AccessLevel.Private][1]);
	}

	[Fact]
	public void Parse_ComplexGlobalPublicOnlyWritePrivateReadWrite_ReturnsPublicOnlyWritePrivateReadWrite()
	{
		string code = """
		global data: Data {
			pub [ro],
			[rw]
		};
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		var globalNode = nodes.First() as GlobalDeclarationNode;
		Assert.NotNull(globalNode);
		Assert.Equal("data", globalNode.Name);
		
		// Public should have ReadOnly
		Assert.True(globalNode.Protections.ContainsKey(AccessLevel.Public));
		Assert.Equal(DataProtection.ReadOnly, globalNode.Protections[AccessLevel.Public][0]);
		
		// Private should have ReadWrite
		Assert.True(globalNode.Protections.ContainsKey(AccessLevel.Private));
		Assert.Equal(DataProtection.ReadWrite, globalNode.Protections[AccessLevel.Private][0]);
	}

	[Fact]
	public void Parse_MixedSimpleAndComplexSyntax_ReturnsError()
	{
		string code = """
		pub global state: State& {
			pub [ro, ro&],
			[rw, rw&]
		};
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Single(errors);
		Assert.Contains("QR219", errors.First().Message);
	}

	[Fact]
	public void Parse_MultipleGlobalsWithComplexProtections_AllParsed()
	{
		string code = """
		global state: State& {
			pub [ro, ro&],
			[rw, rw&]
		};

		pub global other: Other $[rw];

		global cache: Cache {
			pub [ro],
			[rw]
		};
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Equal(3, nodes.Count());

		var globals = nodes.OfType<GlobalDeclarationNode>().ToList();

		Assert.Equal(3, globals.Count);
		Assert.Equal("state", globals[0].Name);
		Assert.Equal("other", globals[1].Name);
		Assert.Equal("cache", globals[2].Name);
	}	
}

