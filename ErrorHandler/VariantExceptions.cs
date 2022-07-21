using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHandlerModule
{
    public class FileNotFoundException : Exception { }

    public class UnknownEscapeCharacterException : Exception { }

    public class IntTooBigException : Exception { }

    public class FunctionNameAlreadyExistsException : Exception { }

    public class StringNotClosedException : Exception { }

    public class IdenfitierTooLongException : Exception { }

    public class StringTooLongException : Exception { }

    public class IdentifierIsNullException : Exception { }

    public class UnexpectedTokenException : Exception { }

    public class UnexpectStatementException : Exception { }

    public class MissingParameterException : Exception { }

    public class ThrowHollowException : Exception { }

    public class FunctionAlreadyDeclaredException : Exception { }

    public class ParameterDuplicatedException : Exception { }

    public class UnrecognisedTypeException : Exception { }

    public class UnresolvedReferenceException : Exception { }

    public class WrongTypeException : Exception { }

    public class FunctionDoesNotReturnAnythingException : Exception { }

    public class IllegalStringOperationException : Exception { }

    public class IllegalZeroOperationException : Exception { }

    public class IllegalIncrementException : Exception { }

    public class IllegalNegationException : Exception { }

    public class VariableAlreadyDeclaredException : Exception { }

    public class VariableDoesNotExistException : Exception { }

    public class IllegalExpressionException : Exception { }

    public class IllegalAdditionOperationException : Exception { }

    public class IllegalMultiplicationOperationException : Exception { }

    public class IllegalInstructionException : Exception { }

    public class MemberDoesNotExistException : Exception { }

    public class MainNotOccuredExist : Exception { }

}
