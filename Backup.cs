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
        private static HashSet<string> cachedPaths = new();
        public static long Size
        {
            get
            {
                if (_size is not null) return _size.Value;
                long result = 0;
                foreach(string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
                {
                    foreach (string filePath in backupTarget.AllFilesRecursive()) result += new FileInfo(filePath).Length;
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
                cachedPaths.Clear();
            }
            cachedPaths = File.ReadAllLines(CACHE_FILE_NAME).ToHashSet();
            Writer = File.AppendText(CACHE_FILE_NAME);
            foreach (string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
            {
                // cache it in case running near midnight                
                string s2 = backupTarget.BackupLocation();
                Directory.CreateDirectory(s2);
                foreach (string filePath in backupTarget.AllFilesRecursive())
                {
                    MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.InProgress), -1);
                    long size = new FileInfo(filePath).Length;
                    RunningTotal += size;
                    Run result = Copy(filePath, filePath.Replace(backupTarget, s2));
                    MainWindow.Instance.UpdateProgress(result, size);
                }
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
            if (cachedPaths.Contains(newFilePath))
            {
                return Utils.RunFor(oldFilePath, LineType.Cached);
            }
            else if(File.Exists(newFilePath))
            {
                Writer.WriteLineAsync(newFilePath);
                return Utils.RunFor(oldFilePath, LineType.Existence);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            try
            {
                File.Copy(oldFilePath, newFilePath);
                Writer.WriteLineAsync(newFilePath);
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
