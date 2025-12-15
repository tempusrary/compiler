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

    public List<FunctionDeclaration> ParseFunctions()
    {
        var functions = new List<FunctionDeclaration>();

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            if (_currentToken.Type == TokenType.Import)
            {
                Eat(TokenType.Import);
                var toImport = _currentToken.Value;
                // TODO: Implement importing
                Eat(TokenType.StringLiteral);
            }
            var function = ParseFunction();
            functions.Add(function);
        }

        return functions;
    }

    public FunctionDeclaration ParseFunction()
    {
        Eat(TokenType.Function);
        var functionName = _currentToken.Value;
        Eat(TokenType.Identifier);

        Eat(TokenType.OpenParen);
        var parameters = new List<string>();
        while (_currentToken.Type == TokenType.Identifier)
        {
            parameters.Add(_currentToken.Value);
            Eat(TokenType.Identifier);
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

    private AstNode ParseStatement()
    {
        switch (_currentToken.Type)
        {
            // Handle assignments like `$var = "asd";`
            case TokenType.Identifier:
            {
                var identifier = _currentToken.Value;
                Eat(TokenType.Identifier);

                switch (_currentToken.Type)
                {
                    // Check if this is a function call or an assignment
                    case TokenType.OpenParen:
                    {
                        // It's a function call
                        var arguments = ParseArguments();
                        Eat(TokenType.Semicolon);
                        return new FunctionCall(identifier, arguments);
                    }
                    case TokenType.Equals:
                    {
                        // It's an assignment
                        Eat(TokenType.Equals);
                        var value = ParsePrimary();
                        if (_currentToken.Type == TokenType.Semicolon) Eat(TokenType.Semicolon);
                        return new Assignment(identifier, value);
                    }
                    default:
                        throw new ParsingException(_lexer, _currentToken, $"Unexpected token after identifier: {_currentToken.Value}");
                }
            }
            // Handle `if` statements like `if ($var == "asd") { ... }`
            case TokenType.If:
                return ParseIfStatement();
            default:
                throw new ParsingException(_lexer, _currentToken, $"Unknown statement: {_currentToken.Value}");
        }
    }

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


    private AstNode ParsePrimary()
    {
        switch (_currentToken.Type)
        {
            case TokenType.StringLiteral:
            {
                var value = _currentToken.Value;
                Eat(TokenType.StringLiteral);
                return new StringLiteralNode(value);
            }
            case TokenType.Identifier:
            {
                var name = _currentToken.Value;
                Eat(TokenType.Identifier);
                return new IdentifierNode(name);
            }
            case TokenType.True:
                Eat(TokenType.True);
                return new BooleanLiteralNode(true);
            case TokenType.False:
                Eat(TokenType.False);
                return new BooleanLiteralNode(false);
            default:
                throw new ParsingException(_lexer, _currentToken, $"Unexpected token in primary expression: {_currentToken.Value}");
        }
    }

    private BinaryExpression ParseExpression()
    {
        var left = ParsePrimary();

        var operatorSymbol = _currentToken.Value;
        Eat(TokenType.DoubleEquals);

        var right = ParsePrimary();
        return new BinaryExpression(left, operatorSymbol, right);
    }

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