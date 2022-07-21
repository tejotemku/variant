using ErrorHandlerModule;
using ExecutorModule;
using ScriptReaderModule;
using LexerModule;
using ParserModule;
using SemanticAnalyzerModule;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Variant
{
    class Program
    {
        static void Main(string[] args)
        {
            //var files = Directory.GetFiles("D:/tkom/");
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "/Tests/data/TestExampleScript0.vrnt";
            LazyErrorHandler errorHandler = new();
            try
            {
                using (FileStream fs = File.Open(path, FileMode.Open))
                {
                    IScriptSource scriptSource = new ScriptReader(fs);
                    ILexer lexer = new Lexer(scriptSource, errorHandler);
                    Parser parser = new Parser(lexer, errorHandler);
                    ParsedProgram parsedProgram = parser.GetParsedProgram();
                    SemanticAnalyzer semcheck = new(parsedProgram, errorHandler);
                    if (!semcheck.ValidateProgram())
                    {
                        Console.WriteLine("Semcheck Failed");
                        return;
                    }
                    Executor executor = new Executor(parsedProgram, errorHandler);
                    executor.ExecuteProgram();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
