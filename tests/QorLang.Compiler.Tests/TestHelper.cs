using QorLang.Compiler.Errors;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.Parser;
using QorLang.Compiler.Parser.Nodes;

namespace QorLang.Compiler.Tests;

public static class TestHelper
{
	public static (IEnumerable<ASTNode>, IEnumerable<CompilationError>) ParseCode(string code)
	{
		var lexer = new DefaultLexer("<test>", code);
		var tokens = lexer.Tokenize();

		var parser = new DefaultParser(tokens);

		var (nodes, errors) = parser.ParseModule();

		return (nodes.FilterErrorNodes(), errors.FilterEOFErrors());
	}
	
	extension (IEnumerable<CompilationError> errors)
	{
		public IEnumerable<CompilationError> FilterEOFErrors()
		{
			return errors.Where((err) => !err.Message.Contains("Unexpected end of input."));
		}
	}

	extension (IEnumerable<ASTNode> nodes)
	{
		public IEnumerable<ASTNode> FilterErrorNodes()
		{
			return nodes.Where(node => node is not ErrorNode);
		}
	}
}