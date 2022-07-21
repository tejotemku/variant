using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserModule
{
    public interface IExpression { }

    public class LogicalExpression : IExpression
    {
        public IExpression InitExpression;
        public List<IExpression> Alternatives;

        public LogicalExpression(IExpression initialConditionConjunction, 
            List<IExpression> alternativeConditionConjunctions)
        {
            InitExpression = initialConditionConjunction;
            Alternatives = alternativeConditionConjunctions;
        }
    }

    public class ConditionConjunction : IExpression
    {
        public IExpression InitExpression;
        public List<IExpression> Conjunctions;

        public ConditionConjunction(IExpression condition, List<IExpression> conjunctedConditions)
        {
            InitExpression = condition;
            Conjunctions = conjunctedConditions;
        }
    }
   
    public class Condition : IExpression
    {
        public IExpression MathExpression;
        public ComparisionOperators ComparisionOperator;
        public IExpression ComparedMathExpression;

        public Condition(IExpression mathExpression, 
            ComparisionOperators comparisionOperator, IExpression comparedMathExpression)
        {
            MathExpression = mathExpression;
            ComparisionOperator = comparisionOperator;
            ComparedMathExpression = comparedMathExpression;
        }

    }

    public class MathExpression : IExpression
    {
        public IExpression InitExpression;
        public List<(AdditionMathOperators Operator, IExpression MathValue)> AdditionOperations;

        public MathExpression(IExpression initialMathValue, 
            List<(AdditionMathOperators Operator, IExpression MathValue)> additionOperations)
        {
            InitExpression = initialMathValue;
            AdditionOperations = additionOperations;
        }
    }

    public class MathMultiplication : IExpression
    {
        public IExpression InitExpression;
        public List<(MultiplicationOperators Operator, IExpression MathValue)> MultiplyOperations;

        public MathMultiplication(IExpression initialValue, 
            List<(MultiplicationOperators Operator, IExpression MathValue)> multiplyOperations)
        {
            InitExpression = initialValue;
            MultiplyOperations = multiplyOperations;
        }
    }

    public class NegatedExpression : IExpression
    {
        public IExpression Expression { get; set; }

        public NegatedExpression(IExpression expressionToNegate)
        {
            Expression = expressionToNegate;
        }
    }

    public class IncrementedExpression : IExpression
    {
        public IExpression Expression { get; set; }

        public IncrementedExpression(IExpression expressionToNegate)
        {
            Expression = expressionToNegate;
        }
    }

    public class ValueGetter : IExpression
    {
        public List<IExpression> ValueGetters;

        public ValueGetter(List<IExpression> valueGetters)
        {
            ValueGetters = valueGetters;
        }
    }

    public class StringExpression : IExpression
    {
        public string Value { get; set; }

        public StringExpression(string value)
        {
            Value = value;
        }

    }
   
    public class IntExpression : IExpression
    {
        public int Value { get; set; }

        public IntExpression(int value)
        {
            Value = value;
        }
    }

    public class Variable : IExpression
    {
        public string Identifier { get; set;  }

        public Variable(string name)
        {
            Identifier = name;
        }
    }

    public class FunctionCall : IExpression
    {
        public string Identifier { get; set; }
        public List<IExpression> Args { get; set; }

        public FunctionCall(string identifier, List<IExpression> args)
        {
            Identifier = identifier;
            Args = args;
        }
    }

    public enum ComparisionOperators
    {
        Equals, // ==
        NotEquals, // !=
        Greater, // >
        GreaterOrEqual, // >=
        Lesser, // <
        LesserOrEqual, // <=
    }

    public enum AdditionMathOperators
    {
        Add, // +
        Subtract // -
    }

    public enum MultiplicationOperators
    {
        Multiply,
        Divide,
        Modulo
    }
}
