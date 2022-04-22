using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace bkp
{
    public class FileAlias
    {
        public FileHash Primary { get; private set; } = null;
        public string Hash => Primary.Hash;
        private HashSet<string> Aliases = new();
        public FileAlias(FileHash primary)
        {
            Primary = primary;
        }
        public FileAlias(string hash, List<string> paths)
        {
            string firstPath = paths.First();
            Primary = new FileHash(hash, firstPath);
            paths.RemoveAt(0);
            foreach(string s in paths)
            {
                Aliases.Add(s);
            }
        }
        public void Add(FileHash alias)
        {
            if (alias is null) throw new ArgumentNullException(nameof(alias));
            if (alias.Invalid) throw new ArgumentException($"Tried to Add an invalid Alias {alias}.");
            if (alias.Path == Primary.Path)
            {
                Utils.Log($"Attempted to alias {alias} to {Primary}, but the former *is* the primary.");
                return;
            }
            if (alias.Hash != Hash)
            {
                Utils.Log($"Attempted to alias {alias} to {Primary}. Their hashes do not match.");
                return;
            }
            Aliases.Add(alias.Path);
        }
        public string Serialize()
        {
            string ret = Hash + "\n";
            ret += "\t" + Primary.Path + "\n";
            foreach (string s in Aliases) ret += "\t" + s + "\n";
            return ret;
        }
    }
    public class FileHash
    {
        public string Path { get; private set; } = null;
        public string Hash { get; private set; } = null;
        public bool Valid { get; private set; } = false;
        public bool Invalid => !Valid;
        public FileHash(string filePath, HashAlgorithm algo)
        {
            if (!File.Exists(filePath)) return;
            Path = filePath;
            try
            {
                using FileStream fs = File.OpenRead(Path);
                // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256?view=net-6.0
                fs.Position = 0;
                Hash = algo.ComputeHash(fs).Readable();
                Valid = true;
            } catch(Exception e)
            {
                Utils.Log(e);
            }
        }
        public FileHash(string hash, string filePath)
        {
            Path = filePath;
            Hash = hash;
            Valid = true;
        }
        public override string ToString() => $"FileHash {Hash} {Path}";
    }
}
