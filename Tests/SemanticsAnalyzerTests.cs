using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ErrorHandlerModule;
using ParserModule;
using LexerModule;
using SemanticAnalyzerModule;
using ScriptReaderModule;
using System.IO;

namespace Variant.Tests
{
    [TestFixture]
    public class SemanticsAnalyzerTests
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
        public void WrongAssignedExprTypeToIntDeclaration()
        {
            Assert.Throws<WrongTypeException>(() => ValidateScript("int a = \"b\";"));
        }

        [Test]
        public void WrongAssignedExprTypeToStringDeclaration()
        {
            Assert.Throws<WrongTypeException>(() => ValidateScript("string a = 1;"));
        }

        [Test]
        public void MainFunctionDoesNotOccur()
        {
            Assert.Throws<MainNotOccuredExist>(() => ValidateScript("int a() {return 0;}", false));
        }

        [Test]
        public void FunctionDoesNotHaveReturn()
        {
            Assert.Throws<FunctionDoesNotReturnAnythingException>(() => ValidateScript("int main() {}", false));
        }

        [Test]
        public void FunctionAlreadyDeclared()
        {
            Assert.Throws<FunctionNameAlreadyExistsException>(() => ValidateScript("int a() {return 0;} int a() {return 0;}", false));
        }

        [Test]
        public void IllegalStringOperation()
        {
            Assert.Throws<IllegalStringOperationException>(() => ValidateScript("string s = \"aab\" - \"ab\";")); ;
        }

        [Test]
        public void IllegalZeroOperation()
        {
            Assert.Throws<IllegalZeroOperationException>(() => ValidateScript("int a = 1 / 0;"));
        }

        [Test]
        public void ParameterDuplicated()
        {
            Assert.Throws<ParameterDuplicatedException>(() => ValidateScript("int main(int a, int a) {}", false));
        }

        [Test]
        public void UnresolvedReference()
        {
            Assert.Throws<UnresolvedReferenceException>(() => ValidateScript("a = 1;"));
        }
        #endregion

        #region ShortTests
        [Test]
        public void IntDeclarationTest()
        {
            Assert.True(ValidateScript("int a = 1;"));
        }
        #endregion

        #region FileScriptTests
        [Test]
        public void ExampleScript1Test()
        {
            ValidateScriptFile("TestExampleScript1.vrnt");
        }

        [Test]
        public void ExampleScript0Test()
        {
            ValidateScriptFile("TestExampleScript0.vrnt");
        }

        [Test]
        public void ExampleScript3Test()
        {
            ValidateScriptFile("TestExampleScript3.vrnt");
        }

        #endregion

        #region HelperMethods
        IScriptSource EntombInstruction(string instruction)
        {
            string script = $"int main() {{{instruction} return 0; }}";
            return new VirtualScriptSource(script);
        }

        bool ValidateScript(string script, bool shouldEntomb = true)
        {
            IScriptSource scriptSource = shouldEntomb ? EntombInstruction(script) : new VirtualScriptSource(script);
            var lexer = new Lexer(scriptSource, errorHandler);
            var parser = new Parser(lexer, errorHandler);
            var parsedProgram = parser.GetParsedProgram();
            var semcheck = new SemanticAnalyzer(parsedProgram, errorHandler);
            return semcheck.ValidateProgram();
        }

        void ValidateScriptFile(string filename)
        {
            using (FileStream fs = File.Open(testFilesDirectory + filename, FileMode.Open))
            {
                var lexer = new Lexer(new ScriptReader(fs), errorHandler);
                var parser = new Parser(lexer, errorHandler);
                var parsedProgram = parser.GetParsedProgram();
                var semcheck = new SemanticAnalyzer(parsedProgram, errorHandler);
                Assert.That(semcheck.ValidateProgram());
                return;
            }
            Assert.Fail();
        }
        #endregion
    }
}
