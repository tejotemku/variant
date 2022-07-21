using ScriptReaderModule;
using ErrorHandlerModule;
using System.Linq;
using System;
using System.Text;

namespace LexerModule
{
    public static class Constant
    {
        public static readonly char EXT = (char)0x03;
        public static readonly int MaxIdentifierLength = 50;
        public static readonly int MaxStringLength = 5000;
    }

    public class Lexer : ILexer
    {
        #region Fields and Properties
        char currentChar;
        Token currentToken;
        TokenPosition currentTokenPosition;
        IScriptSource scriptSource;
        IErrorHandler errorHandler;
        Dictionary<char, TokenType> singleCharTokenDict = new Dictionary<char, TokenType>()
        {
            { '.', TokenType.Dot },
            { ',', TokenType.Comma },
            { '{', TokenType.BracketsOpen },
            { '}', TokenType.BracketsClose },
            { '(', TokenType.ParenthesesOpen },
            { ')', TokenType.ParenthesesClose },
            { '%', TokenType.Modulo },
            { '*', TokenType.Multiplication },
            { '/', TokenType.Division },
            { '-', TokenType.Minus },
            { ';', TokenType.Semicolon}
        };

        Dictionary<string, TokenType> keywordTokenDict = new Dictionary<string, TokenType>()
        {
            { "foreach", TokenType.Foreach},
            { "if", TokenType.If},
            { "else", TokenType.Else},
            { "int", TokenType.Int},
            { "string", TokenType.String},
            { "file", TokenType.File},
            { "directory", TokenType.Directory},
            { "return", TokenType.Return},
            { "dllload", TokenType.DLLLOAD},
            { "in", TokenType.In},
        };
        #endregion

        #region Constructor and Public Methods
        public Lexer(IScriptSource sr, IErrorHandler eh)
        {
            currentToken = new Token(TokenType.Undefined, 0, 0);
            scriptSource = sr;
            errorHandler = eh;

            GetNextChar();
        }

        public Token GetNextToken()
        {
            SkipWhiteSpacesAndComments();
            currentTokenPosition.Line = scriptSource.CurrentCharLine;
            currentTokenPosition.Column = scriptSource.CurrentCharColumn;

            if (currentChar == Constant.EXT)
            {
                currentToken = new Token(TokenType.EndOfFile, currentTokenPosition.Line, currentTokenPosition.Column);
                return currentToken;
            }

            if (TryBuildSingleCharacterToken()
                || TryBuildMultiCharacterToken()
                || TryBuildIntLiteral()
                || TryBuildStringLiteral()
                || TryBuildIdentifierOrKeyword()
                ) return currentToken;

            currentToken = new Token(TokenType.Undefined, currentTokenPosition.Line, currentTokenPosition.Column, currentChar);
            GetNextChar();
            return currentToken;
        }

        #endregion

        #region Helpers
        void GetNextChar()
        {
            currentChar = scriptSource.GetNextChar();
        }
        #endregion

        #region TokenBuilders
        bool TryBuildIntLiteral() 
        {

            if (!Char.IsDigit(currentChar)) return false;
            int literal = 0;
            checked
            {
                try
                {
                    do
                    {
                        literal *= 10;
                        literal += (currentChar - '0');
                        GetNextChar();
                    }
                    while (Char.IsDigit(currentChar));
                }
                catch (OverflowException)
                {
                    errorHandler.IntTooBig(currentTokenPosition.Line, currentTokenPosition.Column);
                }
            }

            currentToken = new Token(
                TokenType.IntLiteral,
                currentTokenPosition.Line,
                currentTokenPosition.Column,
                literal
                );
            return true;
        }

        bool TryBuildStringLiteral() 
        {
            if (currentChar != '\"') return false;
            StringBuilder literal = new StringBuilder("");
            GetNextChar();
            while( ((currentChar != '\"') && (currentChar != Constant.EXT)))
            {
                if (currentChar == '\\')
                {
                    GetNextChar();
                    switch (currentChar)
                    {
                        case 'n':
                            currentChar = '\n';
                            break;
                        case 't':
                            currentChar = '\t';
                            break;
                        case '0':
                            currentChar = '\0';
                            break;
                        case '\\':
                            currentChar = '\\';
                            break;
                        case '\"':
                            currentChar = '\"';
                            break;
                        default:
                            errorHandler.UnknownEscapeCharacter(scriptSource.CurrentCharLine, scriptSource.CurrentCharColumn);
                            break;
                    }
                }
                if (literal.Length == Constant.MaxStringLength)
                {
                    errorHandler.StringTooLong(
                        currentTokenPosition.Line,
                        currentTokenPosition.Column,
                        Constant.MaxStringLength);
                }
                literal.Append(currentChar);
                GetNextChar();
            }
            if (currentChar == Constant.EXT) errorHandler.StringNotClosed(
                currentTokenPosition.Line, 
                currentTokenPosition.Column);
            else GetNextChar();
            
            currentToken = new Token(
                TokenType.StringLiteral,
                currentTokenPosition.Line,
                currentTokenPosition.Column,
                literal.ToString());
            return true;
           
        }

        bool TryBuildSingleCharacterToken() 
        {
            TokenType tokenType;
            if (singleCharTokenDict.TryGetValue(currentChar, out tokenType))
            {
                currentToken = new Token(
                    tokenType,
                    currentTokenPosition.Line,
                    currentTokenPosition.Column);
                GetNextChar();
                return true;
            }
            return false; 
        }

        bool TryBuildMultiCharacterToken()
        {
            TokenType tokenType;
            void CheckTail(char expectedTail, TokenType tokenTypeIfSuccess, TokenType tokenTypeIfFailure)
            {
                if (currentChar == expectedTail )
                {
                    tokenType = tokenTypeIfSuccess;
                    GetNextChar();
                }
                else
                {
                    tokenType = tokenTypeIfFailure;
                }
            }

            switch (currentChar)
            {
                case '>':
                    GetNextChar();
                    CheckTail('=', TokenType.GreaterOrEqual, TokenType.Greater);
                    break;
                case '<':
                    GetNextChar();
                    CheckTail('=', TokenType.LesserOrEqual, TokenType.Lesser);
                    break;
                case '!':
                    GetNextChar();
                    CheckTail('=', TokenType.NotEquals, TokenType.LogicNegation);
                    break;
                case '=':
                    GetNextChar();
                    CheckTail('=', TokenType.Equals, TokenType.Assign);
                    break;
                case '&':
                    GetNextChar();
                    CheckTail('&', TokenType.And, TokenType.Undefined);
                    break;
                case '|':
                    GetNextChar();
                    CheckTail('|', TokenType.Or, TokenType.Undefined);
                    break;
                case '+':
                    GetNextChar();
                    CheckTail('+', TokenType.Increment, TokenType.Plus);
                    break;
                default:
                    return false;
            }
            currentToken = new Token(
                tokenType,
                currentTokenPosition.Line,
                currentTokenPosition.Column);
            return true;
        }

        bool TryBuildIdentifierOrKeyword() 
        {
            if (currentChar == '_' || Char.IsLetter(currentChar))
            {

                StringBuilder literal = new StringBuilder("");
                do
                {
                    if (literal.Length == Constant.MaxIdentifierLength) errorHandler.IdenfitierTooLong(
                        currentTokenPosition.Line,
                        currentTokenPosition.Column,
                        Constant.MaxStringLength);
                    literal.Append(currentChar);
                    GetNextChar();
                }
                while (currentChar == '_' || Char.IsLetter(currentChar) || Char.IsDigit(currentChar));
     
                

                string strLiteral = literal.ToString();
                object? tokenLiteral = null;
                TokenType tokenType;

                if (!keywordTokenDict.TryGetValue(strLiteral, out tokenType))
                {
                    tokenType = TokenType.Identifier;
                    tokenLiteral = strLiteral; 
                }

                currentToken = new Token(
                   tokenType,
                   currentTokenPosition.Line,
                   currentTokenPosition.Column,
                   tokenLiteral
                   );
                return true;
            }
            return false;
        }

        bool SkipComment()
        {
            bool result =  currentChar == '#';
            if (!result) return false;
            char[] commentStopper = new char[] { '\n', Constant.EXT };
            while (!commentStopper.Contains(currentChar))
            {
                GetNextChar();
            }
            return result;
        }

        bool SkipWhitespaces()
        {
            bool result = Char.IsWhiteSpace(currentChar);
            while (Char.IsWhiteSpace(currentChar))
            {
                GetNextChar();
            }
            return result;
        }

        void SkipWhiteSpacesAndComments()
        {
            while (SkipWhitespaces() || SkipComment());
        }

        #endregion
    }
}