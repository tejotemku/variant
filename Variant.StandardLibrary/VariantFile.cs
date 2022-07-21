namespace Variant.StandardLibrary
{
    public class VariantFile
    {
        public string FilePath { get; set; }

        public string Test { get { return "test";  } }
        public string Name { get; set; }
        public string? Extension { get; set; }

        public VariantFile (string path)
        {
            string correected_path = path.Replace("/", "\\");
            int nameStartIndex = correected_path.LastIndexOf('\\');
            int extStartIndex = correected_path.LastIndexOf('.');
            FilePath = path;
            if (extStartIndex <= nameStartIndex)
            {
                Extension = null;
                Name = path.Substring(nameStartIndex + 1);
            }
            else
            {
                Extension = path.Substring(extStartIndex + 1);
                Name = path.Substring(nameStartIndex + 1, extStartIndex - nameStartIndex);
            }
        }
    }
}
