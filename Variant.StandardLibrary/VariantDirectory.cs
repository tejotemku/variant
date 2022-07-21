using System.IO;

namespace Variant.StandardLibrary
{
    public class VariantDirectory
    {
        public List<VariantFile> Files { get; set; }
        public List<VariantDirectory> Directories { get; set; }
        public string DirPath { get; set; }

        public int NumberOfFiles { get { return Files.Count; } }

        public VariantDirectory(string path)
        {
            DirPath = path;
            Files = new ();
            Directories = new ();
            var files = Directory.GetFiles(path);
            var directories = Directory.GetDirectories(path);
            foreach (var f in files)
            {
                if (File.Exists(f))
                    Files.Add(new VariantFile(f));
            }
            foreach (var d in directories)
            {
                if (Directory.Exists(d))
                    Directories.Add(new VariantDirectory(d));
            }
        }
    }
}