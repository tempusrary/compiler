using Tempusrary.Compiler.Library.Utils;

namespace Tempusrary.Compiler.Library.Parsing;

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
    Selector,
    Colon,
    ExclamationMark
}

public class Token(TokenType type, string value, int line, int column)
{
    public TokenType Type { get; } = type;
    public string Value { get; } = value;
    public int Line { get; } = line;
    public int Column { get; } = column;

    public override string ToString() => $"{Type}: {Value}";
}

public class Lexer(string input)
{
    public readonly string Input = input;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "function", TokenType.Function },
        { "if", TokenType.If },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "import", TokenType.Import },
    };

    private char Current => _position < Input.Length ? Input[_position] : '\0';

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

        if (!char.IsLetter(Current) && Current != '$' && !"!:".Contains(Current))
            throw new ParsingException(this, _line, _column, $"Unexpected character: {Current}");
        var identifier = ReadIdentifier();

        return identifier switch
        {
            "true" => new Token(TokenType.True, identifier, _line, _column),
            "false" => new Token(TokenType.False, identifier, _line, _column),
            _ => Keywords.TryGetValue(identifier, out var type)
                ? new Token(type, identifier, _line, _column)
                : new Token(TokenType.Identifier, identifier, _line, _column)
        };

    }

    private string ReadString()
    {
        Next(); // Skip the opening quote
        var start = _position;
        while (Current != '"' && Current != '\0') Next();
        var value = Input.Substring(start, _position - start);
        Next(); // Skip the closing quote
        return value;
    }

    private string ReadIdentifier()
    {
        var start = _position;
        while (char.IsLetterOrDigit(Current) || Current == '$' || "!:".Contains(Current)) Next();
        return Input.Substring(start, _position - start);
    }
}