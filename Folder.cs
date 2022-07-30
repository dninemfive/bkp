using System.IO;
using System.Collections.Generic;

namespace bkp
{
    public class Folder
    {
        public readonly string Name;
        public readonly List<Folder> Subfolders = new();
        public readonly List<File> Files = new();
        public Folder(string path)
        {
            Name = path.FolderName();
            foreach(string subpath in Directory.EnumerateDirectories(path)) Subfolders.Add(new(subpath));
            foreach (string filepath in Directory.EnumerateFiles(path)) Files.Add(new(filepath));
        }
        public string Serialize(int depth = 0)
        {
            string result = Name + "/";
            foreach(Folder sf in Subfolders) result += sf.Serialize(depth + 1);
            foreach (File f in Files) result += f.ToString();
            return result;
        }
    }
    public class File
    {
        public readonly string Name;
        public readonly string Hash;
        public File(string path)
        {
            Name = path.FileName();
            Hash = path.FileHash();
        }
        public override string ToString() => $"{Name}: {Hash}";
    }
}
