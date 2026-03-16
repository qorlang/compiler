using System.Text.Json;
using CommandLine;
using QorLang.Compiler.Lexer;

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
			throw new NotImplementedException("Code generation not implemented yet.");
		}
	}

	private static void HandleParseError(IEnumerable<Error> errs)
	{
		Environment.Exit(1);
	}
}