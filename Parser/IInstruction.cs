using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserModule
{
    public interface IInstruction { }

    public class Returning : IInstruction
    {
        public IExpression ReturnedExpression;

        public Returning(IExpression returnedValue)
        {
            ReturnedExpression = returnedValue;
        }
    }

    public class ForeachLoop : IInstruction
    {
        public DataTypes LoopVariableType;
        public string LoopVariableName;
        public IExpression LoopExpression;
        public List<IInstruction> InstructionBlock;

        public ForeachLoop(
            DataTypes loopVariableType,
            string loopVariableName,
            IExpression loopExpression,
            List<IInstruction> instructionBlock)
        {
            LoopVariableType = loopVariableType;
            LoopVariableName = loopVariableName;
            LoopExpression = loopExpression;
            InstructionBlock = instructionBlock;
        }
    }

    public class DeclaringVariable : IInstruction
    {
        public DataTypes Type;
        public string Name;
        public IExpression? AssignedExpression;

        public DeclaringVariable(DataTypes type,  string name)
        {
            Type = type;
            Name = name;
            AssignedExpression = null;
        }

        public DeclaringVariable(DataTypes type, string name, IExpression assignedExpression)
        {
            Type = type;
            Name = name;
            AssignedExpression = assignedExpression;
        }
    }

    public class ExpressionInstruction : IInstruction
    {
        public IExpression Expression { get; set; }

        public ExpressionInstruction(IExpression expression)
        {
            Expression = expression;
        }
    }

    public class AssigningToMember : IInstruction
    {
        public IExpression Member;
        public IExpression AssignedExpression;

        public AssigningToMember(IExpression member, IExpression assignedExpression)
        {
            Member = member;
            AssignedExpression = assignedExpression;
        }
    }

    public class IfOrIfElse : IInstruction
    {
        public IExpression CheckedExpression;
        public List<IInstruction> MainInstructionBlock;
        public List<IInstruction>? ElseInstructionBlock;

        public IfOrIfElse(IExpression logicalExpression, List<IInstruction> instructions)
        {
            CheckedExpression = logicalExpression;
            MainInstructionBlock = instructions;
            ElseInstructionBlock = null;
        }

        public IfOrIfElse(IExpression logicalExpression, List<IInstruction> instructions, List<IInstruction> elseInstructions)
        {
            CheckedExpression = logicalExpression;
            MainInstructionBlock = instructions;
            ElseInstructionBlock = elseInstructions;
        }
    }
}
