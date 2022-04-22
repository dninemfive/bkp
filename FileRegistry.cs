using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace bkp
{
    public static class FileRegistry
    {
        public static Dictionary<string, FileAlias> Aliases { get; private set; } = new();        
        public static void Load(string registryPath)
        {
            string hash = null;
            List<string> paths = new();
            foreach (string line in File.ReadAllLines("test1.txt"))
            {
                if (!line.StartsWith('\t'))
                {
                    // if there's already a hash, add it to the db before resetting
                    if(hash != null && paths.Any())
                    {
                        FileAlias fa = new(hash, paths);
                        Aliases[fa.Hash] = fa;
                    }
                    hash = line.Trim();
                    paths = new();
                }
                else
                {
                    paths.Add(line.Trim());
                }
            }
            // take care of remaining hash/path combo
            FileAlias fa2 = new(hash, paths);
            Aliases[fa2.Hash] = fa2;
        }
        public static void Save(string registryPath)
        {
            File.WriteAllText(registryPath, "");
            foreach(FileAlias fa in Aliases.Values)
            {
                File.AppendAllText(registryPath, fa.Serialize());
            }
        }
        public static long Size { get; private set; }
        public static void SetSize(string folderPath)
        {
            Size = folderPath.FolderSize();
        }
        private static long RunningTotal;
        private static long RunningTotalNonDuplicates;
        public static Task Index(string folderPath)
        {
            using SHA256 Sha256 = SHA256.Create();
            foreach (string filePath in folderPath.AllFilesRecursive())
            {
                // MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.InProgress), -1, RunningTotal, Size);
                long size = new FileInfo(filePath).Length;
                RunningTotal += size;
                Add(filePath, Sha256);
                MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.Success), size, RunningTotal, Size);
            }
            return Task.CompletedTask;
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
