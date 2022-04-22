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
        public static string Summary => $"{RunningTotal.Readable()}/{Size.Readable()} ({(RunningTotal / (double)Size):P1})" +
                                        $"; compression ratio {(RunningTotalNonDuplicates / (double)RunningTotal):P1}";
        public static Task Index(string folderPath)
        {
            // todo: check if index.bkp exists, and if so load that instead. have a "reindex" flag which updates changed files but DOES NOT remove missing entries
            // (since those could be deleted once the program supports deleting duplicate files)
            using SHA256 Sha256 = SHA256.Create();
            foreach (string filePath in folderPath.AllFilesRecursive().ToHashSet().OrderBy(x => x))
            {
                MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.InProgress), -1, replacePrevious: false);
                long fileSize;
                try
                {
                    fileSize = new FileInfo(filePath).Length;
                } 
                catch
                {
                    MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.Failure), -1);
                    continue;
                }
                if (fileSize == 0) continue;
                RunningTotal += fileSize;
                LineType type = Add(filePath, Sha256);
                if (type == LineType.Success) RunningTotalNonDuplicates += fileSize;
                // todo: progress bar showing each LineType color in proportion to the size of the files with those types
                MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, type), fileSize, Summary, replacePrevious: true);
            }
            // todo: proper async way to print
            MainWindow.Instance.PrintLineFromThread($"Size of non-duplicate entries: {RunningTotalNonDuplicates.Readable()}");
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
