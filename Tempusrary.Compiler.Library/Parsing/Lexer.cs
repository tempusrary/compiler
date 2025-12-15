using Tempusrary.Compiler.Library.Utils;

namespace Tempusrary.Compiler.Library.Parsing;

/// <summary>
/// The types of tokens that can be returned by the lexer
/// </summary>
public enum TokenType
{
    Function,
    Identifier,
    OpenParen,
    CloseParen,
    Comma,
    OpenBrace,
    CloseBrace,
    OpenBracket,
    CloseBracket,
    StringLiteral,
    If,
    Equals,
    DoubleEquals,
    Semicolon,
    True,
    False,
    EndOfFile,
    Import,
    Colon,
    ExclamationMark,
    Integer,
    Decimal
}

/// <summary>
/// Represents a token identified by the lexer with associated metadata such as its type, value, and position in the input source
/// </summary>
public class Token(TokenType type, object value, int line, int column)
{
    public TokenType Type { get; } = type;
    public object Value { get; } = value;
    public int Line { get; } = line;
    public int Column { get; } = column;

    public override string ToString() => $"{Type}: {Value}";
    public T As<T>() => (T)Value;
}

/// <summary>
/// Processes an input string and converts it into a sequence of tokens
/// </summary>
/// <param name="input">The input string to be tokenized</param>
public class Lexer(string input)
{
    public readonly string Input = input;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    /// <summary>
    /// A mapping of keywords to their corresponding token types
    /// </summary>
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "function", TokenType.Function },
        { "if", TokenType.If },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "import", TokenType.Import },
    };

    /// <summary>
    /// Returns the current character in the input string, or '\0' if the end of the string has been reached
    /// </summary>
    private char Current => _position < Input.Length ? Input[_position] : '\0';

    /// <summary>
    /// Advances the lexer to the next character in the input string
    /// </summary>
    private void Next()
    {
        _position++;
        if (Current == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
    }

    /// <summary>
    /// Gets the next token from the input
    /// </summary>
    /// <returns>The next token</returns>
    /// <exception cref="ParsingException">
    /// Thrown when an invalid or unexpected character is encountered
    /// </exception>
    public Token NextToken()
    {
        while (char.IsWhiteSpace(Current)) Next();

        if (_position >= Input.Length)
            return new Token(TokenType.EndOfFile, string.Empty, _line, _column);

        switch (Current)
        {
            case '(':
            {
                Next();
                return new Token(TokenType.OpenParen, "(", _line, _column);
            }
            case ')':
            {
                Next();
                return new Token(TokenType.CloseParen, ")", _line, _column);
            }
            case '{':
            {
                Next();
                return new Token(TokenType.OpenBrace, "{", _line, _column);
            }
            case '}':
            {
                Next();
                return new Token(TokenType.CloseBrace, "}", _line, _column);
            }
            case '[':
            {
                Next();
                return new Token(TokenType.OpenBracket, "[", _line, _column);
            }
            case ']':
            {
                Next();
                return new Token(TokenType.CloseBracket, "]", _line, _column);
            }
            case ',':
            {
                Next();
                return new Token(TokenType.Comma, ",", _line, _column);
            }
            case '=':
            {
                Next();
                if (Current != '=')
                    return new Token(TokenType.Equals, "=", _line, _column);

                Next();
                return new Token(TokenType.DoubleEquals, "==", _line, _column);
            }
            case ':':
            {
                Next();
                return new Token(TokenType.Colon, ":", _line, _column);
            }
            case ';':
            {
                Next();
                return new Token(TokenType.Semicolon, ";", _line, _column);
            }
            case '"':
            {
                var value = ReadString();
                return new Token(TokenType.StringLiteral, value, _line, _column);
            }
        }

        if (!char.IsLetter(Current) && !"!:".Contains(Current) && !char.IsDigit(Current))
            throw new ParsingException(this, _line, _column, $"Unexpected character: {Current}");
        
        if (char.IsDigit(Current))
            return new Token(TokenType.Integer, ReadNumber(), _line, _column);
        
        var identifier = ReadIdentifier();

        return Keywords.TryGetValue(identifier, out var type)
                ? new Token(type, identifier, _line, _column)
                : new Token(TokenType.Identifier, identifier, _line, _column);
    }

    /// <summary>
    /// Reads a string literal from the input, skipping the opening and closing quotes
    /// </summary>
    /// <returns>The read string</returns>
    private string ReadString()
    {
        Next(); // Skip the opening quote
        var start = _position;
        while (Current != '"' && Current != '\0') Next();
        var value = Input.Substring(start, _position - start);
        Next(); // Skip the closing quote
        return value;
    }

    /// <summary>
    /// Reads and extracts an identifier from the current position in the input string until a non-identifier character is encountered
    /// </summary>
    /// <returns>The substring representing the extracted identifier</returns>
    private string ReadIdentifier()
    {
        var start = _position;
        while (char.IsLetterOrDigit(Current) || "!:".Contains(Current)) Next();
        return Input.Substring(start, _position - start);
    }

    /// <summary>
    /// Reads a sequence of numbers
    /// </summary>
    /// <returns>The read number</returns>
    private object ReadNumber()
    {
        var start = _position;
        while (char.IsDigit(Current)) Next();
        if (Current != '.')
            return int.Parse(Input.Substring(start, _position - start));
        
        Next();
        while (char.IsDigit(Current)) Next();
        return decimal.Parse(Input.Substring(start, _position - start));
    }
}