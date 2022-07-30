using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace bkp
{
    public static class Backup
    {
        const string BACKUP_SOURCE_FILE = "backup.txt";
        public static IEnumerable<string> BackupSources => System.IO.File.ReadAllLines(BACKUP_SOURCE_FILE);
        public static string TargetFolder { get; set; } = @"D:/Automatic/";
        private static long? _size = null;
        public static long Size
        {
            get
            {
                if (_size is not null) return _size.Value;
                long result = 0;
                foreach(string backupSource in BackupSources)
                {
                    foreach (string filePath in backupSource.AllFilesRecursive())
                    {
                        try
                        {
                            result += new FileInfo(filePath).Length;
                        } catch(Exception e)
                        {
                            Utils.Log(e);
                        }
                    }
                }
                _size = result;
                return result;
            }
        }
        public static long RunningTotal { get; set; } = 0;
        public static Task DoBackup()
        {
            foreach (string backupSources in BackupSources)
            {
                // cache it in case running near midnight                
                string s2 = backupSources.BackupLocation();
                Directory.CreateDirectory(s2);
                foreach (string filePath in backupSources.AllFilesRecursive())
                {
                    MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.InProgress), -1);
                    long size = new FileInfo(filePath).Length;
                    RunningTotal += size;
                    Run result = Copy(filePath, filePath.Replace(backupSources, s2));
                    MainWindow.Instance.UpdateProgress(result, size);
                }
            }
            return Task.CompletedTask;
        }
        static Run Copy(string oldFilePath, string newFilePath)
        {
            if (System.IO.File.Exists(newFilePath)) return Utils.RunFor(oldFilePath, LineType.Existence);
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            try
            {
                System.IO.File.Copy(oldFilePath, newFilePath);
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
