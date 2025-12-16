using System.Reflection.Metadata.Ecma335;
using Microsoft.VisualBasic;
using Tempusrary.Compiler.Library.Utils;

namespace Tempusrary.Compiler.Library.Parsing;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.NextToken();
    }

    /// <summary>Eats the current token if it matches the expected type, advancing the lexer to the next token</summary>
    /// <param name="type">The expected <see cref="TokenType"/> of the current token</param>
    /// <exception cref="ParsingException">Thrown when the current token does not match the expected type</exception>
    private void Eat(TokenType type)
    {
        if (_currentToken.Type == type)
        {
            _currentToken = _lexer.NextToken();
        }
        else
        {
            throw new ParsingException(_lexer, _currentToken, $"Expected {type} but found {_currentToken.Type}");
        }
    }

    /// <summary>
    /// Parses the file from the input tokens
    /// </summary>
    /// <returns>A list of <see cref="AstNode"/> objects with the parsed functions</returns>
    public List<AstNode> ParseFile()
    {
        var nodes = new List<AstNode>();

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            if (_currentToken.Type == TokenType.Import)
            {
                Eat(TokenType.Import);
                var toImport = _currentToken.As<string>();
                Eat(TokenType.StringLiteral);
                Console.WriteLine($"Importing {toImport}");
                nodes.Add(new Import(toImport));
                Eat(TokenType.Semicolon);
                continue;
            }
            var function = ParseFunction();
            nodes.Add(function);
        }

        return nodes;
    }

    /// <summary>
    /// Parses a function declaration, including its name, parameters, and body
    /// </summary>
    /// <returns>A <see cref="FunctionDeclaration"/> representing the parsed function</returns>
    public FunctionDeclaration ParseFunction()
    {
        var returnType = _currentToken.As<string>();
        Eat(TokenType.Identifier);
        var functionName = _currentToken.As<string>();
        Eat(TokenType.Identifier);

        Eat(TokenType.OpenParen);
        var parameters = new List<FunctionParameter>();
        while (_currentToken.Type == TokenType.Identifier)
        {
            var parameterType = _currentToken.As<string>();
            Eat(TokenType.Identifier);
            
            var parameterName = _currentToken.As<string>();
            Eat(TokenType.Identifier);
            
            parameters.Add(new FunctionParameter(parameterType, parameterName));
            if (_currentToken.Type == TokenType.Comma) Eat(TokenType.Comma);
        }
        Eat(TokenType.CloseParen);

        Eat(TokenType.OpenBrace);
        var body = new List<AstNode>();
        while (_currentToken.Type != TokenType.CloseBrace)
        {
            body.Add(ParseStatement());
        }
        Eat(TokenType.CloseBrace);

        return new FunctionDeclaration(functionName, parameters, body);
    }

    /// <summary>
    /// Parses a general statement
    /// </summary>
    /// <returns>An <see cref="AstNode"/> representing the parsed statement</returns>
    private AstNode ParseStatement()
    {
        switch (_currentToken.Type)
        {
            // Handle assignments like `string var = "asd";`
            case TokenType.Identifier:
            {
                var first = _currentToken.As<string>();
                Eat(TokenType.Identifier);

                switch (_currentToken.Type)
                {
                    // Check if this is a function call or an assignment
                    case TokenType.OpenParen:
                    {
                        // It's a function call
                        var arguments = ParseArguments();
                        Eat(TokenType.Semicolon);
                        return new FunctionCall(first, arguments);
                    }
                    case TokenType.Identifier:
                    {
                        var identifier = _currentToken.As<string>();
                        Eat(TokenType.Identifier);
                        
                        var variable = new Variable(first, identifier);
                        switch (_currentToken.Type)
                        {
                            case TokenType.Equals:
                            {
                                // It's an assignment
                                Eat(TokenType.Equals);
                                var value = ParsePrimary();
                                if (_currentToken.Type == TokenType.Semicolon) Eat(TokenType.Semicolon);
                                return new Assignment(variable, value);
                            }
                        }
                        return variable;
                    }
                    default:
                        throw new ParsingException(_lexer, _currentToken, $"Unexpected token after identifier: {_currentToken.Value}");
                }
            }
            // Handle `if` statements like `if ($var == "asd") { ... }`
            case TokenType.If:
                return ParseIfStatement();
            case TokenType.Return:
            {
                Eat(TokenType.Return);
                var value = ParsePrimary();
                Eat(TokenType.Semicolon);
                return value;
            }
            default:
                throw new ParsingException(_lexer, _currentToken, $"Unknown statement: {_currentToken.Value}");
        }
    }

    /// <summary>
    /// Parses an `if` statement.
    /// </summary>
    /// <returns>An <see cref="IfStatement"/> representing the parsed if statement</returns>
    private IfStatement ParseIfStatement()
    {
        Eat(TokenType.If);
        Eat(TokenType.OpenParen);
        var condition = ParseExpression(); // Use the updated ParseExpression
        Eat(TokenType.CloseParen);

        Eat(TokenType.OpenBrace);
        var body = new List<AstNode>();
        while (_currentToken.Type != TokenType.CloseBrace)
        {
            body.Add(ParseStatement());
        }
        Eat(TokenType.CloseBrace);

        return new IfStatement(condition, body);
    }

    /// <summary>
    /// Parses a primary expression, which can be a literal or an identifier.
    /// </summary>
    /// <returns>The primary expression</returns>
    /// <exception cref="ParsingException"></exception>
    private AstNode ParsePrimary()
    {
        switch (_currentToken.Type)
        {
            case TokenType.StringLiteral:
            {
                var value = _currentToken.As<string>();
                Eat(TokenType.StringLiteral);
                return new StringLiteralNode(value);
            }
            case TokenType.Identifier:
            {
                var name = _currentToken.As<string>();
                Eat(TokenType.Identifier);
                return new IdentifierNode(name);
            }
            case TokenType.True:
                Eat(TokenType.True);
                return new BooleanLiteralNode(true);
            case TokenType.False:
                Eat(TokenType.False);
                return new BooleanLiteralNode(false);
            case TokenType.Integer:
                var integerVal = _currentToken.As<int>();
                Eat(TokenType.Integer);
                return new IntegerLiteralNode(integerVal);
            case TokenType.Decimal:
                var decimalValue = _currentToken.As<decimal>();
                Eat(TokenType.Decimal);
                return new DecimalLiteralNode(decimalValue);
            default:
                throw new ParsingException(_lexer, _currentToken, $"Unexpected token in primary expression: {_currentToken.Value}");
        }
    }

    /// <summary>
    /// Parses a binary expression, specifically for equality checks.
    /// </summary>
    /// <returns>The binary expression</returns>
    private BinaryExpression ParseExpression()
    {
        var left = ParsePrimary();

        var operatorSymbol = _currentToken.As<string>();
        Eat(TokenType.DoubleEquals);

        var right = ParsePrimary();
        return new BinaryExpression(left, operatorSymbol, right);
    }

    /// <summary>
    /// Parses a list of arguments for a function call.
    /// </summary>
    /// <returns>The parsed list of nodes</returns>
    private List<AstNode> ParseArguments()
    {
        var arguments = new List<AstNode>();
        Eat(TokenType.OpenParen);
        while (_currentToken.Type != TokenType.CloseParen)
        {
            arguments.Add(ParsePrimary());
            if (_currentToken.Type == TokenType.Comma) Eat(TokenType.Comma);
        }
        Eat(TokenType.CloseParen);
        return arguments;
    }
}