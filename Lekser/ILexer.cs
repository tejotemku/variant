using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexerModule
{
    public interface ILexer
    {
        abstract Token GetNextToken();
    }
}
