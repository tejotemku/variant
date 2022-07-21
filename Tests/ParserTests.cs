using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ErrorHandlerModule;
using ParserModule;
using LexerModule;
using ScriptReaderModule;
using System.IO;


namespace Variant.Tests
{
    [TestFixture]
    public class ParserTests
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
        public void SemicolonMissingAfterDeclarationTest()
        {
            Assert.Throws<UnexpectedTokenException>(() =>SimulateException("string name}"));
        }

        [Test]
        public void SemicolonMissingAfterDeclarationWithAssignmentTest()
        {
            Assert.Throws<UnexpectedTokenException>(() => SimulateException("string name = \"wer\"}"));
        }

        [Test]
        public void SemicolonMissingAfterAssignmentTest()
        {
            Assert.Throws<UnexpectedTokenException>(() => SimulateException("name = \"wer\"}"));
        }

        [Test]
        public void ExpressionMissingInAssignmentTest()
        {
            Assert.Throws<UnexpectStatementException>(() => SimulateException("name = ;"));
        }

        [Test]
        public void AssignmentExpressionMissingInDeclarationTest()
        {
            Assert.Throws<UnexpectStatementException>(() => SimulateException("string name = ;"));
        }

        [Test]
        public void IdentifierMissingInDeclarationTest()
        {
            Assert.Throws<UnexpectedTokenException>(() => SimulateException("int = 1;"));
        }

        [Test]
        public void SemicolonMissingAfterReturnTest()
        {
            Assert.Throws<UnexpectedTokenException>(() => SimulateException("return 1"));
        }

        [Test]
        public void ExpressionMissingAfterReturnTest()
        {
            Assert.Throws<UnexpectStatementException>(() => SimulateException("return ;"));
        }

        [Test]
        public void ExpressionMissingInIfStatementTest()
        {
            Assert.Throws<UnexpectStatementException>(() => SimulateException("if(){}"));
        }

        [Test]
        public void ParameterMissingTest()
        {
            Assert.Throws<MissingParameterException>(() => SimulateException("int main(int a, ){ return 0;}", false));
        }
        #endregion

        #region ShorterTests
        [Test]
        public void ReturnIntLiteralTest()
        {
            IInstruction instruction = new Returning(new IntExpression(0));
            Assert.That(TestSingleInstruction("return 0;", instruction));
        }

        [Test]
        public void ReturnStringLiteralTest()
        {
            IInstruction instruction = new Returning(new StringExpression("test"));
            Assert.That(TestSingleInstruction("return \"test\";", instruction));
        }

        [Test]
        public void ReturnVariableTest()
        {
            IInstruction instruction = new Returning(new ValueGetter( new () { new Variable("test") }));
            Assert.That(TestSingleInstruction("return test;", instruction));
        }

        [Test]
        public void IfEqualsTest()
        {
            IInstruction instruction = new IfOrIfElse(new Condition(
                new IntExpression(1), ComparisionOperators.Equals, new IntExpression(2)), new());
            Assert.That(TestSingleInstruction("if(1==2) {}", instruction));
        }

        [Test]
        public void IfLesserTest()
        {
            IInstruction instruction = new IfOrIfElse(new Condition(
                new IntExpression(1), ComparisionOperators.Lesser, new IntExpression(2)), new());
            Assert.That(TestSingleInstruction("if(1<2) {}", instruction));
        }

        [Test]
        public void IfLesserOrEqualTest()
        {
            IInstruction instruction = new IfOrIfElse(new Condition(
                new IntExpression(1), ComparisionOperators.LesserOrEqual, new IntExpression(2)), new());
            Assert.That(TestSingleInstruction("if(1<=2) {}", instruction));
        }

        [Test]
        public void IfGreaterTest()
        {
            IInstruction instruction = new IfOrIfElse(new Condition(
                new IntExpression(1), ComparisionOperators.Greater, new IntExpression(2)), new());
            Assert.That(TestSingleInstruction("if(1>2) {}", instruction));
        }

        [Test]
        public void IfGreaterOrEqualTest()
        {
            IInstruction instruction = new IfOrIfElse(new Condition(
                new IntExpression(1), ComparisionOperators.GreaterOrEqual, new IntExpression(2)), new());
            Assert.That(TestSingleInstruction("if(1>=2) {}", instruction));
        }

        [Test]
        public void IfNotEqualTest()
        {
            IInstruction instruction = new IfOrIfElse(new Condition(
                new IntExpression(1), ComparisionOperators.NotEquals, new IntExpression(2)), new());
            Assert.That(TestSingleInstruction("if(1!=2) {}", instruction));
        }

        [Test]
        public void IfConjunctionTest()
        {
            IInstruction instruction = new IfOrIfElse(new ConditionConjunction(new Condition(
                new IntExpression(1), ComparisionOperators.Equals, new IntExpression(2)), new() { new Condition(
                new IntExpression(3), ComparisionOperators.NotEquals, new IntExpression(3)) }
                ), new());
            Assert.That(TestSingleInstruction("if(1==2 && 3!=3) {}", instruction));
        }

        [Test]
        public void IfAlternativeTest()
        {
            IInstruction instruction = new IfOrIfElse(
                new LogicalExpression(
                    new Condition(new IntExpression(1), ComparisionOperators.Equals, new IntExpression(2)), 
                    new() { new Condition(
                        new IntExpression(3), ComparisionOperators.NotEquals, new IntExpression(3)
                        )}), 
                new());
            Assert.That(TestSingleInstruction("if(1==2 || 3!=3) {}", instruction));
        }

        [Test]
        public void ReturnMathExpresionTest()
        {
            IInstruction instruction = new Returning(
                new MathExpression(
                    new MathMultiplication(
                        new IntExpression(2), 
                        new()     
                        { 
                            (MultiplicationOperators.Multiply, new IntExpression(3)) 
                        }), 
                    new() 
                    { 
                        (AdditionMathOperators.Add, new IntExpression(1)),
                        (AdditionMathOperators.Subtract, new MathMultiplication(
                            new IntExpression(4), 
                            new() 
                            {
                                (MultiplicationOperators.Modulo, new IntExpression(3))
                            }
                        ))

                    } 
                )
            );
            Assert.That(TestSingleInstruction("return 2*3 + 1 - 4%3;", instruction));
        }

        [Test]
        public void DeclareInt()
        {
            IInstruction instruction = new DeclaringVariable(DataTypes.Int, "i");
            Assert.That(TestSingleInstruction("int i;", instruction));
        }

        [Test]
        public void DeclareAndAssignInt()
        {
            IInstruction instruction = new DeclaringVariable(DataTypes.Int, "i", new IntExpression(1));
            Assert.That(TestSingleInstruction("int i = 1;", instruction));
        }

        [Test]
        public void DeclarString()
        {
            IInstruction instruction = new DeclaringVariable(DataTypes.String, "str");
            Assert.That(TestSingleInstruction("string str;", instruction));
        }

        [Test]
        public void DeclareAndAssignString()
        {
            IInstruction instruction = new DeclaringVariable(DataTypes.String, "str", new StringExpression("tesścik"));
            Assert.That(TestSingleInstruction("string str = \"tesścik\";", instruction));
        }

        [Test]
        public void DeclareFile()
        {
            IInstruction instruction = new DeclaringVariable(DataTypes.File, "f");
            Assert.That(TestSingleInstruction("file f;", instruction));
        }

        [Test]
        public void DeclareAndAssignFile()
        {
            IInstruction instruction = new DeclaringVariable( 
                DataTypes.File, 
                "fil", 
                new ValueGetter( 
                    new() 
                    { 
                        new FunctionCall(
                            "File", 
                            new() 
                            { 
                                new StringExpression("D:/img.bmp") 
                            }
                        ) 
                    }
                )
            );
            Assert.That(TestSingleInstruction("file fil = file(\"D:/img.bmp\");", instruction));
        }

        [Test]
        public void DeclareDirectory()
        {
            IInstruction instruction = new DeclaringVariable(DataTypes.Directory, "dir");
            Assert.That(TestSingleInstruction("directory dir;", instruction));
        }

        [Test]
        public void DeclareAndAssingDirectory()
        {
            IInstruction instruction = new DeclaringVariable(
                DataTypes.Directory, 
                "dir", 
                new ValueGetter(
                    new()
                    {
                        new FunctionCall(
                            "Directory",
                            new()
                            {
                                new StringExpression("D:/Photos/")
                            }
                        )
                    }
                )
            );
            Assert.That(TestSingleInstruction("directory dir = directory(\"D:/Photos/\");", instruction));
        }

        [Test]
        public void ForeachLoop()
        {
            IInstruction instruction = new ForeachLoop(
                DataTypes.File, 
                "f",
                new ValueGetter(
                    new ()
                    {
                        new FunctionCall(
                            "folder",
                            new ()
                            {
                                new StringExpression("D:/Photos/")
                            }
                        )
                    }
                ),
                new ());
            Assert.That(TestSingleInstruction("foreach(file f in folder(\"D:/Photos/\")){}", instruction));
        }
        #endregion

        #region LongerTests
        [Test]
        public void ExampleScript1Test()
        {
            ParsedProgram parsedProgram;
            var expectedProgram = new ParsedProgram(
                new Dictionary<string, FunctionDefinition>()
                {{ 
                    "main", new FunctionDefinition(DataTypes.Int, "main", new(), new()
                    {
                        new DeclaringVariable(
                            DataTypes.String,
                            "_path",
                            new StringExpression("D:/Files/img.bmp")
                        ),
                        new ExpressionInstruction(
                            new ValueGetter( 
                                new() 
                                {
                                    new FunctionCall(
                                        "sepia",
                                        new()
                                        {
                                            new ValueGetter(
                                                new()
                                                {
                                                    new Variable("_path")
                                                }
                                            ),
                                            new ValueGetter(
                                                new()
                                                {
                                                    new FunctionCall(
                                                        "appendPath",
                                                        new()
                                                        {
                                                            new ValueGetter(
                                                                new()
                                                                {
                                                                    new Variable("_path")
                                                                }
                                                            ),
                                                            new StringExpression("_sepia")
                                                        }

                                                    )
                                                }
                                            )
                                        }
                                        
                                    )
                                }
                            )
                        ),
                        new Returning( new IntExpression(0))
                    })
                }},
                new()
                {
                    new DllLoader("sepia")
                }
                );

            using (FileStream fs = File.Open(testFilesDirectory + "TestExampleScript1.vrnt", FileMode.Open))
            {
                var lexer = new Lexer(new ScriptReader(fs), errorHandler);
                var parser = new Parser(lexer, errorHandler);
                parsedProgram = parser.GetParsedProgram();

            }
            Assert.That(CompareFunctions(expectedProgram.Functions, parsedProgram.Functions));
            Assert.That(CompareDllLoaders(expectedProgram.DllLoaders, parsedProgram.DllLoaders));
        }

        [Test]
        public void ExampleScript2Test()
        {
            ParsedProgram parsedProgram;
            var expectedProgram = new ParsedProgram(
                new Dictionary<string, FunctionDefinition>()
                {{
                    "main", new FunctionDefinition(DataTypes.Int, "main", new(), new()
                    {
                        new ForeachLoop(
                            DataTypes.Int,
                            "index",
                             new ValueGetter(
                                new()
                                {
                                    new FunctionCall(
                                        "folder",
                                        new()
                                        {
                                            new StringExpression("D:/Files/Photos/")
                                        }
                                    ),
                                    new Variable("NumberOfFiles")
                                }
                            ),
                            new()
                            {
                                new IfOrIfElse(
                                    new Condition(
                                        new ValueGetter(
                                            new()
                                            {
                                            new Variable("index")
                                            }
                                        ),
                                        ComparisionOperators.Lesser,
                                        new MathExpression(
                                            new IntExpression(10),
                                            new()
                                            {
                                                (AdditionMathOperators.Add, new IntExpression(2))
                                            }
                                        )
                                    ),
                                    new()
                                    {
                                        new ExpressionInstruction(
                                            new IncrementedExpression(
                                                new ValueGetter(
                                                    new()
                                                    {
                                                    new Variable("index")
                                                    }
                                                )
                                            )
                                        )
                                    }
                                )

                            }
                        ),
                        new Returning( new IntExpression(0))
                    })
                }},
                new()
                {
                    new DllLoader("sepia")
                }
                );

            using (FileStream fs = File.Open(testFilesDirectory + "TestExampleScript2.vrnt", FileMode.Open))
            {
                var lexer = new Lexer(new ScriptReader(fs), errorHandler);
                var parser = new Parser(lexer, errorHandler);
                parsedProgram = parser.GetParsedProgram();
            }

            Assert.That(CompareFunctions(expectedProgram.Functions, parsedProgram.Functions));
            Assert.That(CompareDllLoaders(expectedProgram.DllLoaders, parsedProgram.DllLoaders));
        }
        #endregion

        #region HelperMethods
        IScriptSource EntombInstruction(string instruction)
        {
            string script = $"int main() {{{instruction}}}";
            return new VirtualScriptSource(script);
        }

        bool TestSingleInstruction(string script, IInstruction instruction)
        {
            IScriptSource scriptSource = EntombInstruction(script);
            var lexer = new Lexer(scriptSource, errorHandler);
            var parser = new Parser(lexer, errorHandler);
            var parsedProgram = parser.GetParsedProgram();

            var expectedProgram = new ParsedProgram(
                new Dictionary<string, FunctionDefinition>()
                {{
                    "main", new FunctionDefinition(DataTypes.Int, "main", new(), new()
                    {
                        instruction
                    })
                }},
                new() { }
                );
            return CompareFunctions(expectedProgram.Functions, parsedProgram.Functions);
        }

        void SimulateException(string script, bool shouldEntomb=true)
        {
            IScriptSource scriptSource = shouldEntomb ? EntombInstruction(script): new VirtualScriptSource(script) ;
            var lexer = new Lexer(scriptSource, errorHandler);
            var parser = new Parser(lexer, errorHandler);
            var parsedProgram = parser.GetParsedProgram();
        }
        #endregion

        #region CompareMethods
        bool CompareDllLoaders(List<DllLoader> expected, List<DllLoader> received)
        {
            for(int i = 0; i < expected.Count; i++)
            {
                if (received[i].Name != expected[i].Name) 
                    return false;
            }
            return true;
        }

        bool CompareFunctions(Dictionary<string, FunctionDefinition> expected, Dictionary<string, FunctionDefinition> received)
        {
            var expectedKeys = expected.Keys.ToArray();
            var receivedKeys = received.Keys.ToArray();
            if (expectedKeys.Count() != receivedKeys.Count()) 
                return false;
            for (int i = 0; i < expectedKeys.Count(); i++)
            {
                if (expectedKeys[i] != receivedKeys[i])
                    return false;
            }

            foreach(var key in expected.Keys)
            {
                if (!CompareFunction(expected[key], received[key]))
                    return false;
            }
            return true;
        }

        bool CompareFunction(FunctionDefinition expected, FunctionDefinition received)
        {
            if (expected.Type != received.Type)
                return false;
            if (expected.Name != received.Name)
                return false;
            var expectedParameters = expected.Parameters;
            var receivedParameters = received.Parameters;
            if (expectedParameters.Count() != receivedParameters.Count())
                return false;
            for (int i = 0; i < expectedParameters.Count(); i++)
            {
                if (expectedParameters[i] != receivedParameters[i])
                    return false;
            }

            return CompareInstructionBlock(expected.Instructions, received.Instructions);
        }

        bool CompareInstructionBlock(List<IInstruction> expected, List<IInstruction> received)
        {
            var expectedInstructions = expected;
            var receivedInstructions = received;
            if (expectedInstructions.Count() != receivedInstructions.Count())
                return false;
            for (int i = 0; i < expectedInstructions.Count(); i++)
            {
                if (!CompareInstruction(expectedInstructions[i], receivedInstructions[i]))
                    return false;
            }

            return true;
        }

        bool CompareInstruction(IInstruction expected, IInstruction received)
        {
            switch (expected)
            {
                case Returning e:
                    return CompareReturning(e, (Returning) received);
                case ForeachLoop e:
                    return CompareForeach(e, (ForeachLoop) received);
                case ExpressionInstruction e:
                    return CompareExpressionInstruction(e, (ExpressionInstruction) received);
                case DeclaringVariable e:
                    return CompareDeclaration(e, (DeclaringVariable) received);
                case AssigningToMember e:
                    return CompareAssignation(e, (AssigningToMember) received);
                case IfOrIfElse e:
                    return CompareIfOrIfElse(e, (IfOrIfElse) received);

            }
            return false;
        }

        bool CompareReturning(Returning expected, Returning received)
        {
            return CompareExpression(expected.ReturnedExpression, received.ReturnedExpression);
        }

        bool CompareForeach(ForeachLoop expected, ForeachLoop received)
        {
            return expected.LoopVariableName == received.LoopVariableName
                && expected.LoopVariableType == received.LoopVariableType
                && CompareExpression(expected.LoopExpression, received.LoopExpression)
                && CompareInstructionBlock(expected.InstructionBlock, received.InstructionBlock);
        }

        bool CompareExpressionInstruction(ExpressionInstruction expected, ExpressionInstruction received)
        {
            return CompareExpression(expected.Expression, received.Expression);
        }

        bool CompareDeclaration(DeclaringVariable expected, DeclaringVariable received)
        {
            return expected.Type == received.Type
                && expected.Name == received.Name
                && (expected.AssignedExpression is null
                    || CompareExpression(expected.AssignedExpression, received.AssignedExpression));
        }

        bool CompareAssignation(AssigningToMember expected, AssigningToMember received)
        {
            return expected.Member == received.Member
                && CompareExpression(expected.AssignedExpression, received.AssignedExpression);
        }

        bool CompareIfOrIfElse(IfOrIfElse expected, IfOrIfElse received)
        {
            return CompareExpression(expected.CheckedExpression, received.CheckedExpression)
                && CompareInstructionBlock(expected.MainInstructionBlock, received.MainInstructionBlock)
                && (expected.ElseInstructionBlock is null
                    || CompareInstructionBlock(expected.ElseInstructionBlock, received.ElseInstructionBlock));
        }

        bool CompareExpression(IExpression expected, IExpression received)
        {
            switch (expected)
            {
                case LogicalExpression e:
                    return CompareLogicaExpression(e, (LogicalExpression) received);
                case ConditionConjunction e:
                    return CompareConjunctionExpression(e, (ConditionConjunction) received);
                case Condition e:
                    return CompareCondition(e, (Condition) received);
                case MathExpression e:
                    return CompareMathExpression(e, (MathExpression) received);
                case MathMultiplication e:
                    return CompareMathMultiplication(e, (MathMultiplication) received);
                case NegatedExpression e:
                    return CompareNegatedExpression(e, (NegatedExpression) received);
                case IncrementedExpression e:
                    return CompareIncrementedExpression(e, (IncrementedExpression) received);
                case ValueGetter e:
                    return CompareValueGetters(e, (ValueGetter) received);
                case StringExpression e:
                    return CompareString(e, (StringExpression) received);
                case IntExpression e:
                    return CompareInt(e, (IntExpression) received);
                default:
                    return false;
            } 
        }

        bool CompareLogicaExpression(LogicalExpression expected, LogicalExpression received)
        {
            if (!CompareExpression(expected.InitExpression, received.InitExpression))
                return false;

            var expectedOperations = expected.Alternatives;
            var receivedOperations = received.Alternatives;
            if (expectedOperations.Count() != receivedOperations.Count())
                return false;
            for (int i = 0; i < expectedOperations.Count(); i++)
            {
                if (!CompareExpression(expectedOperations[i], receivedOperations[i]))
                    return false;
            }
            return true;
        }

        bool CompareConjunctionExpression(ConditionConjunction expected, ConditionConjunction received)
        {
            var expectedOperations = expected.Conjunctions;
            var receivedOperations = received.Conjunctions;
            if (expectedOperations is not null
                && expectedOperations.Count() != receivedOperations.Count())
                return false;
            for (int i = 0; i < expectedOperations.Count(); i++)
            {
                if (!CompareExpression(expectedOperations[i], receivedOperations[i]))
                    return false;
            }
            return CompareExpression(expected.InitExpression, received.InitExpression);
        }

        bool CompareCondition(Condition expected, Condition received)
        {
            return CompareExpression(expected.MathExpression, received.MathExpression)
                && (expected.ComparisionOperator == received.ComparisionOperator
                        && CompareExpression(expected.ComparedMathExpression, received.ComparedMathExpression));
        }

        bool CompareMathExpression(MathExpression expected, MathExpression received)
        {
            if (!CompareExpression(expected.InitExpression, received.InitExpression))
                return false;

            var expectedOperations = expected.AdditionOperations;
            var receivedOperations = received.AdditionOperations;
            if (expectedOperations.Count() != receivedOperations.Count())
                return false;
            for (int i = 0; i < expectedOperations.Count(); i++)
            {
                if (expectedOperations[i].Operator != receivedOperations[i].Operator
                    || !CompareExpression(expectedOperations[i].MathValue, receivedOperations[i].MathValue))
                    return false;
            }
            return true;
        }

        bool CompareMathMultiplication(MathMultiplication expected, MathMultiplication received)
        {
            if (!CompareExpression(expected.InitExpression, received.InitExpression))
                return false;

            var expectedOperations = expected.MultiplyOperations;
            var receivedOperations = received.MultiplyOperations;
            if (expectedOperations.Count() != receivedOperations.Count())
                return false;
            for (int i = 0; i < expectedOperations.Count(); i++)
            {
                if (expectedOperations[i].Operator != receivedOperations[i].Operator
                    || !CompareExpression(expectedOperations[i].MathValue, receivedOperations[i].MathValue))
                    return false;
            }
            return true;
        }

        bool CompareNegatedExpression(NegatedExpression expected, NegatedExpression received)
        {
            return CompareExpression(expected.Expression, received.Expression);
        }

        bool CompareIncrementedExpression(IncrementedExpression expected, IncrementedExpression received)
        {
            return CompareExpression(expected.Expression, received.Expression);
        }

        bool CompareInt(IntExpression expected, IntExpression received)
        {
            return expected.Value == received.Value;
        }

        bool CompareString(StringExpression expected, StringExpression received)
        {
            return expected.Value == received.Value;
        }

        bool CompareValueGetters(ValueGetter expected, ValueGetter received)
        {
            var expectedValueGetters = expected.ValueGetters;
            var receivedValueGetters = received.ValueGetters;
            if (expectedValueGetters.Count() != receivedValueGetters.Count())
                return false;
            for (int i = 0; i < expectedValueGetters.Count(); i++)
            {
                if (!CompareValueGetters(expectedValueGetters[i], receivedValueGetters[i]))
                    return false;
            }
            return true;
        }

        bool CompareValueGetters(IExpression expected, IExpression received)
        {
            switch (expected)
            {
                case FunctionCall e:
                    return CompareFunctionCall(e, (FunctionCall)received);
                case Variable e:
                    return CompareVariable(e, (Variable)received);
                default:
                    return false;
            }
        }

        bool CompareFunctionCall (FunctionCall expected, FunctionCall received)
        {
            var expectedArgs = expected.Args;
            var receivedArgs = received.Args;
            if (expectedArgs.Count() != receivedArgs.Count())
                return false;
            for (int i = 0; i < expectedArgs.Count(); i++)
            {
                if (!CompareExpression(expectedArgs[i], receivedArgs[i]))
                    return false;
            }

            return expected.Identifier == received.Identifier;
        }

        bool CompareVariable(Variable expected, Variable received)
        {
            return expected.Identifier == received.Identifier;
        }

        #endregion
    }
}
