using ErrorHandlerModule;
using LexerModule;
using NUnit.Framework;
using ScriptReaderModule;
using System;
using System.Collections.Generic;
using System.IO;

namespace Variant.Tests
{
    [TestFixture]
    public class LexerTests
    {
        #region Setup
        string testFilesDirectory;
        IErrorHandler errorHandler;

        [SetUp]
        public void Setup()
        {
            errorHandler = new StrictErrorHandler();
            testFilesDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "/data/";
        }
        #endregion

        #region ExceptionTests

        [Test]
        public void TestExceptionIdentifierToLong()
        {
            Assert.Throws<IdenfitierTooLongException>(() => SimulateException(new string('A', 51)));
        }

        [Test]
        public void TestExceptionUnknownEscapeCharacter()
        {
            Assert.Throws<UnknownEscapeCharacterException>(() => SimulateException("\"\\=\""));
        }

        [Test]
        public void TestExceptionIntTooBig()
        {
            Assert.Throws<IntTooBigException>(() => SimulateException("1231231314131312423412424241231231"));
        }

        [Test]
        public void TestExceptionStringNotClosed()
        {
            Assert.Throws<StringNotClosedException>(() => SimulateException("\"string"));
        }

        [Test]
        public void TestExceptionStringTooLong()
        {
            Assert.Throws<StringTooLongException>(() => SimulateException("\"" + new string('A', 5001) + "\""));
        }
        #endregion

        #region ShortTests

        [Test]
        public void TestEscapeCharacterTab()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.StringLiteral, "sand\twich", "\"sand\\twich\""));
        }

        [Test]
        public void TestEscapeCharacterQuote()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.StringLiteral, "quo\"te", "\"quo\\\"te\""));
        }

        [Test]
        public void TestEscapeCharacterNewLine()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.StringLiteral, "sand\nwich", "\"sand\\nwich\""));
        }

        [Test]
        public void TestEscapeCharacterZero()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.StringLiteral, "\0", "\"\\0\""));
        }

        [Test]
        public void TestEscapeCharacterBackslash()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.StringLiteral, "this \\ is backslash", "\"this \\\\ is backslash\""));
        }

        [Test]
        public void TestTokenForeach()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Foreach, null, "foreach"));
        }

        [Test]
        public void TestTokenIf()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.If, null, "if"));
        }

        [Test]
        public void TestTokenElse()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Else, null, "else"));
        }

        [Test]
        public void TestTokenInt()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Int, null, "int"));
        }

        [Test]
        public void TestTokenString()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.String, null, "string"));
        }

        [Test]
        public void TestTokenFile()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.File, null, "file"));
        }

        [Test]
        public void TestTokenDirectory()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Directory, null, "directory"));
        }

        [Test]
        public void TestTokenReturn()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Return, null, "return"));
        }

        [Test]
        public void TestTokenEquals()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Equals, null, "=="));
        }

        [Test]
        public void TestTokenNotEquals()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.NotEquals, null, "!="));
        }

        [Test]
        public void TestTokenGreater()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Greater, null, ">"));
        }

        [Test]
        public void TestTokenGreaterOrEqual()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.GreaterOrEqual, null, ">="));
        }

        [Test]
        public void TestTokenLesser()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Lesser, null, "<"));
        }

        [Test]
        public void TestTokenLesserOrEqual()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.LesserOrEqual, null, "<="));
        }

        [Test]
        public void TestTokenAnd()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.And, null, "&&"));
        }

        [Test]
        public void TestTokenOr()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Or, null, "||"));
        }

        [Test]
        public void TestTokenPlus()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Plus, null, "+"));
        }

        [Test]
        public void TestTokenMinus()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Minus, null, "-"));
        }

        [Test]
        public void TestTokenMultiplication()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Multiplication, null, "*"));
        }

        [Test]
        public void TestTokenDivision()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Division, null, "/"));
        }

        [Test]
        public void TestTokenModulo()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Modulo, null, "%"));
        }

        [Test]
        public void TestTokenIncrement()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Increment, null, "++"));
        }

        [Test]
        public void TestTokenLogicNegation()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.LogicNegation, null, "!"));
        }

        [Test]
        public void TestTokenParenthesesOpen()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.ParenthesesOpen, null, "("));
        }

        [Test]
        public void TestTokenParenthesesClose()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.ParenthesesClose, null, ")"));
        }

        [Test]
        public void TestTokenBracesOpen()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.BracketsOpen, null, "{"));
        }

        [Test]
        public void TestTokenBracesClose()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.BracketsClose, null, "}"));
        }

        [Test]
        public void TestTokenComma()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Comma, null, ","));
        }

        [Test]
        public void TestTokenDot()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Dot, null, "."));
        }

        [Test]
        public void TestTokenAssign()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Assign, null, "="));
        }

        [Test]
        public void TestTokenSemicolon()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Semicolon, null, ";"));
        }

        [Test]
        public void TestTokenIdentifier()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Identifier, "pepe", "pepe"));
        }

        [Test]
        public void TestTokenIntLiteral()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.IntLiteral, 13, "013"));
        }

        [Test]
        public void TestTokenStringLiteral()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.StringLiteral, "pepe", "\"pepe\""));
        }

        [Test]
        public void TestTokenEmptyStringLiteral()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.StringLiteral, "", "\"\""));
        }

        [Test]
        public void TestTokenIdentifierStartingWithFloor()
        {
            Assert.True(TestSingleTokenVirtualSource(TokenType.Identifier, "_variable", " \n_variable"));
        }

        #endregion

        #region LongerTests

        [Test]
        public void TestFromFileStringLiterals()
        {
            var expectedTokens = new List<Token>();
            var recievedTokens = new List<Token>();
            expectedTokens.Add(new Token(TokenType.StringLiteral, 1, 3, "21313"));
            expectedTokens.Add(new Token(TokenType.StringLiteral, 1, 11, ""));
            expectedTokens.Add(new Token(TokenType.StringLiteral, 1, 14, "3423fscw23c"));
            expectedTokens.Add(new Token(TokenType.StringLiteral, 2, 1, "||||||||||"));
            expectedTokens.Add(new Token(TokenType.EndOfFile, 2, 13));


            using (FileStream fs = File.Open(testFilesDirectory + "TestStrings.vrnt", FileMode.Open))
            {
                recievedTokens = AnalyseScript(new ScriptReader(fs));
            }

            Assert.That(CompareTokenLists(expectedTokens, recievedTokens));
        }

        [Test]
        public void TestFromFileOperatorsAndPunctuators()
        {
            var expectedTokens = new List<Token>();
            var recievedTokens = new List<Token>();
            expectedTokens.Add(new Token(TokenType.NotEquals, 1, 1));
            expectedTokens.Add(new Token(TokenType.Greater, 1, 4));
            expectedTokens.Add(new Token(TokenType.GreaterOrEqual, 1, 6));
            expectedTokens.Add(new Token(TokenType.And, 1, 9));
            expectedTokens.Add(new Token(TokenType.Or, 1, 12));
            expectedTokens.Add(new Token(TokenType.Minus, 1, 15));
            expectedTokens.Add(new Token(TokenType.Plus, 1, 17));
            expectedTokens.Add(new Token(TokenType.Assign, 1, 19));
            expectedTokens.Add(new Token(TokenType.LesserOrEqual, 1, 21));
            expectedTokens.Add(new Token(TokenType.Lesser, 1, 24));
            expectedTokens.Add(new Token(TokenType.Multiplication, 1, 26));
            expectedTokens.Add(new Token(TokenType.Division, 1, 28));
            expectedTokens.Add(new Token(TokenType.Modulo, 1, 30));
            expectedTokens.Add(new Token(TokenType.LogicNegation, 1, 32));
            expectedTokens.Add(new Token(TokenType.ParenthesesOpen, 1, 34));
            expectedTokens.Add(new Token(TokenType.ParenthesesClose, 1, 36));
            expectedTokens.Add(new Token(TokenType.BracketsOpen, 1, 38));
            expectedTokens.Add(new Token(TokenType.BracketsClose, 1, 40));
            expectedTokens.Add(new Token(TokenType.Comma, 1, 42));
            expectedTokens.Add(new Token(TokenType.Dot, 1, 44));
            expectedTokens.Add(new Token(TokenType.Semicolon, 1, 46));
            expectedTokens.Add(new Token(TokenType.Equals, 1, 48));
            expectedTokens.Add(new Token(TokenType.EndOfFile, 1, 50));

            using (FileStream fs = File.Open(testFilesDirectory + "TestOperatorsAndPunctuators.vrnt", FileMode.Open))
            {
                recievedTokens = AnalyseScript(new ScriptReader(fs));
            }

            Assert.That(CompareTokenLists(expectedTokens, recievedTokens));
        }

        [Test]
        public void TestFromFileNumbers()
        {
            var expectedTokens = new List<Token>();
            var recievedTokens = new List<Token>();
            expectedTokens.Add(new Token(TokenType.IntLiteral, 1, 1, 1231));
            expectedTokens.Add(new Token(TokenType.IntLiteral, 1, 6, 31231));
            expectedTokens.Add(new Token(TokenType.Minus, 1, 12));
            expectedTokens.Add(new Token(TokenType.IntLiteral, 1, 13, 65));
            expectedTokens.Add(new Token(TokenType.IntLiteral, 1, 16, 1));
            expectedTokens.Add(new Token(TokenType.IntLiteral, 1, 18, 0));
            expectedTokens.Add(new Token(TokenType.IntLiteral, 1, 20, 098));
            expectedTokens.Add(new Token(TokenType.EndOfFile, 1, 23));

            using (FileStream fs = File.Open(testFilesDirectory + "TestNumbers.vrnt", FileMode.Open))
            {
                recievedTokens = AnalyseScript(new ScriptReader(fs));
            }

            Assert.That(CompareTokenLists(expectedTokens, recievedTokens));
        }

        [Test]
        public void TestFromFileIdenfitiersAndKeywords()
        {
            var expectedTokens = new List<Token>();
            var recievedTokens = new List<Token>();
            expectedTokens.Add(new Token(TokenType.Foreach, 1, 1));
            expectedTokens.Add(new Token(TokenType.If, 1, 9));
            expectedTokens.Add(new Token(TokenType.Else, 1, 12));
            expectedTokens.Add(new Token(TokenType.Int, 1, 17));
            expectedTokens.Add(new Token(TokenType.String, 1, 21));
            expectedTokens.Add(new Token(TokenType.File, 1, 28));
            expectedTokens.Add(new Token(TokenType.Directory, 1, 33));
            expectedTokens.Add(new Token(TokenType.Return, 1, 43));
            expectedTokens.Add(new Token(TokenType.DLLLOAD, 1, 50));
            expectedTokens.Add(new Token(TokenType.Identifier, 1, 58, "foreachNot"));
            expectedTokens.Add(new Token(TokenType.Identifier, 1, 69, "main"));
            expectedTokens.Add(new Token(TokenType.Identifier, 1, 74, "Sepia"));
            expectedTokens.Add(new Token(TokenType.Identifier, 1, 80, "image"));
            expectedTokens.Add(new Token(TokenType.Dot, 1, 85));
            expectedTokens.Add(new Token(TokenType.Identifier, 1, 86, "extension"));
            expectedTokens.Add(new Token(TokenType.EndOfFile, 1, 95));

            using (FileStream fs = File.Open(testFilesDirectory + "TestKeywordsAndIds.vrnt", FileMode.Open))
            {
                recievedTokens = AnalyseScript(new ScriptReader(fs));
            }

            Assert.That(CompareTokenLists(expectedTokens, recievedTokens));
        }

        [Test]
        public void TestFromFileExampleScript1()
        {
            var expectedTokens = new List<Token>();
            var recievedTokens = new List<Token>();
            expectedTokens.Add(new Token(TokenType.DLLLOAD, 1, 1));
            expectedTokens.Add(new Token(TokenType.Identifier, 1, 9, "sepia"));
            expectedTokens.Add(new Token(TokenType.Semicolon, 1, 14));
            expectedTokens.Add(new Token(TokenType.Int, 3, 1));
            expectedTokens.Add(new Token(TokenType.Identifier, 3, 5, "main"));
            expectedTokens.Add(new Token(TokenType.ParenthesesOpen, 3, 9));
            expectedTokens.Add(new Token(TokenType.ParenthesesClose, 3, 10));
            expectedTokens.Add(new Token(TokenType.BracketsOpen, 4, 1));
            expectedTokens.Add(new Token(TokenType.String, 5, 1));
            expectedTokens.Add(new Token(TokenType.Identifier, 5, 8, "_path"));
            expectedTokens.Add(new Token(TokenType.Assign, 5, 14));
            expectedTokens.Add(new Token(TokenType.StringLiteral, 5, 16, "D:/Files/img.bmp"));
            expectedTokens.Add(new Token(TokenType.Semicolon, 5, 34));
            expectedTokens.Add(new Token(TokenType.Identifier, 6, 1, "sepia"));
            expectedTokens.Add(new Token(TokenType.ParenthesesOpen, 6, 6));
            expectedTokens.Add(new Token(TokenType.Identifier, 6, 7, "_path"));
            expectedTokens.Add(new Token(TokenType.Comma, 6, 12));
            expectedTokens.Add(new Token(TokenType.Identifier, 6, 14, "appendPath"));
            expectedTokens.Add(new Token(TokenType.ParenthesesOpen, 6, 24));
            expectedTokens.Add(new Token(TokenType.Identifier, 6, 25, "_path"));
            expectedTokens.Add(new Token(TokenType.Comma, 6, 30));
            expectedTokens.Add(new Token(TokenType.StringLiteral, 6, 32, "_sepia"));
            expectedTokens.Add(new Token(TokenType.ParenthesesClose, 6, 40));
            expectedTokens.Add(new Token(TokenType.ParenthesesClose, 6, 41));
            expectedTokens.Add(new Token(TokenType.Semicolon, 6, 42));
            expectedTokens.Add(new Token(TokenType.Return, 7, 1));
            expectedTokens.Add(new Token(TokenType.IntLiteral, 7, 8, 0));
            expectedTokens.Add(new Token(TokenType.Semicolon, 7, 9));
            expectedTokens.Add(new Token(TokenType.BracketsClose, 8, 1));
            expectedTokens.Add(new Token(TokenType.EndOfFile, 9, 13));

            using (FileStream fs = File.Open(testFilesDirectory + "TestExampleScript1.vrnt", FileMode.Open))
            {
                recievedTokens = AnalyseScript(new ScriptReader(fs));
            }

            Assert.That(CompareTokenLists(expectedTokens, recievedTokens));
        }

        #endregion

        #region HelperMethods

        void SimulateException(string script)
        {
            var recievedTokens = AnalyseScript(new VirtualScriptSource(script));
        }

        bool TestSingleTokenVirtualSource(TokenType expectedToken, object? expectedValue, string script)
        {
            var expectedTokens = new List<Token>();
            expectedTokens.Add(new Token(expectedToken, 0, 0, expectedValue));
            expectedTokens.Add(new Token(TokenType.EndOfFile, 0, 0));
            var recievedTokens = AnalyseScript(new VirtualScriptSource(script));
            return CompareTokenLists(expectedTokens, recievedTokens);
        }

        List<Token> AnalyseScript(IScriptSource sr)
        {
            var lexer = new Lexer(sr, errorHandler);
            var recievedTokens = new List<Token>();

            Token token;
            do
            {
                token = lexer.GetNextToken();
                recievedTokens.Add(token);
            }
            while (token.TypeOfToken != TokenType.EndOfFile);
            return recievedTokens;
        }

        bool CompareTokenLists(List<Token> expectedList, List<Token> recievedList)
        {
            bool result = true;
            if (expectedList.Count == recievedList.Count)
            {
                for (int i = 0; i < expectedList.Count; i++)
                {
                    result &= CompareTokens(expectedList[i], recievedList[i]);
                }
            }
            else return false;

            return result;
        }

        bool CompareTokens(Token expectedToken, Token recievedToken)
        {
            bool result = true;
            result &= expectedToken.TypeOfToken == recievedToken.TypeOfToken;
            result &= expectedToken.Line == recievedToken.Line;
            result &= expectedToken.Column == recievedToken.Column;
            result &= (expectedToken.TokenValue is null && recievedToken.TokenValue is null)
                || (expectedToken.TokenValue.ToString() == recievedToken.TokenValue.ToString());

            return result;
        }
        #endregion
    }
}