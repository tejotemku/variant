using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserModule
{
    public class DllLoader
    {
        public string Name { get; set; }

        public DllLoader(string name)
        {
            Name = name;
        }
    }
}
