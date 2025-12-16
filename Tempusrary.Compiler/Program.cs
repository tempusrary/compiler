using System.CommandLine;
using System.Numerics;
using Tempusrary.Compiler.Library.Parsing;

namespace Tempusrary.Compiler;

internal class Program
{
    private static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Tempusrary Compiler CLI");
        var compileCommand = new Command("compile", "Compiles a project.");

        rootCommand.SetAction(_ =>
        {
            Console.WriteLine("Use a subcommand, e.g., 'compile'!");
            return 1;
        });
        
        compileCommand.SetAction(_ =>
        {
            Compile();
        });
        
        rootCommand.Add(compileCommand);

        var parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    private static void Compile()
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Blue;

        var started = DateTime.Now;
        Console.WriteLine("Compiling project...");
        
        // Iterate files
        foreach (var file in Directory.EnumerateFiles(".", "*.tpsr", SearchOption.AllDirectories))
        {
            Console.WriteLine($"Compiling {file}...");
            var fileContent = File.ReadAllText(file);
            var lexerPass1 = new Lexer(fileContent);
            var parserPass1 = new Parser(lexerPass1);

            var astPass1 = parserPass1.ParseFile();
            foreach (var function in astPass1) {
                if (function is not Import import)
                    continue;
                fileContent = fileContent.Replace($"import \"{import.Path}\";", File.ReadAllText(import.Path));
            }
            
            var lexer = new Lexer(fileContent);
            var parser = new Parser(lexer);
            var ast = parser.ParseFile();
        }
        
        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("Compilation completed.");
        Console.WriteLine($@"Time elapsed: {(DateTime.Now - started):s\.FFFFFFF}s");

        Console.ForegroundColor = originalColor;
    }
}