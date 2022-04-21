using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace bkp
{
    public static class FileRegistry
    {
        public static Dictionary<byte[], FileAlias> Aliases { get; private set; } = new();
        public static void Index(string folderPath)
        {
            using SHA256 Sha256 = SHA256.Create();
            foreach (string filePath in folderPath.AllFilesRecursive()) Add(filePath, Sha256);
        }
        public static void Add(string path, HashAlgorithm algo)
        {
            FileHash fileHash = new(path, algo);
            if(Aliases.ContainsKey(fileHash.Hash))
            {
                // we can add even if already present as an alias because the set means we won't get duplicates, 
                //    and checking is probably slower anyway
                Aliases[fileHash.Hash].Add(fileHash);
            } 
            else
            {
                Aliases[fileHash.Hash] = new FileAlias(fileHash);
            }
        }
    }
}
