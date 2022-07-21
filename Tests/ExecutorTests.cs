using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ErrorHandlerModule;
using ParserModule;
using LexerModule;
using ExecutorModule;
using SemanticAnalyzerModule;
using ScriptReaderModule;
using System.IO;

namespace Variant.Tests
{
    [TestFixture]
    public class ExecutorTests
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

        #region Tests
        [Test]
        public void UnresolvedReference()
        {
            Assert.Throws<UnresolvedReferenceException>(() => RunVirtualScript("a = 1;"));
        }

        [Test]
        public void IllegalStringSubtraction()
        {
            Assert.Throws<IllegalStringOperationException>(() => RunVirtualScript("string s = \"aab\" - \"ab\";")); ;
        }

        [Test]
        public void DivisionByZeroHandling()
        {
            Assert.Throws<IllegalZeroOperationException>(() => RunVirtualScript("int b = 100 / (1-1);"));
        }
        #endregion

        #region Helper Methods
        IScriptSource EntombInstruction(string instruction)
        {
            string script = $"int main() {{{instruction} return 0; }}";
            return new VirtualScriptSource(script);
        }

        void RunVirtualScript(string script, bool shouldEntomb = true)
        {
            IScriptSource scriptSource = shouldEntomb ? EntombInstruction(script) : new VirtualScriptSource(script);
            var lexer = new Lexer(scriptSource, errorHandler);
            var parser = new Parser(lexer, errorHandler);
            var parsedProgram = parser.GetParsedProgram();
            var semcheck = new SemanticAnalyzer(parsedProgram, errorHandler);
            if (!semcheck.ValidateProgram())
                Assert.Fail();
            var executor = new Executor(parsedProgram, errorHandler);
            executor.ExecuteProgram();
            return;
        }

        void RunScriptFile(string filename)
        {
            using (FileStream fs = File.Open(testFilesDirectory + filename, FileMode.Open))
            {
                var lexer = new Lexer(new ScriptReader(fs), errorHandler);
                var parser = new Parser(lexer, errorHandler);
                var parsedProgram = parser.GetParsedProgram();
                var semcheck = new SemanticAnalyzer(parsedProgram, errorHandler);
                if(!semcheck.ValidateProgram())
                    Assert.Fail();
                var executor = new Executor(parsedProgram, errorHandler);
                executor.ExecuteProgram();
                return;
            }
            Assert.Fail();
        }
        #endregion
    }
}
