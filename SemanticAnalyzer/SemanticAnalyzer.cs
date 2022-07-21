using ParserModule;
using ErrorHandlerModule;
using Variant.StandardLibrary;
using System.Reflection;

namespace SemanticAnalyzerModule
{
    public class SemanticAnalyzer
    {
        #region Fields and Properties
        readonly ParsedProgram parsedProgram;
        readonly IErrorHandler errorHandler;
        Dictionary<string, (Type FunctionType, List<Type> ParameterTypes)> avaliableFunctions;
        Type currentlyValidatedFunctionType;
        string currentlyValidatedFunctionName;
        Dictionary<DataTypes, Type> literalTypes = new()
        {
            { DataTypes.Int, typeof(int) },
            { DataTypes.String, typeof(string) },
            { DataTypes.File, typeof(VariantFile) },
            { DataTypes.Directory, typeof(VariantDirectory) },
        };
        List<Dictionary<string, Type>> variableDeclarationLevels;
        bool discrepancyNotDetected;
        List<Type> incrementableTypes = new() 
        { 
            typeof(ValueGetter)
        };
        List<Type> negateableExpressions = new()
        {
            typeof(IncrementedExpression),
            typeof(LogicalExpression),
            typeof(MathExpression),
            typeof(MathMultiplication),
            typeof(ConditionConjunction),
            typeof(ValueGetter),
            typeof(Condition)
        };
        #endregion

        #region Constructor and Public Methods
        public SemanticAnalyzer(ParsedProgram program, IErrorHandler er)
        {
            discrepancyNotDetected = true;
            parsedProgram = program;
            errorHandler = er;
            avaliableFunctions = new();
            variableDeclarationLevels = new();

            RegisterStandardFunctions();
            RegisterStandardObjectsConstructors();
        }

        public bool ValidateProgram()
        {
            return ValidateAndRegisterDlls()
                && ValidateAndRegisterFunctionDeclaration()
                && ValidateFunctionBodies()
                && discrepancyNotDetected;
        }
        #endregion

        #region Helper Methods
        bool TryAddVariable(string name, Type type)
        {
            if (!variableDeclarationLevels.Last().TryAdd(name, type))
            {
                errorHandler.VariableAlreadyDeclared(name, 0, 0);
                return false;
            }
            return true;
        }

        void RemoveVariableBlock()
        {
            variableDeclarationLevels.RemoveAt(variableDeclarationLevels.Count - 1);
        }

        void AddVariableBlock()
        {
            variableDeclarationLevels.Add(new());
        }

        Type GetVariableType(string name)
        {
            for (int i = variableDeclarationLevels.Count - 1; i >= 0; i--)
            {
                if (variableDeclarationLevels[i].TryGetValue(name, out var type))
                    return type;

            }
            errorHandler.UnresolvedReference(name, 0, 0);
            discrepancyNotDetected = false;
            return typeof(void);
        }

        Type GetFunctionType(string name)
        {
            if (avaliableFunctions.TryGetValue(name, out var type))
                return type.FunctionType;
            errorHandler.UnresolvedReference(name, 0, 0);
            discrepancyNotDetected = false;
            return typeof(void);
        }

        List<Type> HandleFunctionParameters(FunctionDefinition functionDefinition)
        {
            var parameterTypes = new List<Type>();
            var parameterNames = new HashSet<string>();
            foreach (var parameter in functionDefinition.Parameters)
            {
                if (!parameterNames.Add(parameter.Name))
                {
                    errorHandler.ParameterDuplicated(parameter.Name, functionDefinition.Name);
                    discrepancyNotDetected = false;
                }
                parameterTypes.Add(literalTypes[parameter.Type]);
            }
            return parameterTypes;
        }

        Type GetExpressionType(IExpression expression)
        {
            switch (expression)
            {
                case LogicalExpression e:
                    return GetLogicalExpressionType(e);
                case ConditionConjunction e:
                    return GetConditionConjunctionType(e);
                case Condition e:
                    return GetConditionType(e);
                case MathExpression e:
                    return GetMathExpressionType(e);
                case MathMultiplication e:
                    return GetMathMultiplicationType(e);
                case NegatedExpression e:
                    return GetNegatedExpressionType(e);
                case IncrementedExpression e:
                    return GetIncrementedExpressionType(e);
                case ValueGetter e:
                    return GetValueGetterType(e);
                case StringExpression:
                    return typeof(string);
                case IntExpression:
                    return typeof(int);
                default:
                    errorHandler.UnrecognisedType(0, 0);
                    discrepancyNotDetected = false;
                    return typeof(void);
            }
        }

        Type GetIncrementedExpressionType(IncrementedExpression expression)
        {
            if (!incrementableTypes.Contains(expression.Expression.GetType()))
            {
                errorHandler.IllegalIncrement(0, 0);
                discrepancyNotDetected = false;
            }
            
            return GetExpressionType(expression.Expression);
        }

        Type GetNegatedExpressionType(NegatedExpression expression)
        {
            if (!negateableExpressions.Contains(expression.Expression.GetType()))
            {
                errorHandler.IllegalNegation(0, 0);
                discrepancyNotDetected = false;
            }
            return GetExpressionType(expression.Expression);
        }

        Type? GetValueGetterType(ValueGetter expression)
        {
            Type lastGetterType;
            var valueGetter = expression.ValueGetters[0];
            if (valueGetter is Variable variable)
            {
                lastGetterType = GetVariableType(variable.Identifier);
            }
            else if (valueGetter is FunctionCall functionCall)
            {
                lastGetterType = GetFunctionType(functionCall.Identifier);
            }
            else
            {
                return null;
            }

            foreach (var getter in expression.ValueGetters.Skip(1))
            {
                bool successorFound = false;
                if (getter is FunctionCall followingFunctionCall)
                {
                    MethodBase[] mInfos = lastGetterType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var method in mInfos)
                    {
                        if (method.Name == followingFunctionCall.Identifier)
                        {
                            lastGetterType = method.GetType();
                            successorFound = true;
                            break;
                        }
                    }
                    if (successorFound)
                        continue;
                }
                else if (getter is Variable followingVariable)
                {
                    PropertyInfo[] pInfos = lastGetterType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var property in pInfos)
                    {
                        if (property.Name == followingVariable.Identifier)
                        {
                            lastGetterType = property.PropertyType;
                            successorFound = true;
                            break;
                        }
                    }
                    if (successorFound)
                        continue;
                }
                return null;
            }
            return lastGetterType;
        }

        Type GetMathMultiplicationType(MathMultiplication expression)
        {
            var illegalZeroOperations = new List<MultiplicationOperators>() { MultiplicationOperators.Divide, MultiplicationOperators.Modulo };
            var initialExpression = GetExpressionType(expression.InitExpression);
            if (initialExpression == typeof(int))
            {
                foreach (var multiplyOperation in expression.MultiplyOperations)
                {
                    if (!CompareTypes(GetExpressionType(multiplyOperation.MathValue), typeof(int)))
                    {
                        discrepancyNotDetected = false;
                        return typeof(void);
                    }
                    if (IsExpressionZero(multiplyOperation.MathValue) && illegalZeroOperations.Contains(multiplyOperation.Operator))
                    {
                        errorHandler.IllegalZeroOperation(0, 0);
                        discrepancyNotDetected = false;
                    }
                }
                return typeof(int);
            }
            errorHandler.WrongType(typeof(int), initialExpression, 0, 0);
            discrepancyNotDetected = false;
            return typeof(void);
        }

        Type GetMathExpressionType(MathExpression expression)
        {
            var allowedStringOperations = new List<AdditionMathOperators>() { AdditionMathOperators.Add };
            var initialExpression = GetExpressionType(expression.InitExpression);
            if (initialExpression == typeof(string))
            {
                foreach(var additionOperation in expression.AdditionOperations)
                {
                    if (!allowedStringOperations.Contains(additionOperation.Operator) || GetExpressionType(additionOperation.MathValue) != typeof(string))
                    {
                        errorHandler.IllegalStringOperation(0, 0);
                        discrepancyNotDetected = false;
                        return typeof(void);
                    }
                }
                return typeof(string);
            }
            if (initialExpression == typeof(int))
            {
                foreach (var additionOperation in expression.AdditionOperations)
                {
                    if (GetExpressionType(additionOperation.MathValue) != typeof(int))
                    {
                        discrepancyNotDetected = false;
                        return typeof(void);
                    }
                }
                return typeof(int);
            }
            errorHandler.WrongType(typeof(int), initialExpression, 0, 0);
            discrepancyNotDetected = false;
            return typeof(void);
        }

        Type GetLogicalExpressionType(LogicalExpression expression)
        {
            var initialExpression = GetExpressionType(expression.InitExpression);
            if (initialExpression == typeof(bool))
            {
                foreach (var multiplyOperation in expression.Alternatives)
                {
                    if (GetExpressionType(multiplyOperation) != typeof(bool))
                    {
                        errorHandler.WrongType(typeof(bool), GetExpressionType(multiplyOperation), 0, 0);
                        discrepancyNotDetected = false;
                        return typeof(void);
                    }
                }
                return typeof(bool);
            }
            errorHandler.WrongType(typeof(bool), initialExpression, 0, 0);
            discrepancyNotDetected = false;
            return typeof(void);
        }

        Type GetConditionConjunctionType(ConditionConjunction expression)
        {
            var initialExpression = GetExpressionType(expression.InitExpression);
            if (initialExpression == typeof(bool))
            {
                foreach (var multiplyOperation in expression.Conjunctions)
                {
                    if (GetExpressionType(multiplyOperation) != typeof(bool))
                    {
                        errorHandler.WrongType(typeof(bool), GetExpressionType(multiplyOperation), 0, 0);
                        discrepancyNotDetected = false;
                        return typeof(void);
                    }
                }
                return typeof(bool);
            }
            errorHandler.WrongType(typeof(bool), initialExpression, 0, 0);
            discrepancyNotDetected = false;
            return typeof(void);
        }

        Type GetConditionType(Condition expression)
        {
            var allowedStringOperations = new List<ComparisionOperators>() { ComparisionOperators.Equals, ComparisionOperators.NotEquals };
            var initialExpression = GetExpressionType(expression.MathExpression);
            var comparedExpression = GetExpressionType(expression.ComparedMathExpression);
            var comparisionOperator = expression.ComparisionOperator;
            return (
                (initialExpression == typeof(int) && comparedExpression == typeof(int)) 
                || (initialExpression == typeof(string) && comparedExpression == typeof(string) && allowedStringOperations.Contains(comparisionOperator))
                ) ? typeof(bool) : typeof(void);
        }

        bool CompareTypes(Type type1, Type type2)
        {
            bool result = type1 == type2;
            if (!result)
                errorHandler.WrongType(type1, type2, 0, 0);
            return result;
        }

        bool IsExpressionZero(IExpression expression)
        {
            if (expression is IntExpression intExpression)
            {
                if (intExpression.Value == 0)
                    return true;
            }
            return false;
        }

        void RegisterStandardFunctions()
        {
            MethodInfo[] mInfos = typeof(VariantStandardFunctions).GetMethods(
                BindingFlags.DeclaredOnly |
                BindingFlags.Public |
                BindingFlags.Instance);
            foreach (var method in mInfos)
            {
                var name = method.Name;
                ParameterInfo[] Myarray = method.GetParameters();
                var methodParams = new List<Type>();
                foreach (var param in Myarray)
                {
                    methodParams.Add((Type)param.ParameterType);
                }
                avaliableFunctions.TryAdd(name, (method.ReturnType, methodParams));
            }
        }

        void RegisterStandardObjectsConstructors()
        {
            avaliableFunctions.Add("File", (typeof(VariantFile), new() { typeof(string) }));
            avaliableFunctions.Add("Directory", (typeof(VariantDirectory), new() { typeof(string) }));
        }
        #endregion

        #region Validators
        bool InstructionBlockCanReturn(List<IInstruction> instructions)
        {
            try
            {
                var returnInstruction = instructions.First(x => x is Returning);
                return true;
            }
            catch
            {
                try
                {
                    var instructionWithBlocks = instructions.Where(x => x is ForeachLoop || x is IfOrIfElse).ToList();
                    for (int i = instructionWithBlocks.Count()-1; i >= 0; i--)
                    {
                        var instructionWithBlock = instructionWithBlocks[i];
                        // instruction is a foreach loop and has return in it's block then ok
                        // or instruction is an if statement then it has to have an else statement and return instructions in both main block and else block, so it returns something regardless of condition
                        if ((instructionWithBlock is ForeachLoop foreachLoop && InstructionBlockCanReturn(foreachLoop.InstructionBlock))
                            || (instructionWithBlock is IfOrIfElse ifOrIfElse && ifOrIfElse.ElseInstructionBlock is not null
                                && InstructionBlockCanReturn(ifOrIfElse.MainInstructionBlock) && InstructionBlockCanReturn(ifOrIfElse.ElseInstructionBlock)))
                            return true;
                    }
                    errorHandler.FunctionDoesNotReturnAnything(currentlyValidatedFunctionName);
                    return false;
                }
                catch
                {
                    errorHandler.FunctionDoesNotReturnAnything(currentlyValidatedFunctionName);
                    return false;
                }
            }
        }

        bool ValidateAndRegisterFunctionDeclaration()
        {
            bool mainOccured = false;
            foreach (var functionDefinition in parsedProgram.Functions)
            {
                mainOccured |= functionDefinition.Key == "main";
                if (!avaliableFunctions.TryAdd(
                    functionDefinition.Key, 
                    (literalTypes[functionDefinition.Value.Type], 
                    HandleFunctionParameters(functionDefinition.Value))))
                {
                    errorHandler.FunctionAlreadyDeclared(functionDefinition.Key, 0, 0);
                    return false;
                }
            }
            if (!mainOccured)
                errorHandler.MainNotOccured();
            return true && mainOccured;
        }

        bool ValidateAndRegisterDlls()
        {
            foreach (var dll in parsedProgram.DllLoaders)
            {
                if (!avaliableFunctions.TryAdd(dll.Name, (typeof(void), new() { typeof(string), typeof(string) })))
                {
                    errorHandler.FunctionAlreadyDeclared(dll.Name, 0, 0);
                    return false;
                }
            }
            return true;
        }

        bool ValidateFunctionBodies()
        {
            foreach (var functionDefinition in parsedProgram.Functions)
            {
                if (!ValidateFunction(functionDefinition.Value))
                    return false;

            }
            return true;
        }

        bool ValidateFunction(FunctionDefinition functionDefinition)
        {
            List<(string Name, Type Type)> parameters = new() { };
            foreach (var parameter in functionDefinition.Parameters)
            {
                parameters.Add((parameter.Name, literalTypes[parameter.Type]));
            }
            currentlyValidatedFunctionType = literalTypes[functionDefinition.Type];
            currentlyValidatedFunctionName = functionDefinition.Name;
            return InstructionBlockCanReturn(functionDefinition.Instructions)
                && ValidateInstructionBlock(functionDefinition.Instructions, parameters);
        }

        bool ValidateInstructionBlock(List<IInstruction> instructions, List<(string Name, Type Type)>? blockVariables=null)
        {
            AddVariableBlock();
            if (blockVariables is not null)
            {
                foreach (var blockVariable in blockVariables)
                {
                    if (!TryAddVariable(blockVariable.Name, blockVariable.Type))
                    {
                        errorHandler.VariableAlreadyDeclared(blockVariable.Name, 0, 0);
                        return false;
                    }
                }
            }
            foreach (var instruction in instructions)
            {
                if (!ValidateInstruction(instruction))
                {
                    RemoveVariableBlock();
                    return false;
                }
            }
            RemoveVariableBlock();
            return true;
        }

        bool ValidateInstruction(IInstruction instruction)
        {
            switch (instruction)
            {
                case Returning e:
                    return ValidateReturning(e.ReturnedExpression);
                case ForeachLoop e:
                    return ValidateForeachLoop(e);
                case ExpressionInstruction e:
                    return ValidateExpressionInstruction(e);
                case DeclaringVariable e:
                    return ValidateDeclaration(e);
                case AssigningToMember e:
                    return ValidateAssignment(e);
                case IfOrIfElse e:
                    return ValidateIfStatement(e);
                default:
                    return false;
            }
        }

        bool ValidateReturning(IExpression returnedExpression)
        {
            return CompareTypes(GetExpressionType(returnedExpression), currentlyValidatedFunctionType);
        }

        bool ValidateExpressionInstruction(ExpressionInstruction instruction)
        {
            return ValidateExpression(instruction.Expression);
        }

        bool ValidateExpression(IExpression expression)
        {
            GetExpressionType(expression);
            return discrepancyNotDetected; 
        }

        bool ValidateIfStatement(IfOrIfElse instruction)
        {
            var expressionType = GetExpressionType(instruction.CheckedExpression);
            bool isExpressionOk = CompareTypes(expressionType, typeof(bool));
            if (!isExpressionOk)
                errorHandler.WrongType(typeof(bool), expressionType, 0, 0);
            return isExpressionOk
                && ValidateInstructionBlock(instruction.MainInstructionBlock)
                && (instruction.ElseInstructionBlock is null || ValidateInstructionBlock(instruction.ElseInstructionBlock));
        }

        bool ValidateForeachLoop(ForeachLoop instruction)
        {
            Type loopVariableType = literalTypes[instruction.LoopVariableType];
            var expressionType = GetExpressionType(instruction.LoopExpression);
            bool isTypeOk = CompareTypes(typeof(List<>).MakeGenericType(new[] { loopVariableType }), expressionType);
            return isTypeOk
                && TryAddVariable(instruction.LoopVariableName, loopVariableType)
                && ValidateInstructionBlock(instruction.InstructionBlock);
        }

        bool ValidateDeclaration(DeclaringVariable instruction)
        {
            var variableType = literalTypes[instruction.Type];
            return (instruction.AssignedExpression is null || CompareTypes(GetExpressionType(instruction.AssignedExpression), variableType))
                && TryAddVariable(instruction.Name, variableType);
        }

        bool ValidateAssignment(AssigningToMember instruction)
        {
            return instruction.Member is ValueGetter valueGetter
                && CompareTypes(GetExpressionType(valueGetter), GetExpressionType(instruction.AssignedExpression));
        }
        #endregion
    }
}