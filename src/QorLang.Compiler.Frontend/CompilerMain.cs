using System.Text.Json;
using CommandLine;
using Microsoft.Extensions.Options;
using QorLang.Compiler.Lexer;
using QorLang.Compiler.NodeTagger;
using QorLang.Compiler.Parser;

namespace QorLang.Compiler.Frontend;

public class CompilerOptions
{
	[Value(0, MetaName = "file", HelpText = "Source file to compile", Required = true)]
	public required string SourceFile { get; set; }

	[Option('v', "verbose", HelpText = "Enable verbose output")]
	public bool Verbose { get; set; }

	[Option('o', "output", HelpText = "Output file path")]
	public string? OutputFile { get; set; }

	[Option('t', "tokens", HelpText = "Output tokens instead of an assembly")]
	public bool OutputTokens { get; set; }

	[Option('a', "ast", HelpText = "Output AST instead of an assembly")]
	public bool OutputAST { get; set; }

	[Option("type-info", HelpText = "Output AST tagged with type and symbol info")]
	public bool Typeinfo { get; set; }

	[Option("module-refs", Separator = ',', HelpText = "Specify external assembly references")]
	public IEnumerable<string> ReferencedModules { get; set; } = [];
}

public class CompilerMain
{
	public static void Main(string[] args)
	{
		CommandLine.Parser.Default.ParseArguments<CompilerOptions>(args)
			.WithParsed(Run)
			.WithNotParsed(HandleParseError);
	}

	static void Run(CompilerOptions opts)
	{
		if (!File.Exists(opts.SourceFile))
		{
			Console.Error.WriteLine($"Error QR001: File '{opts.SourceFile}' not found.");
			Environment.Exit(1);
			return;
		}

		string sourceCode = File.ReadAllText(opts.SourceFile);
		var lexer = new DefaultLexer(opts.SourceFile, sourceCode);

		if (opts.OutputTokens)
		{
			string outputFile = opts.OutputFile ?? Path.ChangeExtension(opts.SourceFile, ".tok");

			using var writer = new StreamWriter(outputFile);

			foreach (var token in lexer.Tokenize())
			{
				writer.WriteLine($"{token.Type}: {JsonSerializer.Serialize(token.Value)} at {token.Location}");
			}
		}
		else
		{
			var tokens = lexer.Tokenize();
			var parser = new DefaultParser(tokens);
			var (nodes, errors) = parser.ParseModule();

			if (errors.Count != 0)
			{
				foreach (var error in errors)
				{
					Console.Error.WriteLine(error.ToString());
				}

				Environment.Exit(1);
			}

			if (opts.OutputAST)
			{
				string outputFile = opts.OutputFile ?? Path.ChangeExtension(opts.SourceFile, ".ast.json");

				var output = new
				{
					nodes = nodes.Select(n => JsonDocument.Parse(n.ToString()).RootElement)
				};

				string json = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });

				File.WriteAllText(outputFile, json);

				return;
			}

			var moduleImporter = new MetadataImporter();
			var nodeTagger = new InitialNodeTagger(nodes, moduleImporter.ImportModule, opts.ReferencedModules);

			var taggerRes = nodeTagger.Tag();

			foreach (var error in taggerRes.Errors)
			{
				Console.Error.WriteLine(error.ToString());
			}

			throw new NotImplementedException("Code generation not implemented yet.");
		}
	}

	private static void HandleParseError(IEnumerable<Error> errs)
	{
		Environment.Exit(1);
	}
}