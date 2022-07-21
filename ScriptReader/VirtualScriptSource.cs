using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptReaderModule
{
    public class VirtualScriptSource : IScriptSource
    {
        string source;
        int index;

        public int CurrentCharLine
        {
            get { return 0; }
        }

        public int CurrentCharColumn
        {
            get { return 0; }
        }

        public VirtualScriptSource (string scriptString)
        {
            source = scriptString;
            index = 0;
        }

        public char GetNextChar()
        {
            return index < source.Length ? source[index++] : (char)3;
        }
    }
}
