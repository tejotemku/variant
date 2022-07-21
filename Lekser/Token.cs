using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexerModule
{
    public class Token
    {
        TokenType type;
        TokenPosition position;
        object? tokenValue; 

        public TokenType TypeOfToken
        {
            get { return type; }
        }

        public int Line
        {
            get { return position.Line; }
        }

        public int Column
        {
            get { return position.Column; }
        }

        public object? TokenValue
        {
            get { return tokenValue; }
        }
        
        public Token(TokenType tokenType, int line, int collumn, object? value=null)
        {
            type = tokenType;
            position = new TokenPosition()
            {
                Line = line,
                Column = collumn,
            };
            tokenValue = value;
        }


    }

    public struct TokenPosition
    {
        public int Line;
        public int Column;
    }

    public enum TokenType
    {
        Identifier,
        IntLiteral,
        StringLiteral,
        Undefined,
        EndOfFile,
        // keywords
        Foreach,
        If,
        Else,
        Int,
        String,
        File,
        Directory,
        Return,
        DLLLOAD,
        In,
        // operators
        Equals, // ==
        NotEquals, // !=
        Greater, // >
        GreaterOrEqual, // >=
        Lesser, // <
        LesserOrEqual, // <=
        And, // &&
        Or, // ||
        Plus, // +
        Minus, // -
        Multiplication, // *
        Division, // /
        Modulo, // %
        Increment, // ++
        LogicNegation, // !
        // punctuators
        ParenthesesOpen, // (
        ParenthesesClose, // )
        BracketsOpen, // {
        BracketsClose, // }
        Comma, // ,
        Dot, // .
        Assign, // =
        Semicolon // ;
    }
}
