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
        private static long RunningTotal = 0;
        private static long RunningTotalNonDuplicates = 0;
        public static Task Index(string folderPath)
        {
            using SHA256 Sha256 = SHA256.Create();
            foreach (string filePath in folderPath.AllFilesRecursive())
            {
                // MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.InProgress), -1, RunningTotal, Size);
                long size = new FileInfo(filePath).Length;
                RunningTotal += size;
                LineType type = Add(filePath, Sha256);
                if (type == LineType.Success) RunningTotalNonDuplicates += size;
                MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, type), size, RunningTotal, Size);
            }
            // todo: proper async way to print
            MainWindow.Instance.UpdateProgress(Utils.RunFor($"Size of non-duplicate entries: {RunningTotalNonDuplicates.Readable()}", LineType.Other), -1, RunningTotal, Size);
            return Task.CompletedTask;
        }
        public static LineType Add(string path, HashAlgorithm algo)
        {
            FileHash fileHash = new(path, algo);
            if(Aliases.ContainsKey(fileHash.Hash))
            {
                // we can add even if already present as an alias because the set means we won't get duplicates, 
                //    and checking is probably slower anyway
                Aliases[fileHash.Hash].Add(fileHash);
                return LineType.Existence;
            } 
            else
            {
                Aliases[fileHash.Hash] = new FileAlias(fileHash);
                return LineType.Success;
            }
        }
    }
}
