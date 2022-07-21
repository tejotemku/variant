using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserModule
{
    public class ParsedProgram
    {
        public Dictionary<string, FunctionDefinition> Functions { get; set; }
        public List<DllLoader> DllLoaders { get; set; }

        public ParsedProgram(Dictionary<string, FunctionDefinition> funs, List<DllLoader> dllLoaders)
        {
            Functions = funs;
            DllLoaders = dllLoaders;
        }
    }
}
