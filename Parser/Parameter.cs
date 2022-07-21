using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserModule
{
    public class Parameter
    {
        public DataTypes Type { get; set; }
        public string Name { get; set; }

        public Parameter (DataTypes type, string name)
        {
            Name = name;
            Type = type;
        }
    }
}
