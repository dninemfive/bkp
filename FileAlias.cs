using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (alias.Hash != Hash)
            {
                Utils.Log($"Attempted to alias {alias} to {Primary}. Their hashes do not match.");
            }
            Aliases.Add(alias.Path);
        }
    }
    public class FileHash
    {
        public string Path { get; private set; } = null;
        public byte[] Hash { get; private set; } = null;
        public bool Valid { get; private set; } = false;
        public FileHash(string filePath)
        {
            if (!File.Exists(filePath)) return;
            Path = filePath;
            // todo: actually hash the file
            throw new NotImplementedException("haven't written the hashing bit yet");
            try
            {
                // hash
                Valid = true;
            } catch(Exception e)
            {
                Utils.Log(e);
            }
        }
        public override string ToString()
        {
            // todo: readable hash representation
            return $"FileHash\n\t{Path}\n\t{Hash}";
        }
    }
}
