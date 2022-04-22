﻿using System;
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
        const string BACKUP_FILE_NAME = "backup.txt";
        public static string TargetFolder { get; set; } = @"D:/Automatic/";
        private static long? _size = null;
        public static long Size
        {
            get
            {
                if (_size is not null) return _size.Value;
                long result = 0;
                foreach (string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse()) result += backupTarget.FolderSize();
                _size = result;
                return result;
            }
        }
        public static long RunningTotal { get; set; } = 0;
        public static string Summary => $"{RunningTotal.Readable()}/{Size.Readable()} ({(RunningTotal / (double)Size):P1})";
        public static Task DoBackup()
        {
            foreach (string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
            {
                // cache it in case running near midnight                
                string s2 = backupTarget.BackupLocation();
                Directory.CreateDirectory(s2);
                foreach (string filePath in backupTarget.AllFilesRecursive())
                {
                    MainWindow.Instance.UpdateProgress(Utils.RunFor(filePath, LineType.InProgress), -1, replacePrevious: false);
                    long fileSize = new FileInfo(filePath).Length;
                    RunningTotal += fileSize;
                    Run result = Copy(filePath, filePath.Replace(backupTarget, s2));
                    MainWindow.Instance.UpdateProgress(result, fileSize, Summary, true);
                }
            }
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
            if (File.Exists(newFilePath)) return Utils.RunFor(oldFilePath, LineType.Existence);
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            try
            {
                File.Copy(oldFilePath, newFilePath);
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
