namespace Tempusrary.Compiler.Library.Parsing;

public abstract class AstNode
{
}

public class FunctionParameter(string type, string name)
{
    public string Type { get; } = type;
    public string Name { get; } = name;
}

public class FunctionDeclaration(string name, List<FunctionParameter> parameters, List<AstNode> body) : AstNode
{
    public string Name { get; } = name;
    public List<FunctionParameter> Parameters { get; } = parameters;
    public List<AstNode> Body { get; } = body;
}

public class IfStatement(BinaryExpression condition, List<AstNode> body) : AstNode
{
    public static int IfCounter { get; set; }

    public BinaryExpression Condition { get; } = condition;
    public List<AstNode> Body { get; } = body;
}

public class Assignment(string variableName, AstNode value) : AstNode
{
    public string VariableName { get; } = variableName;
    public AstNode Value { get; } = value;
}

public class IdentifierNode(string name) : AstNode
{
    public string Name { get; } = name;
}

public class StringLiteralNode(string value) : AstNode
{
    public string Value { get; } = value;
}

public class FunctionCall(string functionName, List<AstNode> arguments) : AstNode
{
    public string FunctionName { get; } = functionName;
    public List<AstNode> Arguments { get; } = arguments;
}

public class BinaryExpression(AstNode left, string operatorSymbol, AstNode right) : AstNode
{
    public AstNode Left { get; } = left;
    public string Operator { get; } = operatorSymbol;
    public AstNode Right { get; } = right;
}

public class BooleanLiteralNode(bool value) : AstNode
{
    public bool Value { get; } = value;
}

public class IntegerLiteralNode(int value) : AstNode
{
    public int Value { get; } = value;
}

public class DecimalLiteralNode(decimal value) : AstNode
{
    public decimal Value { get; } = value;
}