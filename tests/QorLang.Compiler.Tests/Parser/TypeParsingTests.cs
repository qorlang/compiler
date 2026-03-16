using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.Tests.Parser;

public class TypeParsingTests
{
	[Fact]
	public void Parse_SimpleTypeDeclaration_ReturnsTypeDefinitionNode()
	{
		string code = """
		pub type Point
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal("Point", typeNode.Name);
		Assert.Equal(AccessLevel.Public, typeNode.AccessLevel);
		Assert.Empty(typeNode.TypeParameters);
		Assert.Empty(typeNode.MemberDeclarations);
	}

	[Fact]
	public void Parse_TypeWithGenericParameter_ReturnsTypeWithTypeParameters()
	{
		string code = """
		pub type Vec<T>
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);
		Assert.Single(nodes);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal("Vec", typeNode.Name);
		Assert.Single(typeNode.TypeParameters);
		Assert.Contains("T", typeNode.TypeParameters);
	}

	[Fact]
	public void Parse_TypeWithMultipleGenericParameters_ReturnsAllTypeParameters()
	{
		string code = """
		pub type Pair<K, V>
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal(2, typeNode.TypeParameters.Length);
		Assert.Contains("K", typeNode.TypeParameters);
		Assert.Contains("V", typeNode.TypeParameters);
	}

	[Fact]
	public void Parse_SimpleField_ReturnsFieldDeclarationNode()
	{
		string code = """
		pub type Point
		{
			field x: i32 $[rw];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Single(typeNode.MemberDeclarations);

		var fieldNode = typeNode.MemberDeclarations[0] as FieldDeclarationNode;
		Assert.NotNull(fieldNode);
		Assert.Equal("x", fieldNode.Name);
		Assert.NotNull(fieldNode.DataType);
		Assert.Contains(AccessLevel.Private, fieldNode.Protections.Keys);
		Assert.Equal(DataProtection.ReadWrite, fieldNode.Protections[AccessLevel.Private][0]);
	}

	[Fact]
	public void Parse_PublicField_ReturnsPublicField()
	{
		string code = """
		pub type Point
		{
			pub field x: i32 $[rw];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Single(typeNode.MemberDeclarations);

		var fieldNode = typeNode.MemberDeclarations[0] as FieldDeclarationNode;
		Assert.NotNull(fieldNode);
		Assert.Equal("x", fieldNode.Name);
		Assert.NotNull(fieldNode.DataType);
		Assert.Contains(AccessLevel.Public, fieldNode.Protections.Keys);
		Assert.Equal(DataProtection.ReadWrite, fieldNode.Protections[AccessLevel.Public][0]);
	}

	[Fact]
	public void Parse_FieldWithAccessProtections_ReturnProtectionsDictionary()
	{
		string code = """
		pub type Point
		{
			field x: i32 {
				pub [ro],
				[rw]
			};
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);

		var fieldNode = typeNode.MemberDeclarations[0] as FieldDeclarationNode;
		Assert.NotNull(fieldNode);
		Assert.Equal("x", fieldNode.Name);
		Assert.NotEmpty(fieldNode.Protections);
	}

	[Fact]
	public void Parse_MixedPublicAndPrivateFields_ReturnsAllFields()
	{
		string code = """
		pub type Point
		{
			pub field x: i32 $[rw];
			field y: i32 $[rw];
			pub field z: i32 $[rw];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal(3, typeNode.MemberDeclarations.Length);

		var fieldX = typeNode.MemberDeclarations[0] as FieldDeclarationNode;
		Assert.NotNull(fieldX);
		Assert.Contains(AccessLevel.Public, fieldX.Protections.Keys);

		var fieldY = typeNode.MemberDeclarations[1] as FieldDeclarationNode;
		Assert.NotNull(fieldY);
		Assert.Contains(AccessLevel.Private, fieldY.Protections.Keys);

		var fieldZ = typeNode.MemberDeclarations[2] as FieldDeclarationNode;
		Assert.NotNull(fieldZ);
		Assert.Contains(AccessLevel.Public, fieldZ.Protections.Keys);
	}

	[Fact]
	public void Parse_MultipleFields_ReturnsAllFields()
	{
		string code = """
		pub type Point
		{
			field x: i32 $[rw];
			field y: i32 $[rw];
			field z: i32 $[rw];
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal(3, typeNode.MemberDeclarations.Length);

		var fields = typeNode.MemberDeclarations.Cast<FieldDeclarationNode>().ToList();
		Assert.All(fields, field => 
		{
			Assert.Contains(AccessLevel.Private, field.Protections.Keys);
			Assert.Equal(DataProtection.ReadWrite, field.Protections[AccessLevel.Private][0]);
		});
	}

	[Fact]
	public void Parse_SimpleMethod_ReturnsMethodDefinitionNode()
	{
		string code = """
		pub type Calculator
		{
			pub add(a: i32, b: i32) : i32
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Single(typeNode.MemberDeclarations);

		var methodNode = typeNode.MemberDeclarations[0] as MethodDefinitionNode;
		Assert.NotNull(methodNode);
		Assert.Equal("add", methodNode.Name);
		Assert.Equal(AccessLevel.Public, methodNode.AccessLevel);
		Assert.Equal(2, methodNode.Parameters.Length);
	}

	[Fact]
	public void Parse_MethodWithTypeParameters_ReturnsTypeParameters()
	{
		string code = """
		pub type Container
		{
			pub get<T>() : T
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);

		var methodNode = typeNode.MemberDeclarations[0] as MethodDefinitionNode;
		Assert.NotNull(methodNode);
		Assert.Single(methodNode.TypeParameters);
		Assert.Contains("T", methodNode.TypeParameters);
	}

	[Fact]
	public void Parse_MethodWithMultipleParameters_ReturnsAllParameters()
	{
		string code = """
		pub type Calculator
		{
			pub compute(x: i32, y: i32, z: i32) : i32
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);

		var methodNode = typeNode.MemberDeclarations[0] as MethodDefinitionNode;
		Assert.NotNull(methodNode);
		Assert.Equal(3, methodNode.Parameters.Length);
	}

	[Fact]
	public void Parse_TypeWithFieldsAndMethods_ReturnsMixedMembers()
	{
		string code = """
		pub type Vector
		{
			field magnitude: f64 $[rw];
			field direction: f64 $[rw];

			pub normalize() : Vector
			{
			}

			pub scale(factor: f64) : Vector
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal(4, typeNode.MemberDeclarations.Length);

		Assert.Equal(2, typeNode.MemberDeclarations.OfType<FieldDeclarationNode>().Count());
		Assert.Equal(2, typeNode.MemberDeclarations.OfType<MethodDefinitionNode>().Count());
	}

	[Fact]
	public void Parse_PrivateType_ReturnsPrivateAccessLevel()
	{
		string code = """
		type PrivateType
		{
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal(AccessLevel.Private, typeNode.AccessLevel);
	}

	[Fact]
	public void Parse_MethodWithoutAccessModifier_ReturnsPrivateAccessLevel()
	{
		string code = """
		pub type MyType
		{
			helper() : void
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);

		var methodNode = typeNode.MemberDeclarations[0] as MethodDefinitionNode;
		Assert.NotNull(methodNode);
		Assert.Equal(AccessLevel.Private, methodNode.AccessLevel);
	}

	[Fact]
	public void Parse_MethodWithoutReturnType_ParsesSuccessfully()
	{
		string code = """
		pub type MyType
		{
			pub initialize()
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);

		var methodNode = typeNode.MemberDeclarations[0] as MethodDefinitionNode;
		Assert.NotNull(methodNode);
		Assert.Equal("initialize", methodNode.Name);
	}

	[Fact]
	public void Parse_GenericTypeWithGenericMethod_ParsesSuccessfully()
	{
		string code = """
		pub type Container<T>
		{
			field items: T[] $[rw];

			pub convert<U>(transformer: T) : U
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Single(typeNode.TypeParameters);

		var methodNode = typeNode.MemberDeclarations[1] as MethodDefinitionNode;
		Assert.NotNull(methodNode);
		Assert.Single(methodNode.TypeParameters);
	}

	[Fact]
	public void Parse_MethodWithDataProtection_ParsesSuccessfully()
	{
		string code = """
		pub type Mutable
		{
			pub modify() [rw]
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);

		var methodNode = typeNode.MemberDeclarations[0] as MethodDefinitionNode;
		Assert.NotNull(methodNode);
	}

	[Fact]
	public void Parse_SpecialConstructor_ParsesSuccessfully()
	{
		string code = """
		pub type MyType
		{
			pub @ctor(value: i32)
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);

		var methodNode = typeNode.MemberDeclarations[0] as MethodDefinitionNode;
		Assert.NotNull(methodNode);
		Assert.Equal("@ctor", methodNode.Name);
	}

	[Fact]
	public void Parse_SpecialGetterSetter_ParsesSuccessfully()
	{
		string code = """
		pub type MyType
		{
			pub @geti(idx: i32) : i32
			{
			}

			pub @seti(idx: i32, value: i32)
			{
			}
		}
		""";

		var (nodes, errors) = TestHelper.ParseCode(code);

		Assert.Empty(errors);

		var typeNode = nodes.First() as TypeDefinitionNode;
		Assert.NotNull(typeNode);
		Assert.Equal(2, typeNode.MemberDeclarations.Length);

		var getterNode = typeNode.MemberDeclarations[0] as MethodDefinitionNode;
		Assert.NotNull(getterNode);
		Assert.Equal("@geti", getterNode.Name);

		var setterNode = typeNode.MemberDeclarations[1] as MethodDefinitionNode;
		Assert.NotNull(setterNode);
		Assert.Equal("@seti", setterNode.Name);
	}
}