using System.CommandLine;
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
            var lexer = new Lexer(File.ReadAllText(file));
            var parser = new Parser(lexer);

            var functions = parser.ParseFunctions();
        }
        
        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("Compilation completed.");
        Console.WriteLine($@"Time elapsed: {(DateTime.Now - started):s\.FFFFFFF}s");

        Console.ForegroundColor = originalColor;
    }
}