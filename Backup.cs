﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace bkp
{
    public static class Backup
    {
        const string BACKUP_FILE_NAME = "backup.txt";
        private static long? _size = null;
        public static long Size
        {
            get
            {
                if (_size is not null) return _size.Value;
                long result = 0;
                foreach(string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
                {
                    foreach (string filePath in backupTarget.AllFilesRecursive()) result += 1; // new FileInfo(filePath).Length;
                }
                _size = result;
                return result;
            }
        }
        public static long RunningTotal { get; set; } = 0;
        public static Task Start(Progress<(Run, long)> progress)
        {
            Utils.Log("\t\ti'm stuff");
            return Task.Run(() => DoBackup(progress));
        }
        public static void DoBackup(IProgress<(Run, long)> progress)
        {
            foreach (string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
            {
                Console.WriteLine(backupTarget);
                // cache it in case running near midnight                
                string s2 = backupTarget.BackupLocation();
                Directory.CreateDirectory(s2);
                foreach (string filePath in backupTarget.AllFilesRecursive())
                {
                    // https://stackoverflow.com/questions/66968623/updating-ui-from-the-async-method
                    long size = new FileInfo(filePath).Length;
                    RunningTotal += size;
                    Run result = Copy(filePath, filePath.Replace(backupTarget, s2));
                    progress?.Report((result, size));
                }
            }            
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
            if (File.Exists(oldFilePath)) return Utils.RunFor(oldFilePath, LineType.Existence);
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            try
            {
                // todo: await?
                File.Copy(oldFilePath, newFilePath);
                return Utils.RunFor($"{oldFilePath}\n\t↳{newFilePath}", LineType.Success);
            }
            catch (Exception e)
            {
                Utils.Log(e);
                return Utils.RunFor(oldFilePath, LineType.Failure);
            }
        }
    }
}
