using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHandlerModule
{
    public interface IErrorHandler
    {
        public void UnknownEscapeCharacter(int line, int collumn);

        public void IntTooBig(int line, int collumn);

        public void FunctionNameAlreadyExists(string name, int line, int collumn);

        public void StringNotClosed(int line, int collumn);

        public void IdenfitierTooLong(int line, int collumn, int lengthLimit);

        public void StringTooLong(int line, int collumn, int lengthLimit);

        public void IdentifierIsNull(int line, int collumn);

        public void UnexpectedToken(string expectedToken, int line, int collumn, string receivedToken);

        public void UnexpectStatement(string expected, int line, int collumn, string receivedToken);

        public void MissingParameter(int line, int collumn);

        public void ThrowHollow(string typeOfToken, string tokenValue, int line, int column);

        public void FunctionAlreadyDeclared(string name, int line, int collumn);

        public void ParameterDuplicated(string paramName, string funcName);

        public void UnrecognisedType(int line, int collumn);

        public void UnresolvedReference(string name, int line, int collumn);

        public void WrongType(Type expected, Type received, int line, int collumn);

        public void FunctionDoesNotReturnAnything(string name);

        public void IllegalStringOperation(int line, int collumn);

        public void IllegalZeroOperation(int line, int collumn);

        public void IllegalIncrement(int line, int collumn);

        public void IllegalNegation(int line, int collumn);

        public void VariableAlreadyDeclared(string name, int line, int collumn);

        public void VariableDoesNotExist(string name, int line, int collumn);

        public void IllegalExpression(int line, int collumn);

        public void IllegalAdditionOperation(object obj1, object obj2, int line, int collumn);

        public void IllegalMultiplicationOperation(int line, int collumn);

        public void IllegalInstruction(int line, int collumn);

        public void MainNotOccured();
    }
}
