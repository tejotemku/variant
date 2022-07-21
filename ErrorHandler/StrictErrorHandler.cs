using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHandlerModule
{
    public class StrictErrorHandler : IErrorHandler
    {
        #region LexerExceptions
        public void UnknownEscapeCharacter(int line, int collumn)
        {
            throw new UnknownEscapeCharacterException();
        }

        public void IntTooBig(int line, int collumn)
        {
            throw new IntTooBigException();
        }

        public void StringNotClosed(int line, int collumn)
        {
            throw new StringNotClosedException();
        }

        public void IdenfitierTooLong(int line, int collumn, int lengthLimit)
        {
            throw new IdenfitierTooLongException();
        }

        public void StringTooLong(int line, int collumn, int lengthLimit)
        {
            throw new StringTooLongException();
        }
        #endregion

        #region ParserExceptions
        public void IdentifierIsNull(int line, int collumn)
        {
            throw new IdentifierIsNullException();
        }

        public void UnexpectedToken(string expectedToken, int line, int collumn, string receivedToken)
        {
            throw new UnexpectedTokenException();
        }

        public void UnexpectStatement(string expected, int line, int collumn, string receivedToken)
        {
            throw new UnexpectStatementException();
        }

        public void MissingParameter(int line, int collumn)
        {
            throw new MissingParameterException();
        }

        public void FunctionNameAlreadyExists(string name, int line, int collumn)
        {
            throw new FunctionNameAlreadyExistsException();
        }

        public void ThrowHollow(string typeOfToken, string tokenValue, int line, int column)
        {
            throw new ThrowHollowException();
        }
        #endregion

        #region SemcheckExceptions
        public void MainNotOccured()
        {
            throw new MainNotOccuredExist();
        }

        public void FunctionAlreadyDeclared(string name, int line, int collumn)
        {
            throw new FunctionAlreadyDeclaredException();
        }

        public void ParameterDuplicated(string paramName, string funcName)
        {
            throw new ParameterDuplicatedException();
        }

        public void UnrecognisedType(int line, int collumn)
        {
            throw new UnrecognisedTypeException();
        }

        public void UnresolvedReference(string name, int line, int collumn)
        {
            throw new UnresolvedReferenceException();
        }

        public void WrongType(Type expected, Type received, int line, int collumn)
        {
            throw new WrongTypeException();
        }

        public void FunctionDoesNotReturnAnything(string name)
        {
            throw new FunctionDoesNotReturnAnythingException();
        }

        public void IllegalStringOperation(int line, int collumn)
        {
            throw new IllegalStringOperationException();
        }

        public void IllegalZeroOperation(int line, int collumn)
        {
            throw new IllegalZeroOperationException();
        }
        #endregion

        #region SemcheckAndExecutorExceptions


        public void IllegalIncrement(int line, int collumn)
        {
            throw new IllegalIncrementException();
        }

        public void IllegalNegation(int line, int collumn)
        {
            throw new IllegalNegationException();
        }

        public void VariableAlreadyDeclared(string name, int line, int collumn)
        {
            throw new VariableAlreadyDeclaredException();
        }
        #endregion

        #region ExecutorExceptions

        public void VariableDoesNotExist(string name, int line, int collumn)
        {
            throw new VariableDoesNotExistException();
        }

        public void IllegalExpression(int line, int collumn)
        {
            throw new IllegalExpressionException();
        }

        public void IllegalAdditionOperation(object obj1, object obj2, int line, int collumn)
        {
            throw new IllegalAdditionOperationException();
        }

        public void IllegalMultiplicationOperation(int line, int collumn)
        {
            throw new IllegalMultiplicationOperationException();
        }

        public void IllegalInstruction(int line, int collumn)
        {
            throw new IllegalInstructionException();
        }
        #endregion
    }
}
