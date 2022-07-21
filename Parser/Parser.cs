using LexerModule;
using ErrorHandlerModule;

namespace ParserModule
{
    public class Parser
    {
        #region Fields and Properties
        ILexer lexer;
        IErrorHandler errorHandler;
        Token currentToken;
        Dictionary<string, FunctionDefinition> functions = new Dictionary<string, FunctionDefinition>();
        List<DllLoader> dllLoaders = new List<DllLoader>();
        Dictionary<TokenType, DataTypes> typeRemaper = new Dictionary<TokenType, DataTypes>()
        {
            { TokenType.String, DataTypes.String },
            { TokenType.Int, DataTypes.Int },
            { TokenType.File, DataTypes.File },
            { TokenType.Directory, DataTypes.Directory },
        };

        Dictionary<TokenType, ComparisionOperators> comparisionOperatorRemaper = new Dictionary<TokenType, ComparisionOperators>()
        {
            { TokenType.Equals, ComparisionOperators.Equals},
            { TokenType.NotEquals, ComparisionOperators.NotEquals},
            { TokenType.Greater, ComparisionOperators.Greater},
            { TokenType.GreaterOrEqual, ComparisionOperators.GreaterOrEqual},
            { TokenType.Lesser, ComparisionOperators.Lesser},
            { TokenType.LesserOrEqual, ComparisionOperators.LesserOrEqual}
        };

        Dictionary<TokenType, MultiplicationOperators> multiplicationOperatorRemaper = new Dictionary<TokenType, MultiplicationOperators>()
        {
            { TokenType.Multiplication, MultiplicationOperators.Multiply},
            { TokenType.Division, MultiplicationOperators.Divide},
            { TokenType.Modulo, MultiplicationOperators.Modulo},
        };

        Dictionary<TokenType, AdditionMathOperators> additionMathOperatorRemaper = new Dictionary<TokenType, AdditionMathOperators>()
        {
            { TokenType.Plus, AdditionMathOperators.Add},
            { TokenType.Minus, AdditionMathOperators.Subtract},
        };


        #endregion

        #region Constructor and Public Methods
        public Parser(ILexer l, IErrorHandler eh)
        {
            lexer = l;
            errorHandler = eh;
            currentToken = lexer.GetNextToken();
        }
        public ParsedProgram GetParsedProgram()
        {
            while (TryParse());

            return new ParsedProgram(functions, dllLoaders);
        }

        #endregion

        #region Helpers
        void GetNextToken()
        {
            currentToken = lexer.GetNextToken();
        }

        bool TokenIs(TokenType tokenType)
        {
            return tokenType == currentToken.TypeOfToken;
        }

        bool CheckAndConsume(TokenType type)
        {
            if (!TokenIs(type))
                return false;
            GetNextToken();
            return true;
        }

        string GetIdentifierName()
        {
            var name = currentToken.TokenValue as string;
            if (name is null)
            {
                errorHandler.IdentifierIsNull(currentToken.Line, currentToken.Column);
                name = "identifier_is_hollow";
            }
            GetNextToken();
            return name;
        }
        #endregion

        #region ParsingMethods
        IExpression? ParseLogicalExpression()
        {
            var initialCondition = ParseConditionConjunction();
            if (initialCondition is null)
                return null;

            var alternatives = new List<IExpression>();
            while (CheckAndConsume(TokenType.Or))
            {
                var condition = ParseConditionConjunction();
                if (condition is null)
                    errorHandler.UnexpectStatement("condition", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                alternatives.Add(condition);
            }
            if (alternatives.Count == 0) return initialCondition;
            return new LogicalExpression(initialCondition, alternatives);
        }

        IExpression? ParseConditionConjunction()
        {
            var initialCondition = ParseCondition();
            if (initialCondition is null)
                return null;

            var conjunctions = new List<IExpression>();
            while (CheckAndConsume(TokenType.And))
            {
                var condition = ParseCondition();
                if (condition is null) 
                    errorHandler.UnexpectStatement("condition", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                conjunctions.Add(condition);
            }
            if (conjunctions.Count == 0) return initialCondition;
            return new ConditionConjunction(initialCondition, conjunctions);
        }

        IExpression? ParseCondition()
        {
            var initialCondition = ParseMathExpression();
            if (initialCondition is null)
                return null;

            if (!comparisionOperatorRemaper.TryGetValue(currentToken.TypeOfToken, out ComparisionOperators remappedComparisionOperator))
                return initialCondition;
            GetNextToken();

            var comparedCondition = ParseMathExpression();
            if (comparedCondition is null)
                errorHandler.UnexpectStatement(TokenType.Identifier.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            return new Condition(initialCondition, remappedComparisionOperator, comparedCondition);
        }

        IExpression? ParseMathExpression()
        {
            var initialMathValue = ParseMathMultiplication();
            if (initialMathValue is null)
                return null;

            var additions = new List<(AdditionMathOperators Operator, IExpression MathValue)>();
            while (additionMathOperatorRemaper.TryGetValue(currentToken.TypeOfToken, out AdditionMathOperators remappedOperator))
            {
                GetNextToken();
                var mathValue = ParseMathMultiplication();
                if (mathValue is null)
                    errorHandler.UnexpectStatement("Math Multiplicaton or Value", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                additions.Add((remappedOperator, mathValue));
            }
            if (additions.Count == 0) return initialMathValue;
            return new MathExpression(initialMathValue, additions);
        }

        IExpression? ParseMathMultiplication()
        {
            var initialUnaryValue = ParseUnaryValue();
            if (initialUnaryValue is null)
                return null;

            var multiplicaions = new List<(MultiplicationOperators Operator, IExpression MathValue)>();
            while (multiplicationOperatorRemaper.TryGetValue(currentToken.TypeOfToken, out MultiplicationOperators remappedOperator))
            {
                GetNextToken();
                var unaryValue = ParseUnaryValue();
                if (unaryValue is null)
                    errorHandler.UnexpectStatement("Value or Expression", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                multiplicaions.Add((remappedOperator, unaryValue));
            }
            if (multiplicaions.Count == 0) return initialUnaryValue;
            return new MathMultiplication(initialUnaryValue, multiplicaions);
        }

        IExpression? ParseUnaryValue()
        {
            bool negated = CheckAndConsume(TokenType.Minus);
            var value = ParseIncrementValue() ?? ParseParenthesisExpression();
            if (value is not null)
                return negated ? new NegatedExpression(value) : value;

            if (negated) 
                errorHandler.UnexpectStatement("Value or Expression", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            return null;
        }

        IExpression? ParseParenthesisExpression()
        {
            if (!CheckAndConsume(TokenType.ParenthesesOpen))
                return null;

            var expression = ParseLogicalExpression();
            if (expression is null)
                errorHandler.UnexpectStatement("Logical Expression", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            if (!CheckAndConsume(TokenType.ParenthesesClose))
                errorHandler.UnexpectedToken(TokenType.ParenthesesClose.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            return expression;
        }

        IExpression? ParseIncrementValue()
        {
            var value  = ParseValueOrValueGetter();
            if (value is null)
                return null;
            return CheckAndConsume(TokenType.Increment) ? 
                new IncrementedExpression(value) 
                : value;
        }

        bool TryParseReturn(ref List<IInstruction> instructions)
        {
            if (!CheckAndConsume(TokenType.Return))
                return false; ;

            var expression = ParseMathExpression();
            if (expression is null)
                errorHandler.UnexpectStatement("Expression", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            if (!CheckAndConsume(TokenType.Semicolon))
                errorHandler.UnexpectedToken(TokenType.Semicolon.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            instructions.Add(new Returning(expression));
            return true;
        }

        bool TryParseIfStatement(ref List<IInstruction> instructions)
        {
            if (!CheckAndConsume(TokenType.If))
                return false;

            if (!CheckAndConsume(TokenType.ParenthesesOpen))
                errorHandler.UnexpectedToken(TokenType.ParenthesesOpen.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            var expression = ParseLogicalExpression();
            if (expression is null)
                errorHandler.UnexpectStatement("Logical Expression", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            if (!CheckAndConsume(TokenType.ParenthesesClose))
                errorHandler.UnexpectedToken(TokenType.ParenthesesClose.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            var block = ParseBlock();
            if (block is null)
                errorHandler.UnexpectStatement("Block of Statements", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            if (!CheckAndConsume(TokenType.Else))
            {
                instructions.Add(new IfOrIfElse(expression, block));
                return true;
            }

            var elseBlock = ParseBlock();
            if (elseBlock is null)
                errorHandler.UnexpectStatement("Instruction block", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            instructions.Add(new IfOrIfElse(expression, block, elseBlock));
            return true;
        }

        bool TryParseForeach(ref List<IInstruction> instructions)
        {
            if(!CheckAndConsume(TokenType.Foreach))
                return false;
            
            if (!CheckAndConsume(TokenType.ParenthesesOpen))
                errorHandler.UnexpectedToken(TokenType.ParenthesesOpen.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            if (!typeRemaper.TryGetValue(currentToken.TypeOfToken, out DataTypes type))
                errorHandler.UnexpectStatement("Data type", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            GetNextToken();

            if (!TokenIs(TokenType.Identifier))
                errorHandler.UnexpectedToken(TokenType.Identifier.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            var name = GetIdentifierName();
            
            if (!CheckAndConsume(TokenType.In))
                errorHandler.UnexpectedToken(TokenType.In.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            var expression = ParseMathExpression();
            if (expression is null)
                errorHandler.UnexpectStatement("Expression", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            if (!CheckAndConsume(TokenType.ParenthesesClose))
                errorHandler.UnexpectedToken(TokenType.ParenthesesClose.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            var block = ParseBlock();
            if (block is null)
                errorHandler.UnexpectStatement("Block of Statements", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            instructions.Add(new ForeachLoop(type, name, expression, block));
            return true;
        }

        bool TryParseDeclaration(ref List<IInstruction> instructions)
        {
            if (!typeRemaper.TryGetValue(currentToken.TypeOfToken, out DataTypes tokenType))
                return false;
            GetNextToken();

            if (!TokenIs(TokenType.Identifier))
                errorHandler.UnexpectedToken(TokenType.Identifier.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            var name = GetIdentifierName();

            if (!CheckAndConsume(TokenType.Assign))
            {
                if (!CheckAndConsume(TokenType.Semicolon))
                    errorHandler.UnexpectedToken(TokenType.Semicolon.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                instructions.Add(new DeclaringVariable(tokenType, name));
                return true;
            }

            var expression = ParseMathExpression();
            if (!CheckAndConsume(TokenType.Semicolon))
                errorHandler.UnexpectedToken(TokenType.Semicolon.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            if (expression is null)
                errorHandler.UnexpectStatement("Expression", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            else
                instructions.Add(new DeclaringVariable(tokenType, name, expression));
            return true;

        }


        bool TryParseExpressionOrAssignToMember(ref List<IInstruction> instructions)
        {
            var expression = ParseMathExpression();
            if (expression is null)
                return false;

            if (CheckAndConsume(TokenType.Assign))
            {
                var assignedExpression = ParseMathExpression();
                if (assignedExpression is null)
                    errorHandler.UnexpectStatement("Expression", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                if (!CheckAndConsume(TokenType.Semicolon))
                    errorHandler.UnexpectedToken(TokenType.Semicolon.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                instructions.Add(new AssigningToMember(expression, assignedExpression));
                return true;
            }

            if (!CheckAndConsume(TokenType.Semicolon))
                errorHandler.UnexpectedToken(TokenType.Semicolon.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            instructions.Add(new ExpressionInstruction(expression));
            return true;
        }

        ValueGetter ParseValueGetter()
        {
            var basicValueGetter = ParseBasicValueGetter();
            if (basicValueGetter is null)
                return null;

            var valueGetter = new List<IExpression>();
            valueGetter.Add(basicValueGetter);

            while (CheckAndConsume(TokenType.Dot))
            {
                basicValueGetter = ParseBasicValueGetter();
                if (basicValueGetter is null)
                    errorHandler.UnexpectStatement("Field Access", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                valueGetter.Add(basicValueGetter);
            }

            return new ValueGetter(valueGetter);
        }

        IExpression? ParseBasicValueGetter()
        {
            TokenType[] objectTypes = new[] { TokenType.File, TokenType.Directory };
            string? name = null;
            if (TokenIs(TokenType.Identifier))
                name = GetIdentifierName();
            else foreach (var t in objectTypes)
            {
                if (TokenIs(t))
                {
                    name = t.ToString();
                    GetNextToken();
                }
            }
            if (name == null)
                return null;

            var arguments = ParseFunctionCall();
            if (arguments is null)
                return new Variable(name);
            return new FunctionCall(name, arguments);
        }

        List<IExpression> ParseFunctionCall()
        {
            if (!CheckAndConsume(TokenType.ParenthesesOpen))
                return null;

            var arguments = ParseArguments();

            if (!CheckAndConsume(TokenType.ParenthesesClose))
                errorHandler.UnexpectedToken(TokenType.ParenthesesClose.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            return arguments;
        }

        List<IExpression> ParseArguments()
        {
            var arguments = new List<IExpression>();
            var argument = ParseMathExpression();
            if (argument is null)
                return arguments;

            arguments.Add(argument);

            while (CheckAndConsume(TokenType.Comma))
            {
                argument = ParseValueOrValueGetter();
                if (argument is null)
                    errorHandler.UnexpectStatement("Argument", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
                arguments.Add(argument);
            }
            return arguments;
        }

        IExpression? ParseValueOrValueGetter()
        {
            if (TokenIs(TokenType.StringLiteral))
            {
                var vorvg = (string)currentToken.TokenValue;
                GetNextToken();
                return new StringExpression(vorvg);
            }
            if (TokenIs(TokenType.IntLiteral))
            {
                var vorvg = (int)currentToken.TokenValue;
                GetNextToken();
                return new IntExpression(vorvg);
            }
            var valueGetter = ParseValueGetter();
            if (valueGetter is not null)
            {
                return valueGetter;
            }
            return null;

        }

        bool TryParseInstruction(ref List<IInstruction> instructions)
        {
            return TryParseDeclaration(ref instructions)
                || TryParseForeach(ref instructions)
                || TryParseIfStatement(ref instructions)
                || TryParseReturn(ref instructions)
                || TryParseExpressionOrAssignToMember(ref instructions);
        }

        List<IInstruction> ParseBlock()
        {
            var instructions = new List<IInstruction>();
            if (!CheckAndConsume(TokenType.BracketsOpen))
                return null;

            while (TryParseInstruction(ref instructions)) ;

            if (!CheckAndConsume(TokenType.BracketsClose))
                errorHandler.UnexpectedToken(TokenType.BracketsClose.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            return instructions;

        }

        Parameter ParseParameter()
        {
            if (!typeRemaper.TryGetValue(currentToken.TypeOfToken, out DataTypes tokenType))
                return null;
            GetNextToken();
            if (!TokenIs(TokenType.Identifier))
                errorHandler.UnexpectedToken(currentToken.TypeOfToken.ToString(), currentToken.Line, currentToken.Column, TokenType.Identifier.ToString());
            var name = GetIdentifierName();
            return new Parameter(tokenType, name);
        }

        List<Parameter> ParseParameters()
        {
            var parameters = new List<Parameter>();
            var parameter = ParseParameter();
            if (parameter is null)
                return parameters;
            parameters.Add(parameter);
            while(CheckAndConsume(TokenType.Comma))
            {
                parameter = ParseParameter();
                if (parameter is null)
                    errorHandler.MissingParameter(currentToken.Line, currentToken.Column);
                parameters.Add(parameter);
            }
            return parameters;
        }

        bool TryParseFunctionDefinition()
        {
            if (!typeRemaper.TryGetValue(currentToken.TypeOfToken, out DataTypes tokenType))
                return false;
            GetNextToken();

            if (!TokenIs(TokenType.Identifier))
                errorHandler.UnexpectedToken(TokenType.Identifier.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            var name = GetIdentifierName();

            if (!CheckAndConsume(TokenType.ParenthesesOpen))
                return false;

            var parameters = ParseParameters();
            
            if (!CheckAndConsume(TokenType.ParenthesesClose))
                errorHandler.UnexpectedToken(currentToken.TypeOfToken.ToString(), currentToken.Line, currentToken.Column, TokenType.ParenthesesClose.ToString());
            
            var block = ParseBlock();
            if (block is null)
                errorHandler.UnexpectStatement("Block Of Statements", currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());

            if (functions.ContainsKey(name))
                errorHandler.FunctionNameAlreadyExists(name, currentToken.Line, currentToken.Column);
            functions.Add(name, new FunctionDefinition(tokenType, name, parameters, block));
            return true;
        }

        bool TryParseDllLoad()
        {
            if (!CheckAndConsume(TokenType.DLLLOAD))
                return false;
            if (!TokenIs(TokenType.Identifier))
                errorHandler.UnexpectedToken(TokenType.Identifier.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            var name = GetIdentifierName();
            
            if (!CheckAndConsume(TokenType.Semicolon))
                errorHandler.UnexpectedToken(TokenType.Semicolon.ToString(), currentToken.Line, currentToken.Column, currentToken.TypeOfToken.ToString());
            dllLoaders.Add(new DllLoader(name));
            return true;
        }

        bool TryParse()
        {
            return TryParseDllLoad()
                || TryParseFunctionDefinition();
        }
        #endregion
    }
}