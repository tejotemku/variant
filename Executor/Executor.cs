using ParserModule;
using ErrorHandlerModule;
using Variant.StandardLibrary;
using ImageEditingLib;
using System.Reflection;

namespace ExecutorModule
{
    public class Executor
    {
        #region Fields and Properties
        readonly ParsedProgram parsedProgram;
        readonly IErrorHandler errorHandler;
        Dictionary<string, MethodBase> standardLibFunctions;
        Dictionary<string, MethodBase> loadedDlls;
        Dictionary<string, FunctionDefinition> declaredFunctions;
        Dictionary<string, ConstructorInfo> standardLibClasses;
        List<Dictionary<string, object?>> variableDeclarationLevels;
        #endregion

        #region Constructor and Public Methods
        public Executor(ParsedProgram program, IErrorHandler er)
        {
            parsedProgram = program;
            errorHandler = er;
            standardLibFunctions = new();
            declaredFunctions = new();
            standardLibClasses = new();
            loadedDlls = new();
            variableDeclarationLevels = new();
            RegisterStandardLibFunction();
            RegisterStandardLibClasses();
        }

        public void ExecuteProgram()
        {
            RegisterDLLs();
            RegisterDeclaredFunction();
            ExecuteMain();
        }
        #endregion

        #region Helper Methods
        void RegisterStandardLibFunction()
        {
            MethodBase[] mInfos = typeof(VariantStandardFunctions).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in mInfos)
            {
                standardLibFunctions.Add(method.Name, method);
            }
        }

        void RegisterStandardLibClasses()
        {
            List<(Type Constructor, Type[] Params)> classes = new() 
            { 
                (typeof(VariantFile), new[] { typeof(string) }), 
                (typeof(VariantDirectory), new[] { typeof(string) }) 
            };
            foreach (var clazz in classes)
            {
                standardLibClasses.Add(clazz.Constructor.Name.ToString().Replace("Variant", ""), clazz.Constructor.GetConstructor(clazz.Params));
            }
        }

        void RegisterDLLs()
        {
            var dllLibs = new List<Type>() { typeof(ImageEditingLib.ImageEditingLibrary) };

            foreach (var dllLib in dllLibs)
            {
                MethodBase[] mInfos = dllLib.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in mInfos)
                {
                    loadedDlls.Add(method.Name.ToLower(), method);
                }
            }
        }

        void RegisterDeclaredFunction()
        {
            foreach (var functionDefinition in parsedProgram.Functions)
            {
                declaredFunctions.Add(
                     functionDefinition.Key,
                     functionDefinition.Value);
            }
        }

        void RemoveVariableBlock()
        {
            variableDeclarationLevels.RemoveAt(variableDeclarationLevels.Count - 1);
        }

        void AddVariableBlock()
        {
            variableDeclarationLevels.Add(new());
        }

        void AddVariable(string name, object? value)
        {
            variableDeclarationLevels.Last().Add(name, value);
        }

        object? GetVariableValue(string name)
        {
            for (int i = variableDeclarationLevels.Count - 1; i >= 0; i--)
            {
                if (variableDeclarationLevels[i].TryGetValue(name, out var value))
                    return value;
            }
            errorHandler.VariableDoesNotExist(name, 0, 0);
            return null;
        }

        void SetVariableValue(string name, object value)
        {
            for (int i = variableDeclarationLevels.Count - 1; i >= 0; i--)
            {
                if (variableDeclarationLevels[i].ContainsKey(name))
                    variableDeclarationLevels[i][name] = value;
            }
        }

        object[]? ExecuteArguments(List<IExpression> arguments)
        {
            return arguments.Select(arg => ExecuteExpression(arg)).ToArray();
        }

        object? ExecuteFunction(string name, object[]? arguments)
        {
            if (standardLibFunctions.ContainsKey(name))
            {
                var method = standardLibFunctions[name];
                return method.Invoke(new VariantStandardFunctions(), arguments);
            }
            else if (loadedDlls.ContainsKey(name))
            {
                var method = loadedDlls[name];
                return method.Invoke(new ImageEditingLib.ImageEditingLibrary(), arguments);
            }
            else if (standardLibClasses.ContainsKey(name))
            {
                ConstructorInfo standardClass = standardLibClasses[name];
                return standardClass.Invoke(arguments);
            }
            else if(declaredFunctions.ContainsKey(name))
            {
                var declaredFunction = declaredFunctions[name];
                AddVariableBlock();
                for (int i =0; i<declaredFunction.Parameters.Count; i++)
                {
                    AddVariable(declaredFunction.Parameters[i].Name, arguments[i]);
                }
                var result = ExecuteInstructionBlock(declaredFunction.Instructions);
                RemoveVariableBlock();
                return result;
            }
            return null;
        }


        object? ExecuteExpression(IExpression expression)
        {
            switch (expression)
            {
                case LogicalExpression e:
                    return ExecuteConditionAlternative(e);
                case ConditionConjunction e:
                    return ExecuteConditionConjunction(e);
                case Condition e:
                    return ExecuteCondition(e);
                case MathExpression e:
                    return ExecuteMathExpression(e);
                case MathMultiplication e:
                    return ExecuteMathMultiplication(e);
                case ValueGetter e:
                    return ExecuteValueGetter(e);
                case IntExpression e:
                    return ExecuteIntExpression(e);
                case StringExpression e:
                    return ExecuteStringExpression(e);
                case IncrementedExpression e:
                    return ExecuteIncrementedExpression(e);
                case NegatedExpression e:
                    return ExecuteNegatedExpression(e);
                default:
                    errorHandler.IllegalExpression(0, 0);
                    return null;
            }
        }

        object? ExecuteNegatedExpression(NegatedExpression expression)
        {
            var expr = ExecuteExpression(expression.Expression);
            if (expr is int intExpr)
                return -intExpr;
            if (expr is bool boolExpr)
                return !boolExpr;
            errorHandler.IllegalNegation(0, 0);
            return null;
        }

        int? ExecuteIncrementedExpression(IncrementedExpression expression)
        {
            var expr = ExecuteExpression(expression.Expression);
            if (expr is int intExpr)
            { 
                return (int)SetMemberValue((ValueGetter)expression.Expression, intExpr + 1);
            }
            errorHandler.IllegalIncrement(0, 0);
            return null;
        }

        bool ExecuteConditionAlternative(LogicalExpression expression)
        {
            bool result = (bool)ExecuteExpression(expression.InitExpression);
            foreach (var alternative in expression.Alternatives)
            {
                result |= (bool)ExecuteExpression(alternative);
            }
            
            return result;
        }

        bool ExecuteConditionConjunction(ConditionConjunction expression)
        {
            bool result = (bool)ExecuteExpression(expression.InitExpression);
            foreach (var conjunction in expression.Conjunctions)
            {
                result &= (bool)ExecuteExpression(conjunction);
            }
            return result;
        }

        bool ExecuteCondition(Condition expression)
        {
            object expression1 = ExecuteExpression(expression.MathExpression);
            object expression2 = ExecuteExpression(expression.ComparedMathExpression);
            switch (expression.ComparisionOperator)
            {
                case ComparisionOperators.Equals:
                    return expression1 is int? (int)expression1 == (int)expression2 : (string)expression1 == (string)expression2;
                case ComparisionOperators.NotEquals:
                    return expression1 is int ? (int)expression1 != (int)expression2 : (string)expression1 != (string)expression2;
                case ComparisionOperators.Greater:
                    return (int)expression1 > (int)expression2;
                case ComparisionOperators.GreaterOrEqual:
                    return (int)expression1 >= (int)expression2;
                case ComparisionOperators.Lesser:
                    return (int)expression1 < (int)expression2;
                case ComparisionOperators.LesserOrEqual:
                    return (int)expression1 <= (int)expression2;
                default:
                    return false;
            }
        }

        object? ExecuteMathExpression(MathExpression expression)
        {
            var initExpression = ExecuteExpression(expression.InitExpression);
            if (initExpression is string stringExpression)
            {
                foreach (var operation in expression.AdditionOperations)
                {
                    stringExpression += (string)ExecuteExpression(operation.MathValue);
                }
                return stringExpression;
            }
            if (initExpression is int intExpression)
            {
                foreach (var operation in expression.AdditionOperations)
                {
                    var addedValue = (int)ExecuteExpression(operation.MathValue);
                    switch (operation.Operator)
                    {
                        case AdditionMathOperators.Add:
                            intExpression += addedValue;
                            break;
                        case AdditionMathOperators.Subtract:
                            intExpression -= addedValue;
                            break;
                        default:
                            errorHandler.IllegalAdditionOperation(intExpression, addedValue, 0, 0);
                            return null;
                    }
                }
                return intExpression;
            }
            errorHandler.IllegalAdditionOperation(initExpression, null, 0, 0);
            return null;
        }

        int? ExecuteMathMultiplication(MathMultiplication expression)
        {
            int result = (int)ExecuteExpression(expression.InitExpression);
            foreach (var operation in expression.MultiplyOperations)
            {
                switch (operation.Operator)
                {
                    case MultiplicationOperators.Multiply:
                        result *= (int)ExecuteExpression(operation.MathValue);
                        break;
                    case MultiplicationOperators.Divide:
                        int divider = (int)ExecuteExpression(operation.MathValue);
                        if (divider == 0)
                            errorHandler.IllegalZeroOperation(0, 0);
                        result /= divider;
                        break;
                    case MultiplicationOperators.Modulo:
                        result %= (int)ExecuteExpression(operation.MathValue);
                        break;
                    default:
                        errorHandler.IllegalMultiplicationOperation(0, 0);
                        return null;
                }
            }
            return result;
        }

        string ExecuteStringExpression(StringExpression expression)
        {
            return expression.Value;
        }

        int ExecuteIntExpression(IntExpression expression)
        {
            return expression.Value;
        }

        object? ExecuteValueGetter(ValueGetter expression)
        {
            object? lastGetterValue;
            var valueGetter = expression.ValueGetters[0];
            if (valueGetter is Variable variable)
            {
                lastGetterValue = GetVariableValue(variable.Identifier);
            }
            else if (valueGetter is FunctionCall functionCall)
            {
                lastGetterValue = ExecuteFunction(functionCall.Identifier, ExecuteArguments(functionCall.Args));
            }
            else
            {
                return null;
            }

            foreach (var getter in expression.ValueGetters.Skip(1))
            {
                if (getter is FunctionCall followingFunctionCall)
                {
                    MethodBase[] mInfos = lastGetterValue.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                    var methods = mInfos.Where(method => method.Name == followingFunctionCall.Identifier).ToArray();
                    if (methods.Length > 0)
                    {
                        lastGetterValue = methods[0].Invoke(lastGetterValue, ExecuteArguments(followingFunctionCall.Args));
                        continue;
                    }
                }
                else if (getter is Variable followingVariable)
                {
                    PropertyInfo[] pInfos = lastGetterValue.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                    var properties = pInfos.Where(property => property.Name == followingVariable.Identifier).ToArray();
                    if (properties.Length > 0)
                    {
                        lastGetterValue = properties[0].GetValue(lastGetterValue);
                        continue;
                    }
                }
                return null;
            }
            return lastGetterValue;
        }

        object? SetMemberValue(ValueGetter expression, object value)
        {
            object? lastGetterValue;
            var valueGetter = expression.ValueGetters[0];
            if (valueGetter is Variable variable)
            {
                if (expression.ValueGetters.Count == 1)
                    SetVariableValue(variable.Identifier, value);
                lastGetterValue = GetVariableValue(variable.Identifier);
            }
            else if (valueGetter is FunctionCall functionCall)
                lastGetterValue = ExecuteFunction(functionCall.Identifier, ExecuteArguments(functionCall.Args));
            else
                lastGetterValue = null;

            foreach (var getter in expression.ValueGetters.Skip(1))
            {
                bool successorFound = false;
                if (getter is FunctionCall followingFunctionCall)
                {
                    MethodBase[] mInfos = typeof(VariantStandardFunctions).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var method in mInfos)
                    {
                        if (method.Name == followingFunctionCall.Identifier)
                        {
                            lastGetterValue = method.Invoke(lastGetterValue, ExecuteArguments(followingFunctionCall.Args));
                            successorFound = true;
                            break;
                        }
                    }
                    if (successorFound)
                        continue;
                }
                else if (getter is Variable followingVariable)
                {
                    FieldInfo[] fInfos = typeof(VariantStandardFunctions).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fInfos)
                    {
                        if (field.Name == followingVariable.Identifier)
                        {
                            lastGetterValue = field.GetValue(lastGetterValue);
                            successorFound = true;
                            break;
                        }
                    }
                    if (successorFound)
                        continue;
                }
            }
            lastGetterValue = value;
            return lastGetterValue;
        }

        #endregion

        #region Executors
        void ExecuteMain()
        {
            var mainDefinition = declaredFunctions["main"];
            AddVariableBlock();
            ExecuteInstructionBlock(mainDefinition.Instructions);
            RemoveVariableBlock();
        }

        object? ExecuteInstructionBlock(List<IInstruction> instructions)
        {
            foreach(var instruction in instructions)
            {
                var returnedValue = ExecuteInstruction(instruction);
                if (returnedValue is not null && instruction is not ExpressionInstruction)
                {
                    return returnedValue;
                }
            }
            return null;
        }

        object? ExecuteInstruction(IInstruction instruction)
        {
            switch (instruction)
            {
                case Returning returning:
                    return ExecuteReturning(returning);
                case ForeachLoop foreachLoop:
                    return ExecuteForeach(foreachLoop);
                case DeclaringVariable declaringVariable:
                    return ExecuteDeclaringVariable(declaringVariable);
                case AssigningToMember assigningToMember:
                    return ExecuteAssigningMember(assigningToMember);
                case IfOrIfElse ifOrIfElse:
                    return ExecuteIfOrIfElse(ifOrIfElse);
                case ExpressionInstruction expression:
                    return ExecuteExpressionInstruction(expression);
                default:
                    errorHandler.IllegalInstruction(0, 0);
                    return null;

            }
        }

        object? ExecuteReturning(Returning instruction)
        {
            return ExecuteExpression(instruction.ReturnedExpression);
        }

        object? ExecuteForeach(ForeachLoop instruction)
        {
            var loopExpression = ExecuteExpression(instruction.LoopExpression);
            if (loopExpression is List<VariantFile> fileList)
            {
                foreach (var variable in fileList)
                {
                    AddVariableBlock();
                    AddVariable(instruction.LoopVariableName, variable);
                    var returnedVariable = ExecuteInstructionBlock(instruction.InstructionBlock);
                    RemoveVariableBlock();
                    if (returnedVariable is not null)
                        return returnedVariable;
                }
            }
            if (loopExpression is List<VariantDirectory> directoryList)
            {
                foreach (var variable in directoryList)
                {
                    AddVariableBlock();
                    AddVariable(instruction.LoopVariableName, variable);
                    var returnedVariable = ExecuteInstructionBlock(instruction.InstructionBlock);
                    RemoveVariableBlock();
                    if (returnedVariable is not null)
                        return returnedVariable;
                }
            }
            return null;
        }

        object? ExecuteIfOrIfElse(IfOrIfElse instruction)
        {
            if ((bool)ExecuteExpression(instruction.CheckedExpression))
            {
                AddVariableBlock();
                var result = ExecuteInstructionBlock(instruction.MainInstructionBlock);
                RemoveVariableBlock();
                return result;
            }
            else if (instruction.ElseInstructionBlock is not null)
            {
                AddVariableBlock();
                var result = ExecuteInstructionBlock(instruction.ElseInstructionBlock);
                RemoveVariableBlock();
                return result;
            }
            return null;
        }

        object? ExecuteExpressionInstruction(ExpressionInstruction instruction)
        {
            return ExecuteExpression(instruction.Expression);
        }

        object? ExecuteDeclaringVariable(DeclaringVariable instruction)
        {
            AddVariable(instruction.Name, instruction.AssignedExpression is not null ? ExecuteExpression(instruction.AssignedExpression) : null);
            return null;
        }

        object? ExecuteAssigningMember(AssigningToMember instruction)
        {
            SetMemberValue((ValueGetter)instruction.Member, ExecuteExpression(instruction.AssignedExpression));
            return null;
        }
        #endregion
    }
}