using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Linq;

namespace bkp
{
    public static class Backup
    {
        const string BACKUP_FILE_NAME = "backup.txt";
        public static string TargetFolder { get; set; } = @"D:/Automatic/";
        private static long? _size = null;
        public static bool ClearCache = false;
        private static HashSet<string> _cachedPaths = null;
        private static HashSet<string> CachedPaths
        {
            get
            {
                if(_cachedPaths is null)
                {
                    if (!File.Exists(CACHE_FILE_NAME)) 
                    {
                        File.WriteAllText(CACHE_FILE_NAME, "");
                        _cachedPaths = new();
                    }
                    else
                    {
                        _cachedPaths = File.ReadAllLines(CACHE_FILE_NAME).ToHashSet();
                    }
                }
                return _cachedPaths;
            }
        }
        public static long Size
        {
            get
            {
                if (_size is not null) return _size.Value;
                long result = 0;
                foreach(string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
                {
                    foreach (string filePath in backupTarget.AllFilesRecursive()) 
                        if(!CachedPaths.Contains(filePath))
                            result += new FileInfo(filePath).Length;
                }
                _size = result;
                return result;
            }
        }
        public static long RunningTotal { get; set; } = 0;
        private static StreamWriter Writer = null;
        private const string CACHE_FILE_NAME = "cache.txt";
        public static Task DoBackup()
        {
            if (!File.Exists(CACHE_FILE_NAME) || ClearCache) File.WriteAllText(CACHE_FILE_NAME, "");
            if(ClearCache)
            {
                CachedPaths.Clear();
            }
            Writer = File.AppendText(CACHE_FILE_NAME);
            Queue<(string backupTarget, string filePath)> targetPaths = new();
            foreach (string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
            {
                string cachedPaths = "";
                foreach (string filePath in backupTarget.AllFilesRecursive())
                {
                    if (cachedPaths.Contains(filePath))
                    {
                        cachedPaths += filePath + "\n";
                    } 
                    else
                    {
                        targetPaths.Enqueue((backupTarget, filePath));
                    }
                }
                if (cachedPaths.Length > 1) cachedPaths.TrimEnd('\n');
                MainWindow.Instance.UpdateProgress(Utils.RunFor(cachedPaths, LineType.Cached), -1);
            }
            while(targetPaths.Any())
            {
                (string backupTarget, string filePath) = targetPaths.Dequeue();
                // cache it in case running near midnight  
                string newPath = backupTarget.BackupLocation();
                Directory.CreateDirectory(newPath);
                MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.InProgress), -1);
                long size = new FileInfo(filePath).Length;
                RunningTotal += size;
                Run result = Copy(filePath, filePath.Replace(backupTarget, newPath));
                MainWindow.Instance.UpdateProgress(result, size);
            }
            Writer.Close();
            return Task.CompletedTask;
        }
        static List<string> Parse(this IEnumerable<string> input)
        {
            List<string> ret = new List<string>();
            string basePath = "C:/";
            foreach (string s in input)
            {
                if (!(s.StartsWith('\t') || s.StartsWith(' ')))
                {
                    basePath = s.Trim();
                }
                else
                {
                    ret.Add(basePath + s.Trim());
                }
            }
            return ret;
        }        
        static Run Copy(string oldFilePath, string newFilePath)
        {
            //Utils.Log($"Copying {oldFilePath} to {newFilePath}.");
            if (CachedPaths.Contains(newFilePath))
            {
                return Utils.RunFor(oldFilePath, LineType.Cached);
            }
            else if(File.Exists(newFilePath))
            {
                Writer.WriteLineAsync(oldFilePath);
                return Utils.RunFor(oldFilePath, LineType.Existence);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            try
            {
                File.Copy(oldFilePath, newFilePath);
                Writer.WriteLineAsync(oldFilePath);
                return Utils.RunFor($"{oldFilePath}\n  ↳ {newFilePath}", LineType.Success);
            }
            catch (Exception e)
            {
                Utils.Log(e);
                return Utils.RunFor(oldFilePath, LineType.Failure);
            }
        }
    }
}
