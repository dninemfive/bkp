using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace bkp
{
    public static class Backup
    {
        const string BACKUP_FILE_NAME = "backup.txt";        
        public static long Size
        {
            get
            {
                long result = 0;
                foreach(string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
                {
                    foreach (string filePath in backupTarget.AllFilesRecursive()) result += new FileInfo(filePath).Length;
                }
                return result;
            }
        }
        public static void DoBackup()
        {
            foreach (string backupTarget in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
            {
                Console.WriteLine(backupTarget);
                // cache it in case running near midnight                
                string s2 = backupTarget.BackupLocation();
                Directory.CreateDirectory(s2);
                foreach (string filePath in backupTarget.AllFilesRecursive())
                {
                    MainWindow.Instance.Progress.Value += new FileInfo(filePath).Length;
                    Copy(filePath, filePath.Replace(backupTarget, s2));
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
        static void Copy(string oldFilePath, string newFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            Utils.PrintLine(oldFilePath);
            try
            {
                // todo: await?
                File.Copy(oldFilePath, newFilePath);
                Utils.PrintLine($"\t↳{newFilePath}");
            }
            catch (Exception e)
            {
                Utils.Log(e);
                Utils.PrintLine("\tFAILED");
            }
        }
    }
}
}
