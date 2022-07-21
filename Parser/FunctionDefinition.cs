using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserModule
{
    public class FunctionDefinition
    {
        public string Name { get; set; }
        public DataTypes Type { get; set; }
        public List<Parameter> Parameters { get; set; }
        public List<IInstruction> Instructions { get; set; }

        public FunctionDefinition(DataTypes type, string name, List<Parameter> parameters, List<IInstruction> instructions)
        {
            Name = name;
            Type = type;
            Parameters = parameters;
            Instructions = instructions;

        }

    }
}
