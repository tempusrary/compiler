using Tempusrary.Compiler.Library.Parsing;

namespace Tempusrary.Compiler.Library.Utils;

public class ParsingException : Exception
{
    private static string GenerateMessage(Lexer lexer, int line, int column, string message)
    {
        // Get the relevant lines for context (before, error, and after)
        var lines = lexer.Input.Split('\n');
        var context = new List<string>();

        if (line > 1)
        {
            context.Add($"{line - 1} | {lines[line - 2]}");
        }

        context.Add($"{line} | {lines[line - 1]}");
        context.Add($"{new string(' ', column + line.ToString().Length)}^");

        if (line < lines.Length)
        {
            context.Add($"{line + 1} | {lines[line]}");
        }

        return message + "\n" + string.Join('\n', context);
    }

    public ParsingException(Lexer lexer, Token currentToken, string message): base(GenerateMessage(lexer, currentToken.Line, currentToken.Column, message))
    {
    }

    public ParsingException(Lexer lexer, int line, int column, string message): base(GenerateMessage(lexer, line, column, message))
    {
    }
}