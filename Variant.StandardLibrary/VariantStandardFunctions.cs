namespace Variant.StandardLibrary
{
    public class VariantStandardFunctions
    {

        public string appendPath(string originalPath, string sufix)
        {
            originalPath = originalPath.Replace("/", "\\");
            int extensionIndex = originalPath.LastIndexOf('.');
            return extensionIndex !=-1 ? originalPath.Substring(0, extensionIndex) + sufix + originalPath.Substring(extensionIndex): originalPath + sufix;
        }

        public int toInt(string value)
        {
            return (Int32.Parse(value));
        }

        public string toString(int value)
        {
            return value.ToString();
        }

        public void print(string msg)
        {
            Console.WriteLine(msg);
        }

        public string input(string msg)
        {
            Console.WriteLine(msg);
            return Console.ReadLine() ?? "";
        }
    }
}
