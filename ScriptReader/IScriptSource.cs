using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptReaderModule
{
    public interface IScriptSource
    {
        public char GetNextChar();

        public int CurrentCharLine { get;  }
        public int CurrentCharColumn { get; }
    }
}
