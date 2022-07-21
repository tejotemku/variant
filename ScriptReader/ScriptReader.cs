using ErrorHandlerModule;

namespace ScriptReaderModule
{
    public class ScriptReader : IScriptSource
    {
        int currentLine;
        int currentColumn;
        bool performedCarriageReturn;
        StreamReader scriptStream;

        public int CurrentCharLine 
        { 
            get { return currentLine; }  
        }

        public int CurrentCharColumn
        {
            get { return currentColumn; }
        }

        public ScriptReader(FileStream fs)
        {
            currentLine = 1;
            currentColumn = 0;
            performedCarriageReturn = false;
            scriptStream = new StreamReader(fs);
        }

        public char GetNextChar()
        {
            int nextChar = scriptStream.Read();
            if (nextChar == -1) nextChar = 3;
            char character = (char)nextChar;
            if (character == '\r')
            {
                currentLine++;
                currentColumn = -1;
                performedCarriageReturn = true;
            }
            else if (character == '\n')
            {
                if (!performedCarriageReturn)
                {
                    currentLine++;
                }
                currentColumn = -1;
                performedCarriageReturn = false;
            }
            currentColumn++;
            return character;
        }
    }
}