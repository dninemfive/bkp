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
        static void Start()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            File.Delete(LOG_PATH);
            DateToday = DateTime.Now.ToString(DATE_FORMAT);
            foreach (string s in File.ReadAllLines(BACKUP_FILE_NAME).Parse())
            {
                Console.WriteLine(s);
                // cache it in case running near midnight                
                string s2 = s.BackupLocation();
                Directory.CreateDirectory(s2);
                Console.WriteLine($"{s} ({s.Exists()}) -> {s2} ({s2.Exists()})");
                foreach (string f in s.AllFilesRecursive())
                {
                    string newPath = f.Replace(s, s2);
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                    Console.WriteLine($"\t{f} -> {newPath}");
                    try
                    {
                        File.Copy(f, newPath);
                    }
                    catch (Exception e)
                    {
                        Log(e);
                    }
                }
            }
            Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}");
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
        
    }
}
}
