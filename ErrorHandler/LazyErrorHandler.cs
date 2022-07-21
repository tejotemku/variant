namespace ErrorHandlerModule
{
    public class LazyErrorHandler : IErrorHandler
    {
        public void UnknownEscapeCharacter(int line, int collumn)
        {
            Console.WriteLine($"ERROR: Unknown Escape Character in string on line {line}, collumn {collumn}.");
        }

        public void IntTooBig(int line, int collumn)
        {
            Console.WriteLine($"ERROR: Integer value too big on line {line}, collumn {collumn}.");
            StopProgram();
        }

        public void FunctionNameAlreadyExists(string name, int line, int collumn)
        {
            Console.WriteLine($"ERROR: Function under name {name} has already been declared on {line}, collumn {collumn}.");
        }

        public void StringNotClosed(int line, int collumn)
        {
            Console.WriteLine($"ERROR: String was not properly closed on line {line}, collumn {collumn}.");
            StopProgram();
        }

        public void IdenfitierTooLong(int line, int collumn, int lengthLimit)
        {
            Console.WriteLine($"ERROR: Identifier too long (exceeds {lengthLimit} character limit) on line {line}, collumn {collumn}.");
        }

        public void StringTooLong(int line, int collumn, int lengthLimit)
        {
            Console.WriteLine($"ERROR: String too long (exceeds {lengthLimit} character limit) on line {line}, collumn {collumn}.");
        }

        public void IdenfitierTooLong(int line, int collumn)
        {
            Console.WriteLine($"ERROR: Identifier too long on line {line}, collumn {collumn}.");
        }

        public void IdentifierIsNull(int line, int collumn)
        {
            Console.Write($"ERROR: Identifier is null on line {line}, collumn {collumn}.");
            StopProgram();
        }

        public void UnexpectedToken(string expectedToken, int line, int collumn, string receivedToken)
        {
            Console.WriteLine($"ERROR: {(expectedToken is null? "" : " Expected " + expectedToken + ", ") }Received {receivedToken} on line {line}, collumn {collumn}.");
        }

        public void UnexpectStatement(string expected, int line, int collumn, string receivedToken)
        {
            Console.WriteLine($"ERROR: {expected}, Received {receivedToken} on line {line}, collumn {collumn}.");
        }

        public void MissingParameter(int line, int collumn)
        {
            Console.WriteLine($"ERROR: Missing parameter on line {line}, collumn {collumn}.");
            StopProgram();
        }

        public void ThrowHollow(string typeOfToken, string tokenValue, int line, int column) 
        {
            Console.WriteLine($"Empty Exception, specific exception not yet implemented. \nInfo: \nToken Type - {typeOfToken}\nValue - {tokenValue}\nLine {line}, Column {column}");
            throw new Exception("Wywałka");
        }

        public void FunctionAlreadyDeclared(string name, int line, int collumn)
        {
            Console.WriteLine($"Function under name {name} on line {line}, collumn {collumn} already exists in that namespace. ");
        }

        public void ParameterDuplicated(string paramName, string funcName)
        {
            Console.WriteLine($"Parameter {paramName} already exists in function {funcName}. ");
        }

        public void UnrecognisedType(int line, int collumn)
        {
            Console.WriteLine($"Unsupported type on  on line {line}, collumn {collumn}.");
        }

        public void UnresolvedReference(string name, int line, int collumn)
        {
            Console.WriteLine($"Variable or function {name} referenced before declaration on line {line}, collumn {collumn}.");
        }

        public void WrongType(Type expected, Type received, int line, int collumn)
        {
            Console.WriteLine($"Expected type {expected}, received {received} on line {line}, collumn {collumn}.");
        }

        public void FunctionDoesNotReturnAnything(string name)
        {
            Console.WriteLine($"Function {name} does not return anything.");
        }

        public void IllegalStringOperation(int line, int collumn)
        {
            Console.WriteLine($"Illegal operation on string on line {line}, collumn {collumn}.");
        }

        public void IllegalZeroOperation(int line, int collumn)
        {
            Console.WriteLine($"Ilegal modulo or division operation using 0 on line {line}, collumn {collumn}.");
        }

        public void IllegalIncrement(int line, int collumn)
        {
            Console.WriteLine($"Illegal increment on line {line}, collumn {collumn}.");
        }

        public void IllegalNegation(int line, int collumn)
        {
            Console.WriteLine($"Illegal negation on line {line}, collumn {collumn}.");
        }

        public void VariableAlreadyDeclared(string name, int line, int collumn)
        {
            Console.WriteLine($"Variable under name {name} on line {line}, collumn {collumn} already exists in that namespace. ");
        }

        public void MainNotOccured()
        {
            Console.Write("Program lacks function called \"main\"");
        }

        public void VariableDoesNotExist(string name, int line, int collumn)
        {
            Console.WriteLine($"Variable {name} does not exist on line {line}, collumn {collumn}");
        }

        public void IllegalExpression(int line, int collumn)
        {
            Console.WriteLine($"Illegal expression on line {line}, collumn {collumn}");
        }

        public void IllegalAdditionOperation(object obj1, object obj2, int line, int collumn)
        {
            Console.WriteLine($"Illegal addition between {obj1} and {obj2} operation on line {line}, collumn {collumn}");
        }

        public void IllegalMultiplicationOperation(int line, int collumn)
        {
            Console.WriteLine($"Illegal multiply operation on line {line}, collumn {collumn}");
        }

        public void IllegalInstruction(int line, int collumn)
        {
            Console.WriteLine($"Illegal instruction on line {line}, collumn {collumn}.");
        }

        private void StopProgram()
        {
            Console.WriteLine("Stopping program.");
            Environment.Exit(-1);
        }
    }
}