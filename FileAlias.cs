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
        public byte[] Hash => Primary.Hash;
        public HashSet<string> Aliases;
        public FileAlias(FileHash primary)
        {
            Primary = primary;
        }
        public void Add(FileHash alias)
        {
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
    }
    public class FileHash
    {
        public string Path { get; private set; } = null;
        public byte[] Hash { get; private set; } = null;
        public bool Valid { get; private set; } = false;
        public FileHash(string filePath, HashAlgorithm algo)
        {
            if (!File.Exists(filePath)) return;
            Path = filePath;
            try
            {
                using FileStream fs = File.OpenRead(Path);
                // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256?view=net-6.0
                fs.Position = 0;
                Hash = algo.ComputeHash(fs);
                Valid = true;
            } catch(Exception e)
            {
                Utils.Log(e);
            }
        }
        public override string ToString() => $"FileHash {Hash.Readable()} {Path}";
    }
}
